// Decompiled with JetBrains decompiler
// Type: CustomPostProcessingSight
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.PostProcessing;

public class CustomPostProcessingSight : MonoBehaviour
{
  [HideInInspector]
  public WeaponManager wm;
  [HideInInspector]
  public PostProcessingBehaviour ppb;
  public GameObject canvas;
  public PostProcessingProfile targetProfile;
  public static CustomPostProcessingSight singleton;
  public static bool raycast_bool;
  public static RaycastHit raycast_hit;

  public int GetAmmoLeft()
  {
    return this.wm.AmmoLeft();
  }

  public bool IsHumanHit()
  {
    return (Object) CustomPostProcessingSight.raycast_hit.collider.GetComponentInParent<CharacterClassManager>() == (Object) null;
  }
}
