// Decompiled with JetBrains decompiler
// Type: Scp939PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using Mirror;
using RemoteAdmin;
using Security;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scp939PlayerScript : NetworkBehaviour
{
  public static List<Scp939PlayerScript> instances = new List<Scp939PlayerScript>();
  public bool iAm939;
  public bool sameClass;
  public LayerMask normalVision;
  public LayerMask scpVision;
  public Camera visionCamera;
  public Behaviour[] visualEffects;
  public LayerMask attackMask;
  public AudioClip[] attackSounds;
  public float attackDistance;
  private Camera plyCam;
  [SyncVar]
  public bool usingHumanChat;
  private bool prevuhc;
  private CharacterClassManager _ccm;
  private RateLimit _iawRateLimit;
  private DissonanceUserSetup _dissonance;
  private float cooldown;

  private void Start()
  {
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._dissonance = this.GetComponentInChildren<DissonanceUserSetup>();
    if (!this.isLocalPlayer)
      return;
    Scp939PlayerScript.instances = new List<Scp939PlayerScript>();
    this.plyCam = this.GetComponent<Scp049PlayerScript>().plyCam.GetComponent<Camera>();
  }

  public void Init(RoleType classID, Role c)
  {
    this.sameClass = c.team == Team.SCP;
    this.iAm939 = c.roleId.Is939();
    if (this.iAm939 && !Scp939PlayerScript.instances.Contains(this))
      Scp939PlayerScript.instances.Add(this);
    if (!this.isLocalPlayer)
      return;
    foreach (Behaviour visualEffect in this.visualEffects)
      visualEffect.enabled = this.iAm939;
    this.plyCam.renderingPath = this.iAm939 ? RenderingPath.VertexLit : RenderingPath.DeferredShading;
    this.plyCam.cullingMask = (int) (this.iAm939 ? this.scpVision : this.normalVision);
    this.visionCamera.gameObject.SetActive(this.iAm939);
    this.visionCamera.fieldOfView = this.plyCam.fieldOfView;
    this.visionCamera.farClipPlane = this.plyCam.farClipPlane;
  }

  private void Update()
  {
    this.CheckInstances();
    this.ServersideCode();
  }

  private void ServersideCode()
  {
    if (!NetworkServer.active || (double) this.cooldown < 0.0)
      return;
    this.cooldown -= Time.deltaTime;
  }

  [Command]
  private void CmdChangeHumanchatThing(bool newValue)
  {
    if (this.isServer)
    {
      this.CallCmdChangeHumanchatThing(newValue);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(newValue);
      this.SendCommandInternal(typeof (Scp939PlayerScript), nameof (CmdChangeHumanchatThing), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void Shoot()
  {
  }

  [Command]
  private void CmdShoot(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdShoot(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Scp939PlayerScript), nameof (CmdShoot), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcShoot()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp939PlayerScript), nameof (RpcShoot), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void CheckInstances()
  {
    if (!this._ccm.IsHost)
      return;
    foreach (Scp939PlayerScript instance in Scp939PlayerScript.instances)
    {
      if ((Object) instance == (Object) null || !instance.iAm939)
      {
        Scp939PlayerScript.instances.Remove(instance);
        break;
      }
    }
  }

  static Scp939PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp939PlayerScript), "CmdChangeHumanchatThing", new NetworkBehaviour.CmdDelegate(Scp939PlayerScript.InvokeCmdCmdChangeHumanchatThing));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp939PlayerScript), "CmdShoot", new NetworkBehaviour.CmdDelegate(Scp939PlayerScript.InvokeCmdCmdShoot));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp939PlayerScript), "RpcShoot", new NetworkBehaviour.CmdDelegate(Scp939PlayerScript.InvokeRpcRpcShoot));
  }

  private void MirrorProcessed()
  {
  }

  public bool NetworkusingHumanChat
  {
    get
    {
      return this.usingHumanChat;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.usingHumanChat, 1UL);
    }
  }

  protected static void InvokeCmdCmdChangeHumanchatThing(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdChangeHumanchatThing called on client.");
    else
      ((Scp939PlayerScript) obj).CallCmdChangeHumanchatThing(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdShoot(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdShoot called on client.");
    else
      ((Scp939PlayerScript) obj).CallCmdShoot(reader.ReadGameObject());
  }

  public void CallCmdChangeHumanchatThing(bool newValue)
  {
    this.NetworkusingHumanChat = this.iAm939 & newValue;
    this._dissonance.MimicAs939 = this.usingHumanChat;
  }

  public void CallCmdShoot(GameObject target)
  {
    if (!this._iawRateLimit.CanExecute(true) || (Object) target == (Object) null || (!this.iAm939 || (double) Vector3.Distance(target.transform.position, this.transform.position) >= (double) this.attackDistance * 1.20000004768372) || (double) this.cooldown > 0.0)
      return;
    this.cooldown = 1f;
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo((float) Random.Range(50, 80), this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp939, this.GetComponent<QueryProcessor>().PlayerId), target);
    this.GetComponent<CharacterClassManager>().RpcPlaceBlood(target.transform.position, 0, 2f);
    this.RpcShoot();
  }

  protected static void InvokeRpcRpcShoot(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcShoot called on server.");
    else
      ((Scp939PlayerScript) obj).CallRpcShoot();
  }

  public void CallRpcShoot()
  {
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.usingHumanChat);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.usingHumanChat);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkusingHumanChat = reader.ReadBoolean();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworkusingHumanChat = reader.ReadBoolean();
    }
  }
}
