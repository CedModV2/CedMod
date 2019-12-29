// Decompiled with JetBrains decompiler
// Type: Clutter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System.Collections.Generic;
using UnityEngine;

public class Clutter : MonoBehaviour
{
  public List<GameObject> possiblePrefabs = new List<GameObject>();
  public Vector3 clutterScale = Vector3.zero;
  [Header("Prefab Data")]
  [Space]
  public GameObject holderObject;
  public Vector3 spawnOffset;
  [Space]
  public bool spawned;

  public void SpawnClutter()
  {
    Console.AddDebugLog("MGCLTR", "Spawning clutter component on object of name \"" + this.gameObject.name + "\"", MessageImportance.LeastImportant, true);
    this.spawned = true;
    if (!(bool) (Object) this.holderObject)
      this.holderObject = this.gameObject;
    GameObject gameObject = Object.Instantiate<GameObject>(this.possiblePrefabs.Count > 0 ? this.possiblePrefabs[Random.Range(0, this.possiblePrefabs.Count)] : this.gameObject, this.holderObject.transform.position + this.spawnOffset, this.holderObject.transform.rotation.normalized, this.holderObject.transform.parent);
    gameObject.transform.localScale = !(this.clutterScale != Vector3.zero) ? this.holderObject.transform.localScale : this.clutterScale;
    gameObject.SetActive(true);
    Clutter component;
    if (gameObject.TryGetComponent<Clutter>(out component))
      Object.Destroy((Object) component);
    Object.Destroy((Object) this.holderObject);
  }
}
