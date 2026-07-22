using System;
using System.Runtime.InteropServices;

namespace MyGameScripting;

public static class NativeExports
{
  public delegate void UpdateCallback(float deltaTime);
  private static UpdateCallback? s_updateCallback;

  [NativeExport("nativeInit")]
  public static void NativeInit() { }


  [UnmanagedCallersOnly(EntryPoint = "csharp_init")]
  public static int Init()
  {
    Console.WriteLine("[C#] NativeAOT initialized");
    return 0;
  }


  [UnmanagedCallersOnly(EntryPoint = "csharp_register_update")]
  public static void RegisterUpdateCallback(IntPtr funcPtr)
  {
    s_updateCallback = Marshal.GetDelegateForFunctionPointer<UpdateCallback>(funcPtr);
  }


  [UnmanagedCallersOnly(EntryPoint = "csharp_update")]
  public static void Update(float deltaTime)
  {
    s_updateCallback?.Invoke(deltaTime);
  }

  [UnmanagedCallersOnly(EntryPoint = "csharp_shutdown")]
  public static void Shutdown()
  {
    Console.WriteLine("[C#] Shutting down");
  }
}