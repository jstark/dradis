using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class CaseStatementParser : NonTerminalParser
    {
        // synchronization set for starting a CASE option constant.
        private static HashSet<TokenType> CONSTANT_START_SET;

        // synchronization set for OF.
        private static HashSet<TokenType> OF_SET;

        // synchronization set for COMMA.
        private static HashSet<TokenType> COMMA_SET;

        static CaseStatementParser()
        {
            CONSTANT_START_SET = new HashSet<TokenType>();
            CONSTANT_START_SET.Add(TokenType.IDENTIFIER);
            CONSTANT_START_SET.Add(TokenType.INTEGER);
            CONSTANT_START_SET.Add(TokenType.PLUS);
            CONSTANT_START_SET.Add(TokenType.MINUS);
            CONSTANT_START_SET.Add(TokenType.STRING);

            OF_SET = new HashSet<TokenType>(CONSTANT_START_SET);
            OF_SET.Add(TokenType.OF);
            OF_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);

            COMMA_SET = new HashSet<TokenType>(CONSTANT_START_SET);
            COMMA_SET.Add(TokenType.COMMA);
            COMMA_SET.Add(TokenType.COLON);
            COMMA_SET.UnionWith(StatementParser.STMT_START_SET);
            COMMA_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);
        }

        public ICodeNode Parse(Token token)
        {
            Token tok = InternalScanner.GetNextToken(); // consume the CASE token

            // create a SELECT node
            ICodeNode select_node = ICodeFactory.CreateICodeNode(ICodeNodeType.SELECT);

            // parse the CASE expression.
            // the SELECT node adopts the expression subtree as its first child.
            ExpressionParser expr_parser =
                ExpressionParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            select_node.Add(expr_parser.Parse(tok));

            tok = InternalScanner.CurrentToken;

            // synchronize the OF token.
            tok = Parser.Synchronize(OF_SET, InternalScanner, this);
            if (tok.TokenType == TokenType.OF)
            {
                tok = InternalScanner.GetNextToken(); // consume the OF 
            } else
            {
                ErrorHandler.Flag(tok, ErrorCode.MISSING_OF, this);
            }

            // set of CASE branch constants.
            HashSet<object> constant_set = new HashSet<object>();

            // loop to parse each CASE branch until the END token
            // or the end of the source file.
            while (!tok.IsEof && tok.TokenType != TokenType.END)
            {
                // the SELECT node adopts the CASE branch subtree
                select_node.Add(ParseBranch(tok, constant_set));

                tok = InternalScanner.CurrentToken;
                if (tok.TokenType == TokenType.SEMICOLON)
                {
                    tok = InternalScanner.GetNextToken(); // consume the ;
                } else if (CONSTANT_START_SET.Contains(tok.TokenType))
                {
                    ErrorHandler.Flag(tok, ErrorCode.MISSING_SEMICOLON, this);
                }
            }

            // look for the END token.
            if (tok.TokenType == TokenType.END)
            {
                tok = InternalScanner.GetNextToken(); // consume the END
            } else
            {
                ErrorHandler.Flag(tok, ErrorCode.MISSING_END, this);
            }

            return select_node;
        }

        private ICodeNode ParseBranch(Token token, HashSet<object> const_set)
        {
            // create a SELECT_BRANCH node and a SELECT_CONSTANTS node
            // the SELECT _BRANCH adopts the SELECT_CONSTANTS node as its 
            // first child.
            ICodeNode branch_node = ICodeFactory.CreateICodeNode(ICodeNodeType.SELECT_BRANCH);
            ICodeNode constant_nodes = ICodeFactory.CreateICodeNode(ICodeNodeType.SELECT_CONSTANTS);
            branch_node.Add(constant_nodes);

            // parse the list of CASE branch constants.
            // the SELECT_CONSTANTS node adopts each constant.
            ParseConstantList(token, constant_nodes, const_set);

            // look for the : token
            token = InternalScanner.CurrentToken;
            if (token.TokenType == TokenType.COLON)
            {
                token = InternalScanner.GetNextToken();
            } else
            {
                ErrorHandler.Flag(token, ErrorCode.MISSING_COLON, this);
            }

            // parse the CASE branch statement. The SELECT_BRANCH node adopts
            // the statement subtree as its second child.
            StatementParser stmnt_parser =
                StatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            branch_node.Add(stmnt_parser.Parse(token));

            return branch_node;
        }

        private void ParseConstantList(Token token, ICodeNode constants_node, HashSet<object> constant_set)
        {
            // loop to parse each constant.
            while (CONSTANT_START_SET.Contains(token.TokenType))
            {
                // the constants list node adopts the constant node.
                constants_node.Add(ParseConstant(token, constant_set));

                // synchronize at the comma between constants.
                token = Parser.Synchronize(COMMA_SET, InternalScanner, this);

                // look for the COMMA
                if (token.TokenType == TokenType.COMMA)
                {
                    token = InternalScanner.GetNextToken(); // consume the ,
                } else if (CONSTANT_START_SET.Contains(token.TokenType))
                {
                    ErrorHandler.Flag(token, ErrorCode.MISSING_COMMA, this);
                }
            }
        }

        private ICodeNode ParseConstant(Token token, HashSet<object> constant_set)
        {
            TokenType sign = TokenType.INVALID;
            ICodeNode constant_node = null;

            // synchronize at the start of a constant.
            token = Parser.Synchronize(CONSTANT_START_SET, InternalScanner, this);
            if (token.TokenType == TokenType.PLUS || token.TokenType == TokenType.MINUS)
            {
                sign = token.TokenType;
                token = InternalScanner.GetNextToken(); // consume sign
            }

            // parse the constant.
            switch (token.TokenType)
            {
                case TokenType.IDENTIFIER:
                    constant_node = ParseIdentifierConstant(token, sign);
                    break;
                case TokenType.INTEGER:
                    constant_node = ParseIntegerConstant(token.Lexeme, sign);
                    break;
                case TokenType.STRING:
                    constant_node = ParseCharacterConstant(token, (string)token.Value, sign);
                    break;
                default:
                    ErrorHandler.Flag(token, ErrorCode.INVALID_CONSTANT, this);
                    break;
            }

            // check for reused constants
            if (constant_node != null)
            {
                object value = constant_node.GetAttribute(ICodeKey.VALUE);

                if (constant_set.Contains(value))
                {
                    ErrorHandler.Flag(token, ErrorCode.CASE_CONSTANT_REUSED, this);
                } else
                {
                    constant_set.Add(value);
                }
            }

            InternalScanner.GetNextToken(); // consume the constant
            return constant_node;
        }

        private ICodeNode ParseIdentifierConstant(Token token, TokenType sign)
        {
            // Placeholder! Don't allow for now.
            ErrorHandler.Flag(token, ErrorCode.INVALID_CONSTANT, this);
            return null;
        }

        private ICodeNode ParseIntegerConstant(string value, TokenType sign)
        {
            ICodeNode constant_node = ICodeFactory.CreateICodeNode(ICodeNodeType.INTEGER_CONSTANT);
            int val;
            
            if (!Int32.TryParse(value, out val))
            {
                ErrorHandler.AbortTranslation(ErrorCode.INVALID_NUMBER, this);
            }

            if (sign == TokenType.MINUS)
            {
                val = -val;
            }

            constant_node.SetAttribute(ICodeKey.VALUE, val);
            return constant_node;
        }

        private ICodeNode ParseCharacterConstant(Token token, string value, TokenType sign)
        {
            ICodeNode constant_node = null;

            if (sign != TokenType.INVALID)
            {
                ErrorHandler.Flag(token, ErrorCode.INVALID_CONSTANT, this);
            } else
            {
                if (value.Length == 1)
                {
                    constant_node = ICodeFactory.CreateICodeNode(ICodeNodeType.STRING_CONSTANT);
                    constant_node.SetAttribute(ICodeKey.VALUE, value);
                } else
                {
                    ErrorHandler.Flag(token, ErrorCode.INVALID_CONSTANT, this);
                }
            }

            return constant_node;
        }

        public static CaseStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<CaseStatementParser>(s, stack, obl);
        }
    }
}
