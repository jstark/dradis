using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.frontend
{
    public class ForStatementParser : NonTerminalParser
    {
        // synchronization set for TO or DOWNTO
        private static HashSet<TokenType> TO_DOWNTO_SET;

        // synchronization set for DO
        private static HashSet<TokenType> DO_SET;

        static ForStatementParser()
        {
            TO_DOWNTO_SET = new HashSet<TokenType>(ExpressionParser.EXPR_START_SET);
            TO_DOWNTO_SET.Add(TokenType.TO);
            TO_DOWNTO_SET.Add(TokenType.DOWNTO);
            TO_DOWNTO_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);

            DO_SET = new HashSet<TokenType>(StatementParser.STMT_START_SET);
            DO_SET.Add(TokenType.DO);
            DO_SET.UnionWith(StatementParser.STMT_FOLLOW_SET);
        }

        public ICodeNode Parse(Token token)
        {
            Token tok = InternalScanner.GetNextToken(); // consume the FOR
            Token target = tok;

            // create the COMPOUND, LOOP and TEST nodes
            ICodeNode compound = ICodeFactory.CreateICodeNode(ICodeNodeType.COMPOUND);
            ICodeNode loop = ICodeFactory.CreateICodeNode(ICodeNodeType.LOOP);
            ICodeNode test = ICodeFactory.CreateICodeNode(ICodeNodeType.TEST);

            // parse the embedded initial assignment.
            AssignmentStatementParser assign =
                AssignmentStatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            ICodeNode init_assign_node = assign.Parse(tok);

            // set the current line number
            SetLineNumber(init_assign_node, target.LineNumber);

            // the COMPOUND node adopts the initial ASSIGN and the LOOP nodes
            // as its first and second children.
            compound.Add(init_assign_node);
            compound.Add(loop);

            // synchronize at the TO or DOWNTO
            tok = Parser.Synchronize(TO_DOWNTO_SET, InternalScanner, this);
            TokenType direction = tok.TokenType;

            // look for the TO or DOWNTO
            if (tok.TokenType == TokenType.TO || tok.TokenType == TokenType.DOWNTO)
            {
                tok = InternalScanner.GetNextToken(); // consume the TO or DOWNTO
            } else
            {
                direction = TokenType.TO;
                ErrorHandler.Flag(tok, ErrorCode.MISSING_TO_DOWN_TO, this);
            }

            // create a relational operator node: GT for TO, or LT for DOWNTO
            ICodeNode relop_node =
                ICodeFactory.CreateICodeNode(direction == TokenType.TO ? ICodeNodeType.GT : ICodeNodeType.LT);

            // copy the control VARIABLE node. The relational operator node
            // adopts the copied VARIABLE node, as its first child.
            ICodeNode control_var_node = init_assign_node.GetChildren().ElementAt(0);
            relop_node.Add(control_var_node.Copy());

            // parse the termination expression. The relational operator node
            // adopts the expression as its second child.
            ExpressionParser expr_parser =
                ExpressionParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            relop_node.Add(expr_parser.Parse(tok));

            // the TEST node adopts the relational operator node as its only child.
            // the LOOP node adopts the TEST node as its first child.
            test.Add(relop_node);
            loop.Add(test);

            // synchronize at the DO.
            tok = Parser.Synchronize(DO_SET, InternalScanner, this);
            if (tok.TokenType == TokenType.DO)
            {
                tok = InternalScanner.GetNextToken();
            } else
            {
                ErrorHandler.Flag(tok, ErrorCode.MISSING_DO, this);
            }

            // parse the nested statement. The LOOP node adopts the statement
            // node as its second child.
            StatementParser stmnt_parser =
                StatementParser.CreateWithObservers(InternalScanner, SymTabStack, Observers);
            loop.Add(stmnt_parser.Parse(tok));

            // create an assignment with a copy of the control variable
            // to advance the value of the variable.
            ICodeNode next_assign = ICodeFactory.CreateICodeNode(ICodeNodeType.ASSIGN);
            next_assign.Add(control_var_node.Copy());

            // create an arithmetic operator node
            // ADD for TO, SUBTRACT for DOWNTO
            ICodeNode arith_node = 
                ICodeFactory.CreateICodeNode(direction == TokenType.TO ? ICodeNodeType.ADD : ICodeNodeType.SUBTRACT);

            // the operator node adopts adopts a copy of the loop variable as its
            // first child and the value 1 as its second child.
            arith_node.Add(control_var_node.Copy());
            ICodeNode one_node = ICodeFactory.CreateICodeNode(ICodeNodeType.INTEGER_CONSTANT);
            one_node.SetAttribute(ICodeKey.VALUE, 1);
            arith_node.Add(one_node);

            // the next ASSIGN node adopts the arithmetic operator node as its
            // second child. The loop node adopts to the next ASSIGN node as its
            // third child.
            next_assign.Add(arith_node);
            loop.Add(next_assign);

            // set the current line number attribute.
            SetLineNumber(next_assign, target.LineNumber);

            return compound;
        }

        public static ForStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            return NonTerminalParser.CreateWithObserver<ForStatementParser>(s, stack, obl);
        }

        private void SetLineNumber(ICodeNode node, int line)
        {
            node.SetAttribute(ICodeKey.LINE, line);
        }
    }
}
