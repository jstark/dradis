using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class StatementParser : MessageProducer
    {
        private Scanner scanner;
        private SymbolTableStack symtabstack;

        private StatementParser(Scanner s, SymbolTableStack stack)
        {
            scanner = s;
            symtabstack = stack;
        }

        public ICodeNode Parse(Token token)
        {
            ICodeNode node;
            switch (token.TokenType)
            {
                case TokenType.BEGIN:
                    CompoundStatementParser cmpnd_parser = CompoundStatementParser.CreateWithObservers(scanner, symtabstack, observers);
                    node = cmpnd_parser.Parse(token);
                    break;
                case TokenType.IDENTIFIER:
                    AssignmentStatementParser assign_parser = AssignmentStatementParser.CreateWithObservers(scanner, symtabstack, observers);
                    node = assign_parser.Parse(token);
                    break;
                default:
                    node = ICodeFactory.CreateICodeNode(ICodeNodeType.NO_OP);
                    break;
            }

            // set the current line number as an attribute.
            SetLineNumber(node, token.LineNumber);
            return node;
        }

        public void ParseList(Token token, ICodeNode parent, TokenType terminator, ErrorCode error)
        {
            while(!token.IsEof && token.TokenType != terminator)
            {
                // parse a statement. The parent node adopts the statement node.
                ICodeNode statement_node = Parse(token);
                parent.Add(statement_node);
                token = scanner.CurrentToken;

                // look for the semicolon between the statements.
                if (token.TokenType == TokenType.SEMICOLON)
                {
                    token = scanner.GetNextToken();
                }
                else if (token.TokenType == TokenType.IDENTIFIER)
                {
                    ErrorHandler.Flag(token, ErrorCode.MISSING_SEMICOLON, this);
                }
                else if (token.TokenType != terminator)
                {
                    ErrorHandler.Flag(token, ErrorCode.UNEXPECTED_TOKEN, this);
                    token = scanner.GetNextToken();
                }
            }

            // look for the terminator token
            if (token.TokenType == terminator)
            {
                token = scanner.GetNextToken();
            } else
            {
                ErrorHandler.Flag(token, error, this);
            }
        }

        public static StatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            var stmnt_parser = new StatementParser(s, stack);

            foreach (var o in obl)
                stmnt_parser.Add(o);

            return stmnt_parser;
        }

        private void SetLineNumber(ICodeNode node, int line)
        {
            node.SetAttribute(ICodeKey.LINE, line);
        }
    }
}
