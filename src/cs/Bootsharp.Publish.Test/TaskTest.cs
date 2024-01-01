﻿using System.Text.RegularExpressions;
using Microsoft.Build.Utilities.ProjectCreation;

namespace Bootsharp.Publish.Test;

public abstract class TaskTest : IDisposable
{
    protected MockProject Project { get; } = new();
    protected BuildEngine Engine { get; } = BuildEngine.Create();
    protected string LastAddedAssemblyName { get; private set; }
    protected virtual string TestedContent { get; } = "";

    public virtual void Dispose () => Project.Dispose();

    public abstract void Execute ();

    protected void AddAssembly (string assemblyName, params MockSource[] sources)
    {
        LastAddedAssemblyName = assemblyName;
        Project.AddAssembly(new(assemblyName, sources));
    }

    protected void AddAssembly (params MockSource[] sources)
    {
        AddAssembly($"MockAssembly{Guid.NewGuid():N}.dll", sources);
    }

    protected MockSource With (string @namespace, string code, bool wrapInClass = true)
    {
        return new(@namespace, code, wrapInClass);
    }

    protected MockSource With (string code, bool wrapInClass = true)
    {
        return With(null, code, wrapInClass);
    }

    protected void Contains (string content)
    {
        Assert.Contains(content, TestedContent);
    }

    protected MatchCollection Matches (string pattern)
    {
        Assert.Matches(pattern, TestedContent);
        return Regex.Matches(TestedContent, pattern);
    }

    protected string ReadProjectFile (string fileName)
    {
        var filePath = Path.Combine(Project.Root, fileName);
        return File.Exists(filePath) ? File.ReadAllText(filePath) : null;
    }
}