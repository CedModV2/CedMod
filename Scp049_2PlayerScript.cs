// Decompiled with JetBrains decompiler
// Type: Scp049_2PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using RemoteAdmin;
using Security;
using UnityEngine;

public class Scp049_2PlayerScript : NetworkBehaviour
{
  [Header("Attack")]
  public float distance = 2.4f;
  public float damage = 40f;
  public float attackWindup = 0.2f;
  public float attackCooldown = 1f;
  [Header("Player Properties")]
  public Transform plyCam;
  public Animator animator;
  public bool iAm049_2;
  private RateLimit _iawRateLimit;

  private void Start()
  {
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
  }

  public void Init(RoleType classID, Role c)
  {
    this.iAm049_2 = classID == RoleType.Scp0492;
    this.animator.gameObject.SetActive(this.isLocalPlayer && this.iAm049_2);
  }

  [Command]
  private void CmdHurtPlayer(GameObject ply, string id)
  {
    if (this.isServer)
    {
      this.CallCmdHurtPlayer(ply, id);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(ply);
      writer.WriteString(id);
      this.SendCommandInternal(typeof (Scp049_2PlayerScript), nameof (CmdHurtPlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdShootAnim()
  {
    if (this.isServer)
    {
      this.CallCmdShootAnim();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (Scp049_2PlayerScript), nameof (CmdShootAnim), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientRpc]
  private void RpcShootAnim()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp049_2PlayerScript), nameof (RpcShootAnim), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdHurtPlayer called on client.");
    else
      ((Scp049_2PlayerScript) obj).CallCmdHurtPlayer(reader.ReadGameObject(), reader.ReadString());
  }

  protected static void InvokeCmdCmdShootAnim(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdShootAnim called on client.");
    else
      ((Scp049_2PlayerScript) obj).CallCmdShootAnim();
  }

  public void CallCmdHurtPlayer(GameObject ply, string id)
  {
    if (!this._iawRateLimit.CanExecute(true) || (Object) ply == (Object) null || ((double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, ply.transform.position) > (double) this.distance * 1.5 || !this.iAm049_2))
      return;
    Vector3 position = ply.transform.position;
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(this.damage, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp0492, this.GetComponent<QueryProcessor>().PlayerId), ply);
    this.GetComponent<CharacterClassManager>().RpcPlaceBlood(position, 0, ply.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator ? 1.3f : 0.5f);
  }

  public void CallCmdShootAnim()
  {
    if (!this._iawRateLimit.CanExecute(true))
      return;
    this.RpcShootAnim();
  }

  protected static void InvokeRpcRpcShootAnim(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcShootAnim called on server.");
    else
      ((Scp049_2PlayerScript) obj).CallRpcShootAnim();
  }

  public void CallRpcShootAnim()
  {
  }

  static Scp049_2PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049_2PlayerScript), "CmdHurtPlayer", new NetworkBehaviour.CmdDelegate(Scp049_2PlayerScript.InvokeCmdCmdHurtPlayer));
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp049_2PlayerScript), "CmdShootAnim", new NetworkBehaviour.CmdDelegate(Scp049_2PlayerScript.InvokeCmdCmdShootAnim));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp049_2PlayerScript), "RpcShootAnim", new NetworkBehaviour.CmdDelegate(Scp049_2PlayerScript.InvokeRpcRpcShootAnim));
  }
}
