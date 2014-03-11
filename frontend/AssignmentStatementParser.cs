using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;
using System.Diagnostics.Contracts;

namespace dradis.frontend
{
    public class AssignmentStatementParser : MessageProducer
    {
        private Scanner scanner;
        private SymbolTableStack symtabstack;

        private AssignmentStatementParser(Scanner s, SymbolTableStack stack)
        {
            scanner = s;
            symtabstack = stack;
        }

        public ICodeNode Parse(Token token)
        {
            // create the ASSIGN node
            ICodeNode assign_node = ICodeFactory.CreateICodeNode(ICodeNodeType.ASSIGN);

            // lookup up the target identifier in the symbol table stack.
            // enter the identifier into the table if it's not found.
            string target_name = token.Lexeme.ToLower();
            SymbolTableEntry target_id = symtabstack.FindInLocal(target_name);
            if (target_id == null)
            {
                target_id = symtabstack.CreateInLocal(target_name);
            }
            target_id.AppendLine(token.LineNumber);

            Contract.Requires(token.TokenType == TokenType.IDENTIFIER);
            token = scanner.GetNextToken(); // consume the identifier token

            // create the variable node and set its name attribute.
            ICodeNode variable_node = ICodeFactory.CreateICodeNode(ICodeNodeType.VARIABLE);
            variable_node.SetAttribute(ICodeKey.ID, target_id);

            // the assign node adopts the variable node as its first child.
            assign_node.Add(variable_node);

            // look for the := token
            if (token.TokenType == TokenType.COLON_EQUALS)
            {
                token = scanner.GetNextToken(); // consume the :=
            } else
            {
                ErrorHandler.Flag(token, ErrorCode.MISSING_COLON_EQUALS, this);
            }

            // parse the expression. The ASSIGN node adopts the expression's node
            // as its second child.
            ExpressionParser exp_parser = ExpressionParser.CreateWithObservers(scanner, symtabstack, observers);
            assign_node.Add(exp_parser.Parse(token));

            return assign_node;
        }

        public static AssignmentStatementParser CreateWithObservers(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl)
        {
            var assign_parser = new AssignmentStatementParser(s, stack);

            foreach (var o in obl)
                assign_parser.Add(o);

            return assign_parser;
        }
    }
}
