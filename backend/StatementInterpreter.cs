using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;

namespace dradis.backend
{
    public class StatementInterpreter : MessageProducer
    {
        private StatementInterpreter() : base() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            // send a message about the current source line
            SendSourceLineMessage(node);

            switch (node.Type)
            {
                case ICodeNodeType.COMPOUND:
                    {
                        CompoundInterpreter compound_interpreter = 
                            CompoundInterpreter.CreateWithObservers(observers);
                        return compound_interpreter.Execute(node, ref exec_count);
                    }
                case ICodeNodeType.ASSIGN:
                    {
                        AssignmentInterpreter assign_interpreter = 
                            AssignmentInterpreter.CreateWithObservers(observers);
                        return assign_interpreter.Execute(node, ref exec_count);
                    }
                case ICodeNodeType.LOOP:
                    {
                        LoopInterpreter loop_interpreter = 
                            LoopInterpreter.CreateWithObservers(observers);
                        return loop_interpreter.Execute(node, ref exec_count);
                    }
                case ICodeNodeType.IF:
                    {
                        IfInterpreter if_interpreter =
                            IfInterpreter.CreateWithObservers(observers);
                        return if_interpreter.Execute(node, ref exec_count);
                    }
                case ICodeNodeType.SELECT:
                    {
                        SelectInterpreter select_interpreter =
                            SelectInterpreter.CreateWithObservers(observers);
                        return select_interpreter.Execute(node, ref exec_count);
                    }
                case ICodeNodeType.NO_OP:
                    return null;
                default:
                    RuntimeErrorHandler.Flag(node, RuntimeErrorCode.UNIMPLEMENTED_FEATURE, this);
                    return null;
            }
        }

        private void SendSourceLineMessage(ICodeNode node)
        {
            object line_number = node.GetAttribute(ICodeKey.LINE);

            // send the SourceLine message
            if (line_number != null)
            {
                var args = (Tuple<int>)Tuple.Create((int)line_number);
                Message msg = new Message(MessageType.SourceLine, args);
                Send(msg);
            }
        }

        public static StatementInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            StatementInterpreter stmnt_interpreter = new StatementInterpreter();
            stmnt_interpreter.observers.AddRange(observers);
            return stmnt_interpreter;
        }
    }
}
