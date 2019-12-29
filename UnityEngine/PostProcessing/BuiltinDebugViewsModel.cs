// Decompiled with JetBrains decompiler
// Type: UnityEngine.PostProcessing.BuiltinDebugViewsModel
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace UnityEngine.PostProcessing
{
  [Serializable]
  public class BuiltinDebugViewsModel : PostProcessingModel
  {
    [SerializeField]
    private BuiltinDebugViewsModel.Settings m_Settings = BuiltinDebugViewsModel.Settings.defaultSettings;

    public BuiltinDebugViewsModel.Settings settings
    {
      get
      {
        return this.m_Settings;
      }
      set
      {
        this.m_Settings = value;
      }
    }

    public bool willInterrupt
    {
      get
      {
        return !this.IsModeActive(BuiltinDebugViewsModel.Mode.None) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.EyeAdaptation) && (!this.IsModeActive(BuiltinDebugViewsModel.Mode.PreGradingLog) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.LogLut)) && !this.IsModeActive(BuiltinDebugViewsModel.Mode.UserLut);
      }
    }

    public override void Reset()
    {
      this.settings = BuiltinDebugViewsModel.Settings.defaultSettings;
    }

    public bool IsModeActive(BuiltinDebugViewsModel.Mode mode)
    {
      return this.m_Settings.mode == mode;
    }

    public enum Mode
    {
      None,
      Depth,
      Normals,
      MotionVectors,
      AmbientOcclusion,
      EyeAdaptation,
      FocusPlane,
      PreGradingLog,
      LogLut,
      UserLut,
    }

    [Serializable]
    public struct DepthSettings
    {
      [Range(0.0f, 1f)]
      [Tooltip("Scales the camera far plane before displaying the depth map.")]
      public float scale;

      public static BuiltinDebugViewsModel.DepthSettings defaultSettings
      {
        get
        {
          return new BuiltinDebugViewsModel.DepthSettings()
          {
            scale = 1f
          };
        }
      }
    }

    [Serializable]
    public struct MotionVectorsSettings
    {
      [Range(0.0f, 1f)]
      [Tooltip("Opacity of the source render.")]
      public float sourceOpacity;
      [Range(0.0f, 1f)]
      [Tooltip("Opacity of the per-pixel motion vector colors.")]
      public float motionImageOpacity;
      [Min(0.0f)]
      [Tooltip("Because motion vectors are mainly very small vectors, you can use this setting to make them more visible.")]
      public float motionImageAmplitude;
      [Range(0.0f, 1f)]
      [Tooltip("Opacity for the motion vector arrows.")]
      public float motionVectorsOpacity;
      [Range(8f, 64f)]
      [Tooltip("The arrow density on screen.")]
      public int motionVectorsResolution;
      [Min(0.0f)]
      [Tooltip("Tweaks the arrows length.")]
      public float motionVectorsAmplitude;

      public static BuiltinDebugViewsModel.MotionVectorsSettings defaultSettings
      {
        get
        {
          return new BuiltinDebugViewsModel.MotionVectorsSettings()
          {
            sourceOpacity = 1f,
            motionImageOpacity = 0.0f,
            motionImageAmplitude = 16f,
            motionVectorsOpacity = 1f,
            motionVectorsResolution = 24,
            motionVectorsAmplitude = 64f
          };
        }
      }
    }

    [Serializable]
    public struct Settings
    {
      public BuiltinDebugViewsModel.Mode mode;
      public BuiltinDebugViewsModel.DepthSettings depth;
      public BuiltinDebugViewsModel.MotionVectorsSettings motionVectors;

      public static BuiltinDebugViewsModel.Settings defaultSettings
      {
        get
        {
          return new BuiltinDebugViewsModel.Settings()
          {
            mode = BuiltinDebugViewsModel.Mode.None,
            depth = BuiltinDebugViewsModel.DepthSettings.defaultSettings,
            motionVectors = BuiltinDebugViewsModel.MotionVectorsSettings.defaultSettings
          };
        }
      }
    }
  }
}
