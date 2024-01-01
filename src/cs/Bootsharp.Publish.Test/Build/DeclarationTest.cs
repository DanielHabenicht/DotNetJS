namespace Bootsharp.Publish.Test;

public class DeclarationTest : BuildTest
{
    protected override string TestedContent => GeneratedDeclarations;

    [Fact]
    public void ImportsEventType ()
    {
        Execute();
        Contains("""import type { Event } from "./event";""");
    }

    [Fact]
    public void DeclaresNamespace ()
    {
        AddAssembly(With("Foo", "[JSInvokable] public static void Bar () { }"));
        Execute();
        Contains("export namespace Foo {");
    }

    [Fact]
    public void DotsInSpaceArePreserved ()
    {
        AddAssembly(With("Foo.Bar.Nya", "[JSInvokable] public static void Bar () { }"));
        Execute();
        Contains("export namespace Foo.Bar.Nya {");
    }

    [Fact]
    public void FunctionDeclarationIsExportedForInvokableMethod ()
    {
        AddAssembly(With("Foo", "[JSInvokable] public static void Foo () { }"));
        Execute();
        Contains("export namespace Foo {\n    export function foo(): void;\n}");
    }

    [Fact]
    public void AssignableVariableIsExportedForFunctionCallback ()
    {
        AddAssembly(With("Foo", "[JSFunction] public static void OnFoo () { }"));
        Execute();
        Contains("export namespace Foo {\n    export let onFoo: () => void;\n}");
    }

    [Fact]
    public void EventPropertiesAreExportedForEventMethods ()
    {
        AddAssembly(
            With("Foo", "[JSEvent] public static void OnFoo () { }"),
            With("Foo", "[JSEvent] public static void OnBar (string baz) { }"),
            With("Foo", "[JSEvent] public static void OnFar (int yaz, bool? nya) { }"));
        Execute();
        Contains(
            """
            export namespace Foo {
                export const onFoo: Event<[]>;
                export const onBar: Event<[baz: string]>;
                export const onFar: Event<[yaz: number, nya: boolean | undefined]>;
            }
            """);
    }

    [Fact]
    public void MembersFromSameSpaceAreDeclaredUnderSameSpace ()
    {
        AddAssembly(
            With("Foo", "public class Foo { }"),
            With("Foo", "[JSInvokable] public static Foo GetFoo () => default;"));
        Execute();
        Contains("export namespace Foo {\n    export interface Foo {\n    }\n}");
        Contains("export namespace Foo {\n    export function getFoo(): Foo.Foo;\n}");
    }

    [Fact]
    public void MembersFromDifferentSpacesAreDeclaredUnderRespectiveSpaces ()
    {
        AddAssembly(
            With("Foo", "public class Foo { }", false),
            With("Bar", "[JSInvokable] public static Foo.Foo GetFoo () => default;"));
        Execute();
        Contains("export namespace Foo {\n    export interface Foo {\n    }\n}");
        Contains("export namespace Bar {\n    export function getFoo(): Foo.Foo;\n}");
    }

    [Fact]
    public void MultipleSpacesAreDeclaredFromNewLine ()
    {
        AddAssembly(
            With("a", "[JSInvokable] public static void Foo () { }"),
            With("b", "[JSInvokable] public static void Bar () { }"));
        Execute();
        Contains("\nexport namespace b");
    }

    [Fact]
    public void DifferentSpacesWithSameRootAreDeclaredIndividually ()
    {
        AddAssembly(
            With("Nya.Bar", "[JSInvokable] public static void Fun () { }"),
            With("Nya.Foo", "[JSInvokable] public static void Foo () { }"));
        Execute();
        Contains("export namespace Nya.Bar {\n    export function fun(): void;\n}");
        Contains("export namespace Nya.Foo {\n    export function foo(): void;\n}");
    }

    [Fact]
    public void WhenNoSpaceTypesAreDeclaredUnderGlobalSpace ()
    {
        AddAssembly(
            With("public class Foo { }", false),
            With("[JSFunction] public static void OnFoo (Foo foo) { }"));
        Execute();
        Contains("export namespace Global {\n    export interface Foo {\n    }\n}");
        Contains("export namespace Global {\n    export let onFoo: (foo: Global.Foo) => void;\n}");
    }

