namespace CodeExplorinator
{
    public class ClassFieldReferenceData
    {
        /// <summary>
        /// The class that is referenced
        /// </summary>
        public ClassData ReferencedClass { get; private set; }

        /// <summary>
        /// The field in which the reference is saved in
        /// </summary>
        public FieldData FieldContainingReference;



        public ClassFieldReferenceData(ClassData referencedClass, FieldData fieldContainingReference)
        {
            ReferencedClass = referencedClass;
            FieldContainingReference = fieldContainingReference;
        }
    }
}