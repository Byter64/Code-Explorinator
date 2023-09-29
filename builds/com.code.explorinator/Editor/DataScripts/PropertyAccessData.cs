namespace CodeExplorinator
{
    public class PropertyAccessData
    {
        /// <summary>
        /// The method in which this access takes place
        /// </summary>
        public MethodData ContainingMethod { get; private set; }

        /// <summary>
        /// The property that is accessed
        /// </summary>
        public PropertyData ReferencedProperty { get; private set; }


        public PropertyAccessData(MethodData containingMethod, PropertyData referencedProperty)
        {
            ContainingMethod = containingMethod;
            ReferencedProperty = referencedProperty;
        }
    }
}

