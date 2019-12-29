// Decompiled with JetBrains decompiler
// Type: SkinColorChanger
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class SkinColorChanger : MonoBehaviour
{
  private RoleType lastClass = RoleType.None;
  public Material ci;
  public Material mtf;
  public Material classd;
  public Material scientist;
  public Material guard;

  private void OnEnable()
  {
    Renderer component = (Renderer) this.GetComponent<SkinnedMeshRenderer>();
    CharacterClassManager componentInParent = this.GetComponentInParent<CharacterClassManager>();
    if ((Object) componentInParent == (Object) null || this.lastClass == componentInParent.CurClass)
      return;
    this.lastClass = componentInParent.CurClass;
    switch (componentInParent.Classes.SafeGet(componentInParent.CurClass).team)
    {
      case Team.MTF:
        component.sharedMaterial = componentInParent.CurClass == RoleType.FacilityGuard ? this.guard : this.mtf;
        break;
      case Team.CHI:
        component.sharedMaterial = this.ci;
        break;
      case Team.RSC:
        component.sharedMaterial = this.scientist;
        break;
      default:
        component.sharedMaterial = this.classd;
        break;
    }
  }
}
