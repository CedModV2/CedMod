// Decompiled with JetBrains decompiler
// Type: shooter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class shooter : MonoBehaviour
{
  public int mtpl = 5;

  private void Update()
  {
    if (!Input.GetKeyDown(KeyCode.Return))
      return;
    ScreenCapture.CaptureScreenshot("Taken" + (object) Random.Range(0, 1000) + ".png", this.mtpl);
  }
}
