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
            var icode = ICodeFactory.CreateICode();
            var start = DateTime.Now;
            try
            {
                Token token = scanner.GetNextToken();
                ICodeNode root = null;

                // look for the BEGIN token to parse a compound statement.
                if (token.TokenType == TokenType.BEGIN)
                {
                    StatementParser stmnt_parser = StatementParser.CreateWithObservers(scanner, symtabstack, observers);
                    root = stmnt_parser.Parse(token);
                    token = scanner.CurrentToken;
                } else
                {
                    ErrorHandler.Flag(token, ErrorCode.UNEXPECTED_TOKEN, this);
                }

                // look for the final period.
                if (token.TokenType != TokenType.DOT)
                {
                    ErrorHandler.Flag(token, ErrorCode.MISSING_PERIOD, this);
                }

                token = scanner.CurrentToken;
                if (root != null)
                {
                    icode.Root = root;
                }

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
            return Tuple.Create(icode, symtabstack);
        }

        internal static Token Synchronize(HashSet<TokenType> types, Scanner scanner, MessageProducer mp)
        {
            Token tok = scanner.CurrentToken;

            // if the current token is not in the synchronization set
            // then it is unexpected and the parser must recover.
            if (!types.Contains(tok.TokenType))
            {
                // flag the unexpected token
                ErrorHandler.Flag(tok, ErrorCode.UNEXPECTED_TOKEN, mp);

                // recover by skipping tokens that are not in 
                // the synchronization set.
                do
                {
                    tok = scanner.GetNextToken();
                } while (!tok.IsEof && !types.Contains(tok.TokenType));
            }
            return tok;
        }
    }
}
