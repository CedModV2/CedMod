// Decompiled with JetBrains decompiler
// Type: LocalCurrentRoomEffects
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class LocalCurrentRoomEffects : NetworkBehaviour
{
  public float deltatimeScale = 3f;
  private Color curColor;
  public Color normalColor;
  public Color vhighColor;
  public static bool isVhigh;
  private CharacterClassManager ccm;
  private bool isInFlickerableRoom;
  private GameObject curRoom;
  private Transform lastCast;
  [SyncVar]
  public bool syncFlicker;

  private void Start()
  {
    this.ccm = this.GetComponent<CharacterClassManager>();
  }

  private void Update()
  {
    if (!this.isLocalPlayer && !NetworkServer.active)
      return;
    RaycastHit hitInfo;
    if (Physics.Raycast(new Ray(this.transform.position, Vector3.up), out hitInfo, 100f, (int) Interface079.singleton.roomDetectionMask) && (UnityEngine.Object) this.lastCast != (UnityEngine.Object) hitInfo.transform)
    {
      this.lastCast = hitInfo.transform;
      Transform transform = hitInfo.collider.transform;
      for (byte index = 0; index < (byte) 10 && (!((UnityEngine.Object) transform == (UnityEngine.Object) null) && !transform.transform.name.Contains("ROOT", StringComparison.OrdinalIgnoreCase)) && !(transform.gameObject.tag == "Room"); ++index)
        transform = transform.parent;
      if ((UnityEngine.Object) transform != (UnityEngine.Object) null)
        this.curRoom = transform.gameObject;
    }
    if (NetworkServer.active)
    {
      if ((UnityEngine.Object) this.curRoom != (UnityEngine.Object) null)
      {
        FlickerableLight componentInChildren = this.curRoom.GetComponentInChildren<FlickerableLight>();
        this.isInFlickerableRoom = (UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && componentInChildren.IsDisabled();
      }
      else
        this.isInFlickerableRoom = false;
      if (this.syncFlicker != this.isInFlickerableRoom)
        this.NetworksyncFlicker = this.isInFlickerableRoom;
    }
    if (!this.isLocalPlayer)
      return;
    Color b = LocalCurrentRoomEffects.isVhigh ? this.vhighColor : this.normalColor;
    bool flag = this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP;
    if (this.syncFlicker)
    {
      if (LocalCurrentRoomEffects.isVhigh)
      {
        if (!flag)
          b = Color.black;
      }
      else
        b = !flag ? Color.black : this.vhighColor;
    }
    RenderSettings.ambientLight = Color.Lerp(RenderSettings.ambientLight, b, Time.deltaTime * this.deltatimeScale);
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworksyncFlicker
  {
    get
    {
      return this.syncFlicker;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.syncFlicker, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.syncFlicker);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.syncFlicker);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworksyncFlicker = reader.ReadBoolean();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworksyncFlicker = reader.ReadBoolean();
    }
  }
}
