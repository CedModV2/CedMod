// Decompiled with JetBrains decompiler
// Type: AlphaWarheadOutsitePanel
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class AlphaWarheadOutsitePanel : NetworkBehaviour
{
  public Animator panelButtonCoverAnim;
  public static AlphaWarheadNukesitePanel nukeside;
  private static AlphaWarheadController _host;
  public GameObject[] inevitable;
  [SyncVar]
  public bool keycardEntered;

  private void Update()
  {
    if ((Object) AlphaWarheadOutsitePanel._host == (Object) null)
    {
      AlphaWarheadOutsitePanel._host = AlphaWarheadController.Host;
    }
    else
    {
      this.transform.localPosition = new Vector3(0.0f, 0.0f, 9f);
      foreach (GameObject gameObject in this.inevitable)
        gameObject.SetActive((double) AlphaWarheadOutsitePanel._host.timeToDetonation <= 10.0 && (double) AlphaWarheadOutsitePanel._host.timeToDetonation > 0.0);
      this.panelButtonCoverAnim.SetBool("enabled", this.keycardEntered);
    }
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkkeycardEntered
  {
    get
    {
      return this.keycardEntered;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.keycardEntered, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.keycardEntered);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.keycardEntered);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkkeycardEntered = reader.ReadBoolean();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworkkeycardEntered = reader.ReadBoolean();
    }
  }
}
