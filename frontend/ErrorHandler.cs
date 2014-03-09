using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;

namespace dradis.frontend
{
   public static class ErrorHandler
    {
       private const int MAX_ERRORS = 25;
       private static int errors = 0;

       public static void Flag(Token token, ErrorCode err, message.MessageProducer mp)
       {
           var args = Tuple.Create(token.LineNumber, token.Position, token.Lexeme, err.Message);
           Message msg = new Message(MessageType.SyntaxError, args);
           mp.Send(msg);

           if (++errors > MAX_ERRORS)
           {
               AbortTranslation(ErrorCode.TOO_MANY_ERRORS, mp);
           }
       }

       public static void AbortTranslation(ErrorCode err, message.MessageProducer mp)
       {
           string fatal = "FATAL ERROR: " + err.Message;
           var args = Tuple.Create(0, 0, "", fatal);
           Message msg = new Message(MessageType.SyntaxError, args);
           mp.Send(msg);
           Environment.Exit(-1);
       }

       public static int GetErrorCount()
       {
           return errors;
       }
    }
}
