// Decompiled with JetBrains decompiler
// Type: Grenades.FlashGrenade
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Grenades
{
  public class FlashGrenade : EffectGrenade
  {
    public float distanceMultiplierSurface = 10f;
    public float distanceMultiplierFacility = 2f;
    [Header("Flash Effect")]
    public LayerMask viewLayerMask;
    public AnimationCurve powerOverDistance;
    public AnimationCurve powerOverDot;
    [SyncVar]
    [NonSerialized]
    public bool friendlyFlash;

    protected override void Awake()
    {
      base.Awake();
      if (NetworkServer.active)
        this.NetworkfriendlyFlash = ConfigFile.ServerConfig.GetBool("friendly_flash", false);
      this.viewLayerMask = (LayerMask) ~(int) this.viewLayerMask;
    }

    private void MirrorProcessed()
    {
    }

    public bool NetworkfriendlyFlash
    {
      get
      {
        return this.friendlyFlash;
      }
      [param: In] set
      {
        this.SetSyncVar<bool>(value, ref this.friendlyFlash, 1UL);
      }
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteBoolean(this.friendlyFlash);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteBoolean(this.friendlyFlash);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        this.NetworkfriendlyFlash = reader.ReadBoolean();
      }
      else
      {
        if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
          return;
        this.NetworkfriendlyFlash = reader.ReadBoolean();
      }
    }
  }
}
