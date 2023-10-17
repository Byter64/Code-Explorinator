using Microsoft.CodeAnalysis;

namespace CodeExplorinator
{
    public record PlaceholderClassData : IClassData
    {
        public PlaceholderClassData(INamedTypeSymbol typeData)
        {
            this.typeData = typeData;
        }
    }
}
