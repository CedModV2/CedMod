// Decompiled with JetBrains decompiler
// Type: IESLights.EXRData
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace IESLights
{
  public struct EXRData
  {
    public Color[] Pixels;
    public uint Width;
    public uint Height;

    public EXRData(Color[] pixels, int width, int height)
    {
      this.Pixels = pixels;
      this.Width = (uint) width;
      this.Height = (uint) height;
    }
  }
}
