using GalaSoft.MvvmLight;
using Labs5.Model;
using Labs5.Model.ArithmeticCoding;
using Labs5.Model.Decode;
using Labs5.Model.Encode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs5.ViewModel
{
    class CompressViewModel : ViewModelBase
    {        
        public CompressViewModel()
        {
            
        }

        private int lentgh = 0;
        

        public void CompressStart(string inputFile, string outputFile)
        {
            int max = 0;
            using (var reader = File.OpenRead("C://1.txt"))
            using (var writer = new FileStream("C://1compress.txt", FileMode.Create))
            {
                int b = 0;
                byte[] bytes = new byte[reader.Length];
                for (int i = 0; (b = reader.ReadByte()) != -1; i++)
                {
                    bytes[i] = (byte)b;                  
                }
                max = bytes.Max();
                var mainContext = new ArithmeticSymbolArrayContext(257, _ => 1);
                ArithmeticCodingWriterStream writerStream = new ArithmeticCodingWriterStream(writer, mainContext);

                for (int i = 0; i < bytes.Length; i++)
                {
                    writerStream.WriteByte(bytes[i]);
                    mainContext.IncrementSymbolFrequency(bytes[i]);
                    writerStream.SetContext(mainContext);
                }
                writerStream.Close();
                reader.Close();
            }

            using (var reader = File.OpenRead("C://1compress.txt"))
            using (var writer = new FileStream("C://1Decompress.txt", FileMode.Create))
            {
                var mainContext = new ArithmeticSymbolArrayContext(257, _ => 1);
                ArithmeticCodingReaderStream readerStream = new ArithmeticCodingReaderStream(reader, mainContext);

                int sym = 0;
                for (int i = 0; (sym = readerStream.ReadByte()) != -1; i++)
                {                   
                    writer.WriteByte((byte)sym);
                    mainContext.IncrementSymbolFrequency(sym);
                    readerStream.SetContext(mainContext);
                }

                readerStream.Close();
                writer.Close();
            }
        }
    }
}
