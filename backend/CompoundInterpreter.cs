using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class CompoundInterpreter : MessageProducer
    {
        private CompoundInterpreter() : base() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            // loop and execute each child
            StatementInterpreter stmnt_interpreter = StatementInterpreter.CreateWithObservers(observers);

            foreach (var child in node.GetChildren())
            {
                stmnt_interpreter.Execute(child, ref exec_count);
                exec_count += 1;
            }

            return null;
        }

        public static CompoundInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            CompoundInterpreter compound_interpreter = new CompoundInterpreter();
            compound_interpreter.observers.AddRange(observers);
            return compound_interpreter;
        }
    }
}
