using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public enum SymbolTableKey
    {
        // Constant
        ConstantValue,
        // Procedure or function
        RoutineCode, RoutineSymbolTable, RoutineICode,
        RoutineParams, RoutineRoutines,
        // Variable or record field value
        DataValue
    }


    public sealed class SymbolTableStack
    {
        private List<SymbolTable> stack = new List<SymbolTable>();

        public SymbolTableStack()
        {
            stack.Add(SymbolTableFactory.CreateTable(0));
            NestingLevel = 0;
        }

        public int NestingLevel { get; private set; }

        public SymbolTable GetLocal()
        {
            Contract.Requires(stack.Count > 0);
            return stack[NestingLevel];
        }

        public SymbolTableEntry CreateInLocal(string name)
        {
            var table = GetLocal();
            return table.CreateEntry(name);
        }

        public SymbolTableEntry FindInLocal(string name)
        {
            var table = GetLocal();
            return table.FindEntry(name);
        }

        public SymbolTableEntry Find(string name)
        {
            return FindInLocal(name);
        }
    }

    //
    public sealed class SymbolTable
    {
        private Dictionary<string, SymbolTableEntry> entries;

        public SymbolTable(int nest_level)
        {
            entries = new Dictionary<string, SymbolTableEntry>();
            NestingLevel = nest_level;
        }

        public int NestingLevel { get; private set; }

        public SymbolTableEntry CreateEntry(string name)
        {
            Contract.Requires(!entries.ContainsKey(name));
            var entry = SymbolTableFactory.CreateTableEntry(name, this);
            Contract.Requires(entry != null);
            entries[name] = entry;
            return entry;
        }

        public SymbolTableEntry FindEntry(string name)
        {
            SymbolTableEntry entry = null;
            entries.TryGetValue(name, out entry);
            return entry;
        }

        public List<SymbolTableEntry> GetEntries()
        {
            return entries.Values.OrderBy( e => e.Name ).ToList();
        }
    }

    //
    public sealed class SymbolTableEntry
    {
        private List<int> lines = new List<int>();
        private Dictionary<SymbolTableKey, object> attributes;

        public SymbolTableEntry(string name, SymbolTable table)
        {
            attributes = new Dictionary<SymbolTableKey, object>();
            Name = name;
            SymbolTable = table;
        }

        public string Name { get; private set; }

        public SymbolTable SymbolTable { get; private set; }

        public void AppendLine(int line)
        {
            lines.Add(line);
        }

        public List<int> GetLines()
        {
            List<int> tmp = new List<int>(lines);
            return tmp;
        }

        public void SetAttribute(SymbolTableKey key, object val)
        {
            Contract.Requires(!attributes.ContainsKey(key));
            attributes[key] = val;
        }

        public object GetAttribute(SymbolTableKey key)
        {
            object val;
            attributes.TryGetValue(key, out val);
            return val;
        }
    }

    //
    public sealed class SymbolTableFactory
    {
        public static SymbolTableStack CreateStack()
        {
            return new SymbolTableStack();
        }

        public static SymbolTable CreateTable(int level)
        {
            return new SymbolTable(level);
        }

        public static SymbolTableEntry CreateTableEntry(string name, SymbolTable table)
        {
            return new SymbolTableEntry(name, table);
        }
    }
}
