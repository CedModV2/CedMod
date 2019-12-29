// Decompiled with JetBrains decompiler
// Type: BrowserLerp
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class BrowserLerp : MonoBehaviour
{
  public float speed = 2f;
  private Vector3 prevPos;
  private RectTransform rectTransform;
  private Vector3 targetPos;

  private void Start()
  {
    this.rectTransform = this.GetComponent<RectTransform>();
  }

  private void LateUpdate()
  {
    this.targetPos += this.rectTransform.localPosition - this.prevPos;
    this.rectTransform.localPosition = this.prevPos;
    this.rectTransform.localPosition = Vector3.Lerp(this.rectTransform.localPosition, this.targetPos, (float) ((double) Time.deltaTime * (double) this.speed * 4.0));
    this.prevPos = this.rectTransform.localPosition;
  }
}
