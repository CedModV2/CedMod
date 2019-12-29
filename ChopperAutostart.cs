// Decompiled with JetBrains decompiler
// Type: ChopperAutostart
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class ChopperAutostart : NetworkBehaviour
{
  [SyncVar(hook = "SetState")]
  public bool isLanded = true;

  public void SetState(bool b)
  {
    this.NetworkisLanded = b;
    this.RefreshState();
  }

  private void Start()
  {
    this.RefreshState();
  }

  private void RefreshState()
  {
    this.GetComponent<Animator>().SetBool("IsLanded", this.isLanded);
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkisLanded
  {
    get
    {
      return this.isLanded;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetState(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.isLanded, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.isLanded);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.isLanded);
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
      this.SetState(b);
      this.NetworkisLanded = b;
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      bool b = reader.ReadBoolean();
      this.SetState(b);
      this.NetworkisLanded = b;
    }
  }
}
