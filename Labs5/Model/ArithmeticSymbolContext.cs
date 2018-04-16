using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs5.Model
{
    abstract class ArithmeticSymbolContext
    {
        /// <summary>
        ///    Возвращает всю сумму частот символов. Когда перегружена должна означать реальную сумму <see
        ///cref="GetSymbolFrequency"/> для каждого возможного символа.
        ///</summary>
        public abstract uint GetTotal();

        /// <summary>
        /// Возвращает частоту указанного символа. При переопределении необходимо вернуть значение для каждого возможного ввода;
        /// вернуть 0, если символ находится вне диапазона возможных символов.
        /// </summary>
        public abstract uint GetSymbolFrequency(int symbol);

        /// <summary>
        /// Возвращает сумму частот всех символов меньше, чем <paramref name="symbol"/>. При переопределении
        /// возвращает значение для каждого возможного ввода: 0 для всех символов, меньших, чем первый действительный символ, и значение, равное
        /// <see cref="GetTotal"/> для всех символов, превышающих последний действительный символ.
        /// </summary>
        public abstract uint GetSymbolPosition(int symbol);

        /// <summary>
        /// Максимальная сумма всех частот символов. Кодер / декодер может переполняться, если сумма всех частот
        /// превышает это значение.
        /// </summary>
        public const uint MaxTotal = 0x8000_0000;
    }

    /// <summary>
    /// Реализует контекст символа, наиболее подходящий для относительно небольших диапазонов допустимых символов. 
    /// Поддерживает эффективные обновления отдельных частот символов. 
    /// Наименьший поддерживаемый символ равен 0, а использование памяти растет линейно с
    /// самый большой действительного символа.
    /// </summary>
    class ArithmeticSymbolArrayContext : ArithmeticSymbolContext
    {
        private uint[] frequencies;
        private uint[] position;
        private uint total;
        private int positionsValidUntil;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ArithmeticSymbolArrayContext"/>.</summary>
        /// <param name="length">
        /// Сколько символов нужно отслеживать. Действительные символы находятся в диапазоне 0 .. <paramref name="length"/>. All
        /// символы вне этого диапазона имеют частоту 0.</param>
        /// <param name="initializer">
        /// Функция, которая возвращает начальное значение для частоты каждого символа. Необязательный; если опустить, каждый символ
        /// начинается с частоты 1.</param>
        public ArithmeticSymbolArrayContext(int length, Func<int,uint> initializer = null)
        {
            initializer = initializer ?? (_ => 1U);
            frequencies = new uint[length];
            position = new uint[length];
            positionsValidUntil = -1;
            total = 0;
            for(int i = 0; i < length; i++)
            {
                frequencies[i] = initializer(i);
                total = checked(total + frequencies[i]);
            }
            if(total > MaxTotal)
            {
                throwException();
            }
        }

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ArithmeticSymbolArrayContext"/>.</summary>
        /// <param name="frequencies">
        /// Начальные частоты всех символов. Длина этого массива определяет максимальный представляемый символ; все
        /// другие символы имеют частоту 0.</param>
        public ArithmeticSymbolArrayContext(uint[] frequencies)
        {
            this.frequencies = frequencies;
            position = new uint[frequencies.Length];
            positionsValidUntil = -1;
            total = 0;
            for(int i = 0; i < frequencies.Length; i++)
            {
                total = checked(total + frequencies[i]);
            }
            if (total > MaxTotal)
                throwException();
        }

        public void throwException()
        {
            throw new OverflowException($"Общее количество всех частот не должно превышать {nameof(ArithmeticSymbolContext)}" +
                    $".{nameof(MaxTotal)} ({MaxTotal:#,0})");
        }

        public override uint GetSymbolFrequency(int symbol)
        {
            if (symbol < 0 || symbol >= frequencies.Length)
                return 0;
            return frequencies[symbol];
        }


        public override uint GetSymbolPosition(int symbol)
        {
            if (symbol < 0)
                return 0;
            if (symbol >= frequencies.Length)
                return total;

            if (positionsValidUntil >= symbol)
                return position[symbol];

            uint pos = positionsValidUntil < 0 ? 0 : (position[positionsValidUntil] + frequencies[positionsValidUntil]);
            for(int i = positionsValidUntil + 1; i <= symbol; i++)
            {
                position[i] = pos;
                pos += frequencies[i];
            }
            positionsValidUntil = symbol;
            return position[symbol];
        }

        public override uint GetTotal()
        {
            return total;
        }

        /// <summary>
        /// Обновляет частоты всех символов. Использовать этот метод для обновления большого количества частот. использование <see
        /// cref="SetSymbolFrequency(int, uint)"/> для более эффективного обновления небольшого числа частот.</summary>
        /// <param name="updater">
        /// Метод, который принимает текущий массив частот символов. Этот метод может произвольно изменять массив,
        /// но не может заменить его совершенно новым массивом. Чтобы изменить длину массива частот <see
        /// cref="UpdateFrequencies(Func{uint[], uint[]})"/>.</param>
        public void UpdateFrequencies(Action<uint[]> updater)
        {
            updater(frequencies);
            total = 0;
            for (int i = 0; i < frequencies.Length; i++)
                total = checked(total + frequencies[i]);
            if(total > MaxTotal)
                throwException();
            positionsValidUntil = -1;
        }

        /// <summary>
        /// Обновляет частоты всех символов. Используйте этот метод для обновления большого количества частот. использование <see
        /// cref="SetSymbolFrequency(int, uint)"/> для более эффективного обновления небольшого числа частот.</summary>
        /// <param name="updater">
        /// Функция, которая принимает текущий массив частот символов. Эта функция может изменять массив
        /// произвольно и возвращать его, или строить и возвращать совершенно другой массив.</param>
        public void UpdateFrequencies(Func<uint[], uint[]> updater)
        {
            frequencies = updater(frequencies);
            total = 0;
            for (int i = 0; i < frequencies.Length; i++)
                total = checked(total + frequencies[i]);
            if (total > MaxTotal)
                throwException();
            positionsValidUntil = -1;
        }

        /// <summary>
        /// Обновляет частоту указанного символа. Чтобы обновить большое количество частот за один раз
        /// эффективно использовать <see cref="UpdateFrequencies(Action{uint[]})"/>.</summary>
        public void SetSymbolFrequency(int symbol, uint newFrequency)
        {
            if (symbol < 0 || symbol >= frequencies.Length)
                throw new ArgumentOutOfRangeException(nameof(symbol), "Символ за диапазоном.");
            var oldFrequency = frequencies[symbol];
            frequencies[symbol] = newFrequency;
            total = checked(total - oldFrequency + newFrequency);
            if (total > MaxTotal)
                throwException();
            positionsValidUntil = symbol;
        }

        /// <summary>
        /// Обновляет частоту указанного символа, добавляя <paramref name="incrementBy"/> </summary>
        /// <param name="symbol">
        /// Символ, частота которого должна быть обновлена.</param>
        /// <param name="incrementBy">
        /// Значение, добавляемое к текущей частоте. Это может быть отрицательным. Если частота символа становится
        /// отрицательный <see cref="ArgumentException"/> бросается.</param>
        public void IncrementSymbolFrequency(int symbol, int incrementBy = 1)
        {
            if (symbol < 0 || symbol >= frequencies.Length)
                throw new ArgumentOutOfRangeException(nameof(symbol), "Symbol is out of range.");
            if (incrementBy == 0)
                return;
            if (incrementBy < 0 && frequencies[symbol] < (uint)-incrementBy)
                throw new ArgumentException($"Symbol {symbol} has a probability of {frequencies[symbol]}; decrementing it by {-incrementBy} would make it less than 0.");
            SetSymbolFrequency(symbol, checked(incrementBy > 0 ? (frequencies[symbol] + (uint)incrementBy) : (frequencies[symbol] - (uint)(-incrementBy))));
        }
    }
}
