﻿using System;
using Bootsharp;

public static partial class Program
{
    public static void Main ()
    {
        OnMainInvoked($"Hello {GetFrontendName()}, .NET here!");
    }

    [JSEvent] // used in JS as `bootsharp.Global.onMainInvoked.subscribe`
    public static partial void OnMainInvoked (string message);

    [JSFunction] // assigned in JS as `bootsharp.Global.getName = () => ...`
    public static partial async Task<string> GetFrontendName ();

    [JSInvokable] // invoked from JS as `bootsharp.Global.GetBackendName()`
    public static string GetBackendName () => $".NET {Environment.Version}";

    [JSInvokable]
    public static ExampleValidation ValidateDevice()
    {
        // var device = GetExampleDevice();
        // var error = device.path == null;
        return new ExampleValidation()
        {
            errors = true
        };
    }
}
