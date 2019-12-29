// Decompiled with JetBrains decompiler
// Type: AlphaWarheadNukesitePanel
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class AlphaWarheadNukesitePanel : NetworkBehaviour
{
  public Transform lever;
  public BlastDoor blastDoor;
  public Door outsideDoor;
  public Material led_blastdoors;
  public Material led_outsidedoor;
  public Material led_detonationinprogress;
  public Material led_cancel;
  public Material[] onOffMaterial;
  private float _leverStatus;
  [SyncVar]
  public bool enabled;

  private void Awake()
  {
    AlphaWarheadOutsitePanel.nukeside = this;
  }

  private void FixedUpdate()
  {
    this.UpdateLeverStatus();
  }

  public bool AllowChangeLevelState()
  {
    return (double) this._leverStatus == 0.0 || (double) this._leverStatus == 1.0;
  }

  private void UpdateLeverStatus()
  {
    if ((Object) AlphaWarheadController.Host == (Object) null)
      return;
    Color color = new Color(0.2f, 0.3f, 0.5f);
    this.led_detonationinprogress.SetColor("_EmissionColor", AlphaWarheadController.Host.inProgress ? color : Color.black);
    this.led_outsidedoor.SetColor("_EmissionColor", this.outsideDoor.isOpen ? color : Color.black);
    this.led_blastdoors.SetColor("_EmissionColor", this.blastDoor.isClosed ? color : Color.black);
    this.led_cancel.SetColor("_EmissionColor", (double) AlphaWarheadController.Host.timeToDetonation <= 10.0 || !AlphaWarheadController.Host.inProgress ? Color.black : Color.red);
    this._leverStatus += this.enabled ? 0.04f : -0.04f;
    this._leverStatus = Mathf.Clamp01(this._leverStatus);
    for (int index = 0; index < 2; ++index)
      this.onOffMaterial[index].SetColor("_EmissionColor", index == Mathf.RoundToInt(this._leverStatus) ? new Color(1.2f, 1.2f, 1.2f, 1f) : Color.black);
    this.lever.localRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(10f, -170f, this._leverStatus), -90f, 90f));
  }

  private void MirrorProcessed()
  {
  }

  public bool Networkenabled
  {
    get
    {
      return this.enabled;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.enabled, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.enabled);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.enabled);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkenabled = reader.ReadBoolean();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Networkenabled = reader.ReadBoolean();
    }
  }
}
