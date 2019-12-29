// Decompiled with JetBrains decompiler
// Type: CullingDisabler
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class CullingDisabler : MonoBehaviour
{
  public Behaviour camera;
  public Behaviour culler;
  private bool state;

  private void Start()
  {
    NetworkBehaviour componentInParent = this.GetComponentInParent<NetworkBehaviour>();
    if (!((Object) componentInParent != (Object) null) || componentInParent.isLocalPlayer)
      return;
    Object.Destroy((Object) this.culler);
    Object.Destroy((Object) this.GetComponent<GlobalFog>());
    Object.Destroy((Object) this.GetComponent<VignetteAndChromaticAberration>());
    Object.Destroy((Object) this.GetComponent<NoiseAndGrain>());
    Object.Destroy((Object) this.GetComponent<FlareLayer>());
    Object.Destroy((Object) this.camera.GetComponent<PostProcessingBehaviour>());
    Object.Destroy((Object) this.camera);
    Object.Destroy((Object) this);
  }

  private void Update()
  {
    if (this.state == this.camera.enabled)
      return;
    this.state = !this.state;
    this.culler.enabled = this.state;
  }
}
