// Decompiled with JetBrains decompiler
// Type: AmplifyBloom.AmplifyUtils
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

namespace AmplifyBloom
{
  public class AmplifyUtils
  {
    public static string HighPrecisionKeyword = "AB_HIGH_PRECISION";
    public static string ShaderModeTag = "Mode";
    public static string ShaderModeValue = "Full";
    public static string DebugStr = "[AmplifyBloom] ";
    public static int BokehParamsId = -1;
    public static RenderTextureFormat CurrentRTFormat = RenderTextureFormat.DefaultHDR;
    public static FilterMode CurrentFilterMode = FilterMode.Bilinear;
    public static TextureWrapMode CurrentWrapMode = TextureWrapMode.Clamp;
    public static RenderTextureReadWrite CurrentReadWriteMode = RenderTextureReadWrite.sRGB;
    public static bool IsInitialized = false;
    private static List<RenderTexture> _allocatedRT = new List<RenderTexture>();
    public static int MaskTextureId;
    public static int BlurRadiusId;
    public static int UpscaleContributionId;
    public static int SourceContributionId;
    public static int LensStarburstRTId;
    public static int LensDirtRTId;
    public static int LensFlareRTId;
    public static int LensGlareRTId;
    public static int[] MipResultsRTS;
    public static int[] AnamorphicRTS;
    public static int[] AnamorphicGlareWeightsMatStr;
    public static int[] AnamorphicGlareOffsetsMatStr;
    public static int[] AnamorphicGlareWeightsStr;
    public static int[] UpscaleWeightsStr;
    public static int[] LensDirtWeightsStr;
    public static int[] LensStarburstWeightsStr;
    public static int BloomRangeId;
    public static int LensDirtStrengthId;
    public static int BloomParamsId;
    public static int TempFilterValueId;
    public static int LensFlareStarMatrixId;
    public static int LensFlareStarburstStrengthId;
    public static int LensFlareGhostsParamsId;
    public static int LensFlareLUTId;
    public static int LensFlareHaloParamsId;
    public static int LensFlareGhostChrDistortionId;
    public static int LensFlareHaloChrDistortionId;

