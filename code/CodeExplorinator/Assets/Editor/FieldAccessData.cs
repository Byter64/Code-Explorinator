using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace CodeExplorinator
{
    public class FieldAccessData
    {
        public MethodData containingMethod { get; private set; }
        public FieldData referencedField { get; private set; }


        public FieldAccessData(MethodData containingMethod, FieldData referencedField)
        {
            this.containingMethod = containingMethod;
            this.referencedField = referencedField;
        }

    }
}