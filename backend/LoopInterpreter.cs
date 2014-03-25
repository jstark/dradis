using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class LoopInterpreter : MessageProducer
    {
        private LoopInterpreter() : base() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            bool exit_loop = false;
            ICodeNode expr_node = null;
            List<ICodeNode> loop_children = node.GetChildren();

            ExpressionInterpreter expr_interpreter =
                ExpressionInterpreter.CreateWithObservers(observers);
            StatementInterpreter stmt_interpreter =
                StatementInterpreter.CreateWithObservers(observers);

            // loop until the TEST expression value is true
            while (!exit_loop)
            {
                ++exec_count;

                // execute the children of the LOOP node.
                foreach (var child in loop_children)
                {
                    if (child.Type == ICodeNodeType.TEST)
                    {
                        if (expr_node == null)
                        {
                            expr_node = child.GetChildren().ElementAt(0);
                        }
                        exit_loop = (bool)expr_interpreter.Execute(expr_node, ref exec_count);
                    } else
                    {
                        stmt_interpreter.Execute(child, ref exec_count);
                    }

                    if (exit_loop)
                    {
                        break;
                    }
                }
            }

            return null;
        }

        public static LoopInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            LoopInterpreter loop_interpreter = new LoopInterpreter();
            loop_interpreter.observers.AddRange(observers);
            return loop_interpreter;
        }
    }
}
