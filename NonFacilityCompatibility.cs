// Decompiled with JetBrains decompiler
// Type: NonFacilityCompatibility
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NonFacilityCompatibility : MonoBehaviour
{
  public NonFacilityCompatibility.SceneDescription[] allScenes;
  public static NonFacilityCompatibility singleton;
  public static NonFacilityCompatibility.SceneDescription currentSceneSettings;

  private void Awake()
  {
    NonFacilityCompatibility.singleton = this;
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(NonFacilityCompatibility.RefreshDescription);
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(NonFacilityCompatibility.RefreshDescription);
  }

  public static void RefreshDescription(Scene scene, LoadSceneMode mode)
  {
    foreach (NonFacilityCompatibility.SceneDescription allScene in NonFacilityCompatibility.singleton.allScenes)
    {
      if (allScene.sceneName == scene.name)
        NonFacilityCompatibility.currentSceneSettings = allScene;
    }
  }

  [Serializable]
  public class SceneDescription
  {
    public NonFacilityCompatibility.SceneDescription.VoiceChatSupportMode voiceChatSupport = NonFacilityCompatibility.SceneDescription.VoiceChatSupportMode.FullySupported;
    public bool enableWorldGeneration = true;
    public bool enableRespawning = true;
    public bool enableStandardGamplayItems = true;
    public Vector3 constantRespawnPoint = Vector3.zero;
    public RoleType forcedClass = RoleType.None;
    public string sceneName;
    public bool roundAutostart;

    public enum VoiceChatSupportMode
    {
      Unsupported,
      WithoutIntercom,
      FullySupported,
    }
  }
}
