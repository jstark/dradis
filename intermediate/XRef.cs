using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public static class XRef
    {
        private static readonly int NAME_WIDTH = 16;
        private static readonly string NAME_FORMAT = "{0, -" + NAME_WIDTH.ToString() + "}";
        private static readonly string NUMBERS_LABEL      = " Line numbers    ";
        private static readonly string NUMBERS_UNDERLINE = " ------------    ";
        private static readonly int LABEL_WIDTH = NUMBERS_LABEL.Length;
        private static readonly int INDENT_WIDTH = NAME_WIDTH + LABEL_WIDTH;
        private static readonly string INDENT = new String(' ', INDENT_WIDTH);

        public static void Print(SymbolTableStack stack)
        {
            Console.WriteLine("\n===== CROSS-REFERENCE TABLE =====");
            PrintColumnHeadings();
            PrintSymbolTable(stack.GetLocal());
        }

        private static void PrintColumnHeadings()
        {
            Console.WriteLine();
            Console.WriteLine(String.Format(NAME_FORMAT + "{1}", "Identifier", NUMBERS_LABEL));
            Console.WriteLine(String.Format(NAME_FORMAT + "{1}", "----------", NUMBERS_UNDERLINE));
        }

        private static void PrintSymbolTable(SymbolTable table)
        {
            var entries = table.GetEntries();
            foreach(var e in entries)
            {
                var lines = e.GetLines();
                Console.Write(NAME_FORMAT, e.Name);
                foreach(var i in lines)
                {
                    Console.Write(String.Format("{0, 3} ", i));
                }
                Console.WriteLine();
            }
        }
    }
}
