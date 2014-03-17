using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class AssignmentInterpreter : MessageProducer
    {
        private AssignmentInterpreter() : base() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            // The ASSIGN node's children are the target variable
            // and the expression.
            List<ICodeNode> children = node.GetChildren();
            ICodeNode variable = children[0];
            ICodeNode expression = children[1];

            // Expression the expression and get its value.
            ExpressionInterpreter expr_interpreter = ExpressionInterpreter.CreateWithObservers(observers);
            object value = expr_interpreter.Execute(expression, ref exec_count);

            // Set the value as an attribute of the variable's symbol table entry.
            SymbolTableEntry variable_id = (SymbolTableEntry)variable.GetAttribute(ICodeKey.ID);
            variable_id.SetAttribute(SymbolTableKey.DataValue, value);

            SendMessage(node, variable_id.Name, value);
            ++exec_count;
            return null;
        }

        public static AssignmentInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            AssignmentInterpreter assign_interpreter = new AssignmentInterpreter();
            assign_interpreter.observers.AddRange(observers);
            return assign_interpreter;
        }

        private void SendMessage(ICodeNode node, string variable_name, object value)
        {
            object line_number = node.GetAttribute(ICodeKey.LINE);

            // Send an ASSIGN message.
            if (line_number != null)
            {
                var args = Tuple.Create((int)line_number, variable_name, (object)value);
                Message msg = new Message(MessageType.Assign, args);
                Send(msg);
            }
        }
    }
}
