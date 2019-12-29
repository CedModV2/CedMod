// Decompiled with JetBrains decompiler
// Type: NewInput
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class NewInput
{
  public static readonly Dictionary<string, KeyCode> Bindings = new Dictionary<string, KeyCode>();
  private static readonly string Path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/keybinding.txt";
  private const string DefaultBinding = "Shoot:323;Zoom:324;Jump:32;Interact:101;Inventory:9;Reload:114;Run:304;Voice Chat:113;Sneak:99;Move Forward:119;Move Backward:115;Move Left:97;Move Right:100;Player List:110;Character Info:282;Remote Admin:109;Toggle flashlight:102;Alt Voice Chat:118";

  static NewInput()
  {
    NewInput.Load();
  }

  public static KeyCode GetKey(string axis)
  {
    KeyCode keyCode;
    if (NewInput.Bindings.TryGetValue(axis, out keyCode))
      return keyCode;
    Debug.LogError((object) ("Key axis '" + axis + "' does not exist."));
    return KeyCode.None;
  }

  public static void ChangeKey(string axis, KeyCode code)
  {
    NewInput.Bindings[axis] = code;
    NewInput.Save();
  }

  public static void Save()
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (KeyValuePair<string, KeyCode> binding in NewInput.Bindings)
    {
      stringBuilder.Append(binding.Key);
      stringBuilder.Append(':');
      stringBuilder.Append((int) binding.Value);
      stringBuilder.Append(';');
    }
    File.WriteAllText(NewInput.Path, stringBuilder.ToString(0, stringBuilder.Length - 1));
  }

  private static void CheckForNewBindings()
  {
    foreach (KeyValuePair<string, KeyCode> keyValuePair in ((IEnumerable<string>) "Shoot:323;Zoom:324;Jump:32;Interact:101;Inventory:9;Reload:114;Run:304;Voice Chat:113;Sneak:99;Move Forward:119;Move Backward:115;Move Left:97;Move Right:100;Player List:110;Character Info:282;Remote Admin:109;Toggle flashlight:102;Alt Voice Chat:118".Split(';')).Select<string, KeyValuePair<string, KeyCode>>((Func<string, KeyValuePair<string, KeyCode>>) (item => new KeyValuePair<string, KeyCode>(item.Split(':')[0], (KeyCode) int.Parse(item.Split(':')[1])))))
    {
      if (!NewInput.Bindings.ContainsKey(keyValuePair.Key))
        NewInput.Bindings.Add(keyValuePair.Key, keyValuePair.Value);
    }
  }

  public static void Load()
  {
    try
    {
      NewInput.Bindings.Clear();
      if (!File.Exists(NewInput.Path))
        NewInput.Reset();
      string str1 = File.ReadAllText(NewInput.Path);
      if (!str1.Contains(";"))
      {
        NewInput.Reset();
        str1 = "Shoot:323;Zoom:324;Jump:32;Interact:101;Inventory:9;Reload:114;Run:304;Voice Chat:113;Sneak:99;Move Forward:119;Move Backward:115;Move Left:97;Move Right:100;Player List:110;Character Info:282;Remote Admin:109;Toggle flashlight:102;Alt Voice Chat:118";
      }
      string str2 = str1;
      char[] chArray = new char[1]{ ';' };
      foreach (string str3 in str2.Split(chArray))
        NewInput.Bindings.Add(str3.Split(':')[0], (KeyCode) int.Parse(str3.Split(':')[1]));
      NewInput.CheckForNewBindings();
    }
    catch
    {
      NewInput.Reset();
    }
  }

  public static void Reset()
  {
    Debug.Log((object) "Resetting!");
    File.WriteAllText(NewInput.Path, "Shoot:323;Zoom:324;Jump:32;Interact:101;Inventory:9;Reload:114;Run:304;Voice Chat:113;Sneak:99;Move Forward:119;Move Backward:115;Move Left:97;Move Right:100;Player List:110;Character Info:282;Remote Admin:109;Toggle flashlight:102;Alt Voice Chat:118");
    NewInput.Load();
  }
}
