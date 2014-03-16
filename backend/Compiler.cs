using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.backend
{
    public class Compiler : Backend
    {
        internal Compiler() { }

        public override void Process(ICode iCode, SymbolTableStack symtabstack)
        {
            var start = DateTime.Now;
            // 
            var end = DateTime.Now;
            double elapsed_time = (end - start).Ticks / (1.0 * TimeSpan.TicksPerSecond);
            int instruction_count = 0;

            // send the message
            var args = Tuple.Create(instruction_count, elapsed_time);
            Message msg = new Message(MessageType.CompilerSummary, args); 
            Send(msg);
        }
    }
}
