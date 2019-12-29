// Decompiled with JetBrains decompiler
// Type: HeadlessCallbacks
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class HeadlessCallbacks : Attribute
{
  private static IEnumerable callbackRegistry;

  public static void FindCallbacks()
  {
    if (HeadlessCallbacks.callbackRegistry != null)
      return;
    try
    {
      HeadlessCallbacks.callbackRegistry = (IEnumerable) ((IEnumerable<System.Type>) Assembly.GetExecutingAssembly().GetTypes()).Select(t => new
      {
        t = t,
        attributes = t.GetCustomAttributes(typeof (HeadlessCallbacks), true)
      }).Where(_param1 => _param1.attributes != null && (uint) _param1.attributes.Length > 0U).Select(_param1 => _param1.t);
    }
    catch (ReflectionTypeLoadException ex1)
    {
      try
      {
        HeadlessCallbacks.callbackRegistry = (IEnumerable) ((IEnumerable<System.Type>) ex1.Types).Where<System.Type>((Func<System.Type, bool>) (t => t != (System.Type) null));
      }
      catch (Exception ex2)
      {
        Debug.Log((object) ("Headless Builder could not find callbacks (" + ex2.GetType().Name + "), but will still continue as planned"));
        HeadlessCallbacks.callbackRegistry = (IEnumerable) Enumerable.Empty<System.Type>();
      }
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("Headless Builder could not find callbacks (" + ex.GetType().Name + "), but will still continue as planned"));
      HeadlessCallbacks.callbackRegistry = (IEnumerable) Enumerable.Empty<System.Type>();
    }
  }

  public static void InvokeCallbacks(string callbackName)
  {
    HeadlessCallbacks.FindCallbacks();
    foreach (System.Type type in HeadlessCallbacks.callbackRegistry)
    {
      MethodInfo method = type.GetMethod(callbackName);
      if (method != (MethodInfo) null)
      {
        try
        {
          method.Invoke((object) type, (object[]) null);
        }
        catch (Exception ex)
        {
          Debug.LogError((object) ex);
        }
      }
    }
  }
}
