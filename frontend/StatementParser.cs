using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class StatementParser : NonTerminalParser
    {
        // Synchronization set for starting a statement.
        internal static HashSet<TokenType> STMT_START_SET;

        // Synchronization set for following a statement.
        internal static HashSet<TokenType> STMT_FOLLOW_SET;

        static StatementParser()
        {
            // 
            STMT_START_SET = new HashSet<TokenType>();
            STMT_START_SET.Add(TokenType.BEGIN);
            STMT_START_SET.Add(TokenType.CASE);
            STMT_START_SET.Add(TokenType.FOR);
            STMT_START_SET.Add(TokenType.IF);
            STMT_START_SET.Add(TokenType.REPEAT);
            STMT_START_SET.Add(TokenType.WHILE);
            STMT_START_SET.Add(TokenType.IDENTIFIER);
            STMT_START_SET.Add(TokenType.SEMICOLON);

            //
            STMT_FOLLOW_SET = new HashSet<TokenType>();
            STMT_FOLLOW_SET.Add(TokenType.SEMICOLON);
            STMT_FOLLOW_SET.Add(TokenType.END);
            STMT_FOLLOW_SET.Add(TokenType.ELSE);
            STMT_FOLLOW_SET.Add(TokenType.UNTIL);
            STMT_FOLLOW_SET.Add(TokenType.DOT);
        }

        public override ICodeNode Parse(Token token)
        {
            ICodeNode node;
            switch (token.TokenType)
            {
                case TokenType.BEGIN:
                    CompoundStatementParser cmpnd_parser = 
                        CompoundStatementParser.CreateWithObservers(InternalScanner, SymTabStack, observers);
                    node = cmpnd_parser.Parse(token);
                    break;
                case TokenType.IDENTIFIER:
                    AssignmentStatementParser assign_parser =
                        AssignmentStatementParser.CreateWithObservers(InternalScanner, SymTabStack, observers);
                    node = assign_parser.Parse(token);
                    break;
                case TokenType.REPEAT:
                    RepeatStatementParser repeat_parser =
                        RepeatStatementParser.CreateWithObservers(InternalScanner, SymTabStack, observers);
                    node = repeat_parser.Parse(token);
                    break;
                case TokenType.WHILE:
                    WhileStatementParser while_parser =
                        WhileStatementParser.CreateWithObservers(InternalScanner, SymTabStack, observers);
                    node = while_parser.Parse(token);
                    break;
                case TokenType.FOR:
                    ForStatementParser for_parser =
                        ForStatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
                    node = for_parser.Parse(token);
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
            var terminator_set = new HashSet<TokenType>(STMT_START_SET);
            terminator_set.Add(terminator);

            while(!token.IsEof && token.TokenType != terminator)
            {
                // parse a statement. The parent node adopts the statement node.
                ICodeNode statement_node = Parse(token);
                parent.Add(statement_node);
                token = InternalScanner.CurrentToken;

                // look for the semicolon between the statements.
                if (token.TokenType == TokenType.SEMICOLON)
                {
                    token = InternalScanner.GetNextToken();
                }
                else if (STMT_START_SET.Contains(token.TokenType))
                {
                    ErrorHandler.Flag(token, ErrorCode.MISSING_SEMICOLON, this);
                }

                // Synchronize at the start of the next statement
                // or at the terminator.
                token = Parser.Synchronize(terminator_set, InternalScanner, this);
            }

            // look for the terminator token
            if (token.TokenType == terminator)
            {
                token = InternalScanner.GetNextToken();
            } else
            {
                ErrorHandler.Flag(token, error, this);
            }
        }

        public static StatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<StatementParser>(s, stack, obl);
        }

        private void SetLineNumber(ICodeNode node, int line)
        {
            node.SetAttribute(ICodeKey.LINE, line);
        }
    }
}
