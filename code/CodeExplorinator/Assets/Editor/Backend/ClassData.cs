using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public record ClassData : IClassData
    {
        public ClassData(INamedTypeSymbol typeData)
        {
            this.typeData = typeData;
        }
    }
    
}