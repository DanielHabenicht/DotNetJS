﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bootsharp.Generate;

internal sealed class PartialMethod (MethodDeclarationSyntax syntax)
{
    public string EmitSource (Compilation compilation)
    {
        var symbol = compilation.GetSemanticModel(syntax.SyntaxTree).GetDeclaredSymbol(syntax)!;
        var space = GetNamespace(symbol, compilation);
        new BindingEmitter(symbol, space, symbol.Name).Emit(out var sig, out var body);
        return $"{syntax.Modifiers} {sig} => {body};".Replace("partial async", "async partial");
    }

    private string GetNamespace (IMethodSymbol symbol, Compilation compilation)
    {
        var space = symbol.ContainingNamespace.IsGlobalNamespace ? "Global"
            : string.Join(".", symbol.ContainingNamespace.ConstituentNamespaces);
        return ConvertNamespace(space, symbol.ContainingAssembly);
    }
}
