// Decompiled with JetBrains decompiler
// Type: EnvironmentalHazard
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using UnityEngine;

public abstract class EnvironmentalHazard : NetworkBehaviour
{
  [SerializeField]
  private Vector3 distanceToAffect = Vector3.one;

  protected void Start()
  {
    if (!NetworkServer.active)
      return;
    this.OnStart();
    NetworkServer.Spawn(this.gameObject);
    Console.AddDebugLog("MAPGEN", "Spawning hazard: \"" + this.gameObject.name + "\"", MessageImportance.LessImportant, true);
  }

  protected void Update()
  {
    if (!NetworkServer.active)
      return;
    foreach (GameObject player in PlayerManager.players)
    {
      if ((double) Vector3.Distance(player.transform.position, this.transform.position) <= 7.0)
        this.DistanceChanged(player);
    }
  }

  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.green;
    Gizmos.DrawCube(this.transform.position, this.distanceToAffect);
  }

  [Server]
  public abstract void DistanceChanged(GameObject player);

  [Server]
  public abstract void OnStart();

  private void MirrorProcessed()
  {
  }
}
