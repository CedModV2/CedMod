// Decompiled with JetBrains decompiler
// Type: HostItemSpawner
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class HostItemSpawner : NetworkBehaviour
{
  public void Spawn(int seed)
  {
    if (!NonFacilityCompatibility.currentSceneSettings.enableWorldGeneration)
      return;
    Random.InitState(seed);
    RandomItemSpawner singleton = RandomItemSpawner.singleton;
    RandomItemSpawner.PickupPositionRelation[] pickups = singleton.pickups;
    List<RandomItemSpawner.PositionPosIdRelation> positionPosIdRelationList1 = new List<RandomItemSpawner.PositionPosIdRelation>();
    foreach (RandomItemSpawner.PositionPosIdRelation posId in singleton.posIds)
      positionPosIdRelationList1.Add(posId);
    int num = 0;
    Inventory inventory = ReferenceHub.GetHub(PlayerManager.localPlayer).inventory;
    foreach (RandomItemSpawner.PickupPositionRelation positionRelation in pickups)
    {
      for (int index = 0; index < positionPosIdRelationList1.Count; ++index)
        positionPosIdRelationList1[index].index = index;
      List<RandomItemSpawner.PositionPosIdRelation> positionPosIdRelationList2 = new List<RandomItemSpawner.PositionPosIdRelation>();
      foreach (RandomItemSpawner.PositionPosIdRelation positionPosIdRelation in positionPosIdRelationList1)
      {
        if (positionPosIdRelation.posID == positionRelation.posID)
          positionPosIdRelationList2.Add(positionPosIdRelation);
      }
      if (positionPosIdRelationList2.Count == 0)
        Debug.LogError((object) ("No positions compared to: " + (object) positionRelation.itemID + " (" + positionRelation.posID + ")"));
      int index1 = Random.Range(0, positionPosIdRelationList2.Count);
      RandomItemSpawner.PositionPosIdRelation positionPosIdRelation1 = positionPosIdRelationList2[index1];
      int index2 = positionPosIdRelation1.index;
      Pickup pickup = inventory.SetPickup(positionRelation.itemID, 0.0f, Vector3.zero, Quaternion.identity, 0, 0, 0);
      this.SetPos(pickup.gameObject, positionPosIdRelation1.position.position, positionRelation.itemID, positionPosIdRelation1.position.rotation.eulerAngles);
      pickup.RefreshDurability(true, true);
      positionPosIdRelationList1.RemoveAt(index2);
      ++num;
    }
  }

  [ServerCallback]
  private void SetPos(GameObject obj, Vector3 pos, ItemType item, Vector3 rot)
  {
    if (!NetworkServer.active)
      return;
    obj.GetComponent<Pickup>().SetupPickup(new Pickup.PickupInfo()
    {
      position = pos,
      rotation = Quaternion.Euler(rot),
      itemId = item,
      durability = 0.0f,
      ownerPlayer = (GameObject) null
    });
  }

  private void MirrorProcessed()
  {
  }
}
