// Decompiled with JetBrains decompiler
// Type: Outside
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Outside : MonoBehaviour
{
  private bool isOutside = true;
  private Transform listenerPos;

  private void Update()
  {
    if ((Object) this.listenerPos == (Object) null)
    {
      SpectatorCamera objectOfType = Object.FindObjectOfType<SpectatorCamera>();
      if (!((Object) objectOfType != (Object) null))
        return;
      this.listenerPos = objectOfType.cam.transform;
    }
    else if ((double) this.listenerPos.position.y > 900.0 && !this.isOutside)
    {
      this.isOutside = true;
      this.SetOutside(true);
    }
    else
    {
      if ((double) this.listenerPos.position.y >= 900.0 || !this.isOutside)
        return;
      this.isOutside = false;
      this.SetOutside(false);
    }
  }

  private void SetOutside(bool b)
  {
    GameObject gameObject = GameObject.Find("Directional light");
    if ((Object) gameObject != (Object) null)
      gameObject.GetComponent<Light>().enabled = b;
    foreach (Camera componentsInChild in this.GetComponentsInChildren<Camera>(true))
    {
      if ((double) componentsInChild.farClipPlane == 600.0 || (double) componentsInChild.farClipPlane == 47.0)
      {
        componentsInChild.farClipPlane = b ? 600f : 47f;
        if (componentsInChild.clearFlags <= CameraClearFlags.Color)
          componentsInChild.clearFlags = b ? CameraClearFlags.Skybox : CameraClearFlags.Color;
      }
    }
    foreach (GlobalFog componentsInChild in this.GetComponentsInChildren<GlobalFog>(true))
      componentsInChild.startDistance = b ? 50f : 5f;
  }
}
