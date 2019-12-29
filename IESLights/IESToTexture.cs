// Decompiled with JetBrains decompiler
// Type: IESLights.IESToTexture
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

namespace IESLights
{
  public class IESToTexture : MonoBehaviour
  {
    public static Texture2D ConvertIesData(IESData data)
    {
      Texture2D texture2D1 = new Texture2D(data.NormalizedValues.Count, data.NormalizedValues[0].Count, TextureFormat.RGBAFloat, false, true);
      texture2D1.filterMode = FilterMode.Trilinear;
      texture2D1.wrapMode = TextureWrapMode.Clamp;
      Texture2D texture2D2 = texture2D1;
      Color[] colors = new Color[texture2D2.width * texture2D2.height];
      for (int index1 = 0; index1 < texture2D2.width; ++index1)
      {
        for (int index2 = 0; index2 < texture2D2.height; ++index2)
        {
          float num = data.NormalizedValues[index1][index2];
          colors[index1 + index2 * texture2D2.width] = new Color(num, num, num, num);
        }
      }
      texture2D2.SetPixels(colors);
      texture2D2.Apply();
      return texture2D2;
    }
  }
}
