// Decompiled with JetBrains decompiler
// Type: Scp096PlayerScript
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Grenades;
using MEC;
using Mirror;
using RemoteAdmin;
using Security;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Scp096PlayerScript : NetworkBehaviour
{
  public GameObject camera;
  public LayerMask layerMask;
  public SoundtrackManager.Track[] tracks;
  public AudioClip[] killsounds;
  public AnimationCurve lookingTolerance;
  public AnimationCurve rageCurve;
  private AnimationController _animationController;
  private CharacterClassManager _ccm;
  private FirstPersonController _fpc;
  private Queue<GameObject> _processLookingQueue;
  public bool iAm096;
  private float _rageProgress;
  private float _cooldown;
  private float _normalSpeed;
  private float _jumpSpeed;
  [Space]
  [SyncVar]
  public Scp096PlayerScript.RageState enraged;
  private const float _cooldownTime = 10f;
  private const float _rageTime = 15f;
  [SerializeField]
  private bool debugEnterRage;
  private RateLimit _iawRateLimit;
  public Dictionary<int, ReferenceHub> visiblePlys;
  private bool neonScp096Rework;

  private void Start()
  {
    this._iawRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[3];
    this._animationController = this.GetComponent<AnimationController>();
    this._fpc = this.GetComponent<FirstPersonController>();
    this._ccm = this.GetComponent<CharacterClassManager>();
    this._normalSpeed = this._ccm.Classes.Get(RoleType.Scp096).runSpeed;
    this._jumpSpeed = this._ccm.Classes.Get(RoleType.Scp096).jumpSpeed;
  }

  private void FixedUpdate()
  {
    for (int index = 0; index < this.tracks.Length; ++index)
    {
      this.tracks[index].playing = (Scp096PlayerScript.RageState) index == this.enraged && this.iAm096;
      this.tracks[index].Update(this.tracks.Length + 1);
    }
    if (!this.iAm096 || !NetworkServer.active)
      return;
    if (this.neonScp096Rework)
    {
      if (this.enraged <= Scp096PlayerScript.RageState.Panic)
        this.ProcessLooking();
    }
    else
    {
      switch (this.enraged)
      {
        case Scp096PlayerScript.RageState.NotEnraged:
          this.ProcessLooking();
          break;
        case Scp096PlayerScript.RageState.Enraged:
          this.DeductRage();
          break;
        case Scp096PlayerScript.RageState.Cooldown:
          this.DeductCooldown();
          break;
      }
    }
    if (!this.debugEnterRage)
      return;
    this.debugEnterRage = false;
    this.IncreaseRage(this.rageCurve.Evaluate(20f));
  }

  private void Update()
  {
    this.Animator();
  }

  internal void Init(RoleType classId, Role c)
  {
    this.iAm096 = classId == RoleType.Scp096;
    if (this.iAm096 && NetworkServer.active)
    {
      if (this._processLookingQueue == null)
        this._processLookingQueue = new Queue<GameObject>();
      if (this.visiblePlys == null)
      {
        this.visiblePlys = new Dictionary<int, ReferenceHub>();
        if (Scp096PlayerScript.VisiblePlyLists == null)
          Scp096PlayerScript.VisiblePlyLists = new List<Dictionary<int, ReferenceHub>>();
        try
        {
          Scp096PlayerScript.VisiblePlyLists.Add(this.visiblePlys);
        }
        catch
        {
        }
      }
      this.neonScp096Rework = ConfigFile.ServerConfig.GetBool("neon_scp096", true);
    }
    else
    {
      this._processLookingQueue = (Queue<GameObject>) null;
      this.visiblePlys = (Dictionary<int, ReferenceHub>) null;
    }
  }

  private void Animator()
  {
    if (this.isLocalPlayer || !((Object) this._animationController.animator != (Object) null) || !this.iAm096)
      return;
    this._animationController.animator.SetBool("Rage", this.enraged == Scp096PlayerScript.RageState.Enraged || this.enraged == Scp096PlayerScript.RageState.Panic);
  }

  public float ServerGetTopSpeed()
  {
    return this._normalSpeed * (this.enraged == Scp096PlayerScript.RageState.Panic ? 0.0f : (this.enraged == Scp096PlayerScript.RageState.Enraged ? 2.8f : 1f));
  }

  private void DeductRage()
  {
    this._rageProgress -= Time.fixedDeltaTime;
    if ((double) this._rageProgress > 0.0)
      return;
    this._cooldown = 10f;
    this.Networkenraged = Scp096PlayerScript.RageState.Cooldown;
    this._rageProgress = 0.0f;
  }

  private void DeductCooldown()
  {
    this._cooldown -= Time.fixedDeltaTime;
    if ((double) this._cooldown > 0.0)
      return;
    this.Networkenraged = Scp096PlayerScript.RageState.NotEnraged;
  }

  private void ProcessLooking()
  {
    if (this._processLookingQueue.IsEmpty<GameObject>())
    {
      foreach (GameObject player in PlayerManager.players)
        this._processLookingQueue.Enqueue(player);
    }
    else
    {
      GameObject player = this._processLookingQueue.Dequeue();
      if ((Object) player == (Object) null || !ReferenceHub.GetHub(player).characterClassManager.IsHuman() || player.GetComponent<FlashEffect>().blinded)
        return;
      Transform transform = player.GetComponent<Scp096PlayerScript>().camera.transform;
      float num = this.lookingTolerance.Evaluate(Vector3.Distance(transform.position, this.camera.transform.position));
      Vector3 vector3;
      if ((double) num >= 0.75)
      {
        Vector3 forward = transform.forward;
        vector3 = transform.position - this.camera.transform.position;
        Vector3 normalized = vector3.normalized;
        if ((double) Vector3.Dot(forward, normalized) >= -(double) num)
          return;
      }
      Vector3 position = transform.transform.position;
      vector3 = this.camera.transform.position - transform.position;
      Vector3 normalized1 = vector3.normalized;
      RaycastHit raycastHit;
      int layerMask = (int) this.layerMask;
      if (!Physics.Raycast(position, normalized1, out raycastHit, 20f, layerMask) || raycastHit.collider.gameObject.layer != 24 || !((Object) raycastHit.collider.GetComponentInParent<Scp096PlayerScript>() == (Object) this))
        return;
      if (this.neonScp096Rework)
      {
        ReferenceHub hub = ReferenceHub.GetHub(player);
        if (!this.visiblePlys.ContainsKey(hub.queryProcessor.PlayerId))
          this.visiblePlys.Add(hub.queryProcessor.PlayerId, hub);
        if (this.Networkenraged != Scp096PlayerScript.RageState.NotEnraged)
          return;
        this.Networkenraged = Scp096PlayerScript.RageState.Panic;
        this.ModifiedJokerStuff();
        this.Invoke("StartRage", 5f);
      }
      else
        this.IncreaseRage(Time.fixedDeltaTime);
    }
  }

  public void IncreaseRage(float amount)
  {
    this._rageProgress += amount;
    if ((double) this._rageProgress < (double) this.rageCurve.Evaluate((float) Mathf.Min(PlayerManager.players.Count, 20)))
      return;
    this.Networkenraged = Scp096PlayerScript.RageState.Panic;
    this._rageProgress = 15f;
    this.Invoke("StartRage", 5f);
  }

  private void StartRage()
  {
    this.Networkenraged = Scp096PlayerScript.RageState.Enraged;
  }

  private void Shoot()
  {
    RaycastHit hitInfo;
    if (!Physics.Raycast(this.camera.transform.position, this.camera.transform.forward, out hitInfo, 1.5f))
      return;
    CharacterClassManager component = hitInfo.transform.GetComponent<CharacterClassManager>();
    if ((Object) component == (Object) null || component.Classes.SafeGet(component.CurClass).team == Team.SCP)
      return;
    this.CmdHurtPlayer(hitInfo.transform.gameObject);
  }

  [Command]
  private void CmdHurtPlayer(GameObject target)
  {
    if (this.isServer)
    {
      this.CallCmdHurtPlayer(target);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteGameObject(target);
      this.SendCommandInternal(typeof (Scp096PlayerScript), nameof (CmdHurtPlayer), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [TargetRpc]
  private void TargetHitMarker(NetworkConnection connection)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(connection, typeof (Scp096PlayerScript), nameof (TargetHitMarker), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcSyncAudio()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Scp096PlayerScript), nameof (RpcSyncAudio), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  public Scp096PlayerScript.RageState Networkenraged
  {
    get
    {
      return this.enraged;
    }
    [param: In] set
    {
      this.SetSyncVar<Scp096PlayerScript.RageState>(value, ref this.enraged, 1UL);
    }
  }

  protected static void InvokeCmdCmdHurtPlayer(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdHurtPlayer called on client.");
    else
      ((Scp096PlayerScript) obj).CallCmdHurtPlayer(reader.ReadGameObject());
  }

  public void CallCmdHurtPlayer(GameObject target)
  {
    if (!this._iawRateLimit.CanExecute(true) || (Object) target == (Object) null)
      return;
    CharacterClassManager component = target.GetComponent<CharacterClassManager>();
    if ((Object) component == (Object) null || this._ccm.CurClass != RoleType.Scp096 || ((double) Vector3.Distance(this.GetComponent<PlyMovementSync>().RealModelPosition, target.transform.position) >= 3.0 || this.enraged != Scp096PlayerScript.RageState.Enraged) || component.Classes.SafeGet(component.CurClass).team == Team.SCP)
      return;
    this.GetComponent<CharacterClassManager>().RpcPlaceBlood(target.transform.position, 0, 3.1f);
    this.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(99999f, this.GetComponent<NicknameSync>().MyNick + " (" + this.GetComponent<CharacterClassManager>().UserId + ")", DamageTypes.Scp096, this.GetComponent<QueryProcessor>().PlayerId), target);
    this.TargetHitMarker(this.connectionToClient);
    this.RpcSyncAudio();
  }

  protected static void InvokeRpcRpcSyncAudio(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcSyncAudio called on server.");
    else
      ((Scp096PlayerScript) obj).CallRpcSyncAudio();
  }

  protected static void InvokeRpcTargetHitMarker(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetHitMarker called on server.");
    else
      ((Scp096PlayerScript) obj).CallTargetHitMarker(ClientScene.readyConnection);
  }

  public void CallRpcSyncAudio()
  {
    if (this.killsounds.Length == 0)
      return;
    this.GetComponent<AnimationController>().gunSource.PlayOneShot(this.killsounds[Random.Range(0, this.killsounds.Length)], this.isLocalPlayer ? 0.3f : 1f);
  }

  public void CallTargetHitMarker(NetworkConnection connection)
  {
  }

  static Scp096PlayerScript()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (Scp096PlayerScript), "CmdHurtPlayer", new NetworkBehaviour.CmdDelegate(Scp096PlayerScript.InvokeCmdCmdHurtPlayer));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp096PlayerScript), "RpcSyncAudio", new NetworkBehaviour.CmdDelegate(Scp096PlayerScript.InvokeRpcRpcSyncAudio));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Scp096PlayerScript), "TargetHitMarker", new NetworkBehaviour.CmdDelegate(Scp096PlayerScript.InvokeRpcTargetHitMarker));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      NetworkWriterExtensions.WriteByte(writer, (byte) this.enraged);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      NetworkWriterExtensions.WriteByte(writer, (byte) this.enraged);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkenraged = (Scp096PlayerScript.RageState) NetworkReaderExtensions.ReadByte(reader);
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Networkenraged = (Scp096PlayerScript.RageState) NetworkReaderExtensions.ReadByte(reader);
    }
  }

  private void ModifiedJokerStuff()
  {
    Timing.RunCoroutine(this.GetClosestPlayer(), 1, "gcp");
    Timing.RunCoroutine(this.Punish(ReferenceHub.GetHub(this.gameObject)), 1, "punish");
  }

  public IEnumerator<float> GetClosestPlayer()
  {
    Scp096PlayerScript scp096PlayerScript = this;
    Broadcast bc = PlayerManager.localPlayer.GetComponent<Broadcast>();
    yield return Timing.WaitForSeconds(5.5f);
    scp096PlayerScript.gameObject.GetComponent<ServerRoles>().BypassMode = true;
    while (scp096PlayerScript.Networkenraged == Scp096PlayerScript.RageState.Enraged && scp096PlayerScript.visiblePlys.Count > 0 && scp096PlayerScript.gameObject.GetComponent<CharacterClassManager>().NetworkCurClass == RoleType.Scp096)
    {
      int index = 0;
      float num1 = 81f;
      while (index < scp096PlayerScript.visiblePlys.Count)
      {
        KeyValuePair<int, ReferenceHub> keyValuePair = scp096PlayerScript.visiblePlys.ElementAt<KeyValuePair<int, ReferenceHub>>(index);
        if (!keyValuePair.Value.characterClassManager.IsHuman())
        {
          scp096PlayerScript.visiblePlys.Remove(keyValuePair.Key);
        }
        else
        {
          float num2 = Vector3.Distance(scp096PlayerScript.gameObject.transform.position, keyValuePair.Value.gameObject.transform.position);
          if ((double) num2 <= 80.0)
          {
            if ((double) num2 < (double) num1)
              num1 = num2;
            ++index;
          }
          else
          {
            bc.TargetAddElement(keyValuePair.Value.characterClassManager.connectionToClient, "<i>You feel like SCP-096 forgot you...</i>", 4U, true);
            scp096PlayerScript.visiblePlys.Remove(keyValuePair.Key);
          }
        }
      }
      if ((double) num1 <= 80.0)
      {
        string str = Scp096PlayerScript.DrawBar((80.0 - (double) num1) / 80.0);
        bc.TargetClearElements(scp096PlayerScript.connectionToClient);
        bc.TargetAddElement(scp096PlayerScript.connectionToClient, "<size=30><color=#c50000>Distance to nearest target: </color><color=#10F110>" + str + "</color></size> \n<size=25>Targets Remaining: <color=#c50000>" + (object) scp096PlayerScript.visiblePlys.Count + "</color></size>", 1U, false);
        yield return Timing.WaitForSeconds(0.5f);
      }
      else
        break;
    }
    scp096PlayerScript.Networkenraged = Scp096PlayerScript.RageState.NotEnraged;
    scp096PlayerScript.gameObject.GetComponent<ServerRoles>().BypassMode = false;
    Timing.KillCoroutines("punish");
  }

  private IEnumerator<float> Punish(ReferenceHub rh)
  {
    Scp096PlayerScript scp096PlayerScript = this;
    if (!((Object) rh == (Object) null))
    {
      yield return Timing.WaitForSeconds(5.5f);
      int counter = 0;
      while (scp096PlayerScript.Networkenraged == Scp096PlayerScript.RageState.Enraged && scp096PlayerScript.gameObject.GetComponent<CharacterClassManager>().NetworkCurClass == RoleType.Scp096)
      {
        ++counter;
        float num = Mathf.Pow(1.7f, (float) counter);
        rh.playerStats.HurtPlayer(new PlayerStats.HitInfo((float) Mathf.FloorToInt(ConfigFile.ServerConfig.GetFloat("neon_096_damagemultiplier", 5f) * num), rh.nicknameSync.MyNick, DamageTypes.Decont, rh.queryProcessor.PlayerId), rh.gameObject);
        yield return Timing.WaitForSeconds(5f);
      }
    }
  }

  private static string DrawBar(double percentage)
  {
    string str = "<color=#ffffff>(</color>";
    percentage *= 100.0;
    if (percentage == 0.0)
      return "(      )";
    for (double num = 0.0; num < 100.0; num += 5.0)
      str = num >= percentage ? str + "<color=#c50000>█</color>" : str + "█";
    return str + "<color=#ffffff>)</color>";
  }

  public bool Neon096Rework
  {
    get
    {
      return this.neonScp096Rework;
    }
  }

  public static List<Dictionary<int, ReferenceHub>> VisiblePlyLists { get; private set; }

  public enum RageState : byte
  {
    NotEnraged,
    Panic,
    Enraged,
    Cooldown,
  }
}
