// Decompiled with JetBrains decompiler
// Type: RandomMaterialReplacer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class RandomMaterialReplacer : MonoBehaviour
{
  public Material[] mats;

  private void Start()
  {
    int index = Random.Range(0, this.mats.Length);
    foreach (Renderer componentsInChild in this.GetComponentsInChildren<MeshRenderer>())
      componentsInChild.material = this.mats[index];
  }
}
