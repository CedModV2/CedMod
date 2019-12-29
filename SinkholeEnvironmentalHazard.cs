// Decompiled with JetBrains decompiler
// Type: SinkholeEnvironmentalHazard
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using GameCore;
using Mirror;
using UnityEngine;

public class SinkholeEnvironmentalHazard : EnvironmentalHazard
{
  public float DistanceToBeAffected = 5.25f;
  public bool SCPImmune = true;

  [Server]
  public override void DistanceChanged(GameObject player)
  {
    if (!NetworkServer.active)
    {
      Debug.LogWarning((object) "[Server] function 'System.Void SinkholeEnvironmentalHazard::DistanceChanged(UnityEngine.GameObject)' called on client");
    }
    else
    {
      PlayerEffectsController componentInParent = player.GetComponentInParent<PlayerEffectsController>();
      if ((Object) componentInParent == (Object) null)
        return;
      componentInParent.GetEffect<SinkHole>("SinkHole");
      if ((double) Vector3.Distance(player.transform.position, this.transform.position) <= (double) this.DistanceToBeAffected)
      {
        if (this.SCPImmune)
        {
          CharacterClassManager component = player.GetComponent<CharacterClassManager>();
          if ((Object) component == (Object) null || component.IsAnyScp())
            return;
        }
        componentInParent.EnableEffect("SinkHole");
      }
      else
        componentInParent.DisableEffect("SinkHole");
    }
  }

  public override void OnStart()
  {
    if ((double) ConfigFile.ServerConfig.GetFloat("sinkhole_spawn_chance", 0.0f) >= (double) Random.Range(0, 100))
      return;
    NetworkServer.Destroy(this.gameObject);
  }

  private void MirrorProcessed()
  {
  }
}
