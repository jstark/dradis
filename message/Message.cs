using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.message
{
    public enum MessageType
    {
        SourceLine, SyntaxError,
        ParserSummary, InterpreterSummary, CompilerSummary,
        Miscellaneous, Token,
        Assign, Fetch, Breakpoint, RuntimeError,
        Call, Return
    }

    public class Message
    {
        public Message(MessageType type, object args)
        {
            Type = type;
            Args = args;
            
        }

        public MessageType Type { get; private set; }
        public object Args { get; private set; }
    }

    public interface IMessageObserver
    {
        void AcceptMessage(Message msg);
    }

    public class MessageProducer
    {
        private List<IMessageObserver> observers;

        public MessageProducer()
        {
            observers = new List<IMessageObserver>();
        }

        public void Add(IMessageObserver observer)
        {
            observers.Add(observer);
        }

        public void Send(Message message)
        {
            foreach(var obj in observers)
            {
                obj.AcceptMessage(message);
            }
        }
    }
}
