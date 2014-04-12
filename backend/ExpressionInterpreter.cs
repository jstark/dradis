using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dradis.message;
using dradis.intermediate;
using System.Diagnostics.Contracts;

namespace dradis.backend
{
    public class ExpressionInterpreter : MessageProducer
    {
        private static readonly HashSet<ICodeNodeType> ARITH_OPS;

        static ExpressionInterpreter()
        {
            ARITH_OPS = new HashSet<ICodeNodeType>();
            ARITH_OPS.Add(ICodeNodeType.ADD);
            ARITH_OPS.Add(ICodeNodeType.SUBTRACT);
            ARITH_OPS.Add(ICodeNodeType.MULTIPLY);
            ARITH_OPS.Add(ICodeNodeType.FLOAT_DIVIDE);
            ARITH_OPS.Add(ICodeNodeType.INTEGER_DIVIDE);
        }

        private ExpressionInterpreter() : base() { }

        public object Execute(ICodeNode node, ref int exec_count)
        {
            switch (node.Type)
            {
                case ICodeNodeType.VARIABLE:
                    {
                        // Get the variable's symbol table entry and return its value.
                        SymbolTableEntry entry = (SymbolTableEntry)node.GetAttribute(ICodeKey.ID);
                        return entry.GetAttribute(SymbolTableKey.DataValue);
                    }
                case ICodeNodeType.INTEGER_CONSTANT:
                    {
                        // Return the integer value.
                        return node.GetAttribute(ICodeKey.VALUE);
                    }
                case ICodeNodeType.REAL_CONSTANT:
                    {
                        // Return the integer value.
                        return node.GetAttribute(ICodeKey.VALUE);
                    }
                case ICodeNodeType.STRING_CONSTANT:
                    {
                        // Return the integer value.
                        return node.GetAttribute(ICodeKey.VALUE);
                    }
                case ICodeNodeType.NEGATE:
                    {
                        // Get the NEGATE node's expression node child.
                        List<ICodeNode> children = node.GetChildren();
                        ICodeNode expression = children[0];

                        // Execute the expression and return the negative of its value.
                        object value = Execute(expression, ref exec_count);
                        if (value is int)
                        {
                            return -(int)value;
                        } else if (value is double)
                        {
                            return -(double)value;
                        } else
                        {
                            return null;
                        }
                    }
                case ICodeNodeType.NOT:
                    {
                        // Get the NOT node's expression node child.
                        List<ICodeNode> children = node.GetChildren();
                        ICodeNode expression = children[0];

                        // Execute the expression and return the "not" of its value
                        bool value = (bool)Execute(expression, ref exec_count);
                        return !value;
                    }
                default:
                    // must be a binary operator
                    return ExecuteBinaryOperator(node, ref exec_count);
            }
        }

        public static ExpressionInterpreter CreateWithObservers(List<IMessageObserver> observers)
        {
            ExpressionInterpreter expr_interpreter = new ExpressionInterpreter();
            expr_interpreter.observers.AddRange(observers);
            return expr_interpreter;
        }

        private object ExecuteBinaryOperator(ICodeNode node, ref int exec_count)
        {
            // Get the two operand children of the operator node.
            List<ICodeNode> children = node.GetChildren();
            ICodeNode operand_node1 = children[0];
            ICodeNode operand_node2 = children[1];

            // Operands.
            object operand1 = Execute(operand_node1, ref exec_count);
            object operand2 = Execute(operand_node2, ref exec_count);

            bool integer_mode = (operand1 is int) && (operand2 is int);

            // ====================
            // Arithmetic operators
            // ====================
            if (ARITH_OPS.Contains(node.Type))
            {
                if (integer_mode)
                {
                    int value1 = (int)operand1;
                    int value2 = (int)operand2;

                    // Integer operation
                    switch (node.Type)
                    {
                        case ICodeNodeType.ADD: 
                            return value1 + value2;
                        case ICodeNodeType.SUBTRACT:
                            return value1 - value2;
                        case ICodeNodeType.MULTIPLY:
                            return value1 * value2;
                        case ICodeNodeType.FLOAT_DIVIDE:
                            // check for division by error:
                            if (value2 != 0)
                            {
                                return (double)value1 / (double)value2;
                            } else
                            {
                                RuntimeErrorHandler.Flag(node, RuntimeErrorCode.DIVISION_BY_ZERO, this);
                                return 0;
                            }
                        case ICodeNodeType.INTEGER_DIVIDE:
                            // check for division by error:
                            if (value2 != 0)
                            {
                                return value1 / value2;
                            }
                            else
                            {
                                RuntimeErrorHandler.Flag(node, RuntimeErrorCode.DIVISION_BY_ZERO, this);
                                return 0;
                            }
                        case ICodeNodeType.MOD:
                            // check for division by error:
                            if (value2 != 0)
                            {
                                return value1 % value2;
                            }
                            else
                            {
                                RuntimeErrorHandler.Flag(node, RuntimeErrorCode.DIVISION_BY_ZERO, this);
                                return 0;
                            }
                    }
                } else
                {
                    double value1 = operand1 is int ? (int)operand1 : (double)operand1;
                    double value2 = operand2 is int ? (int)operand2 : (double)operand2;

                    // float operations
                    switch (node.Type)
                    {
                        case ICodeNodeType.ADD:
                            return value1 + value2;
                        case ICodeNodeType.SUBTRACT:
                            return value1 - value2;
                        case ICodeNodeType.MULTIPLY:
                            return value1 * value2;
                        case ICodeNodeType.FLOAT_DIVIDE:
                            // check for division by zero
                            if (value2 != 0.0f)
                            {
                                return value1 / value2;
                            } else
                            {
                                RuntimeErrorHandler.Flag(node, RuntimeErrorCode.DIVISION_BY_ZERO, this);
                                return 0.0f;
                            }
                    }
                }
            } else if (node.Type == ICodeNodeType.AND || node.Type == ICodeNodeType.OR)
            {
                bool value1 = (bool)operand1;
                bool value2 = (bool)operand2;

                switch (node.Type)
                {
                    case ICodeNodeType.AND:
                        return value1 && value2;
                    case ICodeNodeType.OR:
                        return value1 || value2;
                }
            } else if (integer_mode)
            {
                int value1 = (int)operand1;
                int value2 = (int)operand2;

                // integer operands
                switch (node.Type)
                {
                    case ICodeNodeType.EQ:
                        return value1 == value2;
                    case ICodeNodeType.NE:
                        return value1 != value2;
                    case ICodeNodeType.LT:
                        return value1 < value2;
                    case ICodeNodeType.LE:
                        return value1 <= value2;
                    case ICodeNodeType.GT:
                        return value1 > value2;
                    case ICodeNodeType.GE:
                        return value1 >= value2;
                }
            } else
            {
                double value1 = operand1 is int ? (int)operand1 : (double)operand1;
                double value2 = operand2 is int ? (int)operand2 : (double)operand2;

                // float operands
                switch (node.Type)
                {
                    case ICodeNodeType.EQ:
                        return value1 == value2;
                    case ICodeNodeType.NE:
                        return value1 != value2;
                    case ICodeNodeType.LT:
                        return value1 < value2;
                    case ICodeNodeType.LE:
                        return value1 <= value2;
                    case ICodeNodeType.GT:
                        return value1 > value2;
                    case ICodeNodeType.GE:
                        return value1 <= value2;
                }
            }
            return 0; // should never get here.
        }
    }
}
