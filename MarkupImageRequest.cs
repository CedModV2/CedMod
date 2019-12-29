// Decompiled with JetBrains decompiler
// Type: MarkupImageRequest
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class MarkupImageRequest : MonoBehaviour
{
  public static List<MarkupImageRequest.CachedImage> cachedImages = new List<MarkupImageRequest.CachedImage>();
  public string[] allowedExtensions;
  public ulong maxSizeInBytes;
  public Texture errorTexture;

  [Serializable]
  public class CachedImage
  {
    public Texture texture;
    public string url;
  }
}
