using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.frontend;
using dradis.intermediate;
using dradis.message;

namespace dradis.frontend
{
    /// <summary>
    /// NonTerminal parser is the base class of all statement parsers. It is used just
    /// for code reuse. NO LISKOV SUBSTITUTION CRITERION is implied for subclasses.
    /// </summary>
    public abstract class NonTerminalParser : MessageProducer
    {
        protected Scanner InternalScanner { get; private set; }

        protected SymbolTableStack SymTabStack { get; private set; }

        protected List<IMessageObserver> Observers
        {
            get
            {
                return observers;
            }
            private set
            {
                observers = value;
            }
        }

        protected NonTerminalParser() { }

        public abstract ICodeNode Parse(Token token);

        protected static T CreateWithObserver<T>(Scanner s, SymbolTableStack stack, List<IMessageObserver> obl) where T : NonTerminalParser, new()
        {
            return new T() { InternalScanner = s, SymTabStack = stack, Observers = obl };
        }
    }
}
