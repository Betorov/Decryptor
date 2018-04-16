using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs6
{
    class Program
    {
        static string fileNameIn = "C://image1Compr.bmp";
        static string fileNameOut = "C://image1DeCompr.bmp";

        #region Декодирование
        static void Decoding()
        {
            if (File.Exists(fileNameIn))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileNameIn, FileMode.Open)))
                {
                    byte b = 0;

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    //while (reader.PeekChar() > -1)
                    {
                        b = reader.ReadByte();
                        int temp = b;
                        temp = temp & 0x80;

                        if (temp == 0x80)
                        {
                            int count = b;
                            count = count & 0x7F;
                            count += 2;

                            b = reader.ReadByte();

                            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameOut, FileMode.Append)))
                            {
                                for (int i = 0; i < count; i++)
                                    writer.Write(b);
                            }
                        }
                        else
                        {
                            int count = b;
                            count = count & 0x7F;
                            count += 1;

                            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameOut, FileMode.Append)))
                            {
                                for (int i = 0; i < count; i++)
                                {
                                    b = reader.ReadByte();
                                    writer.Write(b);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Кодирование
        // Для функции Encoding_RLE
        static int decCount = 0;
        static bool decFlag = false;
        static List<byte> decList = new List<byte>();
        static bool decFlagEnd = false;

        static void Encoding_RLE(byte b, int count)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(fileNameOut, FileMode.Append)))
            {
                // Если конец
                if (decFlagEnd)
                {
                    if (count == 1)
                    {
                        decCount++;
                        decList.Add(b);
                    }
                    int temp = decList.Count;
                    temp--;
                    temp = temp & 0x7F;
                    byte controlByte = (byte)temp;

                    writer.Write(controlByte);

                    foreach (byte i in decList)
                        writer.Write(i);
                }

                // Если это одиночный байт, то заносим его в List
                if (count == 1)
                {
                    decCount++;
                    decList.Add(b);
                    decFlag = true;

                    if (decCount == 128)
                    {
                        int temp = decList.Count;
                        temp--;
                        temp = temp & 0x7F;
                        byte controlByte = (byte)temp;

                        writer.Write(controlByte);

                        foreach (byte i in decList)
                            writer.Write(i);

                        decFlag = false;
                        decCount = 0;
                        decList.Clear();
                    }
                }
                else
                {
                    // Вывод одиночный байтов
                    if (decFlag && !decFlagEnd)
                    {
                        int temp = decCount;
                        temp--;
                        temp = temp & 0x7F;
                        byte controlByte = (byte)temp;

                        writer.Write(controlByte);

                        foreach (byte i in decList)
                            writer.Write(i);

                        decFlag = false;
                        decCount = 0;
                        decList.Clear();
                    }

                    // Вывод повторяющихся байтов
                    if (count > 129)
                    {
                        while (count != -1)
                        {
                            if (count > 129)
                            {
                                int t = 129;
                                t -= 2;
                                t = t | 0x80;
                                byte controlByte1 = (byte)t;

                                writer.Write(controlByte1);
                                writer.Write(b);

                                count -= 129;
                            }
                            else
                            {
                                int t = count;
                                t -= 2;
                                t = t | 0x80;
                                byte controlByte1 = (byte)t;

                                writer.Write(controlByte1);
                                writer.Write(b);

                                count = -1;
                            }
                        }
                    }
                    else
                    {
                        int t = count;
                        t -= 2;
                        t = t | 0x80;
                        byte controlByte1 = (byte)t;

                        writer.Write(controlByte1);
                        writer.Write(b);
                    }
                }
            }
        }

        static void Encoding()
        {
            if (File.Exists(fileNameIn))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileNameIn, FileMode.Open)))
                {
                    byte b, lastByte;
                    int count = 0;

                    if (reader.PeekChar() > -1)
                    {
                        lastByte = reader.ReadByte();
                        count++;
                    }
                    else
                    {
                        Console.WriteLine("Пустой файл");
                        return;
                    }
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    //while (reader.PeekChar() > -1)
                    {
                        b = reader.ReadByte();

                        if (b == lastByte)
                        {
                            lastByte = b;
                            count++;
                        }
                        else
                        {
                            Encoding_RLE(lastByte, count);
                            count = 1;
                            lastByte = b;
                        }
                    }

                    decFlagEnd = true;
                    Encoding_RLE(lastByte, count);
                }
            }
        }
        #endregion

        static void Main(string[] args)
        {
            Decoding(); // Раскодирование
            //Encoding(); // Кодирование
        }
    }
}
