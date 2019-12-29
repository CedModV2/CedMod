// Decompiled with JetBrains decompiler
// Type: LCZ_Label
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class LCZ_Label : MonoBehaviour
{
  public MeshRenderer chRend;
  public MeshRenderer numRend;

  public void Refresh(Material ch, Material num, string err)
  {
    this.chRend.sharedMaterial = ch;
    if ((Object) this.chRend.sharedMaterial == (Object) null)
      Debug.Log((object) err);
    this.numRend.sharedMaterial = num;
  }
}
