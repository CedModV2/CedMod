// Decompiled with JetBrains decompiler
// Type: GameMenuButton
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class GameMenuButton : MonoBehaviour
{
  public Vector3 normalPos;
  public Vector3 focusedPos;
  public AnimationCurve anim;
  private bool isFocused;
  private float status;
  private RectTransform rectTransform;

  private void Start()
  {
    this.rectTransform = this.GetComponent<RectTransform>();
  }

  public void Focus(bool b)
  {
    this.isFocused = b;
  }

  private void Update()
  {
    this.status += Time.deltaTime * (this.isFocused ? 1f : -1f);
    this.status = Mathf.Clamp01(this.status);
    this.rectTransform.localPosition = this.normalPos + (this.focusedPos - this.normalPos) * this.anim.Evaluate(this.status);
  }
}
