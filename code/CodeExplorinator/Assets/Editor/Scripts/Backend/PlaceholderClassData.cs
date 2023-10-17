using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public abstract record PlaceholderClassData : IClassData
    {
        public INamedTypeSymbol typeData;
        
        PlaceholderClassData(INamedTypeSymbol typeData)
        {
            this.typeData = typeData;
        }
    }
}
