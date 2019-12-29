// Decompiled with JetBrains decompiler
// Type: LoadingCircle
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
  public int framesToNextRotation = 10;
  private int i;

  private void FixedUpdate()
  {
    ++this.i;
    if (this.i <= this.framesToNextRotation)
      return;
    this.i = 0;
    this.transform.Rotate(Vector3.forward * -45f, Space.Self);
  }
}
