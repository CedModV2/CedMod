// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.PostProcessingContext
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

namespace UnityEngine.PostProcessing
{
  public class PostProcessingContext
  {
    public Camera camera;
    public MaterialFactory materialFactory;
    public PostProcessingProfile profile;
    public RenderTextureFactory renderTextureFactory;

    public bool interrupted { get; private set; }

    public void Interrupt()
    {
      this.interrupted = true;
    }

    public PostProcessingContext Reset()
    {
      this.profile = (PostProcessingProfile) null;
      this.camera = (Camera) null;
      this.materialFactory = (MaterialFactory) null;
      this.renderTextureFactory = (RenderTextureFactory) null;
      this.interrupted = false;
      return this;
    }

    public bool isGBufferAvailable
    {
      get
      {
        return this.camera.actualRenderingPath == RenderingPath.DeferredShading;
      }
    }

    public bool isHdr
    {
      get
      {
        return this.camera.allowHDR;
      }
    }

    public int width
    {
      get
      {
        return this.camera.pixelWidth;
      }
    }

    public int height
    {
      get
      {
        return this.camera.pixelHeight;
      }
    }

    public Rect viewport
    {
      get
      {
        return this.camera.rect;
      }
    }
  }
}
