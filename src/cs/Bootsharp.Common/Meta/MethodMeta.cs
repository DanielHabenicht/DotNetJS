﻿namespace Bootsharp;

/// <summary>
/// Bootsharp-specific metadata of an interop method.
/// </summary>
public sealed record MethodMeta
{
    /// <summary>
    /// Type of interop the method is implementing.
    /// </summary>
    public required MethodType Type { get; init; }
    /// <summary>
    /// C# assembly name (DLL file name, w/o the extension), under which the method is declared.
    /// </summary>
    public required string Assembly { get; init; }
    /// <summary>
    /// Full name of the C# type (including namespace), under which the method is declared.
    /// </summary>
    public required string Space { get; init; }
    /// <summary>
    /// JavaScript object name(s) (joined with dot when nested) under which the associated interop
    /// function will be declared; resolved from <see cref="Space"/> with user-defined converters.
    /// </summary>
    public required string JSSpace { get; init; }
    /// <summary>
    /// C# name of the method, as specified in source code.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// JavaScript name of the method (function), as will be specified in source code.
    /// </summary>
    public required string JSName { get; init; }
    /// <summary>
    /// Arguments of the method, in declaration order.
    /// </summary>
    public required IReadOnlyList<ArgumentMeta> Arguments { get; init; }
    /// <summary>
    /// Metadata of the value returned by the method.
    /// </summary>
    public required ValueMeta ReturnValue { get; init; }

    public override string ToString ()
    {
        var args = string.Join(", ", Arguments.Select(a => a.ToString()));
        return $"[{Type}] {Assembly}.{Space}.{Name} ({args}) => {ReturnValue}";
    }
}
