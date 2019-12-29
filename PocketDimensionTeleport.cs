// Decompiled with JetBrains decompiler
// Type: PocketDimensionTeleport
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CustomPlayerEffects;
using GameCore;
using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PocketDimensionTeleport : NetworkBehaviour
{
  private readonly List<Vector3> tpPositions = new List<Vector3>();
  private PocketDimensionTeleport.PDTeleportType type;
  public bool RefreshExit;

  public void SetType(PocketDimensionTeleport.PDTeleportType t)
  {
    this.type = t;
  }

  public PocketDimensionTeleport.PDTeleportType GetTeleportType()
  {
    return this.type;
  }

  private void Start()
  {
    this.RefreshExit = ConfigFile.ServerConfig.GetBool("pd_refresh_exit", false);
  }

  [ServerCallback]
  private void OnTriggerEnter(Collider other)
  {
    if (!NetworkServer.active)
      return;
    NetworkIdentity component1 = other.GetComponent<NetworkIdentity>();
    if (!((Object) component1 != (Object) null))
      return;
    if (this.type == PocketDimensionTeleport.PDTeleportType.Killer || Object.FindObjectOfType<BlastDoor>().isClosed)
      component1.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), other.gameObject);
    else if (this.type == PocketDimensionTeleport.PDTeleportType.Exit)
    {
      this.tpPositions.Clear();
      List<string> stringList = ConfigFile.ServerConfig.GetStringList(GameObject.Find("Host").GetComponent<DecontaminationLCZ>().GetCurAnnouncement() > 5 ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
      if (stringList.Count > 0)
      {
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("RoomID"))
        {
          if ((Object) gameObject.GetComponent<Rid>() != (Object) null && stringList.Contains(gameObject.GetComponent<Rid>().id))
            this.tpPositions.Add(gameObject.transform.position);
        }
        if (stringList.Contains("PORTAL"))
        {
          foreach (Scp106PlayerScript scp106PlayerScript in Object.FindObjectsOfType<Scp106PlayerScript>())
          {
            if (scp106PlayerScript.portalPosition != Vector3.zero)
              this.tpPositions.Add(scp106PlayerScript.portalPosition);
          }
        }
      }
      if (this.tpPositions == null || this.tpPositions.Count == 0)
      {
        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("PD_EXIT"))
          this.tpPositions.Add(gameObject.transform.position);
      }
      Vector3 tpPosition = this.tpPositions[Random.Range(0, this.tpPositions.Count)];
      tpPosition.y += 2f;
      PlyMovementSync component2 = other.GetComponent<PlyMovementSync>();
      component2.SetSafeTime(2f);
      component2.OverridePosition(tpPosition, 0.0f, false);
      this.RemoveCorrosionEffect(other.gameObject);
      PlayerManager.localPlayer.GetComponent<PlayerStats>().TargetAchieve(component1.connectionToClient, "larryisyourfriend");
    }
    if (!this.RefreshExit)
      return;
    ImageGenerator.pocketDimensionGenerator.GenerateRandom();
  }

  [Server]
  private void RemoveCorrosionEffect(GameObject escapee)
  {
    if (!NetworkServer.active)
      Debug.LogWarning((object) "[Server] function 'System.Void PocketDimensionTeleport::RemoveCorrosionEffect(UnityEngine.GameObject)' called on client");
    else
      escapee.GetComponentInParent<PlayerEffectsController>().GetEffect<Corroding>("Corroding").ServerDisable();
  }

  private void MirrorProcessed()
  {
  }

  public enum PDTeleportType
  {
    Killer,
    Exit,
  }
}
