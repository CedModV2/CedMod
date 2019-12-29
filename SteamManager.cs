// Decompiled with JetBrains decompiler
// Type: SteamManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Steamworks;
using Steamworks.Data;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour
{
  private static string _state = "";
  private const uint SteamAppId = 700330;
  private static AuthTicket _ticket;

  public static bool Running { get; private set; }

  public static ulong SteamId64 { get; private set; }

  public static void SetAchievement(string key)
  {
    if (!SteamManager.Running)
      return;
    new Achievement(key).Trigger(true);
  }

  public static void ResetAchievement(string key)
  {
    if (!SteamManager.Running)
      return;
    new Achievement(key).Clear();
  }

  public static bool CheckAchievement(string key)
  {
    return SteamManager.Running && new Achievement(key).State;
  }

  public static void ChangePreset(int classID)
  {
    if (!SteamManager.Running || GameCore.Console.Platform != DistributionPlatform.Steam || !PlayerPrefsSl.Get("RichPresence", true))
      return;
    string str;
    if (classID < 0)
    {
      str = classID == -1 ? "#Status_WaitingForPlayers" : "#Status_MainMenu";
    }
    else
    {
      SteamFriends.SetRichPresence("class", UnityEngine.Object.FindObjectOfType<CharacterClassManager>().Classes[classID].roleId.ToString());
      str = "#PlayingAs";
    }
    SteamFriends.SetRichPresence("steam_display", str);
  }

  public static void ChangeLobbyStatus(int cur, int max)
  {
    if (!SteamManager.Running || GameCore.Console.Platform != DistributionPlatform.Steam || !PlayerPrefsSl.Get("RichPresence", true))
      return;
    SteamFriends.SetRichPresence("lobbystatus", cur.ToString() + "/" + (object) max);
  }

  public static void ClearRichPresence()
  {
    if (!SteamManager.Running || GameCore.Console.Platform != DistributionPlatform.Steam)
      return;
    SteamFriends.ClearRichPresence();
  }

  public static bool IndicateAchievementProgress(string name, int curProgress, int maxProgress)
  {
    return SteamUserStats.IndicateAchievementProgress(name, curProgress, maxProgress);
  }

  public static void SetStat(string key, int value)
  {
    SteamUserStats.SetStat(key, value);
  }

  public static int GetStat(string key)
  {
    return SteamUserStats.GetStatInt(key);
  }

  public static string GetPersonaName(ulong steamid = 0)
  {
    return new Friend((SteamId) steamid).Name;
  }

  public static AuthTicket GetAuthSessionTicket()
  {
    if (!SteamManager.Running)
      return (AuthTicket) null;
    if (SteamManager._ticket == null)
      SteamManager._ticket = SteamUser.GetAuthSessionTicket();
    return SteamManager._ticket;
  }

  public static void CancelTicket()
  {
    if (SteamManager._ticket == null)
      return;
    SteamManager._ticket.Cancel();
    SteamManager._ticket = (AuthTicket) null;
  }

  public static void OpenProfile(ulong steamid = 0)
  {
    SteamFriends.OpenUserOverlay((SteamId) (steamid == 0UL ? SteamManager.SteamId64 : steamid), nameof (steamid));
  }

  public static void StartClient()
  {
    if (SteamManager.Running)
    {
      Debug.Log((object) "Only one Steam Client can be initalized at the same time!");
    }
    else
    {
      try
      {
        SteamClient.Init(700330U, true);
      }
      catch (Exception ex)
      {
        Debug.LogError((object) ("Steam failed to launch: " + ex.Message));
        SteamManager._state = "Client isn't running";
        GameCore.Console.AddLog("Steam client failed to initialize. Please ensure that Steam is running.", (UnityEngine.Color) new Color32(byte.MaxValue, (byte) 25, (byte) 25, byte.MaxValue), false);
        return;
      }
      if (!SteamClient.IsValid)
      {
        Debug.LogWarning((object) SteamManager._state);
        SteamManager._state = "Client is invalid.";
        SteamManager.StopClient();
      }
      else if (!SteamClient.IsLoggedOn)
      {
        SteamManager._state = "Client isn't logged-on.";
        SteamManager.StopClient();
      }
      else
      {
        Debug.Log((object) ("Started Steam Client (" + SteamClient.Name + " - " + (object) SteamClient.SteamId + ")"));
        SteamManager.SteamId64 = (ulong) SteamClient.SteamId;
        SteamManager.Running = true;
      }
    }
  }

  public static void StopClient()
  {
    SteamClient.Shutdown();
    SteamManager.Running = false;
  }

  public static string GetApiState()
  {
    if (SteamManager.Running)
      return "Loaded";
    return !string.IsNullOrEmpty(SteamManager._state) ? SteamManager._state : "Not Loaded";
  }

  public static void RestartSteam()
  {
    SteamManager.StopClient();
    SteamManager.StartClient();
  }

  public static void RefreshToken()
  {
    SteamManager.GetAuthSessionTicket();
    SteamManager.CancelTicket();
    DebugLog.LogBuild("Refreshed auth token!");
  }

  private void Awake()
  {
  }

  private void Start()
  {
    if (GameCore.Console.Platform != DistributionPlatform.Steam)
      return;
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnLevelFinishedLoading);
    SteamManager.ChangeLobbyStatus(0, 20);
  }

  private void Update()
  {
    if (!SteamManager.Running)
      return;
    SteamClient.RunCallbacks();
  }

  private void OnApplicationQuit()
  {
    SteamManager.CancelTicket();
    SteamManager.StopClient();
  }

  private void OnDestroy()
  {
    SteamManager.CancelTicket();
    SteamManager.StopClient();
  }

  private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
  {
    if (scene.buildIndex == 3 || scene.buildIndex == 4)
      SteamManager.ChangePreset(-2);
    if (!(scene.name == "Facility"))
      return;
    SteamManager.ChangePreset(-1);
  }
}
