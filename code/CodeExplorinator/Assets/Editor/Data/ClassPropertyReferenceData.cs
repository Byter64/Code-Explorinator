namespace CodeExplorinator
{
    public class ClassPropertyReferenceData
    {
        /// <summary>
        /// The class that is referenced
        /// </summary>
        public ClassData ReferencedClass { get; private set; }

        /// <summary>
        /// The property in which the reference is saved in
        /// </summary>
        public PropertyData PropertyContainingReference;
            


        public ClassPropertyReferenceData(ClassData referencedClass, PropertyData propertyContainingReference)
        {
            ReferencedClass = referencedClass;
            PropertyContainingReference = propertyContainingReference;
        }
    }
}