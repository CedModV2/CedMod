// Decompiled with JetBrains decompiler
// Type: TextChat
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class TextChat : NetworkBehaviour
{
  private List<GameObject> msgs = new List<GameObject>();

  private void Start()
  {
  }

  private void SendChat(string msg, string nick, Vector3 position)
  {
    this.CmdSendChat(msg, nick, position);
  }

  private void CmdSendChat(string msg, string nick, Vector3 pos)
  {
  }

  private void RpcSendChat(string msg, string nick, Vector3 pos)
  {
  }

  private void AddMsg(string msg, string nick)
  {
  }

  private void MirrorProcessed()
  {
  }
}
