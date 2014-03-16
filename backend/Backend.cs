using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.backend
{
    public abstract class Backend : MessageProducer
    {
        public abstract void Process(ICode iCode, SymbolTableStack symtabstack);

        public static Backend Create(string tp)
        {
            if (String.Equals(tp, "compile", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Compiler();
            } else if (String.Equals(tp, "interpret", StringComparison.CurrentCultureIgnoreCase))
            {
                return new Interpreter();
            }
            return null;
        }
    }
}
