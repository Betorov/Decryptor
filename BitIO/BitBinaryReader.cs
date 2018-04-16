

using System.IO;
using System.Text;

namespace BitIO
{
    /// <summary>
    /// BinaryReader that supports reading and writing individual bits from
    /// the stream.
    /// </summary>
    public class BitBinaryReader : BinaryReader
    {
        private readonly BitStream _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryReader"/> class
        /// using stream <paramref name="input"/> and <paramref name="encoding"/>.
        /// </summary>
        public BitBinaryReader(Stream input, Encoding encoding)
            : this(new BitStreamReader(input), encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryReader"/> class
        /// using stream <paramref name="input"/>.
        /// </summary>
        public BitBinaryReader(Stream input)
            : this(input, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryReader"/> class.
        /// </summary>
        public BitBinaryReader(BitStream input, Encoding encoding)
            : base(input, encoding)
        {
            _reader = input;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryReader"/> class.
        /// </summary>
        public BitBinaryReader(BitStream input)
            : this(input, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Reads a Boolean value from the current stream and advances the current position of the stream by one bit.
        /// </summary>
        /// <returns>
        /// true if the bit is nonzero; otherwise, false.
        /// </returns>
        public override bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }
    }
}