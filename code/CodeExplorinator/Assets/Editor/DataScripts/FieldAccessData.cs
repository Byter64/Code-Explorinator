namespace CodeExplorinator
{
    public class FieldAccessData
    {

        /// <summary>
        /// The method in which this access takes place
        /// </summary>
        public MethodData ContainingMethod { get; private set; }

        /// <summary>
        /// The field that is accessed
        /// </summary>
        public FieldData ReferencedField { get; private set; }


        public FieldAccessData(MethodData containingMethod, FieldData referencedField)
        {
            ContainingMethod = containingMethod;
            ReferencedField = referencedField;
        }

    }
}