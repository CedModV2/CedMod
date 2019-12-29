// Decompiled with JetBrains decompiler
// Type: CmdBinding
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CmdBinding : MonoBehaviour
{
  public static readonly List<CmdBinding.Bind> Bindings = new List<CmdBinding.Bind>();

  static CmdBinding()
  {
    CmdBinding.Load();
  }

  private void Update()
  {
  }

  public static void KeyBind(KeyCode code, string cmd)
  {
    foreach (CmdBinding.Bind binding in CmdBinding.Bindings)
    {
      if (binding.key == code)
      {
        binding.command = cmd;
        CmdBinding.Save();
        return;
      }
    }
    CmdBinding.Bindings.Add(new CmdBinding.Bind()
    {
      command = cmd,
      key = code
    });
  }

  public static void Save()
  {
    string str = "";
    for (int index = 0; index < CmdBinding.Bindings.Count; ++index)
    {
      str = str + (object) (int) CmdBinding.Bindings[index].key + ":" + CmdBinding.Bindings[index].command;
      if (index != CmdBinding.Bindings.Count - 1)
        str += Environment.NewLine;
    }
    StreamWriter streamWriter = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt");
    streamWriter.WriteLine(str);
    streamWriter.Close();
  }

  public static void Load()
  {
    GameCore.Console.AddLog("Loading cmd bindings...", Color.grey, false);
    try
    {
      CmdBinding.Bindings.Clear();
      if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt"))
        CmdBinding.Revent();
      StreamReader streamReader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt");
      string str;
      while ((str = streamReader.ReadLine()) != null)
      {
        if (!string.IsNullOrEmpty(str) && str.Contains(":"))
          CmdBinding.Bindings.Add(new CmdBinding.Bind()
          {
            command = str.Split(':')[1],
            key = (KeyCode) int.Parse(str.Split(':')[0])
          });
      }
      streamReader.Close();
    }
    catch (Exception ex)
    {
      Debug.Log((object) ("REVENT: " + ex.StackTrace + " - " + ex.Message));
      CmdBinding.Revent();
    }
  }

  private static void Revent()
  {
    Debug.Log((object) "Reventing!");
    new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/cmdbinding.txt").Close();
  }

  public class Bind
  {
    public string command;
    public KeyCode key;
  }
}
