

using System;
using System.IO;

namespace BitIO
{
    /// <summary>
    /// Stream class for writing individual bits to a stream.
    /// Packs subsequent bytes written to the stream.
    /// </summary>
    public abstract class BitStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly BitRingBuffer _bitBuffer;
        private readonly byte[] _conversionBuffer;
        private readonly bool _leaveOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitStream"/> class.
        /// </summary>
        protected BitStream()
            : this(null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitStream"/> class
        /// with backing <paramref name="stream"/>.  The stream will be automatically
        /// closed when this stream is closed.
        /// </summary>
        protected BitStream(Stream stream)
            : this(stream, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OneOddSock.IO.BitStream"/> class
        /// with backing <paramref name="stream"/>.  The stream will be closed only if
        /// <paramref name="leaveOpen"/> is false.
        /// </summary>
        protected BitStream(Stream stream, bool leaveOpen)
        {
            _baseStream = stream;
            _leaveOpen = leaveOpen;
            _bitBuffer = new BitRingBuffer();
            _conversionBuffer = new byte[0x100];
        }

        /// <summary>
        /// The base stream being read/written.
        /// </summary>
        protected Stream BaseStream
        {
            get { return _baseStream; }
        }

        /// <summary>
        /// The ring buffer for managing bit level reads and writes.
        /// </summary>
        protected BitRingBuffer BitBuffer
        {
            get { return _bitBuffer; }
        }

        /// <summary>
        /// A temporary buffer for conversions.
        /// </summary>
        protected byte[] TemporaryBuffer
        {
            get { return _conversionBuffer; }
        }

        /// <summary>
        /// The number of bits in the stream.
        /// </summary>
        public abstract long BitLength { get; }

        /// <summary>
        /// The current bit position in the stream.
        /// </summary>
        public abstract long BitPosition { get; }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written
        /// to the underlying device.
        /// </summary>
        public override void Flush()
        {
            InternalFlushBuffer(true);
            _baseStream.Flush();
        }

        /// <summary>
        /// Reads the a boolean as a single bit from the stream.
        /// </summary>
        public abstract bool ReadBoolean();

        /// <summary>
        /// Writes a bit to the stream.
        /// </summary>
        public abstract void Write(bool value);

        /// <summary>
        /// Flushes the internal buffer to disk.
        /// </summary>
        protected void InternalFlushBuffer(bool flushAllBits)
        {
            if (flushAllBits)
            {
                // Add bits until we can write out all bits during the flush.
                int bitsToFlush = (8 - (_bitBuffer.LengthBits%8))%8;
                for (int i = 0; i < bitsToFlush; ++i)
                {
                    _bitBuffer.Write(false);
                }
            }
            int toTransfer = Math.Min(_conversionBuffer.Length, _bitBuffer.LengthBytes);
            _bitBuffer.ReadBytes(_conversionBuffer, 0, toTransfer);
            _baseStream.Write(_conversionBuffer, 0, toTransfer);
        }

        /// <summary>
        /// Disposes the base stream if set and leaveOpen is false.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!_leaveOpen && _baseStream != null)
            {
                _baseStream.Dispose();
            }
        }
    }
}