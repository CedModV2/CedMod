// Decompiled with JetBrains decompiler
// Type: Grenades.FlashEffect
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grenades
{
  [RequireComponent(typeof (CharacterClassManager))]
  public class FlashEffect : NetworkBehaviour
  {
    [SyncVar]
    [NonSerialized]
    public float speed = 0.3333333f;
    [NonSerialized]
    public WeaponManager weapons;
    [NonSerialized]
    public Transform cam;
    [FormerlySerializedAs("e1")]
    public CameraFilterPack_Colors_Brightness filterBrightness;
    [FormerlySerializedAs("e2")]
    public CameraFilterPack_TV_Vignetting filterVignetting;
    private bool localBlinded;
    private float localPower;
    [SyncVar]
    [NonSerialized]
    public bool blinded;

    private void Awake()
    {
      this.weapons = this.GetComponent<WeaponManager>();
      this.cam = this.GetComponent<Scp049PlayerScript>().plyCam.transform;
    }

    private void Update()
    {
      if (!this.isLocalPlayer)
        return;
      if ((double) this.localPower > 0.0)
      {
        this.localPower -= Time.deltaTime * this.speed;
        if (!this.localBlinded)
        {
          this.filterBrightness.enabled = true;
          this.filterVignetting.enabled = true;
          this.CmdBlind(this.localBlinded = true);
        }
        this.filterBrightness._Brightness = Mathf.Clamp((float) ((double) this.localPower * 1.25 + 1.0), 1f, 2.5f);
        this.filterVignetting.Vignetting = this.filterVignetting.VignettingFull = this.filterVignetting.VignettingDirt = Mathf.Clamp01(this.localPower);
      }
      else
      {
        if (!this.localBlinded)
          return;
        this.filterBrightness.enabled = false;
        this.filterVignetting.enabled = false;
        this.localPower = 0.0f;
        this.CmdBlind(this.localBlinded = false);
      }
    }

    [Command]
    public void CmdBlind(bool state)
    {
      if (this.isServer)
      {
        this.CallCmdBlind(state);
      }
      else
      {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(state);
        this.SendCommandInternal(typeof (FlashEffect), nameof (CmdBlind), writer, 0);
        NetworkWriterPool.Recycle(writer);
      }
    }

    [ClientRpc]
    public void RpcPlay(float power)
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteSingle(power);
      this.SendRPCInternal(typeof (FlashEffect), nameof (RpcPlay), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }

    public void Play(float power)
    {
      this.localPower = power;
    }

    public bool Flashable(GameObject sourcePlayer, Vector3 sourcePosition, int ignoreMask)
    {
      if ((UnityEngine.Object) this.gameObject == (UnityEngine.Object) sourcePlayer)
        return false;
      CharacterClassManager component = sourcePlayer.GetComponent<CharacterClassManager>();
      return this.weapons.GetShootPermission(component.Classes.SafeGet(component.CurClass).team, false) && !Physics.Linecast(sourcePosition, this.cam.position, ignoreMask);
    }

    private void MirrorProcessed()
    {
    }

    public float Networkspeed
    {
      get
      {
        return this.speed;
      }
      [param: In] set
      {
        this.SetSyncVar<float>(value, ref this.speed, 1UL);
      }
    }

    public bool Networkblinded
    {
      get
      {
        return this.blinded;
      }
      [param: In] set
      {
        this.SetSyncVar<bool>(value, ref this.blinded, 2UL);
      }
    }

    protected static void InvokeCmdCmdBlind(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkServer.active)
        Debug.LogError((object) "Command CmdBlind called on client.");
      else
        ((FlashEffect) obj).CallCmdBlind(reader.ReadBoolean());
    }

    public void CallCmdBlind(bool state)
    {
      this.Networkblinded = state;
    }

    protected static void InvokeRpcRpcPlay(NetworkBehaviour obj, NetworkReader reader)
    {
      if (!NetworkClient.active)
        Debug.LogError((object) "RPC RpcPlay called on server.");
      else
        ((FlashEffect) obj).CallRpcPlay(reader.ReadSingle());
    }

    public void CallRpcPlay(float power)
    {
      this.Play(power);
    }

    static FlashEffect()
    {
      NetworkBehaviour.RegisterCommandDelegate(typeof (FlashEffect), "CmdBlind", new NetworkBehaviour.CmdDelegate(FlashEffect.InvokeCmdCmdBlind));
      NetworkBehaviour.RegisterRpcDelegate(typeof (FlashEffect), "RpcPlay", new NetworkBehaviour.CmdDelegate(FlashEffect.InvokeRpcRpcPlay));
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteSingle(this.speed);
        writer.WriteBoolean(this.blinded);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteSingle(this.speed);
        flag = true;
      }
      if (((long) this.syncVarDirtyBits & 2L) != 0L)
      {
        writer.WriteBoolean(this.blinded);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        this.Networkspeed = reader.ReadSingle();
        this.Networkblinded = reader.ReadBoolean();
      }
      else
      {
        long num = (long) reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
          this.Networkspeed = reader.ReadSingle();
        if ((num & 2L) == 0L)
          return;
        this.Networkblinded = reader.ReadBoolean();
      }
    }
  }
}
