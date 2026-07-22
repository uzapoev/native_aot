using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

[Generator]
public class NativeExportGenerator : ISourceGenerator
{
  public void Initialize(GeneratorInitializationContext context)
  {
    context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
  }

  public void Execute(GeneratorExecutionContext context)
  {
    if (!System.Diagnostics.Debugger.IsAttached)
    {
      //System.Diagnostics.Debugger.Launch();
    }
    var receiver = context.SyntaxReceiver as SyntaxReceiver;
    if (receiver == null) return;

    var cppHeader = new StringBuilder();
    var csBindings = new StringBuilder();
    
    context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.targetdir", out var targetdir);

    cppHeader.AppendLine("// ================================================");
    cppHeader.AppendLine("// NativeAOT Scripting API - Auto Generated");
    cppHeader.AppendLine("// ================================================");

    cppHeader.AppendLine("#pragma once");
    cppHeader.AppendLine();
    cppHeader.AppendLine("#if defined(NATIVEAOT_STATIC_LIB)");
    cppHeader.AppendLine("// ==================== STATIC LINKING ====================");
    cppHeader.AppendLine("extern \"C\"");
    cppHeader.AppendLine("{");

    csBindings.AppendLine("// Auto-generated NativeAOT bindings");
    csBindings.AppendLine("using System.Runtime.InteropServices;\n");

    foreach (var methodDecl in receiver.CandidateMethods)
    {
      var semanticModel = context.Compilation.GetSemanticModel(methodDecl.SyntaxTree);
      var method = semanticModel.GetDeclaredSymbol(methodDecl) as IMethodSymbol;
      if (method == null) continue;

      var attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "NativeExportAttribute");
      if (attr == null)
        attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "UnmanagedCallersOnlyAttribute");

      if (attr == null) continue;

      string entryPoint = (string)attr.NamedArguments.FirstOrDefault(na => na.Key == "EntryPoint").Value.Value ?? method.Name;

      // C++ Static
      cppHeader.AppendLine($"    {GetCppReturnType(method)} {entryPoint}({GetCppParameters(method)});");

      // C# Binding
      csBindings.AppendLine($"\n[UnmanagedCallersOnly(EntryPoint = \"{entryPoint}\")]");
      csBindings.AppendLine($"public static {method.ReturnType.ToDisplayString()} {method.Name}Native({GetCsParameters(method)})");
      csBindings.AppendLine($"    => {method.ContainingType.Name}.{method.Name}({GetCallArguments(method)});");
    }

    cppHeader.AppendLine("}");
    cppHeader.AppendLine();
    cppHeader.AppendLine("#else");
    cppHeader.AppendLine("// ==================== DYNAMIC LOADING ====================");

    // Dynamic structure
    cppHeader.AppendLine("struct ScriptingApi");
    cppHeader.AppendLine("{");
    foreach (var methodDecl in receiver.CandidateMethods)
    {
      var method = context.Compilation.GetSemanticModel(methodDecl.SyntaxTree).GetDeclaredSymbol(methodDecl) as IMethodSymbol;
      if (method == null) continue;

      var attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "NativeExportAttribute");
      if (attr == null)
        attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "UnmanagedCallersOnlyAttribute");

      if (attr == null) continue;

      string entryPoint = (string)attr.NamedArguments.FirstOrDefault(na => na.Key == "EntryPoint").Value.Value ?? method.Name;
      string funcName = method.Name;

      cppHeader.AppendLine($"    {GetCppReturnType(method)} (*{entryPoint}) ({GetCppParameters(method)}) = nullptr; // {funcName}");
    }
    cppHeader.AppendLine("};");
    cppHeader.AppendLine("#endif");

    var headerPath = Path.Combine(targetdir, $"{context.Compilation.AssemblyName}.h");
    File.WriteAllText(headerPath, cppHeader.ToString(), Encoding.UTF8);
  }

  private string GetCppReturnType(IMethodSymbol m) => m.ReturnType.ToDisplayString().Replace("System.", "");
  private string GetCppParameters(IMethodSymbol m) => string.Join(", ", m.Parameters.Select(p => GetTypeName(p.Type) + " " + p.Name));
  private string GetCsParameters(IMethodSymbol m) => string.Join(", ", m.Parameters.Select(p => p.Type.ToDisplayString() + " " + p.Name));
  private string GetCallArguments(IMethodSymbol m) => string.Join(", ", m.Parameters.Select(p => p.Name));


  private string GetTypeName(ITypeSymbol type)
  {
    // add mapping here (Vector3 -> struct Vector3 etc.)
    var typeMapping = new Dictionary<string, string>
    {
        { "nint", "intptr_t" },
        { "System.IntPtr", "intptr_t" },
        { "System.UIntPtr", "uintptr_t" },
        { "System.Numerics.Vector3", "struct vec3" },
        { "System.Int32", "int32_t" },
        { "System.Int64", "int64_t" }
    };

    string name = type.ToDisplayString();
    if (typeMapping.TryGetValue(name, out string mappedName))
      return mappedName;

    return name.Replace("System.", "");
  }
}

class SyntaxReceiver : ISyntaxReceiver
{
    public List<MethodDeclarationSyntax> CandidateMethods { get; } = new();
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is MethodDeclarationSyntax mds && mds.AttributeLists.Count > 0)
            CandidateMethods.Add(mds);
    }
}