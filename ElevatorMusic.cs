// Decompiled with JetBrains decompiler
// Type: ElevatorMusic
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class ElevatorMusic : MonoBehaviour
{
  public Animator TargetAnimator;
  public Transform SpectCamera;
  private Transform roomRoot;
  private AudioSource src;
  private int animHash;

  private void Start()
  {
    this.src = this.GetComponent<AudioSource>();
    this.src.maxDistance = 7.5f;
    this.animHash = Animator.StringToHash("isOpen");
    Transform parent = this.transform.parent;
    while ((Object) parent != (Object) null && !parent.transform.name.StartsWith("Map_") && !(parent.gameObject.tag == "Room"))
      parent = parent.transform.parent;
    if (!((Object) parent != (Object) null))
      return;
    this.roomRoot = parent;
  }

  public bool IsInThePool()
  {
    return (double) Mathf.Abs(this.SpectCamera.position.y - this.roomRoot.position.y) <= 10.0 && (double) Mathf.Abs(this.SpectCamera.position.x - this.roomRoot.position.x) <= 4.5 && (double) Mathf.Abs(this.SpectCamera.position.z - this.roomRoot.position.z) <= 4.5;
  }

  private void LateUpdate()
  {
    if (!this.IsInThePool())
      return;
    this.src.volume = Mathf.Clamp(this.src.volume + Time.deltaTime * (this.TargetAnimator.GetBool(this.animHash) ? 0.2f : -0.2f), 0.0f, 0.5f);
  }
}
