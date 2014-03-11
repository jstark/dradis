using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public class ParseTreePrinter
    {
        private const int INDENT_WIDTH = 4;
        private const int LINE_WIDTH = 80;

        private TextWriter writer;
        private int length = 0;
        private string indent = new string(' ', INDENT_WIDTH);
        private string indentation = "";
        private StringBuilder line = new StringBuilder();

        public ParseTreePrinter(TextWriter w)
        {
            writer = w;
        }

        public void Print(ICode icode)
        {
            writer.WriteLine("\n===== INTERMEDIATE CODE =====\n");
            PrintNode(icode.Root);
            PrintLine();
        }

        private void PrintNode(ICodeNode node)
        {
            // opening tag
            Append(indentation); Append("<" + node.ToString());

            PrintAttributes(node);
            PrintTypeSpec(node);

            List<ICodeNode> children = node.GetChildren();
            if (children.Count > 0)
            {
                Append(">");
                PrintLine();
                PrintChildNodes(children);
                Append(indentation);
                Append("</" + node.ToString() + ">");
            } else
            {
                Append(" "); Append("/>");
            }
            PrintLine();
        }

        private void PrintAttributes(ICodeNode node)
        {
            string save = indentation;

            node.ForeachAttribute((k, v) => PrintAttribute(k.ToString(), v));

            indentation = save;
        }

        private void PrintAttribute(string key, object value)
        {
            // if the value is a symbol table entry, use the identifier's name.
            // else just use the value string
            SymbolTableEntry entry = value as SymbolTableEntry;
            string value_str = entry != null ? entry.Name : value.ToString();
            string text = key.ToLower() + "=\"" + value_str + "\"";
            Append(" "); Append(text);

            // include the identifier's nesting level.
            if (entry != null)
            {
                int level = entry.SymbolTable.NestingLevel;
                PrintAttribute("LEVEL", level);
            }
        }

        private void PrintChildNodes(List<ICodeNode> child_nodes)
        {
            string save = indentation;
            indentation += indent;

            foreach (var child in child_nodes)
            {
                PrintNode(child);
            }

            indentation = save;
        }

        private void PrintTypeSpec(ICodeNode node)
        {

        }

        private void Append(string text)
        {
            int text_length = text.Length;
            bool line_break = false;

            // wrap lines that are too long
            if (length + text_length > LINE_WIDTH)
            {
                PrintLine();
                line.Append(indentation);
                length = indentation.Length;
                line_break = true;
            }

            // append the text
            if (!(line_break && text == " "))
            {
                line.Append(text);
                length += text_length;
            }
        }

        private void PrintLine()
        {
            if (length > 0)
            {
                writer.WriteLine(line);
                line.Length = 0;
                length = 0;
            }
        }
    }
}