    [Fact]
    public void NamespaceAttributeOverrideSpaceNames ()
    {
        AddAssembly(
            With("""[assembly:JSNamespace(@"Foo\.Bar\.(\S+)", "$1")]""", false),
            With("Foo.Bar.Nya", "public class Nya { }", false),
            With("Foo.Bar.Fun", "[JSFunction] public static void OnFun (Nya.Nya nya) { }"));
        Execute();
        Contains("export namespace Nya {\n    export interface Nya {\n    }\n}");
        Contains("export namespace Fun {\n    export let onFun: (nya: Nya.Nya) => void;\n}");
    }

    [Fact]
    public void NumericsTranslatedToNumber ()
    {
        var nums = new[] { "byte", "sbyte", "ushort", "uint", "ulong", "short", "int", "decimal", "double", "float" };
        var csArgs = string.Join(", ", nums.Select(n => $"{n} v{Array.IndexOf(nums, n)}"));
        var tsArgs = string.Join(", ", nums.Select(n => $"v{Array.IndexOf(nums, n)}: number"));
        AddAssembly(With($"[JSInvokable] public static void Num ({csArgs}) {{ }}"));
        Execute();
        Contains($"num({tsArgs})");
    }

    [Fact]
    public void Int64TranslatedToBigInt ()
    {
        AddAssembly(With("[JSInvokable] public static void Foo (long bar) {}"));
        Execute();
        Contains("foo(bar: bigint): void");
    }

    [Fact]
    public void TaskTranslatedToPromise ()
    {
        AddAssembly(
            With("[JSInvokable] public static Task<bool> AsyBool () => default;"),
            With("[JSInvokable] public static Task AsyVoid () => default;"));
        Execute();
        Contains("asyBool(): Promise<boolean>");
        Contains("asyVoid(): Promise<void>");
    }

    [Fact]
    public void CharAndStringTranslatedToString ()
    {
        AddAssembly(With("[JSInvokable] public static void Cha (char c, string s) {}"));
        Execute();
        Contains("cha(c: string, s: string): void");
    }

    [Fact]
    public void BoolTranslatedToBoolean ()
    {
        AddAssembly(With("[JSInvokable] public static void Boo (bool b) {}"));
        Execute();
        Contains("boo(b: boolean): void");
    }

    [Fact]
    public void DateTimeTranslatedToDate ()
    {
        AddAssembly(With("[JSInvokable] public static void Doo (DateTime time) {}"));
        Execute();
        Contains("doo(time: Date): void");
    }

    [Fact]
    public void ListAndArrayTranslatedToArray ()
    {
        AddAssembly(With("[JSInvokable] public static List<string> Goo (DateTime[] d) => default;"));
        Execute();
        Contains("goo(d: Array<Date>): Array<string>");
    }

    [Fact]
    public void JaggedArrayAndListOfListsTranslatedToArrayOfArrays ()
    {
        AddAssembly(With("[JSInvokable] public static List<List<string>> Goo (DateTime[][] d) => default;"));
        Execute();
        Contains("goo(d: Array<Array<Date>>): Array<Array<string>>");
    }

    [Fact]
    public void IntArraysTranslatedToRelatedTypes ()
    {
        AddAssembly(
            With("[JSInvokable] public static void Uint8 (byte[] foo) {}"),
            With("[JSInvokable] public static void Int8 (sbyte[] foo) {}"),
            With("[JSInvokable] public static void Uint16 (ushort[] foo) {}"),
            With("[JSInvokable] public static void Int16 (short[] foo) {}"),
            With("[JSInvokable] public static void Uint32 (uint[] foo) {}"),
            With("[JSInvokable] public static void Int32 (int[] foo) {}"),
            With("[JSInvokable] public static void BigInt64 (long[] foo) {}")
        );
        Execute();
        Contains("uint8(foo: Uint8Array): void");
        Contains("int8(foo: Int8Array): void");
        Contains("uint16(foo: Uint16Array): void");
        Contains("int16(foo: Int16Array): void");
        Contains("uint32(foo: Uint32Array): void");
        Contains("int32(foo: Int32Array): void");
        Contains("bigInt64(foo: BigInt64Array): void");
    }

