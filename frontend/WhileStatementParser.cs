using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    public class WhileStatementParser : NonTerminalParser
    {
        internal static HashSet<TokenType> DO_SET;

        static WhileStatementParser()
        {
            DO_SET = new HashSet<TokenType>();
            DO_SET.Add(TokenType.DO);
            DO_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);
        }

        public ICodeNode Parse(Token token)
        {
            token = InternalScanner.GetNextToken(); // consume the WHILE

            // create LOOP, TEST and NOT nodes
            ICodeNode loop_node = ICodeFactory.CreateICodeNode(ICodeNodeType.LOOP);
            ICodeNode break_node = ICodeFactory.CreateICodeNode(ICodeNodeType.TEST);
            ICodeNode not_node = ICodeFactory.CreateICodeNode(ICodeNodeType.NOT);

            // the LOOP node adopts the TEST node as its first child.
            // the TEST node adopts the NOT node as its only child.
            loop_node.Add(break_node);
            break_node.Add(not_node);

            ExpressionParser expr_parser = 
                ExpressionParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            not_node.Add(expr_parser.Parse(token));

            // synchronize at the DO.
            token = Parser.Synchronize(DO_SET, InternalScanner, this);
            if (token.TokenType == TokenType.DO)
            {
                token = InternalScanner.GetNextToken();
            } else
            {
                ErrorHandler.Flag(token, ErrorCode.MISSING_DO, this);
            }

            // parse the statement
            // the LOOP node adopts the statement subtree as its second child.
            StatementParser stmnt_parser = 
                StatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            loop_node.Add(stmnt_parser.Parse(token));

            return loop_node;
        }

        public static WhileStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<WhileStatementParser>(s, stack, obl);
        }
    }
}
