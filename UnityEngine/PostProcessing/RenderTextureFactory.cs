// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.RenderTextureFactory
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing
{
  public sealed class RenderTextureFactory : IDisposable
  {
    private readonly HashSet<RenderTexture> m_TemporaryRTs;

    public RenderTextureFactory()
    {
      this.m_TemporaryRTs = new HashSet<RenderTexture>();
    }

    public void Dispose()
    {
      this.ReleaseAll();
    }

    public RenderTexture Get(RenderTexture baseRenderTexture)
    {
      return this.Get(baseRenderTexture.width, baseRenderTexture.height, baseRenderTexture.depth, baseRenderTexture.format, baseRenderTexture.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear, baseRenderTexture.filterMode, baseRenderTexture.wrapMode, "FactoryTempTexture");
    }

    public RenderTexture Get(
      int width,
      int height,
      int depthBuffer = 0,
      RenderTextureFormat format = RenderTextureFormat.ARGBHalf,
      RenderTextureReadWrite rw = RenderTextureReadWrite.Default,
      FilterMode filterMode = FilterMode.Bilinear,
      TextureWrapMode wrapMode = TextureWrapMode.Clamp,
      string name = "FactoryTempTexture")
    {
      RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format, rw);
      temporary.filterMode = filterMode;
      temporary.wrapMode = wrapMode;
      temporary.name = name;
      this.m_TemporaryRTs.Add(temporary);
      return temporary;
    }

    public void Release(RenderTexture rt)
    {
      if ((UnityEngine.Object) rt == (UnityEngine.Object) null)
        return;
      if (!this.m_TemporaryRTs.Contains(rt))
        throw new ArgumentException(string.Format("Attempting to remove a RenderTexture that was not allocated: {0}", (object) rt));
      this.m_TemporaryRTs.Remove(rt);
      RenderTexture.ReleaseTemporary(rt);
    }

    public void ReleaseAll()
    {
      foreach (RenderTexture temporaryRt in this.m_TemporaryRTs)
        RenderTexture.ReleaseTemporary(temporaryRt);
      this.m_TemporaryRTs.Clear();
    }
  }
}
