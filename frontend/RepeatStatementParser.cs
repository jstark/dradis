using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.frontend;
using dradis.intermediate;

namespace dradis.frontend
{
    public class RepeatStatementParser : NonTerminalParser
    {
        public ICodeNode Parse(Token token)
        {
            token = InternalScanner.GetNextToken(); // consume the REPEAT

            // create the LOOP and TEST nodes.
            ICodeNode loop_node = ICodeFactory.CreateICodeNode(ICodeNodeType.LOOP);
            ICodeNode test_node = ICodeFactory.CreateICodeNode(ICodeNodeType.TEST);

            // parse the statement list terminated by the UNTIL token.
            // the LOOP node is the parent of the statement subtrees.
            StatementParser stmnt_parser = 
                StatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            stmnt_parser.ParseList(token, loop_node, TokenType.UNTIL, ErrorCode.MISSING_UNTIL);
            token = InternalScanner.CurrentToken;

            // parse the expression.
            // the TEST node adopts the expression subtree as its only child.
            // the LOOP node adopts the TEST node.
            ExpressionParser expr_parser = 
                ExpressionParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            test_node.Add(expr_parser.Parse(token));
            loop_node.Add(test_node);

            return loop_node;
        }

        public static RepeatStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<RepeatStatementParser>(s, stack, obl);
        }
    }
}
