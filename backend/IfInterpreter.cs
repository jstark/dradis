using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class IfInterpreter : MessageProducer
    {
        private IfInterpreter() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            // get the IF node's children
            var children = node.GetChildren();
            ICodeNode expr_node = children[0];
            ICodeNode then_node = children[1];
            ICodeNode else_node = children.Count > 2 ? children[2] : null;

            ExpressionInterpreter expr_interpreter =
                ExpressionInterpreter.CreateWithObservers(observers);
            StatementInterpreter stmt_interpreter =
                StatementInterpreter.CreateWithObservers(observers);

            // evaluate the expression to determine which statement to execute.
            bool b = (bool)expr_interpreter.Execute(expr_node, ref exec_count);
            if (b)
            {
                stmt_interpreter.Execute(then_node, ref exec_count);
            } else
            {
                stmt_interpreter.Execute(else_node, ref exec_count);
            }

            ++exec_count;
            return null;
        }

        public static IfInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            IfInterpreter if_interpreter = new IfInterpreter();
            if_interpreter.observers.AddRange(observers);
            return if_interpreter;
        }
    }
}
