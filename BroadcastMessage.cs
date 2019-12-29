// Decompiled with JetBrains decompiler
// Type: BroadcastMessage
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

[Serializable]
public class BroadcastMessage
{
  public string Text;
  public uint Time;
  public bool MonoSpaced;

  public BroadcastMessage(string content, uint t, bool mono)
  {
    this.Text = content;
    this.Time = t;
    this.MonoSpaced = mono;
  }
}
