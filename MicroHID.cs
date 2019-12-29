// Decompiled with JetBrains decompiler
// Type: MicroHID
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class MicroHID : NetworkBehaviour
{
  [Header("Backend Settings")]
  [SerializeField]
  [Range(0.0f, 15f)]
  private float chargeupTime = 5.97f;
  [SerializeField]
  [Range(0.0f, 15f)]
  private float chargedownTime = 4.32f;
  private ReferenceHub refHub;
  private KeyCode keyPrimary;
  private KeyCode keySecondary;
  private float keyAntiSpamCooldown;
  private float soundEffectPause;
  private float damagePause;
  private float chargeup;
  private Animator hidAnimator;
  private const float DAMAGE_UPDATE_INTERVAL = 0.2f;
  [Header("Gameplay Settings")]
  [SerializeField]
  [Range(0.0f, 2000f)]
  private float damagePerSecond;
  [SerializeField]
  [Range(0.0f, 100f)]
  private float maxDamageVariationPercent;
  [SerializeField]
  private AnimationCurve speedBasedEnergyLoss;
  [SerializeField]
  [Range(0.0f, 0.99f)]
  private float dischargeEnergyLoss;
  [SerializeField]
  [Range(0.0f, 40f)]
  private float beamLength;
  [SerializeField]
  private Transform beamStart;
  [SerializeField]
  private LayerMask beamMask;
  [Header("Audiovisual Settings")]
  [SerializeField]
  [Range(0.0f, 10f)]
  private float shootingShake;
  [SerializeField]
  private AudioSource audioSource;
  [SerializeField]
  private AudioClip audioSpinUp;
  [SerializeField]
  private AudioClip audioSpinDown;
  [SerializeField]
  private AudioClip audioFireStopped;
  [SerializeField]
  private AudioClip audioFirePaused;
  [SerializeField]
  private AudioClip audioSpinLoop;
  [SerializeField]
  private AudioClip audioFire;
  [SerializeField]
  private Transform gauge;
  [Header("Public backend variables")]
  [SyncVar]
  public MicroHID.MicroHidState CurrentHidState;
  [SyncVar]
  public byte SyncKeyCode;
  [SyncVar]
  public float Energy;
  private Collider lastVictim;
  private ReferenceHub lastVictimRefs;
  private Scp939_VisionController _visionController;
  private bool allowKeycheck;
  private MicroHID.MicroHidState clientPrevState;

  private void Start()
  {
    this.refHub = ReferenceHub.GetHub(this.gameObject);
    this._visionController = this.GetComponent<Scp939_VisionController>();
  }

  private void LateUpdate()
  {
  }

  private void Update()
  {
    this.soundEffectPause = Mathf.Clamp01(this.soundEffectPause + Time.deltaTime / (this.CurrentHidState == MicroHID.MicroHidState.Discharge ? 1.5f : -1.5f));
    if (!NetworkServer.active)
      return;
    this.UpdateServerside();
  }

  private void ChangeEnergy(float newValue)
  {
    this.refHub.inventory.items.ModifyDuration(this.refHub.inventory.GetItemIndex(), newValue);
  }

  private float GetEnergy()
  {
    return this.refHub.inventory.curItem != ItemType.MicroHID ? 0.0f : this.refHub.inventory.items[this.refHub.inventory.GetItemIndex()].durability;
  }

  [TargetRpc]
  private void TargetSendHitmarker(bool isAchievement)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(isAchievement);
    this.SendTargetRPCInternal((NetworkConnection) null, typeof (MicroHID), nameof (TargetSendHitmarker), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ServerCallback]
  private void DealDamage()
  {
    if (!NetworkServer.active)
      return;
    this.damagePause += Time.deltaTime;
    if ((double) this.damagePause < 0.200000002980232)
      return;
    this.damagePause -= 0.2f;
    ReferenceHub refHub = this.refHub;
    this.beamStart.localRotation = Quaternion.Euler(refHub.plyMovementSync.Rotations.x, 0.0f, 0.0f);
    foreach (GameObject player in PlayerManager.players)
    {
      if ((Object) player != (Object) null && (Object) player != (Object) this.gameObject && (double) Vector3.Distance(this.transform.position, player.transform.position) <= (double) this.beamLength)
      {
        Vector3 normalized = (this.beamStart.position - player.transform.position).normalized;
        RaycastHit hitInfo;
        if ((double) Vector3.Dot(this.beamStart.forward, normalized) < -0.949999988079071 && Physics.Raycast(new Ray(this.beamStart.position, -normalized), out hitInfo, this.beamLength, (int) this.beamMask))
        {
          if ((Object) hitInfo.collider != (Object) this.lastVictim)
          {
            this.lastVictim = hitInfo.collider;
            this.lastVictimRefs = hitInfo.collider.GetComponentInParent<ReferenceHub>();
          }
          ReferenceHub lastVictimRefs = this.lastVictimRefs;
          if ((Object) lastVictimRefs != (Object) null && (Object) lastVictimRefs.gameObject != (Object) this.gameObject && refHub.weaponManager.GetShootPermission(lastVictimRefs.characterClassManager, false))
          {
            int num = Mathf.RoundToInt((float) (((double) this.damagePerSecond + (double) (Random.Range(-this.maxDamageVariationPercent, this.maxDamageVariationPercent) / 100f * this.damagePerSecond)) * 0.200000002980232));
            bool isAchievement = lastVictimRefs.characterClassManager.Classes.SafeGet(lastVictimRefs.characterClassManager.CurClass).team == Team.SCP;
            if (!refHub.playerStats.HurtPlayer(new PlayerStats.HitInfo((float) num, refHub.nicknameSync.MyNick + " (" + refHub.characterClassManager.UserId + ")", DamageTypes.MicroHid, refHub.queryProcessor.PlayerId), player))
              isAchievement = false;
            this.TargetSendHitmarker(isAchievement);
          }
        }
      }
    }
  }

  [ServerCallback]
  private void UpdateServerside()
  {
    if (!NetworkServer.active)
      return;
    if (this.refHub.inventory.curItem == ItemType.MicroHID)
    {
      if ((double) this.GetEnergy() != (double) this.Energy)
        this.ChangeEnergy(this.Energy);
    }
    else
    {
      foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.refHub.inventory.items)
      {
        if (syncItemInfo.id == ItemType.MicroHID)
          this.NetworkEnergy = syncItemInfo.durability;
      }
    }
    if ((double) this.keyAntiSpamCooldown > 0.0)
      this.keyAntiSpamCooldown -= Time.deltaTime;
    if (this.refHub.inventory.curItem == ItemType.MicroHID || (double) this.chargeup > 0.0)
    {
      if (this.CurrentHidState != MicroHID.MicroHidState.Idle)
      {
        this.refHub.weaponManager.scp268.ServerDisable();
        this._visionController.MakeNoise(this.CurrentHidState == MicroHID.MicroHidState.Discharge ? 20f : 75f);
      }
      if (this.refHub.inventory.curItem == ItemType.MicroHID)
      {
        if ((double) this.Energy > 0.0 && (double) this.chargeup >= 1.0 && this.SyncKeyCode == (byte) 2)
        {
          if ((double) this.soundEffectPause >= 1.0)
          {
            this.NetworkEnergy = Mathf.Clamp01(this.Energy - Time.deltaTime * this.dischargeEnergyLoss);
            this.DealDamage();
          }
          else
          {
            double num = (double) Mathf.Clamp01(this.Energy - Time.deltaTime * this.speedBasedEnergyLoss.Evaluate(1f));
          }
          this.NetworkCurrentHidState = MicroHID.MicroHidState.Discharge;
        }
        else if ((double) this.Energy > 0.0 && (double) this.chargeup < 1.0 && (this.SyncKeyCode != (byte) 0 && this.CurrentHidState != MicroHID.MicroHidState.RampDown))
        {
          this.chargeup = Mathf.Clamp01(this.chargeup + Time.deltaTime / this.chargeupTime);
          this.NetworkEnergy = Mathf.Clamp01(this.Energy - Time.deltaTime * this.speedBasedEnergyLoss.Evaluate(this.chargeup));
          this.NetworkCurrentHidState = MicroHID.MicroHidState.RampUp;
        }
        else if ((double) this.chargeup > 0.0 && (this.SyncKeyCode == (byte) 0 || (double) this.Energy <= 0.0 || this.CurrentHidState == MicroHID.MicroHidState.RampDown))
        {
          this.chargeup = Mathf.Clamp01(this.chargeup - Time.deltaTime / this.chargedownTime);
          this.NetworkCurrentHidState = MicroHID.MicroHidState.RampDown;
        }
        else if ((double) this.chargeup <= 0.0 && (this.SyncKeyCode == (byte) 0 || (double) this.Energy <= 0.0 || this.CurrentHidState == MicroHID.MicroHidState.RampDown))
          this.NetworkCurrentHidState = MicroHID.MicroHidState.Idle;
        else if ((double) this.chargeup >= 1.0)
        {
          this.NetworkEnergy = Mathf.Clamp01(this.Energy - Time.deltaTime * this.speedBasedEnergyLoss.Evaluate(this.chargeup));
          this.NetworkCurrentHidState = MicroHID.MicroHidState.Spinning;
        }
      }
      else
      {
        this.chargeup = Mathf.Clamp01(this.chargeup - Time.deltaTime / this.chargedownTime);
        this.NetworkCurrentHidState = MicroHID.MicroHidState.RampDown;
      }
    }
    if ((double) this.Energy > 0.0500000007450581)
      return;
    this.NetworkEnergy = 0.0f;
  }

  private void UpdateAudio()
  {
    if (this.refHub.inventory.curItem != ItemType.MicroHID)
    {
      if (this.audioSource.isPlaying)
        this.audioSource.Stop();
    }
    else if (this.CurrentHidState == MicroHID.MicroHidState.Idle || this.CurrentHidState == MicroHID.MicroHidState.RampDown)
    {
      if (this.audioSource.isPlaying && (Object) this.audioSource.clip != (Object) this.audioSpinDown && (Object) this.audioSource.clip != (Object) this.audioFireStopped)
      {
        AudioClip audioClip = this.clientPrevState == MicroHID.MicroHidState.Discharge ? this.audioFireStopped : this.audioSpinDown;
        if ((Object) this.audioSource.clip == (Object) this.audioSpinUp)
        {
          double num = (double) this.audioSource.time / (double) this.audioSource.clip.length;
          double length = (double) audioClip.length;
        }
        this.audioSource.Stop();
        this.audioSource.clip = audioClip;
        this.audioSource.Play();
      }
    }
    else if (this.CurrentHidState == MicroHID.MicroHidState.RampUp || this.CurrentHidState == MicroHID.MicroHidState.Spinning)
    {
      if (!this.audioSource.isPlaying)
      {
        this.audioSource.clip = this.CurrentHidState == MicroHID.MicroHidState.RampUp ? this.audioSpinUp : this.audioSpinLoop;
        this.audioSource.Play();
      }
      else if ((Object) this.audioSource.clip == (Object) this.audioSpinUp && (double) this.audioSpinUp.length - (double) this.audioSource.time < 0.100000001490116)
      {
        this.audioSource.clip = this.audioSpinLoop;
        this.audioSource.Play();
      }
      else if ((Object) this.audioSource.clip != (Object) this.audioSpinUp && (Object) this.audioSource.clip != (Object) this.audioSpinLoop && (Object) this.audioSource.clip != (Object) this.audioFirePaused)
      {
        this.audioSource.Stop();
        this.audioSource.clip = this.CurrentHidState == MicroHID.MicroHidState.RampUp ? this.audioSpinUp : this.audioFirePaused;
        this.audioSource.Play();
      }
    }
    else if ((Object) this.audioSource.clip != (Object) this.audioFire)
    {
      this.audioSource.Stop();
      this.audioSource.clip = this.audioFire;
      this.audioSource.Play();
    }
    this.clientPrevState = this.CurrentHidState;
  }

  private void UpdateAnimations()
  {
  }

  [ClientCallback]
  private void CheckForInput()
  {
    if (NetworkClient.active)
      ;
  }

  [Command]
  private void CmdChangeKeycode(byte code)
  {
    if (this.isServer)
    {
      this.CallCmdChangeKeycode(code);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      NetworkWriterExtensions.WriteByte(writer, code);
      this.SendCommandInternal(typeof (MicroHID), nameof (CmdChangeKeycode), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public MicroHID.MicroHidState NetworkCurrentHidState
  {
    get
    {
      return this.CurrentHidState;
    }
    [param: In] set
    {
      this.SetSyncVar<MicroHID.MicroHidState>(value, ref this.CurrentHidState, 1UL);
    }
  }

  public byte NetworkSyncKeyCode
  {
    get
    {
      return this.SyncKeyCode;
    }
    [param: In] set
    {
      this.SetSyncVar<byte>(value, ref this.SyncKeyCode, 2UL);
    }
  }

  public float NetworkEnergy
  {
    get
    {
      return this.Energy;
    }
    [param: In] set
    {
      this.SetSyncVar<float>(value, ref this.Energy, 4UL);
    }
  }

  protected static void InvokeCmdCmdChangeKeycode(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdChangeKeycode called on client.");
    else
      ((MicroHID) obj).CallCmdChangeKeycode(NetworkReaderExtensions.ReadByte(reader));
  }

  public void CallCmdChangeKeycode(byte code)
  {
    if ((double) this.keyAntiSpamCooldown > 0.0)
      return;
    this.keyAntiSpamCooldown = 0.5f;
    if (code == (byte) 2 && this.SyncKeyCode == (byte) 1)
      this.keyAntiSpamCooldown += 1.7f;
    if (code == (byte) 1 && this.SyncKeyCode == (byte) 2)
      this.keyAntiSpamCooldown += 0.3f;
    this.NetworkSyncKeyCode = code;
  }

  protected static void InvokeRpcTargetSendHitmarker(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSendHitmarker called on server.");
    else
      ((MicroHID) obj).CallTargetSendHitmarker(reader.ReadBoolean());
  }

  public void CallTargetSendHitmarker(bool isAchievement)
  {
    Hitmarker.Hit(2.3f);
    if (!isAchievement)
      return;
    AchievementManager.Achieve("zap", true);
  }

  static MicroHID()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (MicroHID), "CmdChangeKeycode", new NetworkBehaviour.CmdDelegate(MicroHID.InvokeCmdCmdChangeKeycode));
    NetworkBehaviour.RegisterRpcDelegate(typeof (MicroHID), "TargetSendHitmarker", new NetworkBehaviour.CmdDelegate(MicroHID.InvokeRpcTargetSendHitmarker));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32((int) this.CurrentHidState);
      NetworkWriterExtensions.WriteByte(writer, this.SyncKeyCode);
      writer.WriteSingle(this.Energy);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32((int) this.CurrentHidState);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      NetworkWriterExtensions.WriteByte(writer, this.SyncKeyCode);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteSingle(this.Energy);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkCurrentHidState = (MicroHID.MicroHidState) reader.ReadPackedInt32();
      this.NetworkSyncKeyCode = NetworkReaderExtensions.ReadByte(reader);
      this.NetworkEnergy = reader.ReadSingle();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkCurrentHidState = (MicroHID.MicroHidState) reader.ReadPackedInt32();
      if ((num & 2L) != 0L)
        this.NetworkSyncKeyCode = NetworkReaderExtensions.ReadByte(reader);
      if ((num & 4L) == 0L)
        return;
      this.NetworkEnergy = reader.ReadSingle();
    }
  }

  public enum MicroHidState
  {
    Idle,
    RampUp,
    RampDown,
    Spinning,
    Discharge,
  }
}
