# Native AOT CoreCLR C++ Scripting Integration

This repository demonstrates how to seamlessly integrate managed **C# (Native AOT)** code into a native **C++/CMake** application or game engine. It includes automated export table generation and a guide for configuring high-performance, seamless native debugging.

---

## 🏗️ Project Structure

* **`engine/`** — Native C++ project (the host / game engine).
* **`scripting/`** — C# project (.NET CoreCLR) compiled via Native AOT into a native dynamic library (`.dll` / `.so`).
* **`scripting/NativeExportGenerator/`** — Roslyn Source Generator that scans C# code and automatically exports bindings, generating C++ headers (`.h`).
* **`external/`** — Output folder for compiled binaries (`.dll`, `.lib`, `.pdb`) used by the CMake linker.

---

## 🚀 How It Works

1. **Codegen (Roslyn Source Generator)**: During C# compilation, the source generator automatically dumps the generated C++ header (`scripting_api.h`) directly into the target output directory using MSBuild properties.
2. **Native AOT Compilation**: C# code is compiled ahead-of-time (AOT) straight into machine code. This produces a lightweight native binary without requiring a heavy .NET Runtime distribution.
3. **Static Linking in CMake**: On Windows, CMake binds the generated `.dll` and `.lib` files, allowing you to invoke C# functions in C++ as standard C-style functions using `extern "C"`.

---

## 🛠️ Configuration Details & Fixes

## 🪲 Setting Up Seamless Native Debugging

One of the biggest advantages of this setup is the ability to **place breakpoints directly inside C# files** and step into them (F11) straight from your C++ code. 

1. **Generate Native Symbols in C# (`.csproj`)**:
   ```xml
   <PropertyGroup Condition="'$(Configuration)' == 'Debug'">
     <StripSymbols>false</StripSymbols>
     <OptimizationMode>Debug</OptimizationMode>
   </PropertyGroup>
   ```
2. **PDB Copying via CMake**: The `MyGameScripting.pdb` file is copied right next to the engine's executable output.
3. **Debugger Settings**: In Visual Studio, navigate to your *C++ Project Properties -> Debugging -> Debugger Type* and set it to **Native Only** (or **Mixed**). When launching the C++ app, the debugger automatically maps the native PDB to your `.cs` source files.

### 💡 Catching Hidden Runtime Exceptions
Because native debuggers cannot catch managed .NET exceptions by default, you can trap Native AOT runtime crashes by setting a **Function Breakpoint** on:
```text
RhThrowEx
```
The debugger will instantly freeze execution on the exact C# line that triggered the exception.

---

## 💻 Building the Project

The solution is built using standard CMake commands:

```bash
mkdir build && cd build
cmake ..
cmake --build . --config Debug
```

*During the build process, CMake triggers `dotnet publish` behind the scenes, ensuring the AOT library is fully up to date before compiling the C++ engine.*