    public static void InitializeIds()
    {
      AmplifyUtils.IsInitialized = true;
      AmplifyUtils.MaskTextureId = Shader.PropertyToID("_MaskTex");
      AmplifyUtils.MipResultsRTS = new int[6]
      {
        Shader.PropertyToID("_MipResultsRTS0"),
        Shader.PropertyToID("_MipResultsRTS1"),
        Shader.PropertyToID("_MipResultsRTS2"),
        Shader.PropertyToID("_MipResultsRTS3"),
        Shader.PropertyToID("_MipResultsRTS4"),
        Shader.PropertyToID("_MipResultsRTS5")
      };
      AmplifyUtils.AnamorphicRTS = new int[8]
      {
        Shader.PropertyToID("_AnamorphicRTS0"),
        Shader.PropertyToID("_AnamorphicRTS1"),
        Shader.PropertyToID("_AnamorphicRTS2"),
        Shader.PropertyToID("_AnamorphicRTS3"),
        Shader.PropertyToID("_AnamorphicRTS4"),
        Shader.PropertyToID("_AnamorphicRTS5"),
        Shader.PropertyToID("_AnamorphicRTS6"),
        Shader.PropertyToID("_AnamorphicRTS7")
      };
      AmplifyUtils.AnamorphicGlareWeightsMatStr = new int[4]
      {
        Shader.PropertyToID("_AnamorphicGlareWeightsMat0"),
        Shader.PropertyToID("_AnamorphicGlareWeightsMat1"),
        Shader.PropertyToID("_AnamorphicGlareWeightsMat2"),
        Shader.PropertyToID("_AnamorphicGlareWeightsMat3")
      };
      AmplifyUtils.AnamorphicGlareOffsetsMatStr = new int[4]
      {
        Shader.PropertyToID("_AnamorphicGlareOffsetsMat0"),
        Shader.PropertyToID("_AnamorphicGlareOffsetsMat1"),
        Shader.PropertyToID("_AnamorphicGlareOffsetsMat2"),
        Shader.PropertyToID("_AnamorphicGlareOffsetsMat3")
      };
      AmplifyUtils.AnamorphicGlareWeightsStr = new int[16]
      {
        Shader.PropertyToID("_AnamorphicGlareWeights0"),
        Shader.PropertyToID("_AnamorphicGlareWeights1"),
        Shader.PropertyToID("_AnamorphicGlareWeights2"),
        Shader.PropertyToID("_AnamorphicGlareWeights3"),
        Shader.PropertyToID("_AnamorphicGlareWeights4"),
        Shader.PropertyToID("_AnamorphicGlareWeights5"),
        Shader.PropertyToID("_AnamorphicGlareWeights6"),
        Shader.PropertyToID("_AnamorphicGlareWeights7"),
        Shader.PropertyToID("_AnamorphicGlareWeights8"),
        Shader.PropertyToID("_AnamorphicGlareWeights9"),
        Shader.PropertyToID("_AnamorphicGlareWeights10"),
        Shader.PropertyToID("_AnamorphicGlareWeights11"),
        Shader.PropertyToID("_AnamorphicGlareWeights12"),
        Shader.PropertyToID("_AnamorphicGlareWeights13"),
        Shader.PropertyToID("_AnamorphicGlareWeights14"),
        Shader.PropertyToID("_AnamorphicGlareWeights15")
      };
      AmplifyUtils.UpscaleWeightsStr = new int[8]
      {
        Shader.PropertyToID("_UpscaleWeights0"),
        Shader.PropertyToID("_UpscaleWeights1"),
        Shader.PropertyToID("_UpscaleWeights2"),
        Shader.PropertyToID("_UpscaleWeights3"),
        Shader.PropertyToID("_UpscaleWeights4"),
        Shader.PropertyToID("_UpscaleWeights5"),
        Shader.PropertyToID("_UpscaleWeights6"),
        Shader.PropertyToID("_UpscaleWeights7")
      };
      AmplifyUtils.LensDirtWeightsStr = new int[8]
      {
        Shader.PropertyToID("_LensDirtWeights0"),
        Shader.PropertyToID("_LensDirtWeights1"),
        Shader.PropertyToID("_LensDirtWeights2"),
        Shader.PropertyToID("_LensDirtWeights3"),
        Shader.PropertyToID("_LensDirtWeights4"),
        Shader.PropertyToID("_LensDirtWeights5"),
        Shader.PropertyToID("_LensDirtWeights6"),
        Shader.PropertyToID("_LensDirtWeights7")
      };
      AmplifyUtils.LensStarburstWeightsStr = new int[8]
      {
        Shader.PropertyToID("_LensStarburstWeights0"),
        Shader.PropertyToID("_LensStarburstWeights1"),
        Shader.PropertyToID("_LensStarburstWeights2"),
        Shader.PropertyToID("_LensStarburstWeights3"),
        Shader.PropertyToID("_LensStarburstWeights4"),
        Shader.PropertyToID("_LensStarburstWeights5"),
        Shader.PropertyToID("_LensStarburstWeights6"),
        Shader.PropertyToID("_LensStarburstWeights7")
      };
      AmplifyUtils.BloomRangeId = Shader.PropertyToID("_BloomRange");
      AmplifyUtils.LensDirtStrengthId = Shader.PropertyToID("_LensDirtStrength");
      AmplifyUtils.BloomParamsId = Shader.PropertyToID("_BloomParams");
      AmplifyUtils.TempFilterValueId = Shader.PropertyToID("_TempFilterValue");
      AmplifyUtils.LensFlareStarMatrixId = Shader.PropertyToID("_LensFlareStarMatrix");
      AmplifyUtils.LensFlareStarburstStrengthId = Shader.PropertyToID("_LensFlareStarburstStrength");
      AmplifyUtils.LensFlareGhostsParamsId = Shader.PropertyToID("_LensFlareGhostsParams");
      AmplifyUtils.LensFlareLUTId = Shader.PropertyToID("_LensFlareLUT");
      AmplifyUtils.LensFlareHaloParamsId = Shader.PropertyToID("_LensFlareHaloParams");
      AmplifyUtils.LensFlareGhostChrDistortionId = Shader.PropertyToID("_LensFlareGhostChrDistortion");
      AmplifyUtils.LensFlareHaloChrDistortionId = Shader.PropertyToID("_LensFlareHaloChrDistortion");
      AmplifyUtils.BokehParamsId = Shader.PropertyToID("_BokehParams");
      AmplifyUtils.BlurRadiusId = Shader.PropertyToID("_BlurRadius");
      AmplifyUtils.LensStarburstRTId = Shader.PropertyToID("_LensStarburst");
      AmplifyUtils.LensDirtRTId = Shader.PropertyToID("_LensDirt");
      AmplifyUtils.LensFlareRTId = Shader.PropertyToID("_LensFlare");
      AmplifyUtils.LensGlareRTId = Shader.PropertyToID("_LensGlare");
      AmplifyUtils.SourceContributionId = Shader.PropertyToID("_SourceContribution");
      AmplifyUtils.UpscaleContributionId = Shader.PropertyToID("_UpscaleContribution");
    }

    public static void DebugLog(string value, LogType type)
    {
      switch (type)
      {
        case LogType.Normal:
          Debug.Log((object) (AmplifyUtils.DebugStr + value));
          break;
        case LogType.Warning:
          Debug.LogWarning((object) (AmplifyUtils.DebugStr + value));
          break;
        case LogType.Error:
          Debug.LogError((object) (AmplifyUtils.DebugStr + value));
          break;
      }
    }

    public static RenderTexture GetTempRenderTarget(int width, int height)
    {
      RenderTexture temporary = RenderTexture.GetTemporary(width, height, 0, AmplifyUtils.CurrentRTFormat, AmplifyUtils.CurrentReadWriteMode);
      temporary.filterMode = AmplifyUtils.CurrentFilterMode;
      temporary.wrapMode = AmplifyUtils.CurrentWrapMode;
      AmplifyUtils._allocatedRT.Add(temporary);
      return temporary;
    }

    public static void ReleaseTempRenderTarget(RenderTexture renderTarget)
    {
      if (!((Object) renderTarget != (Object) null) || !AmplifyUtils._allocatedRT.Remove(renderTarget))
        return;
      renderTarget.DiscardContents();
      RenderTexture.ReleaseTemporary(renderTarget);
    }

    public static void ReleaseAllRT()
    {
      for (int index = 0; index < AmplifyUtils._allocatedRT.Count; ++index)
      {
        AmplifyUtils._allocatedRT[index].DiscardContents();
        RenderTexture.ReleaseTemporary(AmplifyUtils._allocatedRT[index]);
      }
      AmplifyUtils._allocatedRT.Clear();
    }

    public static void EnsureKeywordEnabled(Material mat, string keyword, bool state)
    {
      if (!((Object) mat != (Object) null))
        return;
      if (state && !mat.IsKeywordEnabled(keyword))
      {
        mat.EnableKeyword(keyword);
      }
      else
      {
        if (state || !mat.IsKeywordEnabled(keyword))
          return;
        mat.DisableKeyword(keyword);
      }
    }
  }
}
