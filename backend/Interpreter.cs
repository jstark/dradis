using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.backend
{
    public class Interpreter : Backend
    {
        internal Interpreter() { }

        public override void Process(ICode iCode, SymbolTableStack symtabstack)
        {
            int execution_count = 0;
            var start = DateTime.Now;

            ICodeNode root = iCode.Root;
            StatementInterpreter stmnt_interpreter = StatementInterpreter.CreateWithObservers(observers);
            var execution_results = stmnt_interpreter.Execute(root, ref execution_count);

            var end = DateTime.Now;
            double elapsed_time = (end - start).Ticks / (1.0 * TimeSpan.TicksPerSecond);
            int runtime_errors = RuntimeErrorHandler.Errors;

            // send the message
            var args = Tuple.Create(execution_count, runtime_errors, elapsed_time);
            Message msg = new Message(MessageType.InterpreterSummary, args);
            Send(msg);
        }
    }
}
