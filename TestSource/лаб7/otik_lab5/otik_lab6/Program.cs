using System;
using System.IO;

namespace otik_lab6
{
    class Program
    {
        static short InsertControlBits(byte b)
        {
            short ret = 0;
            byte mask = 128; //0x80

            if ((mask & b) != 0)
            {
                ret |= 1;
            }

            ret <<= 1;

            for (int i = 1; i < 8; i++)
            {
                ret <<= 1;
                mask >>= 1;
                if (i == 4 || i == 8)
                    ret <<= 1;
                if ((mask & b) != 0)
                    ret |= 1;
            }

            // Вывод результата
            // var outBin = Convert.ToString(ret, 2);
            // var inBin = Convert.ToString(b, 2);
            // Console.WriteLine(inBin + " -> " + outBin);

            return ret;
        }

        static short CalculateControlBits(short b)
        {
            short ret = b;
            short mask = 2048; // 0x800
            int count = 0;
            bool[] r = new bool[4] { false, false, false, false };

            // Вычисление 1 контрольного бита ()
            for (int i = 0; i < 6; i++)
            {
                if ((mask & b) != 0)
                    count++;

                mask >>= 2;
            }
            if (count % 2 != 0)
                r[0] = true;

            // Вычисление 2 контрольного бита ()
            mask = 1024;
            count = 0;

            for (int i = 0; i < 3; i++)
            {
                if ((mask & b) != 0)
                    count++;

                mask >>= 1;

                if ((mask & b) != 0)
                    count++;

                mask >>= 3;
            }
            if (count % 2 != 0)
                r[1] = true;

            // Вычисление 3 контрольного бита
            mask = 256;
            count = 0;

            for (int j = 0; j < 4; j++)
            {
                if ((mask & b) != 0)
                    count++;
                mask >>= 1;
            }

            mask >>= 4;

            if ((mask & b) != 0)
                    count++;

            if (count % 2 != 0)
                r[2] = true;

            // Вычисление 4 контрольного бита
            mask = 16;
            count = 0;

            for (int j = 0; j < 5; j++)
            {
                if ((mask & b) != 0)
                    count++;
                mask >>= 1;
            }
            if (count % 2 != 0)
                r[3] = true;

            // Добавляем вычисленные контрольные биты в результат
            mask = 2048;
            if (r[0])
                ret |= mask;

            mask = 1024;
            if (r[1])
                ret |= mask;

            mask = 256;
            if (r[2])
                ret |= mask;

            mask = 16;
            if (r[3])
                ret |= mask;

            // Вывод результата
            //var outBin = Convert.ToString(ret, 2);
            //var inBin = Convert.ToString(b, 2);
            //Console.WriteLine(inBin + " -> " + outBin);

            return ret;
        }

        static short CheckControlBits(short b, ref bool flag)
        {
            short mask = 2048;
            int count = 0;
            bool[] r = new bool[4] { false, false, false, false };

            // Вычисление 1 контрольного бита
            for (int i = 0; i < 6; i++)
            {
                if ((mask & b) != 0)
                    count++;

                mask >>= 2;
            }
            if (count % 2 != 0)
                r[0] = true;

            // Вычисление 2 контрольного бита
            mask = 1024;
            count = 0;

            for (int i = 0; i < 3; i++)
            {
                if ((mask & b) != 0)
                    count++;

                mask >>= 1;

                if ((mask & b) != 0)
                    count++;

                mask >>= 3;
            }
            if (count % 2 != 0)
                r[1] = true;

            // Вычисление 3 контрольного бита
            mask = 256;
            count = 0;

            for (int j = 0; j < 4; j++)
            {
                if ((mask & b) != 0)
                    count++;
                mask >>= 1;
            }

            mask >>= 4;

            if ((mask & b) != 0)
                count++;

            if (count % 2 != 0)
                r[2] = true;

            // Вычисление 4 контрольного бита
            mask = 16;
            count = 0;

            for (int j = 0; j < 5; j++)
            {
                if ((mask & b) != 0)
                    count++;
                mask >>= 1;
            }
            if (count % 2 != 0)
                r[3] = true;

            // Проверка
            int check = 0;

            if (r[0]) check += 1;
            if (r[1]) check += 2;
            if (r[2]) check += 4;
            if (r[3]) check += 8;

            // Вывод результата + исправление (если требуется)
            if (check != 0)
            {
               /// Console.WriteLine("Ошибка в " + check + " бите.");

                //var inBin = Convert.ToString(b, 2);
                //Console.WriteLine("До: " + inBin);

                mask = (short)Math.Pow(2, 12 - check);
                b = (short)(b ^ mask);

                flag = true;

                //inBin = Convert.ToString(b, 2);
                //Console.WriteLine(inBin);
            }

            return b;
        }

        static byte ExtractControlBits(short b)
        {
            byte ret = 0;
            bool[] bytes = new bool[8] { false, false, false, false, false, false, false, false };
            short mask = 512;

            // Извлечение информации
            if ((mask & b) != 0)
                bytes[0] = true;

            mask = 128;

            if ((mask & b) != 0)
                bytes[1] = true;

            mask = 64;

            if ((mask & b) != 0)
                bytes[2] = true;

            mask = 32;

            if ((mask & b) != 0)
                bytes[3] = true;

            mask = 8;

            if ((mask & b) != 0)
                bytes[4] = true;

            mask = 4;

            if ((mask & b) != 0)
                bytes[5] = true;

            mask = 2;

            if ((mask & b) != 0)
                bytes[6] = true;

            mask = 1;

            if ((mask & b) != 0)
                bytes[7] = true;

            for (int i = 0; i < 8; i++)
            {
                ret <<= 1;
                if (bytes[i])
                    ret |= 1;
            }

            return ret;
        }

        static short Pack(byte b)
        {
            short ret;

            ret = InsertControlBits(b);
            ret = CalculateControlBits(ret);

            return ret;
        }

        static byte Unpack(short b, ref bool flag)
        {
            byte ret;
            short temp;

            temp = CheckControlBits(b, ref flag);
            ret = ExtractControlBits(temp);

            return ret;
        }

        static void encode(string fileIn, string fileOut)
        {
            BinaryWriter writer = null;
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(File.Open(fileIn, FileMode.Open));
                writer = new BinaryWriter(File.Open(fileOut, FileMode.OpenOrCreate));

                byte b;
                short s;
                while (reader.PeekChar() > -1)
                {
                    b = reader.ReadByte();
                    s = Pack(b);

                    writer.Write(s);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (reader != null)
                    reader.Close();
            }
        }

        static void decode(string fileIn, string fileOut)
        {
            BinaryWriter writer = null;
            BinaryReader reader = null;

            try
            {
                reader = new BinaryReader(File.Open(fileIn, FileMode.Open));
                writer = new BinaryWriter(File.Open(fileOut, FileMode.OpenOrCreate));

                byte b;
                short s;
                bool flag = false;
                int count = 0;

                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    s = reader.ReadInt16();
                    count++;
                    b = Unpack(s, ref flag);

                    if (flag)
                    {
                        //Console.WriteLine("Ошибка в " + count + " символе.");
                        flag = false;
                    }

                    writer.Write(b);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
                if (reader != null)
                    reader.Close();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage\nlab in.txt  out.txt C/D");
                return;
            }
            if (args[2].ToLower().Equals("d"))
            {

                decode(args[0], args[1]);
            }
            else if (args[2].ToLower().Equals("c"))
            {
                encode(args[0], args[1]);
            } else
            {
                Console.WriteLine("Usage\nlab in.txt  out.txt C/D");
            }
        }
    }
}
