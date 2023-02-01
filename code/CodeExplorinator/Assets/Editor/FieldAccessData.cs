using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    public class FieldAccessData
    {
        public MethodData ContainingMethod { get; private set; }
        public FieldData ReferencedField { get; private set; }


        public FieldAccessData(MethodData containingMethod, FieldData referencedField)
        {
            ContainingMethod = containingMethod;
            ReferencedField = referencedField;
        }

    }
}