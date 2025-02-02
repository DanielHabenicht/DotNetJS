﻿namespace Bootsharp.Generate.Test;

public static class InvokableTest
{
    public static IEnumerable<object[]> Data { get; } = new[] {
        // Generates dynamic dependencies registration.
        [
            """
            partial class Foo
            {
                [JSInvokable]
                public static void Bar () { }
            }
            """,
            """
            partial class Foo
            {
                [ModuleInitializer]
                [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Foo", "GeneratorTest")]
                internal static void RegisterDynamicDependencies () { }
            }
            """
        ],
        // Generates dynamic dependencies registration under namespace.
        new object[] {
            """
            namespace Space;

            partial class Foo
            {
                [JSInvokable]
                public static void Bar () { }
            }
            """,
            """
            namespace Space;

            partial class Foo
            {
                [ModuleInitializer]
                [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Space.Foo", "GeneratorTest")]
                internal static void RegisterDynamicDependencies () { }
            }
            """
        }
    };
}
