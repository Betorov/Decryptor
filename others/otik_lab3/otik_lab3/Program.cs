using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;



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
        static List<Dictionary<byte, double>> count = new List<Dictionary<byte, double>>();//256
        static List<Dictionary<byte, string>> codingbytes = new List<Dictionary<byte, string>>();//256
        static List<Dictionary<byte, long>> countBytes = new List<Dictionary<byte, long>>();//256
        static List<Int64> lengthFile = new List<Int64>();

        static void Main(string[] args)
        {          
            List<string> asd = new List<string>();
            asd.Add("test1.txt");
            asd.Add("test2.txt");
            Pack(asd);
            unPack("test1.shf");
            Console.WriteLine("Eazy breezy");
        }



        static void function(List<Dictionary<byte, double>> count, int left, int right, double p, string res)
        {
            
            if (left == right)
            {
                codingbytes.Last().Add(count.Last().ElementAt(left).Key, res);
                //Console.WriteLine(count.ElementAt(left).Key + " " + res);
                return;
            }

            double suml = 0;
            double sumr = 0;
            int leftCount = 0;
            double z = 0;
            var k = 0;
            foreach (var item in count.Last())
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
        

        #region Частотный анализ
        static void prePack(ICollection<string> fileName)
        {
            for (int k = 0; k < fileName.Count; k++)
            {
                countBytes.Add(new Dictionary<byte, long>(256));
                codingbytes.Add(new Dictionary<byte, string>(256));
                count.Add(new Dictionary<byte, double>(256));

                long lengthFileBytes = 0;
                Int64 length = 0;
                if (File.Exists(fileName.ElementAt(k)))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileName.ElementAt(k), FileMode.Open)))
                    {
                        byte b;
                        while (reader.PeekChar() > -1)
                        {
                            b = reader.ReadByte();
                            if (!count.ElementAt(k).ContainsKey(b)) count.ElementAt(k).Add(b, 1);
                            else count.ElementAt(k)[b]++;
                            lengthFileBytes++;
                        }
                    }
                }
              
                foreach (var item in count.ElementAt(k))
                    countBytes.Last().Add(item.Key, Convert.ToInt64(item.Value));

                for (byte i = 0; i < 255; i++)
                    if (count.ElementAt(k).ContainsKey(i)) count[k][i] /= lengthFileBytes;

                Dictionary<byte, double> bufcount = new Dictionary<byte, double>(256);
                bufcount = count.ElementAt(k).OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);
                count.Remove(count.ElementAt(k));
                count.Add(bufcount);

                function(count, 0, count.Last().Count - 1, 0.5, "");

                foreach (var item in codingbytes.Last())
                {
                    length += countBytes.Last()[item.Key] * item.Value.Length;
                }
                double summ = 0;
                foreach (var item in count[k])
                {
                    summ += (-Math.Log(item.Value, 2)) * countBytes.Last()[item.Key];
                }

                lengthFile.Add(length);
            }
            
        }
        #endregion

        static void Pack(ICollection<string> fileName)
        {
            //Название запакованного файла
            string filename = fileName.ElementAt(0).Substring(0, fileName.ElementAt(0).LastIndexOf('.'));
            filename += ".shf";
     
            prePack(fileName);
                                                                  
            if (File.Exists(filename)) File.Delete(filename);

            BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate));
            writer.Write((byte)'s');
            writer.Write((byte)'h');
            writer.Write((byte)'f');

            for (int i = 0; i < fileName.Count; i++)
            {
                string Format = fileName.ElementAt(i).Substring(fileName.ElementAt(i).LastIndexOf('.') + 1);
                //Запись длину формата файла
                writer.Write((byte)Format.Length);
                //Запись формата файла
                for (int j = 0; j < Format.Length; j++)
                    writer.Write((byte)Format[j]);
                writer.Write(lengthFile.ElementAt(i));
                writer.Write((byte)count[i].Count);


                byte sdvig = 0;
                BitArray myB = new BitArray(8);
                foreach (var item in codingbytes.ElementAt(i))
                {
                    byte[] myBytes = new byte[1] { item.Key };
                    myB = new BitArray(myBytes);
                    for (int k = 0; k < 8; k++)
                        writeBit(ref writer, myB[k]);
                    myBytes = new byte[1] { (byte)item.Value.Length };
                    myB = new BitArray(myBytes);
                    for (int j = 0; j < 8; j++)
                        writeBit(ref writer, myB[j]);
                    for (var k = 0; k < item.Value.Length; k++)
                        writeBit(ref writer, item.Value[k] == '1');
                }


                if (File.Exists(fileName.ElementAt(i)))
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(fileName.ElementAt(i), FileMode.Open)))
                    {
                        byte b;
                        while (reader.PeekChar() > -1)
                        {
                            b = reader.ReadByte();
                            for (int l = 0; l < codingbytes.ElementAt(i)[b].Length; l++)
                                writeBit(ref writer, codingbytes.ElementAt(i)[b][l] == '1');
                        }
                    }
                }
                while (wbytecount != 8)
                    writeBit(ref writer, false);
                writeBit(ref writer, false);
            }          
            writer.Close();
        }
        

        #region Декодирование
        static void unPack(string file)
        {
            if (File.Exists(file))
            {
                var node = new Node();
                BinaryReader reader = new BinaryReader(File.Open(file, FileMode.Open));

                //Чтение сигнатуры файла

                string signature = "";
                signature += (char)reader.ReadByte();
                signature += (char)reader.ReadByte();
                signature += (char)reader.ReadByte();
                if (signature != "shf") return;

                int indexfiles = 0;
                //List<string> format = new List<string>();
                do
                {                  
                    byte lengthformat = reader.ReadByte();
                    string format = "";
                    for (int i = 0; i < lengthformat; i++)
                        format += (char)reader.ReadByte();
                    lengthFile.Add(reader.ReadInt64());

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
                        node.build(str, sym);
                    }
                    //string filename = file.Substring(0, file.LastIndexOf('.'));
                    string filename = indexfiles + "_unpack." + format;
                    indexfiles++;
                    if (File.Exists(filename)) File.Delete(filename);
                    BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate));
                    string s = "";
                    for (int i = 0; i < lengthFile.Last(); i++)
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
                } while (reader.PeekChar() != '♪');
                reader.Close();
            }
        }
        #endregion

        #region Вспомогательные методы

        #region Побитовое чтение
        static bool readBit(ref BinaryReader reader)
        {           
            try
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
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
        #endregion

        #region Побитовая запись
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
