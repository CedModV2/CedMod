// Decompiled with JetBrains decompiler
// Type: BlastDoor
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof (NetworkIdentity))]
public class BlastDoor : NetworkBehaviour
{
  [SyncVar(hook = "SetClosed")]
  public bool isClosed;

  public void SetClosed(bool b)
  {
    this.NetworkisClosed = b;
    if (!this.isClosed)
      return;
    this.GetComponent<Animator>().SetTrigger("Close");
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkisClosed
  {
    get
    {
      return this.isClosed;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetClosed(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.isClosed, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.isClosed);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.isClosed);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      bool b = reader.ReadBoolean();
      this.SetClosed(b);
      this.NetworkisClosed = b;
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      bool b = reader.ReadBoolean();
      this.SetClosed(b);
      this.NetworkisClosed = b;
    }
  }
}
