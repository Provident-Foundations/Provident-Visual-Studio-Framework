using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Provident.Foundations.VisualStudio.Code;

/// <summary>
/// Provides stateless Roslyn transformations for applying a consistent layout to C# classes.
/// </summary>
public static class ClassLayoutRefactorer
{
    /// <summary>Applies the default class layout to a class in a Roslyn document.</summary>
    /// <param name="document">The document containing the class.</param>
    /// <param name="classNode">The class declaration to transform.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A document containing the transformed class.</returns>
    public static Task<Document> RefactorClassAsync(Document document, ClassDeclarationSyntax classNode, CancellationToken cancellationToken)
        => RefactorClassAsync(document, classNode, cancellationToken, null, false);

    /// <summary>Applies a configured class layout to a class in a Roslyn document.</summary>
    /// <param name="document">The document containing the class.</param>
    /// <param name="classNode">The class declaration to transform.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <param name="configuration">The layout configuration, or <see langword="null"/> for the default.</param>
    /// <param name="ensureXmlDocumentation">Whether missing member documentation should be added.</param>
    /// <returns>A document containing the transformed class.</returns>
    public static async Task<Document> RefactorClassAsync(Document document, ClassDeclarationSyntax classNode, CancellationToken cancellationToken, LayoutConfiguration? configuration, bool ensureXmlDocumentation = false)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(classNode);
        cancellationToken.ThrowIfCancellationRequested();
        SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("The document does not contain a syntax root.");
        if (!root.DescendantNodes().Contains(classNode))
        {
            throw new ArgumentException("The class declaration does not belong to the supplied document.", nameof(classNode));
        }

