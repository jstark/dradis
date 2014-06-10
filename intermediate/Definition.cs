using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public class Definition
    {
        private Definition()
        {
            Text = "";
        }

        private Definition(string text)
        {
            Text = text;
        }

        public string Text { get; private set; }

        public static Definition CONSTANT = new Definition();
        public static Definition ENUMERATION_CONSTANT = new Definition("enumeration constant");
        public static Definition TYPE = new Definition();
        public static Definition VARIABLE = new Definition();
        public static Definition FIELD = new Definition("record field");
        public static Definition VALUE_PARM = new Definition("value parameter");
        public static Definition VAR_PARM = new Definition("VAR parameter");
        public static Definition PROGRAM = new Definition();
        public static Definition PROCEDURE = new Definition();
        public static Definition FUNCTION = new Definition();
        public static Definition UNDEFINED = new Definition();
    }
}
