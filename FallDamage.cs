// Decompiled with JetBrains decompiler
// Type: FallDamage
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using Security;
using UnityEngine;

public class FallDamage : NetworkBehaviour
{
  private static readonly Vector3 GroundCheckSize = new Vector3(0.5f, 0.7f, 0.5f);
  private static readonly Vector3 CloseGroundCheckSize = new Vector3(0.52f, 1.25f, 0.52f);
  private static readonly Collider[] CheckIfGroundedBuffer = new Collider[1];
  private static readonly Collider[] CheckIfCloseToGroundBuffer = new Collider[1];
  private static readonly Collider[] CheckUnsafePositionBuffer = new Collider[1];
  [SerializeField]
  private float groundMaxDistance = 1.3f;
  public bool isGrounded = true;
  public bool isCloseToGround = true;
  public AudioClip sound;
  public AudioSource sfxsrc;
  public AnimationCurve damageOverDistance;
  private CharacterClassManager _ccm;
  private PlyMovementSync _pms;
  private FootstepSync _footstepSync;
  private RateLimit _fallDamageSoundRateLimit;
  private Transform _lastCast;
  private LayerMask _groundMask;
  public string zone;
  public float _previousHeight;
  private static Vector3 _posG;
  private static Vector3 _posCg;
  private static LayerMask _staticGroundMask;

  private void Start()
  {
    this._fallDamageSoundRateLimit = new RateLimit(3, 2.5f, (NetworkConnection) null);
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._footstepSync = this.GetComponent<FootstepSync>();
    this._pms = this.GetComponent<PlyMovementSync>();
    this._groundMask = this._pms.CollidableSurfaces;
    FallDamage._staticGroundMask = this._groundMask;
  }

  private void FixedUpdate()
  {
    if (!this.isLocalPlayer)
      return;
    this.CalculateGround();
    RaycastHit hitInfo;
    string name;
    if (!Physics.Raycast(new Ray(this.transform.position, Vector3.down), out hitInfo, this.groundMaxDistance, (int) this._groundMask) || (Object) this._lastCast == (Object) hitInfo.transform || this.zone == (name = hitInfo.transform.root.name))
      return;
    this._lastCast = hitInfo.transform;
    this.zone = name;
    if (this.zone.Contains("Heavy"))
      SoundtrackManager.singleton.mainIndex = 1;
    else if (this.zone.Contains("Out"))
      SoundtrackManager.singleton.mainIndex = 2;
    else
      SoundtrackManager.singleton.mainIndex = 0;
  }

  public void CalculateGround()
  {
    if (TutorialManager.status || this._ccm.CurClass < RoleType.Scp173 || this._ccm.CurClass == RoleType.Spectator)
    {
      this.isCloseToGround = true;
    }
    else
    {
      bool flag = FallDamage.CheckIfGrounded(this.transform.position);
      this.isCloseToGround = flag || FallDamage.CheckIfCloseToGround();
      if (flag == this.isGrounded)
        return;
      this.isGrounded = flag;
      if ((double) this._ccm.AliveTime < 5.0 || !NetworkServer.active)
        return;
      if (this.isGrounded)
        this.OnTouchdown();
      else
        this.OnLoseContactWithGround();
    }
  }

  private static bool CheckIfGrounded(Vector3 pos)
  {
    pos.y -= 0.8f;
    FallDamage._posG = pos;
    FallDamage._posCg = pos;
    FallDamage._posCg.y -= 0.8f;
    return (uint) Physics.OverlapBoxNonAlloc(pos, FallDamage.GroundCheckSize, FallDamage.CheckIfGroundedBuffer, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), (int) FallDamage._staticGroundMask) > 0U;
  }

  private static bool CheckIfCloseToGround()
  {
    return (uint) Physics.OverlapBoxNonAlloc(FallDamage._posCg, FallDamage.CloseGroundCheckSize, FallDamage.CheckIfCloseToGroundBuffer, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), (int) FallDamage._staticGroundMask) > 0U;
  }

  public static bool CheckUnsafePosition(Vector3 pos)
  {
    return Physics.OverlapBoxNonAlloc(pos, new Vector3(0.6f, 1.2f, 0.6f), FallDamage.CheckUnsafePositionBuffer, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f), (int) FallDamage._staticGroundMask) == 0 && Physics.Raycast(pos, Vector3.down, 10f, (int) FallDamage._staticGroundMask);
  }

  private void OnLoseContactWithGround()
  {
    this._previousHeight = this.transform.position.y;
  }

  private void OnTouchdown()
  {
    if ((Object) this._footstepSync != (Object) null)
      this._footstepSync.RpcPlayLandingFootstep(true);
    float num = this.damageOverDistance.Evaluate(this._previousHeight - this.transform.position.y);
    if ((double) num <= 5.0 || this._ccm.NoclipEnabled || (this._ccm.GodMode || this._ccm.Classes.SafeGet(this._ccm.CurClass).team == Team.SCP) || this._pms.InSafeTime)
      return;
    if ((double) this.GetComponent<PlayerStats>().health - (double) num <= 0.0)
      this.TargetAchieve(this._ccm.connectionToClient);
    Vector3 position = this.transform.position;
    this.RpcDoSound(position, num);
    this._ccm.RpcPlaceBlood(position, 0, Mathf.Clamp(num / 30f, 0.8f, 2f));
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(Mathf.Abs(num), "WORLD", DamageTypes.Falldown, 0), this.gameObject);
  }

  [TargetRpc]
  private void TargetAchieve(NetworkConnection conn)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(conn, typeof (FallDamage), nameof (TargetAchieve), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcDoSound(Vector3 pos, float dmg)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteVector3(pos);
    writer.WriteSingle(dmg);
    this.SendRPCInternal(typeof (FallDamage), nameof (RpcDoSound), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  static FallDamage()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (FallDamage), "RpcDoSound", new NetworkBehaviour.CmdDelegate(FallDamage.InvokeRpcRpcDoSound));
    NetworkBehaviour.RegisterRpcDelegate(typeof (FallDamage), "TargetAchieve", new NetworkBehaviour.CmdDelegate(FallDamage.InvokeRpcTargetAchieve));
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDoSound called on server.");
    else
      ((FallDamage) obj).CallRpcDoSound(reader.ReadVector3(), reader.ReadSingle());
  }

  protected static void InvokeRpcTargetAchieve(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetAchieve called on server.");
    else
      ((FallDamage) obj).CallTargetAchieve(ClientScene.readyConnection);
  }

  public void CallRpcDoSound(Vector3 pos, float dmg)
  {
  }

  public void CallTargetAchieve(NetworkConnection conn)
  {
  }
}
