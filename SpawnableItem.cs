// Decompiled with JetBrains decompiler
// Type: SpawnableItem
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class SpawnableItem
{
  [Tooltip("Optional label/description of an item")]
  public string name;
  public string itemTag;
  [Tooltip("ID of the item")]
  public ItemType inventoryId;
  [Range(0.0f, 100f)]
  public int chanceOfSpawn;
  [Range(0.0f, 10f)]
  public int copies;
}
