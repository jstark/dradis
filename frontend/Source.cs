using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using dradis.message;

namespace dradis.frontend
{
    public class Source : MessageProducer
    {
        public const char END_OF_LINE = '\n';
        public const char END_OF_FILE = '\0';

        private string line;
        private TextReader reader;

        public Source(StreamReader r)
        {
            reader = r;
            LineNumber = 0;
            Position = -2;
        }

        public int LineNumber { get; private set; }
        public int Position { get; private set; }

        public char GetCurrentChar()
        {
            if (Position == -2)
            {
                ReadLine();
                return GetNextChar();
            }
            else if (line == null)
            {
                return END_OF_FILE;
            }
            else if (Position == -1 || Position == line.Length)
            {
                return END_OF_LINE;
            }
            else if (Position > line.Length)
            {
                ReadLine();
                return GetNextChar();
            }
            else
            {
                return line[Position];
            }
        }

        public char GetNextChar()
        {
            ++Position;
            return GetCurrentChar();
        }

        public char PeekNextChar()
        {
            GetCurrentChar();
            if (line == null)
            {
                return END_OF_FILE;
            }
            int next = Position + 1;
            return next < line.Length ? line[next] : END_OF_LINE;
        }

        private void ReadLine()
        {
            line = reader.ReadLine();
            Position = -1;

            if (line != null)
            {
                ++LineNumber;
                var args = Tuple.Create(LineNumber, line);
                Message msg = new Message(MessageType.SourceLine, args);
                Send(msg);
            }
        }
    }
}
