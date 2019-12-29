// Decompiled with JetBrains decompiler
// Type: Scp049PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using RemoteAdmin;
using Security;
using UnityEngine;

public class Scp049PlayerScript : NetworkBehaviour
{
  [Header("Balance Parameters")]
  public float killCooldown = 1.5f;
  public float attackDistance = 2.4f;
  public float timeToRevive = 7f;
  public float reviveDistance = 3.5f;
  public float reviveEligibilityDuration = 10f;
  [Header("Player Properties")]
  public GameObject plyCam;
  public bool iAm049;
  [Header("Attack & Recall")]
  public float recallProgress;
  public float remainingKillCooldown;
  private GameObject _recallObjectServer;
  private float _recallProgressServer;
  private bool _recallInProgressServer;
  private RateLimit _interactRateLimit;

  private void Start()
  {
    this._interactRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[0];
  }

  public void Init(RoleType classId, Role c)
  {
    this.iAm049 = classId == RoleType.Scp049;
  }

  private void Update()
  {
    if (!this._recallInProgressServer)
      return;
    this._recallProgressServer += Time.deltaTime;
    if ((double) this._recallProgressServer < (double) this.timeToRevive * 2.0)
      return;
    this._recallInProgressServer = false;
    this._recallProgressServer = 0.0f;
    this._recallObjectServer = (GameObject) null;
  }

  private void DestroyPlayer(GameObject recallingRagdoll)
  {
    if (!recallingRagdoll.CompareTag("Ragdoll"))
      return;
    NetworkServer.Destroy(recallingRagdoll);
  }

  [Command]
  public void CmdStartInfecting(GameObject target, GameObject rd)
  {
    if (this.isServer)
    {
      this.CallCmdStartInfecting(target, rd);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      writer.WriteGameObject(rd);
      this.SendCommandInternal(typeof (Scp049PlayerScript), nameof (CmdStartInfecting), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdAbortInfecting()
  {
    if (this.isServer)
    {
      this.CallCmdAbortInfecting();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Scp049PlayerScript), nameof (CmdAbortInfecting), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdInfectPlayer(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdInfectPlayer(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Scp049PlayerScript), nameof (CmdInfectPlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  public void RpcSetDeathTime(GameObject target)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteGameObject(target);
    this.SendRPCInternal(typeof (Scp049PlayerScript), nameof (RpcSetDeathTime), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  private void CmdRecallPlayer(GameObject target, GameObject ragdoll)
  {
    if (this.isServer)
    {
      this.CallCmdRecallPlayer(target, ragdoll);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      writer.WriteGameObject(ragdoll);
      this.SendCommandInternal(typeof (Scp049PlayerScript), nameof (CmdRecallPlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdStartInfecting(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdStartInfecting called on client.");
    else
      ((Scp049PlayerScript) obj).CallCmdStartInfecting(reader.ReadGameObject(), reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdAbortInfecting(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdAbortInfecting called on client.");
    else
      ((Scp049PlayerScript) obj).CallCmdAbortInfecting();
  }

  protected static void InvokeCmdCmdInfectPlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdInfectPlayer called on client.");
    else
      ((Scp049PlayerScript) obj).CallCmdInfectPlayer(reader.ReadGameObject());
  }

  protected static void InvokeCmdCmdRecallPlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRecallPlayer called on client.");
    else
      ((Scp049PlayerScript) obj).CallCmdRecallPlayer(reader.ReadGameObject(), reader.ReadGameObject());
  }

  public void CallCmdStartInfecting(GameObject target, GameObject rd)
  {
    if (!this._interactRateLimit.CanExecute(true) || (Object) target == (Object) null || (Object) rd == (Object) null)
      return;
    Ragdoll component1 = rd.GetComponent<Ragdoll>();
    QueryProcessor component2 = target.GetComponent<QueryProcessor>();
    if ((Object) component1 == (Object) null || (Object) component2 == (Object) null || (!this.iAm049 || (double) Vector3.Distance(rd.transform.position, this.plyCam.transform.position) >= (double) this.attackDistance * 1.29999995231628))
      return;
    this._recallObjectServer = target;
    this._recallProgressServer = 0.0f;
    this._recallInProgressServer = true;
  }

  public void CallCmdAbortInfecting()
  {
    if (!this._interactRateLimit.CanExecute(true))
      return;
    this._recallInProgressServer = false;
    this._recallObjectServer = (GameObject) null;
    this._recallProgressServer = 0.0f;
  }

  public void CallCmdInfectPlayer(GameObject target)
  {
    if (!this._interactRateLimit.CanExecute(true) || (Object) target == (Object) null || (!this.iAm049 || (double) Vector3.Distance(target.transform.position, this.GetComponent<PlyMovementSync>().RealModelPosition) >= (double) this.attackDistance * 1.29999995231628))
      return;
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(4949f, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp049, this.GetComponent<QueryProcessor>().PlayerId), target);
  }

  public void CallCmdRecallPlayer(GameObject target, GameObject ragdoll)
  {
    if (!this._interactRateLimit.CanExecute(true) || (Object) target == (Object) null || ((Object) ragdoll == (Object) null || !this._recallInProgressServer) || ((Object) target != (Object) this._recallObjectServer || (double) this._recallProgressServer < 0.850000023841858))
      return;
    CharacterClassManager component = target.GetComponent<CharacterClassManager>();
    if ((Object) ragdoll.GetComponent<Ragdoll>() == (Object) null || (Object) component == (Object) null || (component.CurClass != RoleType.Spectator || !this.iAm049))
      return;
    ++RoundSummary.changed_into_zombies;
    component.SetClassID(RoleType.Scp0492);
    target.GetComponent<PlayerStats>().health = (float) component.Classes.Get(RoleType.Scp0492).maxHP;
    this.DestroyPlayer(ragdoll);
    this._recallInProgressServer = false;
    this._recallObjectServer = (GameObject) null;
    this._recallProgressServer = 0.0f;
  }

  protected static void InvokeRpcRpcSetDeathTime(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcSetDeathTime called on server.");
    else
      ((Scp049PlayerScript) obj).CallRpcSetDeathTime(reader.ReadGameObject());
  }

  public void CallRpcSetDeathTime(GameObject target)
  {
  }

  static Scp049PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049PlayerScript), "CmdStartInfecting", new NetworkBehaviour.CmdDelegate(Scp049PlayerScript.InvokeCmdCmdStartInfecting));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049PlayerScript), "CmdAbortInfecting", new NetworkBehaviour.CmdDelegate(Scp049PlayerScript.InvokeCmdCmdAbortInfecting));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049PlayerScript), "CmdInfectPlayer", new NetworkBehaviour.CmdDelegate(Scp049PlayerScript.InvokeCmdCmdInfectPlayer));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049PlayerScript), "CmdRecallPlayer", new NetworkBehaviour.CmdDelegate(Scp049PlayerScript.InvokeCmdCmdRecallPlayer));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp049PlayerScript), "RpcSetDeathTime", new NetworkBehaviour.CmdDelegate(Scp049PlayerScript.InvokeRpcRpcSetDeathTime));
  }
}
