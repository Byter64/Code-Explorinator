using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public abstract record ClassData : IClassData
    {
        public INamedTypeSymbol typeData;


        ClassData(INamedTypeSymbol typeData)
        {
            this.typeData = typeData;
        }
    }
    
}