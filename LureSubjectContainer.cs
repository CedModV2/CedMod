// Decompiled with JetBrains decompiler
// Type: LureSubjectContainer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class LureSubjectContainer : NetworkBehaviour
{
  private Vector3 position = new Vector3(-1471f, 160.5f, -3426.9f);
  private Vector3 rotation = new Vector3(0.0f, 180f, 0.0f);
  public float range;
  [SyncVar(hook = "SetState")]
  public bool allowContain;
  private CharacterClassManager ccm;
  [Space(10f)]
  public Transform hatch;
  public Vector3 closedPos;
  public Vector3 openPosition;
  private GameObject localplayer;

  public void SetState(bool b)
  {
    this.NetworkallowContain = b;
    if (!b)
      return;
    this.hatch.GetComponent<AudioSource>().Play();
  }

  private void Start()
  {
    this.transform.localPosition = this.position;
    this.transform.localRotation = Quaternion.Euler(this.rotation);
  }

  private void Update()
  {
    this.CheckForLure();
    this.hatch.localPosition = Vector3.Slerp(this.hatch.localPosition, this.allowContain ? this.closedPos : this.openPosition, Time.deltaTime * 3f);
  }

  private void CheckForLure()
  {
    if ((Object) this.ccm == (Object) null)
    {
      this.localplayer = PlayerManager.localPlayer;
      if (!((Object) this.localplayer != (Object) null))
        return;
      this.ccm = this.localplayer.GetComponent<CharacterClassManager>();
    }
    else
      this.GetComponent<BoxCollider>().enabled = this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP || this.ccm.GodMode;
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(this.transform.position, this.range);
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkallowContain
  {
    get
    {
      return this.allowContain;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetState(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<bool>(value, ref this.allowContain, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.allowContain);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.allowContain);
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
      this.NetworkallowContain = b;
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      bool b = reader.ReadBoolean();
      this.SetState(b);
      this.NetworkallowContain = b;
    }
  }
}
