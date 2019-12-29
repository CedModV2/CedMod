// Decompiled with JetBrains decompiler
// Type: FakeSoundScope
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class FakeSoundScope : MonoBehaviour
{
  public AnimationCurve highOverVolume;
  public int numOfPos;
  private LineRenderer line;
  public float maxH;

  private void Awake()
  {
    this.line = this.GetComponent<LineRenderer>();
  }

  private void LateUpdate()
  {
    Vector3[] positions = new Vector3[this.numOfPos];
    float num1 = Random.value;
    float num2 = 0.0f;
    for (int index = 0; index < this.numOfPos; ++index)
    {
      float num3 = (float) index / (float) this.numOfPos;
      float num4 = Mathf.Abs((float) (1.0 - (double) Mathf.Abs(num3 - 0.5f) * 2.0));
      positions[index][0] = num3 * 100f;
      positions[index][2] = (float) ((double) Mathf.Sin((float) (index * 7) * num1) * (double) num4 * (double) this.maxH * ((double) Mathf.Sin((float) index) / 3.0)) * num2;
    }
    this.line.SetPositions(positions);
  }
}
