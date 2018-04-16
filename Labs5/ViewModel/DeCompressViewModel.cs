using GalaSoft.MvvmLight;
using Labs5.Model.ArithmeticCoding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs5.ViewModel
{
    class DeCompressViewModel : ViewModelBase
    {
        

        public DeCompressViewModel()
        {
            
        }

        public void DeCompressStart(string inputFile, string outputFile)
        {
            using (var reader = File.OpenRead(inputFile))
            using (var writer = new FileStream(outputFile, FileMode.Create))
            {
                
                //HeaderFiles
                byte[] bytesLength = new byte[4];
                //for (int i = 0; i < 3; i++)
                //bytesLength[i] = (byte)reader.ReadByte();

                //int lentgh = BitConverter.ToInt32(bytesLength, 0);
                int lentgh = 36;
                var decoded = new byte[lentgh];
                var decoder = new ArithmeticStream(reader, CompressionMode.Decompress, true);
                decoder.Read(decoded, 0, decoded.Length);

                for (int i = 0; i < decoded.Length; i++)
                    writer.WriteByte(decoded[i]);
                //for(int )
                // decoders.Decode(reader, writer);
            }
        }
    }
}
