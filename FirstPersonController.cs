// Decompiled with JetBrains decompiler
// Type: FirstPersonController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Assets._Scripts.Dissonance;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;

[RequireComponent(typeof (CharacterController))]
[RequireComponent(typeof (AudioSource))]
public class FirstPersonController : MonoBehaviour
{
  [SerializeField]
  private FOVKick MFovKick = new FOVKick();
  [SerializeField]
  private CurveControlledBob MHeadBob = new CurveControlledBob();
  public Vector3 MMoveDir = Vector3.zero;
  public float SmoothSize = 0.5f;
  public float ZoomSlowdown = 1f;
  [SerializeField]
  public float WalkSpeed;
  [SerializeField]
  public float MRunSpeed;
  [SerializeField]
  public float MJumpSpeed;
  [SerializeField]
  private float MStickToGroundForce;
  [SerializeField]
  private float MGravityMultiplier;
  [SerializeField]
  private bool MIsWalking;
  [SerializeField]
  private bool MUseFovKick;
  [SerializeField]
  private float MStepInterval;
  public Camera MCamera;
  public Vector2 PlySpeed;
  public CharacterController MCharacterController;
  public MouseLook MMouseLook;
  public int AnimationId;
  public float BlinkAddition;
  private DissonanceUserSetup dissonance;

  private void Start()
  {
    this.dissonance = this.GetComponentInChildren<DissonanceUserSetup>();
  }

  private void OnDisable()
  {
  }

  private void Update()
  {
  }

  private void FixedUpdate()
  {
  }

  private void OnControllerColliderHit(ControllerColliderHit hit)
  {
  }

  private void LateUpdate()
  {
  }
}
