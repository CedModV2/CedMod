// Decompiled with JetBrains decompiler
// Type: MaterialBlink
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class MaterialBlink : MonoBehaviour
{
  public Color lowestColor = Color.white;
  public Color highestColor = Color.white;
  public float speed = 1f;
  public float colorMultiplier = 1f;
  public Material materal;
  private float time;

  private void Update()
  {
    this.time += Time.deltaTime * this.speed;
    if ((double) this.time > 1.0)
      --this.time;
    this.materal.SetColor("_EmissionColor", Color.Lerp(this.lowestColor, this.highestColor, Mathf.Abs(Mathf.Lerp(-1f, 1f, this.time))) * this.colorMultiplier);
  }

  private void OnDisable()
  {
    this.materal.SetColor("_EmissionColor", this.highestColor);
  }
}
