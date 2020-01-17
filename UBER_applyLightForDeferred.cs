// Decompiled with JetBrains decompiler
// Type: UBER_applyLightForDeferred
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[AddComponentMenu("UBER/Apply Light for Deferred")]
[ExecuteInEditMode]
public class UBER_applyLightForDeferred : MonoBehaviour
{
  public Light lightForSelfShadowing;
  private Renderer _renderer;

  private void Start()
  {
    this.Reset();
  }

  private void Reset()
  {
    if ((bool) (Object) this.GetComponent<Light>() && (Object) this.lightForSelfShadowing == (Object) null)
      this.lightForSelfShadowing = this.GetComponent<Light>();
    if (!(bool) (Object) this.GetComponent<Renderer>() || !((Object) this._renderer == (Object) null))
      return;
    this._renderer = this.GetComponent<Renderer>();
  }

  private void Update()
  {
    if (!(bool) (Object) this.lightForSelfShadowing)
      return;
    if ((bool) (Object) this._renderer)
    {
      if (this.lightForSelfShadowing.type == LightType.Directional)
      {
        for (int index = 0; index < this._renderer.sharedMaterials.Length; ++index)
          this._renderer.sharedMaterials[index].SetVector("_WorldSpaceLightPosCustom", (Vector4) (-this.lightForSelfShadowing.transform.forward));
      }
      else
      {
        for (int index = 0; index < this._renderer.materials.Length; ++index)
          this._renderer.sharedMaterials[index].SetVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
      }
    }
    else if (this.lightForSelfShadowing.type == LightType.Directional)
      Shader.SetGlobalVector("_WorldSpaceLightPosCustom", (Vector4) (-this.lightForSelfShadowing.transform.forward));
    else
      Shader.SetGlobalVector("_WorldSpaceLightPosCustom", new Vector4(this.lightForSelfShadowing.transform.position.x, this.lightForSelfShadowing.transform.position.y, this.lightForSelfShadowing.transform.position.z, 1f));
  }
}
