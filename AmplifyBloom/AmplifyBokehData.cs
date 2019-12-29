// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.AmplifyBokehData
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

namespace AmplifyBloom
{
  [Serializable]
  public class AmplifyBokehData
  {
    internal RenderTexture BokehRenderTexture;
    internal Vector4[] Offsets;

    public AmplifyBokehData(Vector4[] offsets)
    {
      this.Offsets = offsets;
    }

    public void Destroy()
    {
      if ((UnityEngine.Object) this.BokehRenderTexture != (UnityEngine.Object) null)
      {
        AmplifyUtils.ReleaseTempRenderTarget(this.BokehRenderTexture);
        this.BokehRenderTexture = (RenderTexture) null;
      }
      this.Offsets = (Vector4[]) null;
    }
  }
}
