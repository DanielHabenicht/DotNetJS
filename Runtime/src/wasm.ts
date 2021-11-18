﻿export let wasm: DotNetModule;

export interface DotNetModule extends EmscriptenModule {
    MONO: any,
    BINDING: any,
    ccall(ident: string,
          returnType: Emscripten.JSType | null,
          argTypes: Emscripten.JSType[],
          args: Emscripten.TypeCompatibleWithC[],
          opts?: Emscripten.CCallOpts): any;
}

export function initializeWasm(wasmBinary: Uint8Array): Promise<void> {
    return new Promise<void>(resolve => {
        // "Module" global is expected in the emscripten's autogenerated js wrapper.
        global["Module"] = wasm = {
            wasmBinary: wasmBinary,
            onRuntimeInitialized: resolve
        } as any;
        require("../runtime/artifacts/bin/native/net6.0-Browser-Release-wasm/dotnet.js")(wasm);
    });
}

export function destroyWasm(): void {
    // EM.ccall("emscripten_force_exit", null, ["number"], [0]);
    // TODO: Find out how to destroy emscripten module (or if it's necessary).
}