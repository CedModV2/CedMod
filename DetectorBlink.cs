// Decompiled with JetBrains decompiler
// Type: DetectorBlink
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class DetectorBlink : MonoBehaviour
{
  public Material mat;
  private bool state;

  private void Start()
  {
    this.Blink();
  }

  private void Blink()
  {
    this.state = !this.state;
    int num = this.state ? 2 : 0;
    this.mat.SetColor("_EmissionColor", new Color((float) num, (float) num, (float) num));
    this.Invoke(nameof (Blink), this.state ? 0.2f : 1.3f);
  }
}
