// Decompiled with JetBrains decompiler
// Type: ContentFitter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

public class ContentFitter : MonoBehaviour
{
  private List<RectTransform> transforms = new List<RectTransform>();
  public bool continuousUpdate;
  public RectTransform targetTransform;

  private void LateUpdate()
  {
    if (!this.continuousUpdate)
      return;
    this.continuousUpdate = false;
    this.Fit();
  }

  public void Fit()
  {
    this.transforms.Clear();
    foreach (RectTransform componentsInChild in this.GetComponentsInChildren<RectTransform>())
    {
      if ((Object) componentsInChild != (Object) this.GetComponent<RectTransform>())
        this.transforms.Add(componentsInChild);
    }
    Vector2 vector2_1 = new Vector2(1E+09f, -1E+09f);
    Vector2 vector2_2 = new Vector2(-1E+09f, 1E+09f);
    foreach (RectTransform transform in this.transforms)
    {
      Vector2 vector2_3 = new Vector2(transform.localPosition.x - transform.sizeDelta.x * transform.pivot.x, transform.localPosition.y + transform.sizeDelta.y * transform.pivot.y);
      Vector2 vector2_4 = new Vector2(transform.localPosition.x + transform.sizeDelta.x * (1f - transform.pivot.x), transform.localPosition.y - transform.sizeDelta.y * (1f - transform.pivot.y));
      if ((double) vector2_3.x < (double) vector2_1.x)
        vector2_1.x = vector2_3.x;
      if ((double) vector2_3.y > (double) vector2_1.y)
        vector2_1.y = vector2_3.y;
      if ((double) vector2_4.y < (double) vector2_2.y)
        vector2_2.y = vector2_4.y;
      if ((double) vector2_4.x > (double) vector2_2.x)
        vector2_2.x = vector2_4.x;
    }
    Vector2 vector2_5 = new Vector2(Mathf.Abs(vector2_1.x - vector2_2.x), Mathf.Abs(vector2_1.y - vector2_2.y));
    this.targetTransform.localPosition = (Vector3) vector2_1;
    this.targetTransform.sizeDelta = vector2_5;
  }
}
