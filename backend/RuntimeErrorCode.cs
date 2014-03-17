using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.backend
{
    public class RuntimeErrorCode
    {
        public string Message { get; private set; }

        public RuntimeErrorCode(string msg)
        {
            Message = msg;
        }

        public static RuntimeErrorCode UNITIALIZED_VALUE                = new RuntimeErrorCode("Uninitialized value");
        public static RuntimeErrorCode VALUE_RANGE                      = new RuntimeErrorCode("Value out of range");
        public static RuntimeErrorCode INVALID_CASE_EXPRESSION_VALUE    = new RuntimeErrorCode("Invalid CASE expression value");
        public static RuntimeErrorCode DIVISION_BY_ZERO                 = new RuntimeErrorCode("Division by zero");
        public static RuntimeErrorCode INVALID_STANDARD_FUNCTION_ARGUMENT = new RuntimeErrorCode("Invalid standard function argument");
        public static RuntimeErrorCode INVALID_INPUT                    = new RuntimeErrorCode("Invalid input");
        public static RuntimeErrorCode STACK_OVERFLOW                   = new RuntimeErrorCode("Runtime stack overflow");
        public static RuntimeErrorCode UNIMPLEMENTED_FEATURE            = new RuntimeErrorCode("Unimplemented runtime feature");
    }
}