        LayoutConfiguration layout = configuration ?? CreateDefaultConfiguration();
        ValidateConfiguration(layout);
        Dictionary<MemberCategory, List<MemberDeclarationSyntax>> categories = CategorizeMembers(classNode);
        ClassDeclarationSyntax rebuilt = RebuildClass(classNode, categories, layout, ensureXmlDocumentation);
        return document.WithSyntaxRoot(root.ReplaceNode(classNode, rebuilt));
    }

    /// <summary>Creates the standard member layout used when no configuration is supplied.</summary>
    /// <returns>A new default layout configuration.</returns>
    public static LayoutConfiguration CreateDefaultConfiguration() => new()
    {
        Regions =
        [
            Region("Public Events", MemberCategory.PublicEvents, MemberSortRule.NameAscending),
            Region("Private Member Declarations", MemberCategory.PrivateFields, MemberSortRule.ReadonlyFirst, MemberSortRule.StaticFirst, MemberSortRule.NameAscending),
            Region("Constructors / Dispose Methods", MemberCategory.ConstructorsAndDispose, MemberSortRule.NameAscending),
            Region("Public Properties", MemberCategory.PublicProperties, MemberSortRule.NameAscending),
            Region("Public Methods", MemberCategory.PublicMethods, MemberSortRule.ConstructorsFirst, MemberSortRule.AsyncLast, MemberSortRule.NameAscending),
            Region("Private Methods", MemberCategory.PrivateMethods, MemberSortRule.AsyncLast, MemberSortRule.NameAscending),
            Region("Other Members", MemberCategory.OtherMembers, MemberSortRule.NameAscending)
        ]
    };

    private static RegionConfiguration Region(string name, MemberCategory category, params MemberSortRule[] rules) => new() { Name = name, Category = category, SortRules = rules };

    private static Dictionary<MemberCategory, List<MemberDeclarationSyntax>> CategorizeMembers(ClassDeclarationSyntax classNode)
    {
        Dictionary<MemberCategory, List<MemberDeclarationSyntax>> result = Enum.GetValues<MemberCategory>().ToDictionary(x => x, _ => new List<MemberDeclarationSyntax>());
        foreach (MemberDeclarationSyntax member in classNode.Members)
        {
            result[GetCategory(member)].Add(member);
        }
        return result;
    }

    private static MemberCategory GetCategory(MemberDeclarationSyntax member) => member switch
    {
        EventFieldDeclarationSyntax eventField when IsPublic(eventField) => MemberCategory.PublicEvents,
        FieldDeclarationSyntax field when IsPrivateOrUnspecified(field) => MemberCategory.PrivateFields,
        ConstructorDeclarationSyntax or DestructorDeclarationSyntax => MemberCategory.ConstructorsAndDispose,
        MethodDeclarationSyntax method when IsDisposeMethod(method) => MemberCategory.ConstructorsAndDispose,
        PropertyDeclarationSyntax property when IsPublic(property) => MemberCategory.PublicProperties,
        MethodDeclarationSyntax method when IsPublic(method) => MemberCategory.PublicMethods,
        MethodDeclarationSyntax => MemberCategory.PrivateMethods,
        _ => MemberCategory.OtherMembers
    };

    private static ClassDeclarationSyntax RebuildClass(ClassDeclarationSyntax original, IReadOnlyDictionary<MemberCategory, List<MemberDeclarationSyntax>> categories, LayoutConfiguration configuration, bool addDocumentation)
    {
        List<MemberDeclarationSyntax> rebuilt = [];
        HashSet<MemberCategory> configured = [];
        foreach (RegionConfiguration region in configuration.Regions)
        {
            configured.Add(region.Category);
            List<MemberDeclarationSyntax> members = categories.TryGetValue(region.Category, out List<MemberDeclarationSyntax>? source)
                ? SortMembers(source, region.SortRules).ToList() : [];
            if (members.Count == 0) continue;
            if (addDocumentation) members = members.Select(EnsureXmlDocumentation).ToList();
            AddRegion(rebuilt, region.Name, members);
        }

        foreach ((MemberCategory category, List<MemberDeclarationSyntax> members) in categories)
        {
            if (!configured.Contains(category) && members.Count > 0) AddRegion(rebuilt, category.ToString(), members);
        }
        return original.WithMembers(SyntaxFactory.List(rebuilt));
    }

    private static void AddRegion(List<MemberDeclarationSyntax> target, string name, IReadOnlyList<MemberDeclarationSyntax> members)
    {
        MemberDeclarationSyntax first = members[0].WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia($"#region {name}{Environment.NewLine}{Environment.NewLine}").AddRange(members[0].GetLeadingTrivia()));
        if (members.Count == 1)
        {
            target.Add(first.WithTrailingTrivia(first.GetTrailingTrivia().AddRange(SyntaxFactory.ParseTrailingTrivia($"{Environment.NewLine}{Environment.NewLine}#endregion{Environment.NewLine}"))));
            return;
        }

        target.Add(first);
        for (int i = 1; i < members.Count - 1; i++) target.Add(members[i]);
        MemberDeclarationSyntax last = members[^1].WithTrailingTrivia(members[^1].GetTrailingTrivia().AddRange(SyntaxFactory.ParseTrailingTrivia($"{Environment.NewLine}{Environment.NewLine}#endregion{Environment.NewLine}")));
        target.Add(last);
    }

    /// <summary>Adds placeholder XML documentation when a member has no documentation comment.</summary>
    /// <param name="member">The member to inspect.</param>
    /// <returns>The original member or a member with placeholder documentation.</returns>
    public static MemberDeclarationSyntax EnsureXmlDocumentation(MemberDeclarationSyntax member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (member.GetLeadingTrivia().Any(x => x.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || x.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))) return member;
        const string documentation = "/// <summary>\n/// TODO: Describe member.\n/// </summary>\n";
        return member.WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(documentation).AddRange(member.GetLeadingTrivia()));
    }

    private static IList<MemberDeclarationSyntax> SortMembers(IEnumerable<MemberDeclarationSyntax> members, IEnumerable<MemberSortRule> rules)
    {
        IOrderedEnumerable<MemberDeclarationSyntax>? ordered = null;
        foreach (MemberSortRule rule in rules) ordered = ApplyRule(ordered ?? members.OrderBy(_ => 0), rule);
        return ordered?.ToList() ?? members.ToList();
    }

    private static IOrderedEnumerable<MemberDeclarationSyntax> ApplyRule(IOrderedEnumerable<MemberDeclarationSyntax> members, MemberSortRule rule) => rule switch
    {
        MemberSortRule.NameAscending => members.ThenBy(GetName, StringComparer.Ordinal),
        MemberSortRule.NameDescending => members.ThenByDescending(GetName, StringComparer.Ordinal),
        MemberSortRule.StaticFirst => members.ThenByDescending(IsStatic),
        MemberSortRule.StaticLast => members.ThenBy(IsStatic),
        MemberSortRule.ReadonlyFirst => members.ThenByDescending(IsReadonly),
        MemberSortRule.ReadonlyLast => members.ThenBy(IsReadonly),
        MemberSortRule.AsyncFirst => members.ThenByDescending(IsAsync),
        MemberSortRule.AsyncLast => members.ThenBy(IsAsync),
        MemberSortRule.VirtualFirst => members.ThenByDescending(IsVirtual),
        MemberSortRule.VirtualLast => members.ThenBy(IsVirtual),
        MemberSortRule.OverrideFirst => members.ThenByDescending(IsOverride),
        MemberSortRule.OverrideLast => members.ThenBy(IsOverride),
        MemberSortRule.ConstructorsFirst => members.ThenByDescending(IsConstructor),
        MemberSortRule.ConstructorsLast => members.ThenBy(IsConstructor),
        MemberSortRule.ByParameterCount => members.ThenBy(GetParameterCount),
        MemberSortRule.ByLineCount => members.ThenBy(GetLineCount),
        _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, "Unsupported member sort rule.")
    };

    private static string GetName(MemberDeclarationSyntax member) => member switch
    {
        BaseTypeDeclarationSyntax type => type.Identifier.Text,
        BaseFieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? string.Empty,
        MethodDeclarationSyntax method => method.Identifier.Text,
        ConstructorDeclarationSyntax constructor => constructor.Identifier.Text,
        DestructorDeclarationSyntax destructor => destructor.Identifier.Text,
        PropertyDeclarationSyntax property => property.Identifier.Text,
        IndexerDeclarationSyntax => "this",
        _ => member.GetFirstToken().ValueText
    };

    private static int GetParameterCount(MemberDeclarationSyntax member) => member switch
    {
        BaseMethodDeclarationSyntax method => method.ParameterList.Parameters.Count,
        IndexerDeclarationSyntax indexer => indexer.ParameterList.Parameters.Count,
        _ => 0
    };

    private static int GetLineCount(MemberDeclarationSyntax member)
    {
        FileLinePositionSpan span = member.SyntaxTree?.GetLineSpan(member.Span) ?? default;
        return span.EndLinePosition.Line - span.StartLinePosition.Line + 1;
    }

    private static bool IsStatic(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.StaticKeyword);
    private static bool IsReadonly(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
    private static bool IsAsync(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.AsyncKeyword);
    private static bool IsVirtual(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.VirtualKeyword);
    private static bool IsOverride(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.OverrideKeyword);
    private static bool IsConstructor(MemberDeclarationSyntax member) => member is ConstructorDeclarationSyntax;
    private static bool IsPublic(MemberDeclarationSyntax member) => member.Modifiers.Any(SyntaxKind.PublicKeyword);
    private static bool IsPrivateOrUnspecified(MemberDeclarationSyntax member) =>
        !member.Modifiers.Any(SyntaxKind.PublicKeyword) &&
        !member.Modifiers.Any(SyntaxKind.ProtectedKeyword) &&
        !member.Modifiers.Any(SyntaxKind.InternalKeyword);
    private static bool IsDisposeMethod(MethodDeclarationSyntax method) => method.Identifier.ValueText is "Dispose" or "DisposeAsync" && method.ParameterList.Parameters.Count == 0;

    private static void ValidateConfiguration(LayoutConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        HashSet<MemberCategory> categories = [];
        foreach (RegionConfiguration region in configuration.Regions)
        {
            if (string.IsNullOrWhiteSpace(region.Name)) throw new ArgumentException("Every configured region must have a name.", nameof(configuration));
            if (!categories.Add(region.Category)) throw new ArgumentException($"Member category '{region.Category}' is configured more than once.", nameof(configuration));
        }
    }
}
