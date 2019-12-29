// Decompiled with JetBrains decompiler
// Type: Escape
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class Escape : NetworkBehaviour
{
  public static int radius = 10;
  public Vector3 worldPosition;

  private void Start()
  {
  }

  private void Update()
  {
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(this.worldPosition, (float) Escape.radius);
  }

  private void MirrorProcessed()
  {
  }
}
