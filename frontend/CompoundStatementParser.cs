using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.intermediate;
using System.Diagnostics.Contracts;

namespace dradis.frontend
{
    public class CompoundStatementParser
    {
        private Scanner scanner;
        private SymbolTableStack symtabstack;

        public CompoundStatementParser(Scanner s, SymbolTableStack stack)
        {
            scanner = s;
            symtabstack = stack;
        }

        public ICodeNode Parse(Token token)
        {
            Contract.Requires(token.TokenType == TokenType.BEGIN);
            // ignore argument !
            token = scanner.GetNextToken();

            // create the COMPOUND node.
            ICodeNode compound_node = ICodeFactory.CreateICodeNode(ICodeNodeType.COMPOUND);

            // parse the statement list terminated by the END token.
            StatementParser statement_parser = new StatementParser(scanner, symtabstack);
            statement_parser.ParseList(token, compound_node, TokenType.END, ErrorCode.MISSING_END);
            return compound_node;
        }
    }
}
