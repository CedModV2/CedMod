// Decompiled with JetBrains decompiler
// Type: Decal
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Rendering;

public class Decal : MonoBehaviour
{
  [HideInInspector]
  public MeshRenderer quad;
  public Vector3 startPos;

  private void Awake()
  {
    this.startPos = this.transform.position;
    this.quad = this.GetComponentInChildren<MeshRenderer>();
    this.quad.shadowCastingMode = ShadowCastingMode.Off;
  }

  private void LateUpdate()
  {
    DecalFader.UpdateDistance(this);
  }
}
