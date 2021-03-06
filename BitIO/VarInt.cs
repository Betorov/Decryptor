

using BitIO.Converters;
using System;
using System.IO;


namespace BitIO
{
    /// <summary>
    /// Extension methods to write variable length integers to a 
    /// BinaryWriter.
    /// </summary>
    public static class VarInt
    {
        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, ulong value)
        {
            ulong temp = value;
            uint iteration = 1;
            do
            {
                byte b = (iteration < 9)
                             ? (byte) ((byte) (temp & 0x7f) | (byte) ((temp > 0x7f) ? 0x80 : 0))
                             : (byte) (temp & 0xff);
                writer.Write(b);
                temp = temp >> ((iteration < 9) ? 7 : 8);
                ++iteration;
            } while (temp != 0);
        }

        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, uint value)
        {
            writer.WriteVar((ulong) value);
        }

        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, ushort value)
        {
            writer.WriteVar((ulong) value);
        }

        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, long value)
        {
            writer.WriteVar(value.ZigZag());
        }

        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, int value)
        {
            writer.WriteVar(value.ZigZag());
        }

        /// <summary>
        /// Writes a variable length representation of <paramref name="value"/>
        /// to the stream managed by <paramref name="writer"/>.
        /// </summary>
        public static void WriteVar(this BinaryWriter writer, short value)
        {
            writer.WriteVar(value.ZigZag());
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static UInt64 ReadVarUInt64(this BinaryReader reader)
        {
            ulong result = 0;
            int iteration = 0;
            bool pendingData;
            do
            {
                byte b = reader.ReadByte();
                pendingData = (iteration < 8) && (b & 0x80) != 0;
                var v = (ulong) (iteration < 8 ? (b & 0x7f) : b);
                result = result + (v << (7*iteration));
                ++iteration;
            } while (pendingData);
            return result;
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static uint ReadVarUInt32(this BinaryReader reader)
        {
            ulong result = reader.ReadVarUInt64();
            if (result <= uint.MaxValue)
            {
                return (uint) result;
            }
            throw new OverflowException();
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static ushort ReadVarUInt16(this BinaryReader reader)
        {
            ulong result = reader.ReadVarUInt64();
            if (result <= ushort.MaxValue)
            {
                return (ushort) result;
            }
            throw new OverflowException();
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static long ReadVarInt64(this BinaryReader reader)
        {
            return reader.ReadVarUInt64().ZigZag();
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static int ReadVarInt32(this BinaryReader reader)
        {
            long result = reader.ReadVarUInt64().ZigZag();
            if (result >= int.MinValue && result <= int.MaxValue)
            {
                return (int) result;
            }
            throw new OverflowException();
        }

        /// <summary>
        /// Reads a variable length representation of an integer from the stream
        /// managed by <paramref name="reader"/>.
        /// </summary>
        public static short ReadVarInt16(this BinaryReader reader)
        {
            long result = reader.ReadVarUInt64().ZigZag();
            if (result >= short.MinValue && result <= short.MaxValue)
            {
                return (short) result;
            }
            throw new OverflowException();
        }
    }
}