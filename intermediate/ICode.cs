using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    //
    public enum ICodeNodeType
    {
        // Program structure
        PROGRAM, PROCEDURE, FUNCTION,
        // Statements
        COMPOUND, ASSIGN, LOOP, TEST, CALL, PARAMETERS,
        IF, SELECT, SELECT_BRANCH, SELECT_CONSTANTS, NO_OP,
        // Relational operators
        EQ, NE, LT, LE, GT, GE, NOT,
        // Additive operators
        ADD, SUBTRACT, OR, NEGATE,
        // Multiplicative operators
        MULTIPLY, INTEGER_DIVIDE, FLOAT_DIVIDE, MOD, AND,
        // Operands
        VARIABLE, SUBSCRIPTS, FIELD,
        INTEGER_CONSTANT, REAL_CONSTANT,
        STRING_CONSTANT, BOOLEAN_CONSTANT
    }

    //
    public enum ICodeKey
    {
        LINE, ID, VALUE
    }

    //
    public sealed class ICodeNode
    {
        private List<ICodeNode> children;
        private Dictionary<ICodeKey, object> attributes;

        public ICodeNode(ICodeNodeType type)
        {
            children = new List<ICodeNode>();
            attributes = new Dictionary<ICodeKey, object>();
            Type = type;
        }

        public ICodeNodeType Type { get; private set; }
        public ICodeNode Parent { get; private set; }

        public ICodeNode Add(ICodeNode node)
        {
            Contract.Requires(!children.Contains(node));
            if (node != null)
            {
                children.Add(node);
                node.Parent = this;
            }
            return node;
        }

        public List<ICodeNode> GetChildren()
        {
            var tmp = new List<ICodeNode>(children);
            return tmp;
        }

        public void ForeachAttribute(Action<ICodeKey, object> action)
        {
            foreach(var p in attributes)
            {
                action(p.Key, p.Value);
            }
        }

        public void SetAttribute(ICodeKey key, object val)
        {
            attributes[key] = val;
        }

        public object GetAttribute(ICodeKey key)
        {
            object val = null;
            attributes.TryGetValue(key, out val);
            return val;
        }

        public ICodeNode Copy()
        {
            ICodeNode cp = ICodeFactory.CreateICodeNode(this.Type);
            foreach (var p in attributes)
                cp.SetAttribute(p.Key, p.Value);
            return cp;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    //
    public sealed class ICode 
    {
        public ICode() { }
        public ICodeNode Root { get; set; }
    }

    //
    public static class ICodeFactory
    {
        public static ICode CreateICode()
        {
            return new ICode();
        }

        public static ICodeNode CreateICodeNode(ICodeNodeType type)
        {
            return new ICodeNode(type);
        }
    }
}
