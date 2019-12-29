// Decompiled with JetBrains decompiler
// Type: CheckpointKiller
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class CheckpointKiller : MonoBehaviour
{
  private void OnTriggerEnter(Collider other)
  {
    if (!NetworkServer.active)
      return;
    PlayerStats component = other.GetComponent<PlayerStats>();
    if (!((Object) component != (Object) null))
      return;
    component.HurtPlayer(new PlayerStats.HitInfo(99999f, "WORLD", DamageTypes.Wall, 0), component.gameObject);
  }
}
