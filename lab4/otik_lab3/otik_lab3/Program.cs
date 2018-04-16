using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;




namespace otik_lab3
{
    class Program
    {
        static bool byteisempty = true;
        static BitArray Byte;
        static byte bytecount;
        static bool wbyteisfull = false;
        static BitArray wByte=new BitArray(8);
        static byte wbytecount=0;
        static Dictionary<byte, double> count = new Dictionary<byte, double>(256);
        static Dictionary<byte, string> codingbytes = new Dictionary<byte, string>(256);
        static Dictionary<byte, long> countBytes = new Dictionary<byte, long>(256);
        static Int64 lengthFile = 0;

        static void Main(string[] args)
        {
            //unPack("123.shf");
            otherFilesPack();
        }

        static void otherFilesPack()
        {
           
        }

        static void function(Dictionary<byte, double> count, int left, int right, double p, string res)
        {
            if (left == right)
            {
                codingbytes.Add(count.ElementAt(left).Key, res);
                //Console.WriteLine(count.ElementAt(left).Key + " " + res);
                return;
            }

            double suml = 0;
            double sumr = 0;
            int leftCount = 0;
            double z = 0;
            var k = 0;
            foreach (var item in count)
            {
                if (k >= left && k < right)
                {
                    if (suml < p)
                    {
                        leftCount++;
                        suml += item.Value;
                        z = item.Value;
                    }
                    else
                        sumr += item.Value;
                }
                if (k >= right)
                {
                    if (suml != p)
                    {
                        suml -= z;
                        sumr += z;
                    }
                    function(count, left, left + leftCount - 1, suml / 2, res + "0");
                    function(count, left + leftCount, right, sumr / 2, res + "1");
                    break;
                }
                k++;
            }
        }
        
        void checkFormat()
        {
            
        }

        #region Частотный анализ
        static void prePack(string fileName)
        {
            long lengthFileBytes = 0;
            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    byte b;
                    while (reader.PeekChar() > -1)
                    {
                        b = reader.ReadByte();
                        if (!count.ContainsKey(b)) count.Add(b, 1);
                        else count[b]++;
                        lengthFileBytes++;
                    }
                }
            }
            foreach (var item in count)
                countBytes.Add(item.Key, Convert.ToInt64(item.Value));
            for (byte i = 0; i < 255; i++)
                if (count.ContainsKey(i)) count[i] /= lengthFileBytes;
            count = count.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
            function(count, 0, count.Count - 1, 0.5, "");
            foreach (var item in codingbytes)
            {
                lengthFile += countBytes[item.Key] * item.Value.Length;
            }
            double summ = 0;
            foreach (var item in count)
            {
                summ += (-Math.Log(item.Value, 2)) * countBytes[item.Key];
            }
            Console.WriteLine(lengthFile + " " + summ);
        }
        #endregion

        static void Pack(string fileName)
        {
            
            prePack(fileName);
            string Format = fileName.Substring(fileName.LastIndexOf('.') + 1);
            string filename = fileName.Substring(0, fileName.LastIndexOf('.'));
            filename += ".shf";

            if (File.Exists(filename)) File.Delete(filename);
            BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate));

            writer.Write((byte)'s');
            writer.Write((byte)'h');
            writer.Write((byte)'f');
            writer.Write((byte)Format.Length);
            for (int i = 0; i < Format.Length; i++)
                writer.Write((byte)Format[i]);
            writer.Write(lengthFile);



            writer.Write((byte)count.Count);
            byte sdvig = 0;
            BitArray myB = new BitArray(8);
            foreach (var item in codingbytes)
            {
                byte[] myBytes = new byte[1] { item.Key };
                myB = new BitArray(myBytes);
                for (int i = 0; i < 8; i++)
                    writeBit(ref writer, myB[i]);
                myBytes = new byte[1] { (byte)item.Value.Length };
                myB = new BitArray(myBytes);
                for (int i = 0; i < 8; i++)
                    writeBit(ref writer, myB[i]);
                for (var i = 0; i < item.Value.Length; i++)
                    writeBit(ref writer, item.Value[i] == '1');
            }
            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    byte b;
                    while (reader.PeekChar() > -1)
                    {
                        b = reader.ReadByte();
                        for (int i = 0; i < codingbytes[b].Length; i++)
                            writeBit(ref writer, codingbytes[b][i] == '1');
                    }
                }
            }
            while (wbytecount != 8)
                writeBit(ref writer, false);
            writeBit(ref writer, false);
            writer.Close();
        }

        #region Декодирование
        static void unPack(string file)
        {
            if (File.Exists(file))
            {
                var node = new Node();
                BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open));
                string signature = "";
                signature += (char)reader.ReadByte();
                signature += (char)reader.ReadByte();
                signature += (char)reader.ReadByte();
                if (signature != "shf") return;
                byte lengthformat = reader.ReadByte();
                string format = "";
                for (int i = 0; i < lengthformat; i++)
                    format += (char)reader.ReadByte();
                lengthFile = reader.ReadInt64();
                byte countdictionary = reader.ReadByte();
                BitArray myB = new BitArray(8);
                for (int i = 0; i < countdictionary; i++)
                {
                    byte sym = 0;
                    byte size = 0;
                    for (int j = 0; j < 8; j++)
                        myB[j] = readBit(ref reader);
                    sym = ConvertToByte(myB);
                    for (int j = 0; j < 8; j++)
                        myB[j] = readBit(ref reader);
                    size = ConvertToByte(myB);
                    string str = "";
                    for (int j = 0; j < size; j++)
                        if (readBit(ref reader)) str += "1";
                        else
                            str += "0";
                    node.build(str,sym);
                }
                string filename = file.Substring(0, file.LastIndexOf('.'));
                filename +="_unpack."+format;
                if (File.Exists(filename)) File.Delete(filename);
                BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate));
                string s = "";
                for (int i = 0; i < lengthFile; i++)
                {
                    if (readBit(ref reader)) s += "1";
                    else s += "0";
                    if (node.search(s))
                    {
                        s = "";
                        writer.Write(Node.B);
                    }
                }
                writer.Close();
                reader.Close();
            }
        }
        #endregion

        #region Вспомогательные методы

        #region Побитовое чтение
        static bool readBit(ref BinaryReader reader)
        {
            if (bytecount == 8)
                byteisempty = true;
            if (byteisempty)
            {
                byte b = reader.ReadByte();
                byte[] myBytes = new byte[1] { b };
                Byte = new BitArray(myBytes);
                byteisempty = false;
                bytecount = 0;
            }
            bytecount++;
            return Byte[bytecount - 1];
        }
        #endregion

        #region Побитвоая запись
        static void writeBit(ref BinaryWriter writer, bool bit)
        {
            if (wbytecount == 8)
                wbyteisfull = true;
            if (wbyteisfull)
            {
                writer.Write(ConvertToByte(wByte));
                wbyteisfull = false;
                wbytecount = 0;
            }
            wByte[wbytecount] = bit;
            wbytecount++;
        }
        #endregion

        #region BitArray to Byte
        static byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }
        #endregion

        #endregion

    }
}
