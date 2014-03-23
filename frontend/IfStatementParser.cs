using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class IfStatementParser : NonTerminalParser
    {
        private static HashSet<TokenType> THEN_SET;

        static IfStatementParser()
        {
            THEN_SET = new HashSet<TokenType>(StatementParser.STMT_START_SET);
            THEN_SET.Add(TokenType.THEN);
            THEN_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);
        }

        public override ICodeNode Parse(Token token)
        {
            Token tok = InternalScanner.GetNextToken(); // consume the IF token

            // create an IF node.
            ICodeNode if_node = ICodeFactory.CreateICodeNode(ICodeNodeType.IF);

            // parse the expression.
            // the IF node adopts the expression subtree as its first child.
            ExpressionParser expr_parser =
                ExpressionParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            if_node.Add(expr_parser.Parse(tok));

            // synchronize the THEN
            tok = Parser.Synchronize(THEN_SET, InternalScanner, this);
            if (tok.TokenType == TokenType.THEN)
            {
                tok = InternalScanner.GetNextToken(); // consume the THEN
            } else
            {
                ErrorHandler.Flag(tok, ErrorCode.MISSING_THEN, this);
            }

            // parse the THEN statement.
            // the IF node adopts the statement subtree as its second child. 
            StatementParser stmt_parser =
                StatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            if_node.Add(stmt_parser.Parse(tok));
            tok = InternalScanner.CurrentToken;

            // look for an ELSE
            if (tok.TokenType == TokenType.ELSE)
            {
                tok = InternalScanner.GetNextToken(); // consume the THEN token
                
                // parse the ELSE statement.
                // the IF node adopts the statement subtree as its third child.
                if_node.Add(stmt_parser.Parse(tok));
            }

            return if_node;
        }

        public static IfStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<IfStatementParser>(s, stack, obl);
        }
    }
}
