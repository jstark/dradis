using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public static class Predefined
    {
        // predefined types
        public static TypeSpec IntegerType;
        public static TypeSpec RealType;
        public static TypeSpec BooleanType;
        public static TypeSpec CharType;
        public static TypeSpec UndefinedType;

        // predefined identifiers
        public static SymbolTableEntry IntegerId;
        public static SymbolTableEntry RealId;
        public static SymbolTableEntry BooleanId;
        public static SymbolTableEntry CharId;
        public static SymbolTableEntry FalseId;
        public static SymbolTableEntry TrueId;

        /**
         * Initialize a symbol table stack with predefined identifiers. 
         * 
         */
        public static void Initialize(SymbolTableStack symtabstack)
        {
            InitializeTypes(symtabstack);
            InitializeConstants(symtabstack);
        }

        private static void InitializeTypes(SymbolTableStack symtabstack)
        {
            // int
            IntegerId = symtabstack.CreateInLocal("integer");
            IntegerType = TypeSpec.CreateType(TypeForm.Scalar);
            IntegerType.Identifier = IntegerId;
            IntegerId.Definition = Definition.TYPE;
            IntegerId.Type = IntegerType;

            // real
            RealId = symtabstack.CreateInLocal("real");
            RealType = TypeSpec.CreateType(TypeForm.Scalar);
            RealType.Identifier = RealId;
            RealId.Definition = Definition.TYPE;
            RealId.Type = RealType;

            // boolean
            BooleanId = symtabstack.CreateInLocal("boolean");
            BooleanType = TypeSpec.CreateType(TypeForm.Enumeration);
            BooleanType.Identifier = BooleanId;
            BooleanId.Definition = Definition.TYPE;
            BooleanId.Type = BooleanType;

            // char
            CharId = symtabstack.CreateInLocal("char");
            CharType = TypeSpec.CreateType(TypeForm.Scalar);
            CharType.Identifier = CharId;
            CharId.Definition = Definition.TYPE;
            CharId.Type = CharType;

            //
            UndefinedType = TypeSpec.CreateType(TypeForm.Scalar);

        }

        private static void InitializeConstants(SymbolTableStack symtabstack)
        {
            // boolean enum constant false
            FalseId = symtabstack.CreateInLocal("false");
            FalseId.Definition = Definition.ENUMERATION_CONSTANT;
            FalseId.Type = BooleanType;
            FalseId.SetAttribute(SymbolTableKey.ConstantValue, 0);

            // boolean enum constant true
            TrueId = symtabstack.CreateInLocal("true");
            TrueId.Definition = Definition.ENUMERATION_CONSTANT;
            TrueId.Type = BooleanType;
            TrueId.SetAttribute(SymbolTableKey.ConstantValue, 1);

            // add false & true to the boolean enumeration type
            var constants = new List<SymbolTableEntry>() { FalseId, TrueId };
            BooleanType.SetAttribute(TypeKey.EnumerationConstants, constants);
        }
    }
}