    [Fact]
    public void DefinitionIsGeneratedForObjectType ()
    {
        AddAssembly(
            With("n", "public class Foo { public string S { get; set; } public int I { get; set; } }"),
            With("n", "[JSInvokable] public static Foo Method (Foo t) => default;"));
        Execute();
        Matches(@"export interface Foo {\s*s: string;\s*i: number;\s*}");
        Contains("method(t: n.Foo): n.Foo");
    }

    [Fact]
    public void DefinitionIsGeneratedForInterfaceAndImplementation ()
    {
        AddAssembly(
            With("n", "public interface Interface { Interface Foo { get; } void Bar (Interface b); }"),
            With("n", "public class Base { }"),
            With("n", "public class Derived : Base, Interface { public Interface Foo { get; } public void Bar (Interface b) {} }"),
            With("n", "[JSInvokable] public static Derived Method (Interface b) => default;"));
        Execute();
        Matches(@"export interface Interface {\s*foo: n.Interface;\s*}");
        Matches(@"export interface Base {\s*}");
        Matches(@"export interface Derived extends n.Base, n.Interface {\s*foo: n.Interface;\s*}");
        Contains("method(b: n.Interface): n.Derived");
    }

    [Fact]
    public void DefinitionIsGeneratedForTypeWithListProperty ()
    {
        AddAssembly(
            With("n", "public interface Item { }"),
            With("n", "public class Container { public List<Item> Items { get; } }"),
            With("n", "[JSInvokable] public static Container Combine (List<Item> items) => default;"));
        Execute();
        Matches(@"export interface Item {\s*}");
        Matches(@"export interface Container {\s*items: Array<n.Item>;\s*}");
        Contains("combine(items: Array<n.Item>): n.Container");
    }

    [Fact]
    public void DefinitionIsGeneratedForTypeWithJaggedArrayProperty ()
    {
        AddAssembly(
            With("n", "public interface Item { }"),
            With("n", "public class Container { public Item[][] Items { get; } }"),
            With("n", "[JSInvokable] public static Container Get () => default;"));
        Execute();
        Matches(@"export interface Item {\s*}");
        Matches(@"export interface Container {\s*items: Array<Array<n.Item>>;\s*}");
        Contains("get(): n.Container");
    }

    [Fact]
    public void DefinitionIsGeneratedForTypeWithReadOnlyListProperty ()
    {
        AddAssembly(
            With("n", "public interface Item { }"),
            With("n", "public class Container { public IReadOnlyList<Item> Items { get; } }"),
            With("n", "[JSInvokable] public static Container Combine (IReadOnlyList<Item> items) => default;"));
        Execute();
        Matches(@"export interface Item {\s*}");
        Matches(@"export interface Container {\s*items: Array<n.Item>;\s*}");
        Contains("combine(items: Array<n.Item>): n.Container");
    }

    [Fact]
    public void DefinitionIsGeneratedForTypeWithDictionaryProperty ()
    {
        AddAssembly(
            With("n", "public interface Item { }"),
            With("n", "public class Container { public Dictionary<string, Item> Items { get; } }"),
            With("n", "[JSInvokable] public static Container Combine (Dictionary<string, Item> items) => default;"));
        Execute();
        Matches(@"export interface Item {\s*}");
        Matches(@"export interface Container {\s*items: Map<string, n.Item>;\s*}");
        Contains("combine(items: Map<string, n.Item>): n.Container");
    }

    [Fact]
    public void DefinitionIsGeneratedForTypeWithReadOnlyDictionaryProperty ()
    {
        AddAssembly(
            With("n", "public interface Item { }"),
            With("n", "public class Container { public IReadOnlyDictionary<string, Item> Items { get; } }"),
            With("n", "[JSInvokable] public static Container Combine (IReadOnlyDictionary<string, Item> items) => default;"));
        Execute();
        Matches(@"export interface Item {\s*}");
        Matches(@"export interface Container {\s*items: Map<string, n.Item>;\s*}");
        Contains("combine(items: Map<string, n.Item>): n.Container");
    }

    [Fact]
    public void DefinitionIsGeneratedForGenericClass ()
    {
        AddAssembly(
            With("n", "public class GenericClass<T> { public T Value { get; set; } }"),
            With("n", "[JSInvokable] public static void Method (GenericClass<string> p) { }"));
        Execute();
        Matches(@"export interface GenericClass<T> {\s*value: T;\s*}");
        Contains("method(p: n.GenericClass<string>): void");
    }

