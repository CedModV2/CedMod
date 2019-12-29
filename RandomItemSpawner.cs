// Decompiled with JetBrains decompiler
// Type: RandomItemSpawner
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

public class RandomItemSpawner : MonoBehaviour
{
  public static RandomItemSpawner singleton;
  public RandomItemSpawner.PickupPositionRelation[] pickups;
  public RandomItemSpawner.PositionPosIdRelation[] posIds;

  private void Awake()
  {
    RandomItemSpawner.singleton = this;
  }

  public void RefreshIndexes()
  {
    for (int index = 0; index < this.posIds.Length; ++index)
      this.posIds[index].index = index;
  }

  [Serializable]
  public class PickupPositionRelation
  {
    public ItemType itemID;
    public string posID;
  }

  [Serializable]
  public class PositionPosIdRelation
  {
    public string posID;
    public Transform position;
    public int index;
  }
}
