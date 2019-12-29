// Decompiled with JetBrains decompiler
// Type: FootstepHandler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class FootstepHandler : MonoBehaviour
{
  private Vector2 localCameraPosition = new Vector2(0.0f, 0.0f);
  [Header("Footstep Volume")]
  public bool useDefaultFootstepVolume = true;
  public float defaultFootstepVolume = 1f;
  public bool defaultFootstepVolumeToSpeed = true;
  public float footstepVolumeSpeedMultiplier = 1f;
  [SerializeField]
  private AnimationCurve footstepVolume = new AnimationCurve(AnimationCurveUtils.MakeLinearKeyframes(new Keyframe(0.0f, 0.0f), new Keyframe(2f, 2f)))
  {
    preWrapMode = WrapMode.Once,
    postWrapMode = WrapMode.Once
  };
  [SerializeField]
  private float maxFootstepsPerSecond = 4f;
  [Header("Walking Camera Bob Multiplier")]
  public bool defaultWalkBobMultiplierToSpeed = true;
  [SerializeField]
  private AnimationCurve walkBobMultiplier = new AnimationCurve(AnimationCurveUtils.MakeLinearKeyframes(new Keyframe(-2f, 2f), new Keyframe(0.0f, 0.0f), new Keyframe(2f, 2f)))
  {
    preWrapMode = WrapMode.Once,
    postWrapMode = WrapMode.Once
  };
  [Header("Strafing Camera Bob")]
  public bool defaultStrafeXBobToWalkXBob = true;
  public bool defaultStrafeYBobToWalkYBob = true;
  [Header("Strafing Camera Bob Multiplier")]
  public bool defaultStrafeBobMultiplierToSpeed = true;
  public bool defaultStrafeBobMultiplierToWalkBobMultiplier = true;
  private bool playLandingBob;
  private float currentLandingBobTime;
  private bool disableHeadBob;
  private float rateLimiterTimer;
  private CharacterClassManager _ccm;
  [SerializeField]
  private bool useFootstepVolume;
  [Header("Walking Camera Bob")]
  [SerializeField]
  private bool useWalkXBob;
  [SerializeField]
  private AnimationCurve walkXBob;
  [SerializeField]
  private bool useWalkYBob;
  [SerializeField]
  private AnimationCurve walkYBob;
  [SerializeField]
  private bool useWalkBobMultiplier;
  [SerializeField]
  private bool useStrafeXBob;
  [SerializeField]
  private AnimationCurve strafeXBob;
  [SerializeField]
  private bool useStrafeYBob;
  [SerializeField]
  private AnimationCurve strafeYBob;
  [SerializeField]
  private bool useStrafeBobMultiplier;
  [SerializeField]
  private AnimationCurve strafeBobMultiplier;
  [Header("Landing Camera Bob")]
  [SerializeField]
  private bool useLandingYBob;
  [SerializeField]
  private AnimationCurve landingYBob;
  [Tooltip("In seconds")]
  public float landingBobLength;
  private bool hasFootstepSync;
  private FootstepSync footstepSync;
  private bool hasCameraTransform;
  private Transform fpsCameraTransform;
  private bool hasAnimator;
  private Animator animator;
  private bool hasAnimationController;
  private AnimationController animationController;

  public FootstepSync FootstepSync
  {
    get
    {
      return this.footstepSync;
    }
    set
    {
      this.footstepSync = value;
      this.hasFootstepSync = (Object) value != (Object) null;
    }
  }

  public Transform FpsCameraTransform
  {
    get
    {
      return this.fpsCameraTransform;
    }
    set
    {
      this.fpsCameraTransform = value;
      this.hasCameraTransform = (Object) value != (Object) null;
    }
  }

  public Animator Animator
  {
    get
    {
      return this.animator;
    }
    set
    {
      this.animator = value;
      this.hasAnimator = (Object) value != (Object) null;
    }
  }

  public AnimationController AnimationController
  {
    get
    {
      return this.animationController;
    }
    set
    {
      this.animationController = value;
      this.hasAnimationController = (Object) value != (Object) null;
    }
  }

  public AnimationCurve FootstepVolume
  {
    get
    {
      return !this.useFootstepVolume ? (AnimationCurve) null : this.footstepVolume;
    }
    set
    {
      this.footstepVolume = value;
      this.useFootstepVolume = value != null;
    }
  }

  public AnimationCurve WalkXBob
  {
    get
    {
      return !this.useWalkXBob ? (AnimationCurve) null : this.walkXBob;
    }
    set
    {
      this.walkXBob = value;
      this.useWalkXBob = value != null;
    }
  }

  public AnimationCurve WalkYBob
  {
    get
    {
      return !this.useWalkYBob ? (AnimationCurve) null : this.walkYBob;
    }
    set
    {
      this.walkYBob = value;
      this.useWalkYBob = value != null;
    }
  }

  public AnimationCurve WalkBobMultiplier
  {
    get
    {
      return !this.useWalkBobMultiplier ? (AnimationCurve) null : this.walkBobMultiplier;
    }
    set
    {
      this.walkBobMultiplier = value;
      this.useWalkBobMultiplier = value != null;
    }
  }

  public AnimationCurve StrafeXBob
  {
    get
    {
      if (this.useStrafeXBob)
        return this.strafeXBob;
      return !this.defaultStrafeXBobToWalkXBob ? (AnimationCurve) null : this.WalkXBob;
    }
    set
    {
      this.strafeXBob = value;
      this.useStrafeXBob = value != null;
    }
  }

  public AnimationCurve StrafeYBob
  {
    get
    {
      if (this.useStrafeYBob)
        return this.strafeYBob;
      return !this.defaultStrafeYBobToWalkYBob ? (AnimationCurve) null : this.WalkYBob;
    }
    set
    {
      this.strafeYBob = value;
      this.useStrafeYBob = value != null;
    }
  }

  public AnimationCurve StrafeBobMultiplier
  {
    get
    {
      if (this.useStrafeBobMultiplier)
        return this.strafeBobMultiplier;
      return !this.defaultStrafeBobMultiplierToWalkBobMultiplier ? (AnimationCurve) null : this.WalkBobMultiplier;
    }
    set
    {
      this.strafeBobMultiplier = value;
      this.useStrafeBobMultiplier = value != null;
    }
  }

  public AnimationCurve LandingYBob
  {
    get
    {
      return !this.useLandingYBob ? (AnimationCurve) null : this.landingYBob;
    }
    set
    {
      this.landingYBob = value;
      this.useLandingYBob = value != null;
    }
  }

  private void Start()
  {
    this.Animator = this.GetComponent<Animator>();
    this.disableHeadBob = !PlayerPrefsSl.Get("HeadBob", true);
    this._ccm = this.GetComponentInParent<CharacterClassManager>();
  }

  public void PlayFootstep(AnimationEvent animationEvent)
  {
    if (animationEvent == null)
      return;
    this.PlayFootstepWithParams(animationEvent.intParameter, animationEvent.floatParameter, false);
  }

  public void PlayFootstepRun(AnimationEvent animationEvent)
  {
    if (animationEvent == null)
      return;
    this.PlayFootstepWithParams(animationEvent.intParameter, animationEvent.floatParameter, true);
  }

  public void PlayFootstepWithParams(int speedThreshold = 0, float volume = 0.0f, bool running = false)
  {
    if (!this.hasFootstepSync || !this.hasAnimationController)
      return;
    float animSpeedForward = this.animationController.animSpeedForward;
    float animSpeedRight = this.animationController.animSpeedRight;
    float animSpeedMagnitude = this.animationController.animSpeedMagnitude;
    if (speedThreshold > 0 && speedThreshold < FootstepThreshold.Thresholds.Length && !FootstepThreshold.Thresholds[speedThreshold].ContainsValue(animSpeedForward, animSpeedRight))
      return;
    if ((double) volume <= 0.0)
    {
      if (this.useFootstepVolume)
        volume = this.footstepVolume.Evaluate(animSpeedMagnitude);
      else if (this.defaultFootstepVolumeToSpeed && (double) this.footstepVolumeSpeedMultiplier > 0.0)
        volume = animSpeedMagnitude * this.footstepVolumeSpeedMultiplier;
      else if (this.useDefaultFootstepVolume)
        volume = this.defaultFootstepVolume;
    }
    if ((double) volume <= 0.0)
      return;
    this.footstepSync.PlayFootstepSound(volume, running, false);
  }

  public bool CanExecuteFootstepSound()
  {
    if ((double) this.rateLimiterTimer <= (double) (1f / this.maxFootstepsPerSecond))
      return false;
    this.rateLimiterTimer = 0.0f;
    return true;
  }

  private void Update()
  {
    if ((double) this.rateLimiterTimer < 10.0)
      this.rateLimiterTimer += Time.deltaTime;
    if (this.disableHeadBob || !this.hasCameraTransform || (!this.hasAnimator || !this.hasAnimationController) || this._ccm.NoclipEnabled)
      return;
    this.localCameraPosition.x = 0.0f;
    this.localCameraPosition.y = 0.0f;
    double num1 = (double) this.animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0;
    float animSpeedForward = this.animationController.animSpeedForward;
    float animSpeedRight = this.animationController.animSpeedRight;
    float time1 = (float) num1 * (float) this.animationController.animSpeedForwardDir;
    float time2 = (float) num1 * (float) this.animationController.animSpeedRightDir;
    float num2 = 1f;
    float num3 = 1f;
    if (this.useWalkBobMultiplier)
      num2 = this.walkBobMultiplier.Evaluate(animSpeedForward);
    else if (this.defaultWalkBobMultiplierToSpeed)
      num2 = Mathf.Abs(animSpeedForward);
    if (this.useStrafeBobMultiplier)
      num3 = this.strafeBobMultiplier.Evaluate(animSpeedRight);
    else if (this.defaultStrafeBobMultiplierToSpeed)
      num3 = Mathf.Abs(animSpeedRight);
    else if (this.defaultStrafeBobMultiplierToWalkBobMultiplier && this.useWalkBobMultiplier)
      num3 = this.walkBobMultiplier.Evaluate(animSpeedRight);
    if (this.useWalkXBob)
      this.localCameraPosition.x += this.walkXBob.Evaluate(time1) * num2;
    if (this.useWalkYBob)
      this.localCameraPosition.y += this.walkYBob.Evaluate(time1) * num2;
    if (this.useStrafeXBob)
      this.localCameraPosition.x += this.strafeXBob.Evaluate(time2) * num3;
    else if (this.defaultStrafeXBobToWalkXBob && this.useWalkXBob)
      this.localCameraPosition.x += this.walkXBob.Evaluate(time2) * num3;
    if (this.useStrafeYBob)
      this.localCameraPosition.y += this.strafeYBob.Evaluate(time2) * num3;
    else if (this.defaultStrafeYBobToWalkYBob && this.useWalkYBob)
      this.localCameraPosition.y += this.walkYBob.Evaluate(time2) * num3;
    if (this.playLandingBob)
    {
      this.currentLandingBobTime += Time.deltaTime;
      if ((double) this.currentLandingBobTime > (double) this.landingBobLength)
        this.playLandingBob = false;
      else if (this.useLandingYBob)
        this.localCameraPosition.y += this.landingYBob.Evaluate(this.currentLandingBobTime);
    }
    this.fpsCameraTransform.localPosition = (Vector3) this.localCameraPosition;
  }

  public void PlayLandingCameraBob(float timeFromNow = 0.0f)
  {
    this.currentLandingBobTime = -timeFromNow;
    this.playLandingBob = true;
  }
}
