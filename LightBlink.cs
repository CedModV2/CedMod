// Decompiled with JetBrains decompiler
// Type: LightBlink
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class LightBlink : MonoBehaviour
{
  public float noshadowIntensMultiplier = 1f;
  public float innerVariationPercent = 10f;
  private float outerVaration = 0.1f;
  public float FREQ = 12f;
  private float startIntes;
  private float curOut;
  private float curIn;
  private float innerVariation;
  private Light l;
  public bool disabled;
  private int i;

  private void Start()
  {
    if (QualitySettings.shadows != ShadowQuality.Disable)
      this.noshadowIntensMultiplier = 1f;
    this.startIntes = this.GetComponent<Light>().intensity * 1.2f;
    this.outerVaration = (float) ((double) this.startIntes * (double) this.noshadowIntensMultiplier / 10.0);
    this.innerVariation = (float) ((double) this.startIntes * (double) this.noshadowIntensMultiplier * ((double) this.innerVariationPercent / 100.0));
    this.l = this.GetComponent<Light>();
    this.RandomOuter();
    if ((double) this.innerVariationPercent >= 100.0)
      return;
    this.InvokeRepeating("RefreshLight", 0.0f, 1f / this.FREQ);
  }

  private void FixedUpdate()
  {
    if (!this.disabled && (double) this.innerVariationPercent == 100.0)
    {
      ++this.i;
      if (this.i <= 3)
        return;
      this.i = 0;
      this.l.enabled = !this.l.enabled;
    }
    else
      this.l.enabled = true;
  }

  private void RandomOuter()
  {
    this.curOut = Random.Range(-this.outerVaration, this.outerVaration);
    this.Invoke(nameof (RandomOuter), (float) Random.Range(1, 3));
  }

  private void RefreshLight()
  {
    if (!this.disabled)
    {
      this.curIn = Random.Range(this.startIntes * this.noshadowIntensMultiplier + this.innerVariation, this.startIntes * this.noshadowIntensMultiplier - this.innerVariation);
      this.l.intensity = this.curIn + this.curOut;
    }
    else
    {
      this.l.enabled = true;
      this.l.intensity = this.startIntes;
    }
  }
}
