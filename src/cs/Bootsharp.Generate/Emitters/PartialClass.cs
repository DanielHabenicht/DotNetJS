﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bootsharp.Generate;

internal sealed class PartialClass (
    Compilation compilation,
    ClassDeclarationSyntax syntax,
    IReadOnlyList<PartialMethod> methods,
    bool emitDependenciesRegistration
)
{
    public string Name { get; } = syntax.Identifier.ToString();

    public string EmitSource () => EmitCommon(
        EmitUsings() +
        WrapNamespace(
            EmitHeader() +
            EmitDynamicDependenciesRegistration() +
            EmitMethods() +
            EmitFooter()
        )
    );

    private string EmitUsings ()
    {
        var imports = syntax.SyntaxTree.GetRoot().DescendantNodesAndSelf()
            .OfType<UsingDirectiveSyntax>().Where(u => u.Name?.ToString() != "Bootsharp");
        var result = string.Join("\n", imports);
        return string.IsNullOrEmpty(result) ? "" : result + "\n\n";
    }

    private string EmitHeader () => $"{syntax.Modifiers} class {syntax.Identifier}\n{{";

    private string EmitDynamicDependenciesRegistration ()
    {
        if (!emitDependenciesRegistration) return "";
        var space = syntax.Parent is BaseNamespaceDeclarationSyntax decl ? $"{decl.Name}." : "";
        var fullClassName = space + syntax.Identifier;
        var assemblyName = compilation.Assembly.Name;
        return $$"""

                     [ModuleInitializer]
                     [DynamicDependency(DynamicallyAccessedMemberTypes.All, "{{fullClassName}}", "{{assemblyName}}")]
                     internal static void RegisterDynamicDependencies () { }
                 """;
    }

    private string EmitMethods ()
    {
        if (methods.Count == 0) return "";
        var sources = methods.Select(m => "    " + m.EmitSource(compilation));
        return "\n\n" + string.Join("\n", sources);
    }

    private string EmitFooter () => "\n}";

    private string WrapNamespace (string source)
    {
        if (syntax.Parent is NamespaceDeclarationSyntax space)
            return $$"""
                     namespace {{space.Name}}
                     {
                         {{string.Join("\n", source.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                             .Select((s, i) => i > 0 && s.Length > 0 ? "    " + s : s))}}
                     }
                     """;
        if (syntax.Parent is FileScopedNamespaceDeclarationSyntax fileSpace)
            return $"namespace {fileSpace.Name};\n\n{source}";
        return source;
    }
}
