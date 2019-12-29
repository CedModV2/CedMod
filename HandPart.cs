// Decompiled with JetBrains decompiler
// Type: HandPart
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class HandPart : MonoBehaviour
{
  public GameObject part;
  public int id;
  public Animator anim;
  private Inventory inv;

  private void Start()
  {
    if ((Object) this.anim == (Object) null)
      this.anim = this.GetComponentsInParent<Animator>()[0];
    if (!((Object) this.inv == (Object) null))
      return;
    this.inv = this.GetComponentInParent<Inventory>();
  }

  public void UpdateItem()
  {
    this.part.SetActive(this.inv.curItem == (ItemType) this.id);
  }
}
