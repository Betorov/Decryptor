

using BitIO.Converters;
using System;


namespace BitIO
{
    /// <summary>
    /// Endian identification support for the current platform
    /// and streams.  Supports conversion of data between Endian
    /// settings.
    /// 
    /// Note: this class still assumes that bit orderings are 
    /// the same regardless of byte Endian configurations.
    /// </summary>
    public class Endian
    {
        static Endian()
        {
            Little = new Endian();
            Big = new Endian();
            Native = BitConverter.IsLittleEndian ? Little : Big;
            NonNative = BitConverter.IsLittleEndian ? Big : Little;
        }

        private Endian()
        {
        }

        /// <summary>
        /// Retrieve the big Endian instance.
        /// </summary>
        public static Endian Big { get; private set; }

        /// <summary>
        /// Retrieve the little Endian instance.
        /// </summary>
        public static Endian Little { get; private set; }

        /// <summary>
        /// Retrieve the platform native Endian instance.
        /// </summary>
        public static Endian Native { get; private set; }

        /// <summary>
        /// Retrieve the non-native Endian instance.
        /// </summary>
        public static Endian NonNative { get; private set; }

        /// <summary>
        /// Retrieves the other Endian instance from the current.
        /// </summary>
        public Endian Switch
        {
            get { return this == Big ? Little : Big; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is native.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is native; otherwise, <c>false</c>.
        /// </value>
        public bool IsNative
        {
            get { return this == Native; }
        }

        /// <summary>
        /// Creates a converter for changing data from the current
        /// Endian setting to the target Endian setting.
        /// </summary>
        public EndianConverter To(Endian target)
        {
            return EndianConverter.Create(this != target);
        }
    };
}