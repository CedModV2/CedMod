// Decompiled with JetBrains decompiler
// Type: SkyboxFollower
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class SkyboxFollower : MonoBehaviour
{
  public Transform camera;
  public static bool iAm939;

  private void Update()
  {
    if (SkyboxFollower.iAm939 || (double) this.camera.position.y < 800.0)
      this.transform.position = Vector3.down * 12345f;
    else
      this.transform.position = this.camera.position;
  }
}