    [Fact]
    public void DefinitionIsGeneratedForGenericInterface ()
    {
        AddAssembly(
            With("n", "public interface GenericInterface<T> { public T Value { get; set; } }"),
            With("n", "[JSInvokable] public static GenericInterface<string> Method () => default;"));
        Execute();
        Matches(@"export interface GenericInterface<T> {\s*value: T;\s*}");
        Contains("method(): n.GenericInterface<string>");
    }

    [Fact]
    public void DefinitionIsGeneratedForNestedGenericTypes ()
    {
        AddAssembly(
            With("Foo", "public class GenericClass<T> { public T Value { get; set; } }", false),
            With("Bar", "public interface GenericInterface<T> { public T Value { get; set; } }", false),
            With("n", "[JSInvokable] public static void Method (Foo.GenericClass<Bar.GenericInterface<string>> p) { }"));
        Execute();
        Matches(@"export namespace Foo {\s*export interface GenericClass<T> {\s*value: T;\s*}\s*}");
        Matches(@"export namespace Bar {\s*export interface GenericInterface<T> {\s*value: T;\s*}\s*}");
        Contains("method(p: Foo.GenericClass<Bar.GenericInterface<string>>): void");
    }

    [Fact]
    public void DefinitionIsGeneratedForGenericClassWithMultipleTypeArguments ()
    {
        AddAssembly(
            With("n", "public class GenericClass<T1, T2> { public T1 Key { get; set; } public T2 Value { get; set; } }"),
            With("n", "[JSInvokable] public static void Method (GenericClass<string, int> p) { }"));
        Execute();
        Matches(@"export interface GenericClass<T1, T2> {\s*key: T1;\s*value: T2;\s*}");
        Contains("method(p: n.GenericClass<string, number>): void");
    }

    [Fact]
    public void CanCrawlCustomTypes ()
    {
        AddAssembly(
            With("n", "public struct Struct { public double A { get; set; } }"),
            With("n", "public readonly struct ReadonlyStruct { public double A { get; init; } }"),
            With("n", "public readonly record struct ReadonlyRecordStruct(double A);"),
            With("n", "public record class RecordClass(double A);"),
            With("n", "public enum Enum { A, B }"),
            With("n", "public class Foo { public Struct S { get; } public ReadonlyStruct Rs { get; } }"),
            With("n", "public class Bar : Foo { public ReadonlyRecordStruct Rrs { get; } public RecordClass Rc { get; } }"),
            With("n", "public class Baz { public List<Bar> Bars { get; } public Enum E { get; } }"),
            With("n", "[JSInvokable] public static Baz GetBaz () => default;"));
        Execute();
        Matches(@"export interface Struct {\s*a: number;\s*}");
        Matches(@"export interface ReadonlyStruct {\s*a: number;\s*}");
        Matches(@"export interface ReadonlyRecordStruct {\s*a: number;\s*}");
        Matches(@"export interface RecordClass {\s*a: number;\s*}");
        Matches(@"export enum Enum {\s*A,\s*B\s*}");
        Matches(@"export interface Foo {\s*s: n.Struct;\s*rs: n.ReadonlyStruct;\s*}");
        Matches(@"export interface Bar extends n.Foo {\s*rrs: n.ReadonlyRecordStruct;\s*rc: n.RecordClass;\s*}");
        Matches(@"export interface Baz {\s*bars: Array<n.Bar>;\s*e: n.Enum;\s*}");
    }

    [Fact]
    public void OtherTypesAreTranslatedToAny ()
    {
        AddAssembly(With("[JSInvokable] public static DBNull Method (IEnumerable<string> t) => default;"));
        Execute();
        Contains("method(t: any): any");
    }

    [Fact]
    public void StaticPropertiesAreNotIncluded ()
    {
        AddAssembly(
            With("public class Foo { public static string Soo { get; } }"),
            With("[JSInvokable] public static Foo Bar () => default;"));
        Execute();
        Matches(@"export interface Foo {\s*}");
    }

