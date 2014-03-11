using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NDesk.Options;

using dradis.frontend;
using dradis.intermediate;
using dradis.message;

namespace dradis
{
    sealed class SourceMessageObserver : IMessageObserver
    {
        public void AcceptMessage(Message msg)
        {
            MessageType type = msg.Type;

            if (type == MessageType.SourceLine)
            {
                var args = (Tuple<int, string>)msg.Args;
                Console.WriteLine("{0,-3} {1}", args.Item1, args.Item2);
            }
        }
    }

    sealed class ParserMessageObserver : IMessageObserver
    {
        private const string PARSER_SUMMARY_FORMAT = "\n{0,20} source lines \n{1, 20} syntax errors \n{2,20:0.00} seconds total parsing time.";
        private const string PARSER_TOKEN_FORMAT = ">>> {0, -15} line={1, 3}, pos={2, 2}, text = \"{3}\"";
        private const string PARSER_VALUE_FORMAT = ">>>                       value=\"{0}\"";
        private const int PREFIX_WIDTH = 5;
        public void AcceptMessage(Message msg)
        {
            MessageType type = msg.Type;
            if (type == MessageType.ParserSummary)
            {
                var args = (Tuple<int, int, double>)msg.Args;
                Console.WriteLine(PARSER_SUMMARY_FORMAT, args.Item1, args.Item2, args.Item3);
            } else if (type == MessageType.Token)
            {
                var args = (Tuple<int, int, TokenType, string, object>)msg.Args;
                Console.WriteLine(PARSER_TOKEN_FORMAT, args.Item3.ToString(), args.Item1, args.Item2, args.Item4);
                if (args.Item5 != null)
                {
                    Console.WriteLine(PARSER_VALUE_FORMAT, args.Item5.ToString());
                }
            } else if (type == MessageType.SyntaxError)
            {
                var args = (Tuple<int, int, string, string>)msg.Args;
                int space = PREFIX_WIDTH + args.Item2;
                StringBuilder flagbuilder = new StringBuilder();
                flagbuilder.Append(' ', space - 1);
                flagbuilder.Append("^\n*** ").Append(args.Item4);

                if (!String.IsNullOrEmpty(args.Item3))
                {
                    flagbuilder.Append(" [at \"").Append(args.Item3).Append("\"]");
                }
                Console.WriteLine("{0}", flagbuilder.ToString());
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            bool show_help = false;
            bool xref = false;
            bool print_ast = false;
            var names = new List<string>();

            var p = new OptionSet()
            {
                { "h|help", "show this message and exit", v => show_help = v != null },
                { "x|xref", "perform a cross-reference of identifiers", v => xref = v != null },
                { "i|intermediate", "print the parse tree of the input", v => print_ast = v != null }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);
            }
            catch (OptionException option_exception)
            {
                Console.Write("dradis: ");
                Console.WriteLine(option_exception.Message);
                Console.WriteLine("Try `dradis --help' for more information.");
                return;
            }

            if (show_help)
            {
                ShowHelp(p);
                return;
            }

            if (extra.Count == 0)
            {
                Console.WriteLine("dradis: ");
                Console.WriteLine("No source file given.");
                Console.WriteLine("Try `dradis --help' for more information.");
                return;
            }

            string source_path = extra[0];
            try
            {
                using (StreamReader reader = new StreamReader(source_path))
                {
                    SourceMessageObserver sourceObserver = new SourceMessageObserver();
                    Source source = new Source(reader);
                    source.Add(sourceObserver);

                    Scanner scanner = new Scanner(source);
                    Parser parser = new Parser(scanner);
                    ParserMessageObserver parserObserver = new ParserMessageObserver();
                    parser.Add(parserObserver);

                    var result = parser.Parse();
                    ICode icode = result.Item1;
                    SymbolTableStack symtabstack = result.Item2;

                    if (xref)
                    {
                        XRef.Print(symtabstack);
                    }

                    if (print_ast)
                    {
                        ParseTreePrinter printer = new ParseTreePrinter(Console.Out);
                        printer.Print(icode);
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine("dradis: ");
                Console.WriteLine("FATAL ERROR: " + ex.Message);
                Console.WriteLine("Try `dradis --help' for more information.");
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: dradis [OPTIONS]+ file");
            Console.WriteLine("Check a pascal source file for invalid tokens. You can also");
            Console.WriteLine("perform a cross-reference of all identifier tokens.");
            Console.WriteLine();
            Console.WriteLine("Options: ");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}
