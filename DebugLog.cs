// Decompiled with JetBrains decompiler
// Type: DebugLog
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public static class DebugLog
{
  [MethodImpl((MethodImplOptions) 256)]
  public static void Log(string text)
  {
    UnityEngine.Debug.Log((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void Log(object text)
  {
    UnityEngine.Debug.Log(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarning(string text)
  {
    UnityEngine.Debug.LogWarning((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarning(object text)
  {
    UnityEngine.Debug.LogWarning(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogError(string text)
  {
    UnityEngine.Debug.LogError((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogError(object text)
  {
    UnityEngine.Debug.LogError(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogException(Exception exception)
  {
    UnityEngine.Debug.LogException(exception);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogEditor(string text)
  {
    UnityEngine.Debug.Log((object) text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogEditor(object text)
  {
    UnityEngine.Debug.Log(text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarningEditor(string text)
  {
    UnityEngine.Debug.LogWarning((object) text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarningEditor(object text)
  {
    UnityEngine.Debug.LogWarning(text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogErrorEditor(string text)
  {
    UnityEngine.Debug.LogError((object) text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogErrorEditor(object text)
  {
    UnityEngine.Debug.LogError(text);
  }

  [Conditional("UNITY_EDITOR")]
  [MethodImpl((MethodImplOptions) 256)]
  public static void LogExceptionEditor(Exception exception)
  {
    UnityEngine.Debug.LogException(exception);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogBuild(string text)
  {
    UnityEngine.Debug.Log((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogBuild(object text)
  {
    UnityEngine.Debug.Log(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarningBuild(string text)
  {
    UnityEngine.Debug.LogWarning((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogWarningBuild(object text)
  {
    UnityEngine.Debug.LogWarning(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogErrorBuild(string text)
  {
    UnityEngine.Debug.LogError((object) text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogErrorBuild(object text)
  {
    UnityEngine.Debug.LogError(text);
  }

  [MethodImpl((MethodImplOptions) 256)]
  public static void LogExceptionBuild(Exception exception)
  {
    UnityEngine.Debug.LogException(exception);
  }
}
