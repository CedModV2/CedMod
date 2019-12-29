// Decompiled with JetBrains decompiler
// Type: HidThirdpersonNeedle
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class HidThirdpersonNeedle : MonoBehaviour
{
  private Inventory inv;

  private void Start()
  {
    this.inv = ReferenceHub.GetHub(this.transform.root.gameObject).inventory;
  }

  private void Update()
  {
    foreach (Inventory.SyncItemInfo syncItemInfo in (SyncList<Inventory.SyncItemInfo>) this.inv.items)
    {
      if (syncItemInfo.id == ItemType.MicroHID)
        this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, Quaternion.Euler(0.0f, 0.0f, Mathf.Lerp(-81.6f, 68.7f, syncItemInfo.durability)), 4f * Time.deltaTime);
    }
  }
}
