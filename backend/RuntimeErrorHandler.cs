using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using dradis.message;

namespace dradis.backend
{
    public static class RuntimeErrorHandler
    {
        private const int MAX_ERRORS = 5;

        public static void Flag(ICodeNode node, RuntimeErrorCode error_code, MessageProducer backend)
        {
            //string line_number = null;
            while (node != null && node.GetAttribute(ICodeKey.LINE) == null)
            {
                node = node.Parent;
            }

            // notify observers
            var args = (Tuple<string, int>)Tuple.Create(error_code.Message, (int)node.GetAttribute(ICodeKey.LINE));
            Message msg = new Message(MessageType.RuntimeError, args);
            backend.Send(msg);

            if (++Errors > MAX_ERRORS)
            {
                Console.WriteLine("*** ABORTED AFTER TOO MANY RUNTIME ERRORS.*");
                Environment.Exit(-1);
            }
        }

        public static int Errors { get; private set; }
    }
}
