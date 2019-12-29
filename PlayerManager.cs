// Decompiled with JetBrains decompiler
// Type: PlayerManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class PlayerManager
{
  public static readonly List<GameObject> players = new List<GameObject>();
  public static GameObject localPlayer;
  public static SpectatorManager spect;
  public static bool LocalPlayerSet;

  static PlayerManager()
  {
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(PlayerManager.OnSceneLoaded);
  }

  private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
  {
    PlayerManager.players.Clear();
  }

  public static void AddPlayer(GameObject player)
  {
    Console.AddDebugLog("PLIST", "[PlayerManager] AddPlayer: " + player.GetComponent<NicknameSync>().MyNick, MessageImportance.LessImportant, false);
    if (CollectionExtensions.Contains(PlayerManager.players, player))
      return;
    PlayerManager.players.Add(player);
    ServerConsole.PlayersAmount = PlayerManager.players.Count;
    ServerConsole.PlayersListChanged = true;
  }

  public static void RemovePlayer(GameObject player)
  {
    PlayerList.DestroyPlayer(player);
    if (!CollectionExtensions.Contains(PlayerManager.players, player))
      return;
    PlayerManager.players.Remove(player);
    ServerConsole.PlayersAmount = PlayerManager.players.Count;
    ServerConsole.PlayersListChanged = true;
  }
}
