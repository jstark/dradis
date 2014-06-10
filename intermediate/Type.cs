using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dradis.intermediate
{
    public enum TypeForm
    {
        Scalar, Enumeration, Subrange, Array, Record
    }

    public enum TypeKey
    {
        // Enumeration constants
        EnumerationConstants,
        // Subrange
        SubrangeBaseType, SubrangeMinValue, SubrangeMaxValue,
        // Array
        ArrayIndexType, ArrayElementType, ArrayElementCount,
        // Record
        RecordSymbolTable
    }

    public class TypeSpec
    {
        private Dictionary<TypeKey, object> attributes = new Dictionary<TypeKey,object>();
        private TypeForm form;
        private SymbolTableEntry identifier;

        //
        private TypeSpec(TypeForm f)
        {
            form = f;
            identifier = null;
        }

        // 
        private TypeSpec(string value)
        {
            form = TypeForm.Array;

            TypeSpec indexType = new TypeSpec(TypeForm.Subrange);
            // FIXME: Predefined
            // indexType.SetAttribute(TypeKey.SubrangeBaseType, Predefined.IntegerType);
            indexType.SetAttribute(TypeKey.SubrangeMinValue, 1);
            indexType.SetAttribute(TypeKey.SubrangeMaxValue, value.Length);
            SetAttribute(TypeKey.ArrayIndexType, indexType);
            // FIXME: Predefined
            // SetAttribute(TypeKey.ArrayElementType, Predefined.CharType);
            SetAttribute(TypeKey.ArrayElementCount, value.Length);
        }

        public void SetAttribute(TypeKey key, object value)
        {
            System.Diagnostics.Contracts.Contract.Requires(attributes.ContainsKey(key) == false);
            attributes[key] = value;
        }

        public object GetAttribute(TypeKey key)
        {
            object attr = null;

            if (attributes.TryGetValue(key, out attr))
            {
                return attr;
            }
            return null;
        }

        public TypeSpec BaseType()
        {
            return form == TypeForm.Subrange ? (TypeSpec)GetAttribute(TypeKey.SubrangeBaseType) : this;
        }

        public bool IsPascalString()
        {
            bool isPascalString = false;
            if (form == TypeForm.Array)
            {
                TypeSpec elementType = (TypeSpec)GetAttribute(TypeKey.ArrayElementType);
                TypeSpec indexType = (TypeSpec)GetAttribute(TypeKey.ArrayIndexType);
                // FIXME: Predefined
                // return elementType.BaseType() == Predefined.CharType && indexType.BaseType() == Predefined.IntegerType
            }
            return isPascalString;
        }

        public static TypeSpec CreateType(TypeForm form)
        {
            return new TypeSpec(form);
        }

        public static TypeSpec CreateStringType(string value)
        {
            return new TypeSpec(value);
        }
    }
}
