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
            var start = DateTime.Now;
            // 
            var end = DateTime.Now;
            double elapsed_time = (end - start).Ticks / (1.0 * TimeSpan.TicksPerSecond);
            int execution_count = 0;
            int runtime_errors = 0;

            // send the message
            var args = Tuple.Create(execution_count, runtime_errors, elapsed_time);
            Message msg = new Message(MessageType.InterpreterSummary, args);
            Send(msg);
        }
    }
}
