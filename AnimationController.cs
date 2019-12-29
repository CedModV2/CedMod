// Decompiled with JetBrains decompiler
// Type: AnimationController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using Security;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AnimationController : NetworkBehaviour
{
  private static readonly List<AnimationController> controllers = new List<AnimationController>();
  private static readonly int Cuffed = Animator.StringToHash(nameof (Cuffed));
  private static readonly int CurItem = Animator.StringToHash(nameof (CurItem));
  private static readonly int PlayUniversalAnimation = Animator.StringToHash(nameof (PlayUniversalAnimation));
  private static readonly int HeadRotation = Animator.StringToHash(nameof (HeadRotation));
  private static readonly int Running = Animator.StringToHash(nameof (Running));
  private static readonly int SmoothRun = Animator.StringToHash("SmoothedRunning");
  private static readonly int Strafe = Animator.StringToHash(nameof (Strafe));
  private static readonly int Jump = Animator.StringToHash(nameof (Jump));
  public static readonly int SpectatorHead = Animator.StringToHash(nameof (SpectatorHead));
  internal static readonly int Speed = Animator.StringToHash(nameof (Speed));
  internal static readonly int Direction = Animator.StringToHash(nameof (Direction));
  private float lastWalkSpeed = -1f;
  private float lastRunSpeed = -1f;
  private readonly RateLimit dirChangeFootstepRateLimit = new RateLimit(1, 0.3f, (NetworkConnection) null);
  public AnimationController.AnimAudioClip[] clips;
  public AudioSource walkSource;
  public AudioSource runSource;
  public AudioSource gunSource;
  public Animator animator;
  public Animator handAnimator;
  [SyncVar]
  public bool sneaking;
  private bool prevSneaking;
  [SyncVar]
  public bool sprinting;
  private bool prevSprinting;
  [SyncVar]
  public int curAnim;
  [SyncVar]
  public Vector2 speed;
  public AnimationCurve animationSpeedCurve;
  private FirstPersonController fpc;
  private Inventory inv;
  private PlyMovementSync pms;
  private Scp096PlayerScript scp096;
  private Scp939PlayerScript scp939;
  private DistanceTo dt;
  private CharacterClassManager ccm;
  private RateLimit _mSyncRateLimit;
  private RateLimit _sneakRateLimit;
  private Handcuffs cuffs;
  private FootstepSync footstepSync;
  private ItemType prevItem;
  public float animSpeedForward;
  public float animSpeedRight;
  public int animSpeedForwardDir;
  public int animSpeedRightDir;
  public float animSpeedMagnitude;
  private float prevRotX;
  private bool antiposed;

  private void Start()
  {
    PlayerRateLimitHandler component = this.GetComponent<PlayerRateLimitHandler>();
    this._mSyncRateLimit = component.RateLimits[6];
    this._sneakRateLimit = component.RateLimits[6];
    ReferenceHub hub = ReferenceHub.GetHub(this.gameObject);
    this.cuffs = hub.handcuffs;
    this.ccm = hub.characterClassManager;
    this.pms = hub.plyMovementSync;
    this.inv = hub.inventory;
    this.scp096 = this.GetComponent<Scp096PlayerScript>();
    this.scp939 = this.GetComponent<Scp939PlayerScript>();
    this.dt = this.GetComponent<DistanceTo>();
    this.fpc = this.GetComponent<FirstPersonController>();
    this.footstepSync = this.GetComponent<FootstepSync>();
    if (this.isLocalPlayer)
      return;
    AnimationController.controllers.Add(this);
    this.Invoke("RefreshItems", 6f);
  }

  private void OnDestroy()
  {
    if (this.isLocalPlayer)
      return;
    AnimationController.controllers.Remove(this);
  }

  private float GetCameraRotation()
  {
    if ((UnityEngine.Object) this.pms == (UnityEngine.Object) null)
      return 0.0f;
    float x = this.pms.Rotations.x;
    float num = Mathf.Lerp(this.prevRotX, ((double) x > 270.0 ? x - 360f : x) / 3f, Time.deltaTime * 15f);
    this.prevRotX = num;
    return num;
  }

  public void PlaySound(int id, bool isGun)
  {
    if (this.isLocalPlayer)
      return;
    if (isGun)
      this.gunSource.PlayOneShot(this.clips[id].audio);
    else
      this.runSource.PlayOneShot(this.clips[id].audio);
  }

  public void PlaySound(string label, bool isGun)
  {
    if (this.isLocalPlayer)
      return;
    int index1 = 0;
    for (ushort index2 = 0; (int) index2 < this.clips.Length; ++index2)
    {
      if (this.clips[(int) index2].clipName == label)
        index1 = (int) index2;
    }
    if (isGun)
      this.gunSource.PlayOneShot(this.clips[index1].audio);
    else
      this.runSource.PlayOneShot(this.clips[index1].audio);
  }

  public void DoAnimation(string trigger)
  {
    if (this.isLocalPlayer || !((UnityEngine.Object) this.handAnimator != (UnityEngine.Object) null))
      return;
    this.handAnimator.SetTrigger(trigger);
  }

  private void FixedUpdate()
  {
    if (RoundStart.singleton.Timer != (short) -1 || this.isLocalPlayer)
      return;
    if ((UnityEngine.Object) this.inv != (UnityEngine.Object) null && this.prevItem != this.inv.curItem)
    {
      this.prevItem = this.inv.curItem;
      this.RefreshItems();
    }
    this.RecieveData();
  }

  private void RefreshItems()
  {
    foreach (MonoBehaviour componentsInChild in this.GetComponentsInChildren<HandPart>(true))
      componentsInChild.Invoke("UpdateItem", 0.3f);
  }

  public void SetState(int i)
  {
    this.NetworkcurAnim = i;
  }

  private IEnumerator<float> _StartAntiposing()
  {
    if (this.ccm.CurClass >= RoleType.Scp173 && this.ccm.Classes.Get(this.ccm.CurClass).team == Team.SCP)
    {
      yield return float.NegativeInfinity;
      this.handAnimator.gameObject.SetActive(false);
      yield return float.NegativeInfinity;
      this.handAnimator.gameObject.SetActive(true);
      yield return float.NegativeInfinity;
      this.handAnimator.updateMode = AnimatorUpdateMode.AnimatePhysics;
      yield return float.NegativeInfinity;
      this.handAnimator.updateMode = AnimatorUpdateMode.Normal;
      yield return float.NegativeInfinity;
      this.handAnimator.gameObject.SetActive(false);
      yield return float.NegativeInfinity;
      this.handAnimator.gameObject.SetActive(true);
    }
  }

  public void OnChangeClass()
  {
    this.antiposed = false;
  }

  public void RecieveData()
  {
    bool flag;
    if (TutorialManager.status)
      flag = true;
    else if ((flag = this.dt.IsInRange()) && !this.antiposed && (UnityEngine.Object) this.handAnimator != (UnityEngine.Object) null)
    {
      Timing.RunCoroutine(this._StartAntiposing(), Segment.FixedUpdate);
      this.antiposed = true;
    }
    if ((UnityEngine.Object) this.handAnimator != (UnityEngine.Object) null)
      this.handAnimator.enabled = flag;
    if ((UnityEngine.Object) this.animator == (UnityEngine.Object) null)
      return;
    this.animator.enabled = flag || this.ccm.CurClass == RoleType.Scp106;
    if (!flag)
      return;
    this.CalculateAnimation(this.curAnim, this.speed);
    if ((UnityEngine.Object) this.handAnimator == (UnityEngine.Object) null)
    {
      foreach (Animator componentsInChild in this.animator.GetComponentsInChildren<Animator>())
      {
        if (!((UnityEngine.Object) componentsInChild == (UnityEngine.Object) this.animator))
          this.handAnimator = componentsInChild;
      }
    }
    else
    {
      this.handAnimator.SetInteger(AnimationController.CurItem, (int) this.inv.curItem);
      this.handAnimator.SetBool(AnimationController.PlayUniversalAnimation, this.inv.curItem > ItemType.None && this.inv.GetItemByID(this.inv.curItem).useDefaultAnimation);
      this.handAnimator.SetBool(AnimationController.Cuffed, this.cuffs.CufferId >= 0);
      this.handAnimator.SetFloat(AnimationController.HeadRotation, this.GetCameraRotation());
      this.handAnimator.SetFloat(AnimationController.Running, (double) this.speed.x != 0.0 ? (this.curAnim == 1 ? 2f : 1f) : 0.0f);
      this.handAnimator.SetFloat(AnimationController.SmoothRun, Mathf.Lerp(this.handAnimator.GetFloat(AnimationController.SmoothRun), this.handAnimator.GetFloat(AnimationController.Running), 0.1f));
    }
  }

  private void CalculateAnimation(int animState, Vector2 movementSpeed)
  {
    this.animator.SetBool(AnimationController.Jump, animState == 2);
    float f1 = 0.0f;
    float f2 = 0.0f;
    float f3 = 0.0f;
    if (animState != 2 && (UnityEngine.Object) this.ccm != (UnityEngine.Object) null && this.ccm.Classes.CheckBounds(this.ccm.CurClass))
    {
      float num1 = this.scp096.enraged == Scp096PlayerScript.RageState.Enraged ? 2.8f : 1f;
      float time1 = this.ccm.Classes.SafeGet(this.ccm.CurClass).walkSpeed * num1;
      float time2 = this.ccm.Classes.SafeGet(this.ccm.CurClass).runSpeed * num1;
      if ((double) time1 != (double) this.lastWalkSpeed || (double) time2 != (double) this.lastRunSpeed)
      {
        List<Keyframe> keyframeList = new List<Keyframe>()
        {
          new Keyframe(0.0f, 0.0f),
          new Keyframe(-time1, -1f),
          new Keyframe(time1, 1f)
        };
        if ((double) time1 > 1.0)
        {
          keyframeList.Add(new Keyframe(-1f, -0.9f));
          keyframeList.Add(new Keyframe(1f, 0.9f));
        }
        if ((double) time2 > (double) time1)
        {
          keyframeList.Add(new Keyframe(-time2, -2f));
          keyframeList.Add(new Keyframe(time2, 2f));
        }
        this.animationSpeedCurve = new AnimationCurve(keyframeList.ToArray())
        {
          preWrapMode = WrapMode.Once,
          postWrapMode = WrapMode.Once
        };
        this.lastWalkSpeed = time1;
        this.lastRunSpeed = time2;
      }
      if ((double) movementSpeed.x != 0.0 && (double) movementSpeed.y != 0.0)
      {
        float magnitude = movementSpeed.magnitude;
        if ((double) Mathf.Abs(movementSpeed.x) > (double) Mathf.Abs(movementSpeed.y))
        {
          f3 = this.animationSpeedCurve.Evaluate((double) movementSpeed.x < 0.0 ? -magnitude : magnitude);
          f1 = f3;
        }
        else
        {
          f3 = this.animationSpeedCurve.Evaluate((double) movementSpeed.y < 0.0 ? -magnitude : magnitude);
          f2 = f3;
        }
      }
      else if ((double) movementSpeed.x != 0.0)
      {
        f1 = this.animationSpeedCurve.Evaluate(movementSpeed.x);
        f3 = f1;
      }
      else if ((double) movementSpeed.y != 0.0)
      {
        f2 = this.animationSpeedCurve.Evaluate(movementSpeed.y);
        f3 = f2;
      }
      int num2 = (double) Mathf.Abs(f1) <= 0.05 ? 1 : 0;
      bool flag = (double) Mathf.Abs(f2) <= 0.05;
      if (num2 != 0)
        f1 = 0.0f;
      if (flag)
        f2 = 0.0f;
      if ((num2 & (flag ? 1 : 0)) != 0)
        f3 = 0.0f;
    }
    this.animSpeedForward = f1;
    this.animSpeedRight = f2;
    this.animSpeedMagnitude = Mathf.Abs(f3);
    int animSpeedForwardDir = this.animSpeedForwardDir;
    int animSpeedRightDir = this.animSpeedRightDir;
    this.animSpeedForwardDir = AnimationController.VelocityToDirection(this.animSpeedForward);
    this.animSpeedRightDir = AnimationController.VelocityToDirection(this.animSpeedRight);
    this.animator.SetFloat(AnimationController.Speed, f1, 0.1f, Time.deltaTime);
    this.animator.SetFloat(AnimationController.Direction, f2, 0.1f, Time.deltaTime);
    this.animator.SetBool(AnimationController.Strafe, (double) Mathf.Abs(this.animSpeedRight) > 1.40129846432482E-45);
  }

  private static int VelocityToDirection(float velocity)
  {
    if ((double) Mathf.Abs(velocity) <= 1.40129846432482E-45)
      return 0;
    return (double) velocity >= 0.0 ? 1 : -1;
  }

  [Command]
  private void CmdSneakHasChanged(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSneakHasChanged(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (AnimationController), nameof (CmdSneakHasChanged), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  private void CmdSprintHasChanged(bool b)
  {
    if (this.isServer)
    {
      this.CallCmdSprintHasChanged(b);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(b);
      this.SendCommandInternal(typeof (AnimationController), nameof (CmdSprintHasChanged), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ClientCallback]
  private void TransmitData(int state, Vector2 v2)
  {
    if (!NetworkClient.active)
      return;
    this.CmdSyncData(state, v2);
  }

  [Command]
  private void CmdSyncData(int state, Vector2 v2)
  {
    if (this.isServer)
    {
      this.CallCmdSyncData(state, v2);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(state);
      writer.WriteVector2(v2);
      this.SendCommandInternal(typeof (AnimationController), nameof (CmdSyncData), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  static AnimationController()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (AnimationController), "CmdSneakHasChanged", new NetworkBehaviour.CmdDelegate(AnimationController.InvokeCmdCmdSneakHasChanged));
    NetworkBehaviour.RegisterCommandDelegate(typeof (AnimationController), "CmdSprintHasChanged", new NetworkBehaviour.CmdDelegate(AnimationController.InvokeCmdCmdSprintHasChanged));
    NetworkBehaviour.RegisterCommandDelegate(typeof (AnimationController), "CmdSyncData", new NetworkBehaviour.CmdDelegate(AnimationController.InvokeCmdCmdSyncData));
  }

  private void MirrorProcessed()
  {
  }

  public bool Networksneaking
  {
    get
    {
      return this.sneaking;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.sneaking, 1UL);
    }
  }

  public bool Networksprinting
  {
    get
    {
      return this.sprinting;
    }
    [param: In] set
    {
      this.SetSyncVar<bool>(value, ref this.sprinting, 2UL);
    }
  }

  public int NetworkcurAnim
  {
    get
    {
      return this.curAnim;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.curAnim, 4UL);
    }
  }

  public Vector2 Networkspeed
  {
    get
    {
      return this.speed;
    }
    [param: In] set
    {
      this.SetSyncVar<Vector2>(value, ref this.speed, 8UL);
    }
  }

  protected static void InvokeCmdCmdSneakHasChanged(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSneakHasChanged called on client.");
    else
      ((AnimationController) obj).CallCmdSneakHasChanged(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSprintHasChanged(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSprintHasChanged called on client.");
    else
      ((AnimationController) obj).CallCmdSprintHasChanged(reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSyncData(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSyncData called on client.");
    else
      ((AnimationController) obj).CallCmdSyncData(reader.ReadPackedInt32(), reader.ReadVector2());
  }

  public void CallCmdSneakHasChanged(bool b)
  {
    if (!this._sneakRateLimit.CanExecute(true))
      return;
    if (b && this.ccm.Classes.SafeGet(this.ccm.CurClass).team == Team.SCP)
      b = false;
    if (b == this.sneaking)
      return;
    this.Networksneaking = b;
  }

  public void CallCmdSprintHasChanged(bool b)
  {
    if (!this._sneakRateLimit.CanExecute(true) || b == this.sprinting)
      return;
    this.Networksprinting = b;
  }

  public void CallCmdSyncData(int state, Vector2 v2)
  {
    if (!this._mSyncRateLimit.CanExecute(true))
      return;
    this.NetworkcurAnim = state;
    this.Networkspeed = v2;
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteBoolean(this.sneaking);
      writer.WriteBoolean(this.sprinting);
      writer.WritePackedInt32(this.curAnim);
      writer.WriteVector2(this.speed);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteBoolean(this.sneaking);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteBoolean(this.sprinting);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WritePackedInt32(this.curAnim);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteVector2(this.speed);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networksneaking = reader.ReadBoolean();
      this.Networksprinting = reader.ReadBoolean();
      this.NetworkcurAnim = reader.ReadPackedInt32();
      this.Networkspeed = reader.ReadVector2();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.Networksneaking = reader.ReadBoolean();
      if ((num & 2L) != 0L)
        this.Networksprinting = reader.ReadBoolean();
      if ((num & 4L) != 0L)
        this.NetworkcurAnim = reader.ReadPackedInt32();
      if ((num & 8L) == 0L)
        return;
      this.Networkspeed = reader.ReadVector2();
    }
  }

  [Serializable]
  public class AnimAudioClip
  {
    public string clipName;
    public AudioClip audio;
  }
}
