using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class ExpressionParser : MessageProducer
    {
        private static readonly HashSet<TokenType> REL_OPS;
        private static readonly Dictionary<TokenType, ICodeNodeType> REL_OPS_MAP;
        private static readonly HashSet<TokenType> ADD_OPS;
        private static readonly Dictionary<TokenType, ICodeNodeType> ADD_OPS_OPS_MAP;
        private static readonly HashSet<TokenType> MUL_OPS;
        private static readonly Dictionary<TokenType, ICodeNodeType> MUL_OPS_OPS_MAP;
        //
        private Scanner scanner;
        private SymbolTableStack symtabstack;

        static ExpressionParser()
        {
            REL_OPS = new HashSet<TokenType>();
            REL_OPS.Add(TokenType.EQUALS);
            REL_OPS.Add(TokenType.NOT_EQUALS);
            REL_OPS.Add(TokenType.LESS_THAN);
            REL_OPS.Add(TokenType.LESS_EQUALS);
            REL_OPS.Add(TokenType.GREATER_THAN);
            REL_OPS.Add(TokenType.GREATER_EQUALS);

            REL_OPS_MAP = new Dictionary<TokenType, ICodeNodeType>()
            {
                {TokenType.EQUALS, ICodeNodeType.EQ},
                {TokenType.NOT_EQUALS, ICodeNodeType.NE},
                {TokenType.LESS_THAN, ICodeNodeType.LT},
                {TokenType.LESS_EQUALS, ICodeNodeType.LE},
                {TokenType.GREATER_THAN, ICodeNodeType.GT},
                {TokenType.GREATER_EQUALS, ICodeNodeType.GE},
            };

            ADD_OPS = new HashSet<TokenType>();
            ADD_OPS.Add(TokenType.PLUS);
            ADD_OPS.Add(TokenType.MINUS);
            ADD_OPS.Add(TokenType.OR);

            ADD_OPS_OPS_MAP = new Dictionary<TokenType, ICodeNodeType>()
            {
                {TokenType.PLUS, ICodeNodeType.ADD},
                {TokenType.MINUS, ICodeNodeType.SUBTRACT},
                {TokenType.OR, ICodeNodeType.OR}
            };

            MUL_OPS = new HashSet<TokenType>();
            MUL_OPS.Add(TokenType.STAR);
            MUL_OPS.Add(TokenType.SLASH);
            MUL_OPS.Add(TokenType.DIV);
            MUL_OPS.Add(TokenType.MOD);
            MUL_OPS.Add(TokenType.AND);

            MUL_OPS_OPS_MAP = new Dictionary<TokenType, ICodeNodeType>()
            {
                {TokenType.STAR, ICodeNodeType.MULTIPLY},
                {TokenType.SLASH, ICodeNodeType.FLOAT_DIVIDE},
                {TokenType.DIV, ICodeNodeType.INTEGER_DIVIDE},
                {TokenType.MOD, ICodeNodeType.MOD},
                {TokenType.AND, ICodeNodeType.AND}
            };
        }

        private ExpressionParser(Scanner s, SymbolTableStack stack)
        {
            scanner = s;
            symtabstack = stack;
        }

        public ICodeNode Parse(Token token)
        {
            return ParseExpression(token);
        }

        private ICodeNode ParseExpression(Token token)
        {
            // parse a simple expression, and make it the root of its tree
            // root node.
            ICodeNode root = ParseSimpleExpression(token);

            token = scanner.CurrentToken;
            if (REL_OPS.Contains(token.TokenType))
            {
                // create a new operator node and adopt the current tree
                // as its first child.
                ICodeNodeType type = REL_OPS_MAP[token.TokenType];
                ICodeNode op_node = ICodeFactory.CreateICodeNode(type);
                op_node.Add(root);

                token = scanner.GetNextToken(); // consume the operator

                // parse the second simple expression. The operator node adopts
                // the simple expression's tree as its second child.
                op_node.Add(ParseSimpleExpression(token));

                // the operator node becomes the new node
                root = op_node;
            }
            return root;
        }

        public static ExpressionParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            var expr_parser = new ExpressionParser(s, stack);

            foreach (var o in obl)
            {
                expr_parser.Add(o);
            }

            return expr_parser;
        }

        private ICodeNode ParseSimpleExpression(Token token)
        {
            TokenType sign = TokenType.INVALID; // type of leading sign, if any

            // look for a leading + or - sign
            if (token.TokenType == TokenType.PLUS || token.TokenType == TokenType.MINUS)
            {
                sign = token.TokenType;
                token = scanner.GetNextToken(); // consume + or -
            }

            // parse a term and make the root of its tree the root node/
            ICodeNode root = ParseTerm(token);

            // was there a leading - sign ?
            if (sign == TokenType.MINUS)
            {
                // create a NEGATE node and adopt the current tree
                // as its child. The NEGATE node becomes the new root node.
                ICodeNode negate = ICodeFactory.CreateICodeNode(ICodeNodeType.NEGATE);
                negate.Add(root);
                root = negate;
            }

            token = scanner.CurrentToken;

            // loop over additive operators
            while (ADD_OPS.Contains(token.TokenType))
            {
                // create a new operator node and adopt the current tree as its first child
                ICodeNodeType op_type = ADD_OPS_OPS_MAP[token.TokenType];
                ICodeNode op_node = ICodeFactory.CreateICodeNode(op_type);
                op_node.Add(root);

                token = scanner.GetNextToken(); // consume the operator

                // parse another term. The operator node adopts
                // the term's tree as its second child.
                op_node.Add(ParseTerm(token));

                // the operator node becomes the new root node.
                root = op_node;
                token = scanner.CurrentToken;
            }
            return root;
        }

        private ICodeNode ParseTerm(Token token)
        {
            // parse a factor and make its node the root node.
            ICodeNode root = ParseFactor(token);

            token = scanner.CurrentToken;
            while (MUL_OPS.Contains(token.TokenType))
            {
                // create a new operator node and adopt the current tree
                // as its first child.
                ICodeNodeType op = MUL_OPS_OPS_MAP[token.TokenType];
                ICodeNode op_node = ICodeFactory.CreateICodeNode(op);
                op_node.Add(root);

                token = scanner.GetNextToken(); // consume the operator

                // parse another factor. The operator node adopts
                // the term's tree as its second child.
                op_node.Add(ParseFactor(token));

                // the operator node becomes the new root node.
                root = op_node;
                token = scanner.CurrentToken;
            }
            return root;
        }

        private ICodeNode ParseFactor(Token token)
        {
            ICodeNode root = null;

            switch (token.TokenType)
            {
                case TokenType.IDENTIFIER:
                    {
                        // look the identifier in the symbol table stack. 
                        // flag the identifier as undefined if it's not found.
                        string name = token.Lexeme.ToLower();
                        SymbolTableEntry id = symtabstack.Find(name);
                        if (id == null)
                        {
                            ErrorHandler.Flag(token, ErrorCode.IDENTIFIER_UNDEFINED, this);
                            id = symtabstack.CreateInLocal(name);
                        }

                        root = ICodeFactory.CreateICodeNode(ICodeNodeType.VARIABLE);
                        root.SetAttribute(ICodeKey.ID, id);
                        id.AppendLine(token.LineNumber);
                        token = scanner.GetNextToken();
                    }
                    break;
                case TokenType.INTEGER:
                    // create an INTEGER_CONSTANT node as the root node.
                    root = ICodeFactory.CreateICodeNode(ICodeNodeType.INTEGER_CONSTANT);
                    root.SetAttribute(ICodeKey.VALUE, token.Value);
                    token = scanner.GetNextToken();
                    break;
                case TokenType.REAL:
                    // create a REAL_CONSTANT node as the root node.
                    root = ICodeFactory.CreateICodeNode(ICodeNodeType.REAL_CONSTANT);
                    root.SetAttribute(ICodeKey.VALUE, token.Value);
                    token = scanner.GetNextToken();
                    break;
                case TokenType.STRING:
                    // create a STRING_CONSTANT node as the root node.
                    root = ICodeFactory.CreateICodeNode(ICodeNodeType.STRING_CONSTANT);
                    root.SetAttribute(ICodeKey.VALUE, token.Value);
                    token = scanner.GetNextToken();
                    break;
                case TokenType.NOT:
                    token = scanner.GetNextToken(); // consume NOT

                    // create a NOT node as the root node
                    root = ICodeFactory.CreateICodeNode(ICodeNodeType.NOT);
                    
                    // parse a factor. The NOT node adopts the 
                    // factor as its child.
                    root.Add(ParseFactor(token));
                    break;
                case TokenType.LEFT_PAREN:
                    token = scanner.GetNextToken(); // consume the (

                    // parse an expression and make its node the root node.
                    root = ParseExpression(token);

                    // look for the matching ) token.
                    token = scanner.CurrentToken;
                    if (token.TokenType == TokenType.RIGHT_PAREN)
                    {
                        token = scanner.GetNextToken(); // consume the )
                    }
                    else
                    {
                        ErrorHandler.Flag(token, ErrorCode.MISSING_RIGHT_PAREN, this);
                    }
                    break;
                default:
                    ErrorHandler.Flag(token, ErrorCode.UNEXPECTED_TOKEN, this);
                    break;
            }
            return root;
        }
    }
}
