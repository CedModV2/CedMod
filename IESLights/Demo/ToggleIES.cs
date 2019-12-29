// Decompiled with JetBrains decompiler
// Type: IESLights.Demo.ToggleIES
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

namespace IESLights.Demo
{
  public class ToggleIES : MonoBehaviour
  {
    private Dictionary<Light, Texture> _spotsToCookies = new Dictionary<Light, Texture>();

    private void Start()
    {
      foreach (Light componentsInChild in this.GetComponentsInChildren<Light>())
        this._spotsToCookies.Add(componentsInChild, componentsInChild.cookie);
    }

    private void Update()
    {
      if (!Input.GetKeyDown(KeyCode.Space))
        return;
      foreach (Light key in this._spotsToCookies.Keys)
      {
        if ((Object) key.cookie == (Object) null)
        {
          key.cookie = this._spotsToCookies[key];
          key.intensity = 0.7f;
        }
        else
        {
          key.cookie = (Texture) null;
          key.intensity = 0.4f;
        }
      }
    }
  }
}
