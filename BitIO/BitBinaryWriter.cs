

using System.IO;
using System.Text;

namespace BitIO
{
    /// <summary>
    /// A BinaryWriter implementation to write individual bits to a stream.
    /// </summary>
    public class BitBinaryWriter : BinaryWriter
    {
        private readonly BitStream _stream;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryWriter"/> class
        /// with the underlying <paramref name="stream"/> and <paramref name="encoding"/>.
        /// </summary>
        public BitBinaryWriter(Stream stream, Encoding encoding)
            : this(new BitStreamWriter(stream), encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryWriter"/> class
        /// with the underlying <paramref name="stream"/> and default encoding (UTF8).
        /// </summary>
        public BitBinaryWriter(Stream stream)
            : this(stream, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryWriter"/> class
        /// with the underlying <paramref name="stream"/> and <paramref name="encoding"/>.
        /// </summary>
        public BitBinaryWriter(BitStream stream, Encoding encoding)
            : base(stream, encoding)
        {
            _stream = stream;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryWriter"/> class
        /// with the underlying <paramref name="stream"/> and default encoding (UTF8).
        /// </summary>
        public BitBinaryWriter(BitStream stream)
            : this(stream, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitBinaryWriter"/> class.
        /// </summary>
        public BitBinaryWriter()
            : this(null, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Writes a one-bit Boolean value to the current stream, with 0 representing false and 1 representing true.
        /// </summary>
        public override void Write(bool value)
        {
            _stream.Write(value);
        }
    }
}