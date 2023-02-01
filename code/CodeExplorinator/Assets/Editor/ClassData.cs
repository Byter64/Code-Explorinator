using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

namespace CodeExplorinator
{
    public class ClassData
    {
        public INamedTypeSymbol ClassInformation { get; private set; }
        public List<FieldData> PublicVariables { get; private set; }
        public List<FieldData> PrivateVariables { get; private set; }
        public List<MethodData> PublicMethods { get; private set; }
        public List<MethodData> PrivateMethods { get; private set; }

        public ClassData(INamedTypeSymbol classInformation)
        {
            PublicVariables = new List<FieldData>();
            PrivateVariables = new List<FieldData>();
            PublicMethods = new List<MethodData>();
            PrivateMethods = new List<MethodData>();
            ClassInformation = classInformation;
        }

        public void ClearAllPublicMethodInvocations()
        {
            foreach(MethodData method in PublicMethods)
            {
                method.InternalInvocations.Clear();
                method.ExternalInvocations.Clear();
            }
        }

        public void ClearAllPublicFieldAccesses()
        {
            foreach (FieldData field in PublicVariables)
            {
                field.InternalAccesses.Clear();
                field.ExternalAccesses.Clear();
            }
        }

        public override string ToString()
        {
            return ClassInformation.Name;
        }
    }
}