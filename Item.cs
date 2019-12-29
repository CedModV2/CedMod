// Decompiled with JetBrains decompiler
// Type: Item
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

[Serializable]
public class Item
{
  public float pickingtime = 1f;
  public string label;
  public string shortname;
  public Texture2D icon;
  public GameObject prefab;
  public string[] permissions;
  public GameObject firstpersonModel;
  public float durability;
  public bool noEquipable;
  public ConsumableAndWearableItems.UsableItem.ItemSlot slot;
  public Texture crosshair;
  public Color crosshairColor;
  public ItemCategory itemCategory;
  public bool useDefaultAnimation;
  public ItemType id;

  public Item(Item item)
  {
    this.label = item.label;
    this.icon = item.icon;
    this.prefab = item.prefab;
    this.pickingtime = item.pickingtime;
    this.permissions = item.permissions;
    this.firstpersonModel = item.firstpersonModel;
    this.durability = item.durability;
    this.id = item.id;
    this.crosshair = item.crosshair;
    this.crosshairColor = item.crosshairColor;
  }
}
