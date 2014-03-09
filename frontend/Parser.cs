using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.frontend
{
    public class Parser : MessageProducer
    {
        private Scanner scanner;

        public Parser(Scanner s)
        {
            scanner = s;
        }

        public int ErrorCount { get; private set; }

        public Tuple<ICode, SymbolTableStack> Parse()
        {
            var symtabstack = SymbolTableFactory.CreateStack();
            var start = DateTime.Now;
            Token token;
            try
            {
                do
                {
                    token = scanner.GetNextToken();
                    string lname = token.Lexeme.ToLower();
                    if (token.TokenType != TokenType.ERROR)
                    {
                        var tkmsgargs = Tuple.Create(token.LineNumber, token.Position, token.TokenType, token.Lexeme, token.Value);
                        Message tkmsg = new Message(MessageType.Token, tkmsgargs);
                        Send(tkmsg);
                        if (token.TokenType == TokenType.IDENTIFIER)
                        {
                            var entry = symtabstack.FindInLocal(lname);
                            if (entry == null)
                            {
                                entry = symtabstack.CreateInLocal(lname);
                            }
                            entry.AppendLine(token.LineNumber);
                        }
                    }
                    else 
                    {
                        ErrorHandler.Flag(token, (ErrorCode)token.Value, this);
                    }
                } while (token.IsEof == false);

                var end = DateTime.Now;
                double elapsedTime = (end - start).Ticks / (1.0 * TimeSpan.TicksPerSecond);
                var args = Tuple.Create(token.LineNumber, ErrorHandler.GetErrorCount(), elapsedTime);
                Message msg = new Message(MessageType.ParserSummary, args);
                Send(msg);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ErrorHandler.AbortTranslation(ErrorCode.IO_ERROR, this);
            }
            return Tuple.Create((ICode)null, symtabstack);
        }
    }
}
