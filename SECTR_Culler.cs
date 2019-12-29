// Decompiled with JetBrains decompiler
// Type: SECTR_Culler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof (SECTR_Member))]
[AddComponentMenu("")]
public class SECTR_Culler : MonoBehaviour
{
  private SECTR_Member cachedMember;
  [SECTR_ToolTip("Overrides the culling information on Member.")]
  public bool CullEachChild;

  private void OnEnable()
  {
    this.cachedMember = this.GetComponent<SECTR_Member>();
    this.cachedMember.ChildCulling = this.CullEachChild ? SECTR_Member.ChildCullModes.Individual : SECTR_Member.ChildCullModes.Group;
  }
}
