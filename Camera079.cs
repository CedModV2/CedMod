// Decompiled with JetBrains decompiler
// Type: Camera079
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class Camera079 : MonoBehaviour
{
  [Space]
  public float stepSpeed = 1f;
  public string cameraName;
  public ushort cameraId;
  public bool isMain;
  [Space]
  public float curPitch;
  public float curRot;
  private float smoothPitch;
  private float smoothRot;
  [Space]
  public float minRot;
  public float maxRot;
  public float minPitch;
  public float maxPitch;
  public Material activatedMaterial;
  public Material deactivatedMaterial;
  public MeshRenderer renderer;
  [Space]
  public Transform head;
  public Transform targetPosition;
  [SerializeField]
  private float timeToAnimate;

  private void FixedUpdate()
  {
    if ((double) this.timeToAnimate <= 0.0)
      return;
    this.timeToAnimate -= 0.02f;
    this.Animate();
  }

  private void Awake()
  {
    this.targetPosition = this.GetComponentInChildren<CameraPos079>(true).transform;
    Object.Destroy((Object) this.targetPosition.GetComponent<CameraPos079>());
  }

  private void Start()
  {
    this.UpdatePosition(this.curRot, this.curPitch);
  }

  public void UpdatePosition(float _curRot, float _curPitch)
  {
    this.timeToAnimate = 5f;
    this.curRot = _curRot;
    this.curPitch = _curPitch;
  }

  private void Animate()
  {
    this.curRot = Mathf.Clamp(this.curRot, this.minRot, this.maxRot);
    this.curPitch = Mathf.Clamp(this.curPitch, this.minPitch, this.maxPitch);
    bool flag = false;
    foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
    {
      if ((Object) instance.currentCamera == (Object) this)
        flag = true;
    }
    if ((Object) this.renderer != (Object) null)
      this.renderer.sharedMaterial = flag ? this.activatedMaterial : this.deactivatedMaterial;
    if ((double) this.smoothRot > (double) this.curRot + (double) this.stepSpeed)
      this.smoothRot -= this.stepSpeed;
    if ((double) this.smoothRot < (double) this.curRot - (double) this.stepSpeed)
      this.smoothRot += this.stepSpeed;
    if ((double) this.smoothPitch > (double) this.curPitch + (double) this.stepSpeed)
      this.smoothPitch -= this.stepSpeed;
    if ((double) this.smoothPitch < (double) this.curPitch - (double) this.stepSpeed)
      this.smoothPitch += this.stepSpeed;
    this.head.localRotation = !((Object) Interface079.lply != (Object) null) || !((Object) Interface079.lply.currentCamera == (Object) this) ? Quaternion.Euler(this.smoothPitch, this.smoothRot, 0.0f) : Quaternion.Lerp(this.head.localRotation, Quaternion.Euler(this.curPitch, this.curRot, 0.0f), Time.deltaTime * 12f);
    if (!flag)
      return;
    this.timeToAnimate = 5f;
  }
}
