using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Provident.Foundations.VisualStudio.Analysis.Naming;

/// <summary>Extracts C# names from syntax trees and optional semantic models.</summary>
public sealed class CSharpNameExtractor
{
    private readonly INamingClassifier _classifier;

    /// <summary>Initializes a new extractor.</summary>
    public CSharpNameExtractor(INamingClassifier? classifier = null)
    {
        _classifier = classifier ?? new NamingClassifier();
    }

    /// <summary>Extracts names from a C# source file.</summary>
    public IReadOnlyList<NamingObservation> Extract(
        string source,
        NamingSourceFileIdentity sourceFile,
        NamingProjectIdentity project,
        bool includeReferences = true,
        SemanticModel? semanticModel = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sourceFile);
        ArgumentNullException.ThrowIfNull(project);
        cancellationToken.ThrowIfCancellationRequested();

        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, path: sourceFile.Path, cancellationToken: cancellationToken);
        SyntaxNode root = tree.GetRoot(cancellationToken);
        HashSet<TextSpan> declarationSpans = new();
        List<NamingObservation> observations = new();

        foreach (SyntaxNode node in root.DescendantNodesAndSelf())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryGetDeclaration(node, out string? name, out NamingSymbolKind kind, out NamingNameRole role))
            {
                TextSpan span = GetNameSpan(node);
                declarationSpans.Add(span);
                observations.Add(CreateObservation(name, kind, role, sourceFile, project, node, semanticModel, tree, span));
            }
        }

        if (includeReferences)
        {
            foreach (IdentifierNameSyntax identifier in root.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (declarationSpans.Contains(identifier.Identifier.Span))
                    continue;
                ISymbol? symbol = semanticModel?.GetSymbolInfo(identifier, cancellationToken).Symbol;
                if (symbol is null)
                    continue;
                NamingSymbolKind kind = ToSymbolKind(symbol);
                observations.Add(CreateObservation(identifier.Identifier.ValueText, kind, NamingNameRole.Reference, sourceFile, project, identifier, semanticModel, tree, identifier.Identifier.Span));
            }
        }
        return observations;
    }

    private static TextSpan GetNameSpan(SyntaxNode node) => node switch
    {
        BaseTypeDeclarationSyntax declaration => declaration.Identifier.Span,
        DelegateDeclarationSyntax declaration => declaration.Identifier.Span,
        MethodDeclarationSyntax declaration => declaration.Identifier.Span,
        ConstructorDeclarationSyntax declaration => declaration.Identifier.Span,
        PropertyDeclarationSyntax declaration => declaration.Identifier.Span,
        EventDeclarationSyntax declaration => declaration.Identifier.Span,
        EnumMemberDeclarationSyntax declaration => declaration.Identifier.Span,
        ParameterSyntax declaration => declaration.Identifier.Span,
        VariableDeclaratorSyntax declaration => declaration.Identifier.Span,
        LocalFunctionStatementSyntax declaration => declaration.Identifier.Span,
        TypeParameterSyntax declaration => declaration.Identifier.Span,
        NamespaceDeclarationSyntax declaration => declaration.Name.Span,
        FileScopedNamespaceDeclarationSyntax declaration => declaration.Name.Span,
        IndexerDeclarationSyntax declaration => declaration.ThisKeyword.Span,
        _ => node.Span
    };

    private NamingObservation CreateObservation(
        string name,
        NamingSymbolKind kind,
        NamingNameRole role,
        NamingSourceFileIdentity sourceFile,
        NamingProjectIdentity project,
        SyntaxNode node,
        SemanticModel? semanticModel,
        SyntaxTree tree,
        TextSpan nameSpan)
    {
        ISymbol? symbol = role == NamingNameRole.Reference
            ? semanticModel?.GetSymbolInfo(node).Symbol
            : semanticModel?.GetDeclaredSymbol(node);
        string fullyQualified = symbol?.ToDisplayString() ?? name;
        string key = symbol is null ? $"{project.FilePath}|{sourceFile.Path}|{node.SpanStart}|{kind}" : $"{symbol.Kind}|{fullyQualified}";
        NamingSymbolIdentity symbolIdentity = new()
        {
            Key = key,
            Name = symbol?.Name ?? name,
            FullyQualifiedName = fullyQualified,
            Kind = symbol is null ? kind : ToSymbolKind(symbol),
            Namespace = symbol?.ContainingNamespace?.ToDisplayString(),
            ContainingSymbolKey = symbol?.ContainingType?.ToDisplayString(),
            Project = project,
            Accessibility = symbol?.DeclaredAccessibility.ToString(),
            IsStatic = symbol?.IsStatic ?? false,
            IsPartial = symbol?.Locations.Length > 1
        };
        FileLinePositionSpan lineSpan = tree.GetLineSpan(nameSpan);
        NamingClassification classification = _classifier.Classify(name);
        return new NamingObservation
        {
            Id = $"{sourceFile.Path}:{nameSpan.Start}:{kind}",
            Name = name,
            NormalizedName = name.TrimStart('_').ToLowerInvariant(),
            Role = role,
            Kind = kind,
            Language = NamingSourceLanguage.CSharp,
            SourceFile = sourceFile,
            Location = new NamingSourceSpan
            {
                FilePath = sourceFile.Path,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                StartColumn = lineSpan.StartLinePosition.Character + 1,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndColumn = lineSpan.EndLinePosition.Character + 1,
                CharacterOffset = nameSpan.Start,
                CharacterLength = nameSpan.Length
            },
            Symbol = symbolIdentity,
            Namespace = symbol?.ContainingNamespace?.ToDisplayString(),
            ContainingType = symbol?.ContainingType?.ToDisplayString(),
            Classification = classification
        };
    }

    private static bool TryGetDeclaration(SyntaxNode node, out string name, out NamingSymbolKind kind, out NamingNameRole role)
    {
        name = string.Empty;
        kind = NamingSymbolKind.Other;
        role = NamingNameRole.Declaration;
        switch (node)
        {
            case BaseTypeDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText;
                kind = declaration switch { RecordDeclarationSyntax => NamingSymbolKind.Record, ClassDeclarationSyntax => NamingSymbolKind.Class, StructDeclarationSyntax => NamingSymbolKind.Struct, InterfaceDeclarationSyntax => NamingSymbolKind.Interface, EnumDeclarationSyntax => NamingSymbolKind.Enum, _ => NamingSymbolKind.Other };
                return true;
            case DelegateDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Delegate; return true;
            case MethodDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Method; return true;
            case ConstructorDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Constructor; return true;
            case PropertyDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Property; return true;
            case IndexerDeclarationSyntax:
                name = "this"; kind = NamingSymbolKind.Indexer; return true;
            case EventDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Event; return true;
            case EnumMemberDeclarationSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.EnumMember; return true;
            case ParameterSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.Parameter; return true;
            case VariableDeclaratorSyntax declaration:
                name = declaration.Identifier.ValueText;
                kind = declaration.Parent?.Parent switch
                {
                    EventFieldDeclarationSyntax => NamingSymbolKind.Event,
                    FieldDeclarationSyntax field when field.Modifiers.Any(SyntaxKind.ConstKeyword) => NamingSymbolKind.Constant,
                    FieldDeclarationSyntax => NamingSymbolKind.Field,
                    _ => NamingSymbolKind.Local
                };
                return true;
            case LocalFunctionStatementSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.LocalFunction; return true;
            case TypeParameterSyntax declaration:
                name = declaration.Identifier.ValueText; kind = NamingSymbolKind.TypeParameter; return true;
            case NamespaceDeclarationSyntax declaration:
                name = declaration.Name.ToString(); kind = NamingSymbolKind.Namespace; return true;
            case FileScopedNamespaceDeclarationSyntax declaration:
                name = declaration.Name.ToString(); kind = NamingSymbolKind.Namespace; return true;
            default:
                return false;
        }
    }

    private static NamingSymbolKind ToSymbolKind(ISymbol symbol) => symbol switch
    {
        INamespaceSymbol => NamingSymbolKind.Namespace,
        INamedTypeSymbol named when named.TypeKind == TypeKind.Interface => NamingSymbolKind.Interface,
        INamedTypeSymbol named when named.TypeKind == TypeKind.Struct => NamingSymbolKind.Struct,
        INamedTypeSymbol named when named.TypeKind == TypeKind.Enum => NamingSymbolKind.Enum,
        INamedTypeSymbol named when named.IsRecord => NamingSymbolKind.Record,
        INamedTypeSymbol => NamingSymbolKind.Class,
        IMethodSymbol method when method.MethodKind == MethodKind.Constructor => NamingSymbolKind.Constructor,
        IMethodSymbol => NamingSymbolKind.Method,
        IPropertySymbol property when property.IsIndexer => NamingSymbolKind.Indexer,
        IPropertySymbol => NamingSymbolKind.Property,
        IFieldSymbol field when field.IsConst => NamingSymbolKind.Constant,
        IFieldSymbol => NamingSymbolKind.Field,
        IEventSymbol => NamingSymbolKind.Event,
        IParameterSymbol => NamingSymbolKind.Parameter,
        ITypeParameterSymbol => NamingSymbolKind.TypeParameter,
        _ => NamingSymbolKind.Other
    };
}