    [Fact]
    public void ExpressionPropertiesAreNotIncluded ()
    {
        AddAssembly(
            With("public class Foo { public bool Boo => true; }"),
            With("[JSInvokable] public static Foo Bar () => default;"));
        Execute();
        Matches(@"export interface Foo {\s*}");
    }

    [Fact]
    public void NullableMethodArgumentsUnionWithUndefined ()
    {
        AddAssembly(
            With("[JSInvokable] public static void Foo (string? bar) { }"),
            With("[JSFunction] public static void Fun (int? nya) { }")
        );
        Execute();
        Contains("export function foo(bar: string | undefined): void;");
        Contains("export let fun: (nya: number | undefined) => void;");
    }

    [Fact]
    public void NullableMethodReturnTypesUnionWithNull ()
    {
        AddAssembly(
            With("[JSInvokable] public static string? Foo () => default;"),
            With("[JSInvokable] public static Task<byte[]?> Bar () => default;"),
            With("[JSFunction] public static ValueTask<List<string>?> Nya () => default;")
        );
        Execute();
        Contains("export function foo(): string | null;");
        Contains("export function bar(): Promise<Uint8Array | null>;");
        Contains("export let nya: () => Promise<Array<string> | null>;");
    }

    [Fact]
    public void NullableCollectionElementTypesUnionWithNull ()
    {
        AddAssembly(
            With("public class Foo { }"),
            With("[JSFunction] public static List<Foo?>? Fun (int?[]? bar, Foo[]?[]? nya, Foo?[]?[]? far) => default;")
        );
        Execute();
        Contains("export let fun: (bar: Array<number | null> | undefined," +
                 " nya: Array<Array<Global.Foo> | null> | undefined," +
                 " far: Array<Array<Global.Foo | null> | null> | undefined) =>" +
                 " Array<Global.Foo | null> | null;");
    }

    [Fact]
    public void NullableCollectionElementTypesOfCustomTypeUnionWithNull ()
    {
        AddAssembly(
            With("public interface IFoo<T> { }"),
            With("public record Foo (List<List<IFoo<string>?>?>? Bar, IFoo<int>?[]?[]? Nya) : IFoo<bool>;"),
            With("[JSFunction] public static IFoo<bool> Fun (Foo foo) => default;")
        );
        Execute();
        Contains(@"bar?: Array<Array<Global.IFoo<string> | null> | null>;");
        Contains(@"nya?: Array<Array<Global.IFoo<number> | null> | null>;");
    }

    [Fact]
    public void NullablePropertiesHaveOptionalModificator ()
    {
        AddAssembly(
            With("n", "public class Foo { public bool? Bool { get; } }"),
            With("n", "public class Bar { public Foo? Foo { get; } }"),
            With("n", "[JSInvokable] public static Foo FooBar (Bar bar) => default;"));
        Execute();
        Matches(@"export interface Foo {\s*bool\?: boolean;\s*}");
        Matches(@"export interface Bar {\s*foo\?: n.Foo;\s*}");
    }

    [Fact]
    public void NullableEnumsAreCrawled ()
    {
        AddAssembly(
            With("n", "public enum Foo { A, B }"),
            With("n", "public class Bar { public Foo? Foo { get; } }"),
            With("n", "[JSInvokable] public static Bar GetBar () => default;"));
        Execute();
        Matches(@"export enum Foo {\s*A,\s*B\s*}");
        Matches(@"export interface Bar {\s*foo\?: n.Foo;\s*}");
    }

    [Fact]
    public void WhenTypeReferencedMultipleTimesItsDeclaredOnlyOnce ()
    {
        AddAssembly(
            With("public interface Foo { }"),
            With("public class Bar : Foo { public Foo Foo { get; } }"),
            With("public class Far : Bar { public Bar Bar { get; } }"),
            With("[JSInvokable] public static Bar TakeFooGiveBar (Foo f) => default;"),
            With("[JSInvokable] public static Foo TakeBarGiveFoo (Bar b) => default;"),
            With("[JSInvokable] public static Far TakeAllGiveFar (Foo f, Bar b, Far ff) => default;"));
        Execute();
        Assert.Single(Matches("export interface Foo"));
        Assert.Single(Matches("export interface Bar"));
        Assert.Single(Matches("export interface Far"));
    }
}