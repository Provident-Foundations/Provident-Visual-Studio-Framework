using System.Reflection.Metadata;

namespace Provident.Foundations.VisualStudio.DeveloperTools.Model
{
    public sealed class ClassDefinition
    {
        public string Namespace { get; set; } = string.Empty;

        public string ClassName { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string? BaseClass { get; set; }

        public List<string> Interfaces { get; } = [];

        public List<PropertyDefinition> Properties { get; } = [];

        public List<MethodDefinition> Methods { get; } = [];
    }
}