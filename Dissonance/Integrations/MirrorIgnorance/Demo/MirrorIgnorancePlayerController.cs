// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.Demo.MirrorIgnorancePlayerController
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance.Demo
{
  public class MirrorIgnorancePlayerController : NetworkBehaviour
  {
    private void Update()
    {
      if (!this.isLocalPlayer)
        return;
      CharacterController component = this.GetComponent<CharacterController>();
      float yAngle = (float) ((double) Input.GetAxis("Horizontal") * (double) Time.deltaTime * 150.0);
      float num = Input.GetAxis("Vertical") * 3f;
      this.transform.Rotate(0.0f, yAngle, 0.0f);
      Vector3 speed = this.transform.TransformDirection(Vector3.forward) * num;
      component.SimpleMove(speed);
      if ((double) this.transform.position.y >= -3.0)
        return;
      this.transform.position = Vector3.zero;
      this.transform.rotation = Quaternion.identity;
    }

    private void MirrorProcessed()
    {
    }
  }
}
