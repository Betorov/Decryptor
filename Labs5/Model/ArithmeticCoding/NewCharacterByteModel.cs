

namespace Labs5.Model.ArithmeticCoding
{
    internal class NewCharacterByteModel : IModel<byte>
    {
        private readonly PartialSumTreeFixedSize _stats;

        public NewCharacterByteModel()
        {
            _stats = new PartialSumTreeFixedSize(byte.MaxValue);
            Init();
        }

        private uint this[byte index]
        {
            get { return _stats.GetWeight(index); }
        }

        private void Init()
        {
            for(uint symbol = 0; symbol <= byte.MaxValue; ++symbol)
            {
                _stats.UpdateWeight(symbol, 1);
            }
        }

        #region IModel<byte> Members

        public uint TotalFrequencies { get { return _stats.TotalWeight; } }

        public Range GetRange(byte symbol)
        {
            // cumulate frequencies
            uint low = 0;
            byte j = 0;
            for (; j < symbol; j++)
            {
                low += this[j];
            }
            return new Range {Low = low, High = low + 1};
        }

        public void Update(byte symbol)
        {
            if (_stats.GetWeight(symbol) > 0)
            {
                _stats.UpdateWeight(symbol, -1);
            }
        }

        public RangeSymbol<byte> Decode(uint value)
        {
            byte symbol = (byte) _stats.GetSymbol(value);

            return new RangeSymbol<byte>
                       {
                           Range = _stats[symbol],
                           Symbol = symbol
                       };
        }

        public void Reset()
        {
            _stats.Reset();
            Init();
        }

        #endregion

        public bool Emitted(byte index)
        {
            return this[index] == 0;
        }
    }
}