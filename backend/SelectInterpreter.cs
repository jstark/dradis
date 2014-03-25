using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class SelectInterpreter : MessageProducer
    {
        private SelectInterpreter() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            // get the SELECT node's children
            var children = node.GetChildren();
            ICodeNode expr_node = children[0];

            // evaluate SELECT expression
            ExpressionInterpreter expr_interpreter =
                ExpressionInterpreter.CreateWithObservers(observers);
            object value = expr_interpreter.Execute(expr_node, ref exec_count);

            // attempt to select a SELECT_BRANCH.
            ICodeNode selected_branch = SearchBranches(value, children.Skip(1));

            // if there was a selection, execute the SELECT_BRANCH's statement
            if (selected_branch != null)
            {
                ICodeNode stmnt_node = selected_branch.GetChildren().ElementAt(1);
                StatementInterpreter stmnt_interpreter =
                    StatementInterpreter.CreateWithObservers(observers);
                stmnt_interpreter.Execute(stmnt_node, ref exec_count);
            }

            ++exec_count;
            return null;
        }

        private ICodeNode SearchBranches(object value, IEnumerable<ICodeNode> children)
        {
            // loop over the SELECT_BRANCHes to find a match
            foreach(var child in children)
            {
                if (SearchConstants(value, child))
                {
                    return child;
                }
            }
            return null;
        }

        private bool SearchConstants(object value, ICodeNode child)
        {
            // are the values integer or string? 
            bool integer_mode = value is int;

            // get the list of SELECT_CONSTANTS values
            ICodeNode constants_node = child.GetChildren().ElementAt(0);
            var all_constants = constants_node.GetChildren();

            // search the list of constants
            if (integer_mode)
            {
                int v = (int)value;
                foreach (var constant in all_constants)
                {
                    int con = (int)constant.GetAttribute(ICodeKey.VALUE);
                    if (v == con)
                    {
                        return true;
                    }
                }
            } else
            {
                string v = (string)value;
                foreach (var constant in all_constants)
                {
                    string con = (string)constant.GetAttribute(ICodeKey.VALUE);
                    if (v == con)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static SelectInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            SelectInterpreter sel_interpreter = new SelectInterpreter();
            sel_interpreter.observers.AddRange(observers);
            return sel_interpreter;
        }
    }
}
