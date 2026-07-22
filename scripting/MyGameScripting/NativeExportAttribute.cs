using System;
namespace MyGameScripting;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
public sealed class NativeExportAttribute : Attribute
{
  public NativeExportAttribute(string name) { EntryPoint = name; }

  public string? EntryPoint { get; set; }
}