using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs5.Model.Encode
{
    class ArithmeticCodingWriter
    {
        private ulong high, low;
        private int underflow;
        private ArithmeticSymbolContext context;
        private Stream stream;
        private int curbyte;
        private bool anyWrites = false;

        /// <summary>
        /// Инициализирует экземпляр <see cref = "ArithmeticCodingWriter" /></summary>
        /// <param name="stream">
        /// Поток, в который будут записаны закодированные данные.</param>
        /// <param name="frequencies">
        /// Частота появления каждого символа. При чтении данных с использованием <see
        /// cref="ArithmeticCodingReader"/>, набор частот должен быть точно таким же.</param>
        /// <remarks>
        /// Закодированные данные не будут заполнены до тех пор, пока запись не будет завершена с использованием
        /// <see cref="Finalize"/>.</remarks>
        public ArithmeticCodingWriter(Stream stream, uint[] frequencies)
            : this(stream, new ArithmeticSymbolArrayContext(frequencies ?? throw new ArgumentNullException(nameof(frequencies))))
        {
        }

        /// <summary>
        /// Инициализирует элемент <see cref = "ArithmeticCodingWriter" />.</summary>
        /// <param name="stream">
        /// Поток, в который будут записаны закодированные данные.</param>
        /// <param name="context">
        /// Контекст, используемый для определения относительных частот кодированных символов. Вызывающий может внести изменения в
        /// экземпляр контекста, который он передал; такие изменения вступят в силу немедленно. Смотрите также <see
        /// cref="SetContext"/>.</param>
        /// <remarks>
        /// Закодированные данные не будут заполнены до тех пор, пока запись не будет завершена с использованием <see cref="Finalize"/>.</remarks>
        public ArithmeticCodingWriter(Stream stream, ArithmeticSymbolContext context)
        {
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            high = 0xFFFF_FFFF;
            low = 0;
            curbyte = 1;
            underflow = 0;
        }

        /// <summary>
        /// Изменяет контекст символа.</summary>
        /// <remarks>
        /// Экземпляр контекста может быть изменен после его применения <see cref="SetContext"/> (или в начальном
        /// вызов конструктора) с немедленным эффектом. Нет необходимости вызывать этот метод после изменения уже
        /// прикладной контекст. Контексты символов могут быть изменены произвольно между вызовами <see cref="WriteSymbol"/>, as
        /// поскольку такие же изменения происходят при декодировании между вызовами <see
        /// cref="ArithmeticCodingReader.ReadSymbol"/>.</remarks>
        public void SetContext(ArithmeticSymbolContext context)
        {
            if (this.context == null)
                throw new InvalidOperationException("Запись завершена; дальнейшие изменения контекста не допускаются.");
            this.context = context;
        }

        /// <summary>
        /// Кодирует один символ.</summary>
        /// <param name="symbol">
        /// Символ для записи. Должно быть неотрицательное целое число с ненулевой частотой в текущем контексте.</param>
        public void WriteSymbol(int symbol)
        {
            if (context == null)
                throw new InvalidOperationException("Запись завершена; не допускается дальнейшая запись символов.");
            ulong total = context.GetTotal();
            ulong symbolFreq = context.GetSymbolFrequency(symbol);
            ulong symbolPos = context.GetSymbolPosition(symbol);
            if (symbolFreq == 0)
                throw new ArgumentException("Попытка кодирования символа с нулевой частотой");
            if (symbolPos + symbolFreq > total)
                throw new InvalidOperationException("Попытка кодирования символа, для которого контекст символа возвращает несогласованные результаты (pos + prob> total)");

            ulong high = this.high;
            ulong low = this.low;
            int curbyte = this.curbyte;
            int underflow = this.underflow;

            anyWrites = true;

            // Установите высокие и низкие значения в новые значения
            ulong newlow = checked((high - low + 1) * symbolPos / total + low);
            high = checked((high - low + 1) * (symbolPos + symbolFreq) / total + low - 1);
            low = newlow;
            if (high < low)
                throw new OverflowException();

            // В то время как большинство значащих бит совпадают, вытесняют их и выводят их
            while ((high & 0x8000_0000) == (low & 0x8000_0000))
            {
                // inlined: outputBit((high & 0x8000_0000) != 0);
                curbyte <<= 1;
                if ((high & 0x8000_0000) != 0)
                    curbyte++;
                if (curbyte >= 0x100)
                {
                    stream.WriteByte((byte)curbyte);
                    curbyte = 1;
                }

                while (underflow > 0)
                {
                    // inlined: outputBit((high & 0x8000_0000) == 0);
                    curbyte <<= 1;
                    if ((high & 0x8000_0000) == 0)
                        curbyte++;
                    if (curbyte >= 0x100)
                    {
                        stream.WriteByte((byte)curbyte);
                        curbyte = 1;
                    }

                    underflow--;
                }
                high = ((high << 1) & 0xFFFF_FFFF) | 1;
                low = (low << 1) & 0xFFFF_FFFF;
                if (high < low)
                    throw new OverflowException();
            }

            // Если подтекание неизбежно, сдвиньте его
            while (((low & 0x4000_0000) != 0) && ((high & 0x4000_0000) == 0))
            {
                underflow++;
                high = ((high & 0x7FFF_FFFF) << 1) | 0x8000_0001;
                low = (low << 1) & 0x7FFF_FFFF;
            }
            if (high < low)
                throw new OverflowException();

            this.high = high;
            this.low = low;
            this.curbyte = curbyte;
            this.underflow = underflow;
        }

        /// <summary>
        /// Завершает поток, очищая все оставшиеся буферизованные данные и записывая требуемое дополнение синхронизации
        /// читателем. Этот вызов является обязательным поток не будет читаться полностью, если этот метод не будет вызван.
        /// Этот метод не записывает достаточно информации в поток, чтобы читатель мог обнаружить, что нет
        /// символа; see Remarks on <see cref="ArithmeticCodingWriter"/> для получения дополнительной информации.</summary>
        /// <param name="closeStream">
        /// Указывает, должен ли выходной поток быть закрыт. Необязательный; по умолчанию <c>false</c>.</param>
        public void Finalize(bool closeStream = false)
        {
            if (anyWrites)
            {
                outputBit((low & 0x4000_0000) != 0);
                underflow++;
                while (underflow > 0)
                {
                    outputBit((low & 0x4000_0000) == 0);
                    underflow--;
                }
                if (curbyte != 1)
                {
                    while (curbyte < 0x100)
                        curbyte <<= 1;
                    stream.WriteByte((byte)curbyte);
                }
                // Читателю нужно заглядывать вперед на несколько байт, так что проведите финал, чтобы синхронизировать их. Читатель и писатель
                // используйте немного другое количество байтов, чтобы эта последовательность помогла читателю закончить в нужном месте.
                stream.WriteByte(0x51);
                stream.WriteByte(0x51);
                stream.WriteByte(0x51);
                stream.WriteByte(0x50);
            }
            if (closeStream)
                stream.Close();
            context = null; // предотвращать дальнейшие записи символов
        }

        private void outputBit(bool p)
        {
            curbyte <<= 1;
            if (p)
                curbyte++;
            if (curbyte >= 0x100)
            {
                stream.WriteByte((byte)curbyte);
                curbyte = 1;
            }
        }
    }
}
