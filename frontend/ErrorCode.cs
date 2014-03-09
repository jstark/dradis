using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.frontend
{
    public class ErrorCode
    {
        static int id = 0;

        private ErrorCode(string msg) : this(msg, GetNextErrorCodeId())
        {}

        private ErrorCode(string msg, int id)
        {
            Message = msg;
            ID = id;
        }

        public string Message { get; private set; }

        public int ID { get; private set; }
    
        private static int GetNextErrorCodeId()
        {
            return ++id;
        }

        public static ErrorCode ALREADY_FORWARDED       = new ErrorCode("Already specified in FORWARD");
        public static ErrorCode IDENTIFIER_REDEFINED    = new ErrorCode("Redefined identifier");
        public static ErrorCode IDENTIFIER_UNDEFINED    = new ErrorCode("Undefined identifier");
        public static ErrorCode INCOMPATIBLE_ASSIGNMENT = new ErrorCode("Incompatible assignment");
        public static ErrorCode INCOMPATIBLE_TYPES      = new ErrorCode("Incompatible types");
        public static ErrorCode INVALID_ASSIGNMENT      = new ErrorCode("Invalid assignment statement");
        public static ErrorCode INVALID_CHARACTER       = new ErrorCode("Invalid character");
        public static ErrorCode INVALID_CONSTANT        = new ErrorCode("Invalid constant");
        public static ErrorCode INVALID_EXPONENT        = new ErrorCode("Invalid exponent");
        public static ErrorCode INVALID_EXPRESSION      = new ErrorCode("Invalid expression");
        public static ErrorCode INVALID_FIELD           = new ErrorCode("Invalid field");
        public static ErrorCode INVALID_FRACTION        = new ErrorCode("Invalid fraction");
        public static ErrorCode INVALID_IDENTIFIER_USAGE= new ErrorCode("Invalid index type");
        public static ErrorCode INVALID_NUMBER          = new ErrorCode("Invalid number");
        public static ErrorCode INVALID_STATEMENT       = new ErrorCode("Invalid statement");
        public static ErrorCode INVALID_SUBRANGE_TYPE   = new ErrorCode("Invalid subrange type");
        public static ErrorCode INVALID_TARGET          = new ErrorCode("Invalid assignment target");
        public static ErrorCode INVALID_TYPE            = new ErrorCode("Invalid type");
        public static ErrorCode INVALID_VAR_PARM        = new ErrorCode("Invalid VAR parameter");
        public static ErrorCode MIN_GT_MAX              = new ErrorCode("Min limit greater than max limit");
        public static ErrorCode MISSING_BEGIN           = new ErrorCode("Missing BEGIN");
        public static ErrorCode MISSING_COLON           = new ErrorCode("Missing :");
        public static ErrorCode MISSING_COLON_EQUALS    = new ErrorCode("Missing :=");
        public static ErrorCode MISSING_COMMA           = new ErrorCode("Missing ,");
        public static ErrorCode MISSING_CONSTANT        = new ErrorCode("Missing constant");
        public static ErrorCode MISSING_DO              = new ErrorCode("Missing DO");
        public static ErrorCode MISSING_DOT_DOT         = new ErrorCode("Missing ..");
        public static ErrorCode MISSING_END             = new ErrorCode("Missing END");
        public static ErrorCode MISSING_EQUALS          = new ErrorCode("Missing =");
        public static ErrorCode MISSING_FOR_CONTROL     = new ErrorCode("Invalid FOR control variable");
        public static ErrorCode MISSING_IDENTIFIER      = new ErrorCode("Missing identifier");
        public static ErrorCode MISSING_LEFT_BRACKET    = new ErrorCode("Missing [");
        public static ErrorCode MISSING_OF              = new ErrorCode("Missing OF");
        public static ErrorCode MISSING_PERIOD          = new ErrorCode("Missing .");
        public static ErrorCode MISSING_PROGRAM         = new ErrorCode("Missing PROGRAM");
        public static ErrorCode MISSING_RIGHT_BRACKET   = new ErrorCode("Missing ]");
        public static ErrorCode MISSING_RIGHT_PAREN     = new ErrorCode("Missing )");
        public static ErrorCode MISSING_SEMICOLON       = new ErrorCode("Missing ;");
        public static ErrorCode MISSING_THEN            = new ErrorCode("Missing THEN");
        public static ErrorCode MISSING_TO_DOWN_TO      = new ErrorCode("Missing TO or DOWNTO");
        public static ErrorCode MISSING_UNTIL           = new ErrorCode("Missing UNTIL");
        public static ErrorCode MISSING_VARIABLE        = new ErrorCode("Missing variable");
        public static ErrorCode CASE_CONSTANT_REUSED    = new ErrorCode("CASE constant reused");
        public static ErrorCode NOT_CONSTANT_IDENTIFIER = new ErrorCode("Not a constant identifier");
        public static ErrorCode NOT_RECORD_VARIABLE     = new ErrorCode("Not a record variable");
        public static ErrorCode NOT_TYPE_IDENTIFIER     = new ErrorCode("Not a type identifier");
        public static ErrorCode RANGE_INTEGER           = new ErrorCode("Integer literal out of range");
        public static ErrorCode RANGE_REAL              = new ErrorCode("Real literal out of range");
        public static ErrorCode STACK_OVERFLOW          = new ErrorCode("Stack overflow");
        public static ErrorCode TOO_MANY_LEVELS         = new ErrorCode("Nesting level too deep");
        public static ErrorCode TOO_MANY_SUBSCRIPTS     = new ErrorCode("Too many subscripts");
        public static ErrorCode UNEXPECTED_EOF          = new ErrorCode("Unexpected end of file");
        public static ErrorCode UNEXPECTED_TOKEN        = new ErrorCode("Unexpected token");
        public static ErrorCode UNIMPLEMENTED           = new ErrorCode("Unimplemented feature");
        public static ErrorCode UNRECOGNIZABLE          = new ErrorCode("Unrecognizable input");
        public static ErrorCode WRONG_NUMBER_OF_PARMS = new ErrorCode("Wrong number of actual parameters");

        // FATAL
        public static ErrorCode IO_ERROR = new ErrorCode("Object I/O error", -101);
        public static ErrorCode TOO_MANY_ERRORS = new ErrorCode("Too many errors", -102);
    }
}
