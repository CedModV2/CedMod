// Decompiled with JetBrains decompiler
// Type: PlayerList
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils.ConfigHandler;

public class PlayerList : NetworkBehaviour
{
  private static readonly ConfigEntry<float> _refreshRate = new ConfigEntry<float>("player_list_title_rate", 5f, "Player List Title Refresh Rate", "The amount of time (in seconds) between refreshing the title of the player list");
  public static readonly ConfigEntry<string> Title = new ConfigEntry<string>("player_list_title", (string) null, "Player List Title", "The title at the top of the player list menu.");
  public static bool anyAdminOnServer = false;
  public static List<PlayerList.Instance> instances = new List<PlayerList.Instance>();
  public Transform parent;
  public Transform template;
  public GameObject reportForm;
  public GameObject panel;
  public static InterfaceColorAdjuster ica;
  public static PlayerList singleton;
  [SyncVar]
  public int RoundStartTime;
  [SyncVar]
  public string syncServerName;
  private int timer;
  private static Transform s_parent;
  private static Transform s_template;
  private KeyCode openKey;

  private void Update()
  {
    RectTransform component = this.GetComponent<RectTransform>();
    component.localPosition = Vector3.zero;
    component.sizeDelta = Vector2.zero;
  }

  private void Start()
  {
    this.openKey = NewInput.GetKey("Player List");
    PlayerList.anyAdminOnServer = false;
    if (!NetworkServer.active)
      return;
    ConfigFile.ServerConfig.UpdateConfigValue((ConfigEntry) PlayerList._refreshRate);
    ConfigFile.ServerConfig.UpdateConfigValue((ConfigEntry) PlayerList.Title);
    Timing.RunCoroutine(this._RefreshTitleLoop(), Segment.FixedUpdate);
  }

  private void Awake()
  {
    PlayerList.instances.Clear();
    PlayerList.singleton = this;
    PlayerList.s_parent = this.parent;
    PlayerList.s_template = this.template;
  }

  public static void AddPlayer(GameObject instance)
  {
    PlayerList.UpdatePlayerRole(instance);
  }

  public static void UpdatePlayerRole(GameObject instance)
  {
    PlayerList.anyAdminOnServer = false;
    GameCore.Console.AddDebugLog("PLIST", "[PlayerList] UpdatePlayerRole: " + ReferenceHub.GetHub(instance).nicknameSync.MyNick, MessageImportance.LessImportant, false);
    foreach (PlayerList.Instance instance1 in PlayerList.instances)
    {
      ReferenceHub hub = ReferenceHub.GetHub(instance1.owner);
      if (!PlayerList.anyAdminOnServer && !string.IsNullOrEmpty(hub.serverRoles.GetUncoloredRoleString()))
        PlayerList.anyAdminOnServer = true;
      int num = (UnityEngine.Object) instance != (UnityEngine.Object) instance1.owner ? 1 : 0;
    }
  }

  public static void UpdateColors()
  {
  }

  public static void DestroyPlayer(GameObject instance)
  {
    foreach (PlayerList.Instance instance1 in PlayerList.instances)
    {
      if (!((UnityEngine.Object) instance1.owner != (UnityEngine.Object) instance))
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) instance1.listElementReference.gameObject);
        PlayerList.instances.Remove(instance1);
        break;
      }
    }
  }

  public void RefreshTitleSafe()
  {
    if (string.IsNullOrEmpty(PlayerList.Title.Value))
    {
      this.NetworksyncServerName = ServerConsole.singleton.RefreshServerNameSafe();
    }
    else
    {
      string result;
      if (!ServerConsole.singleton.NameFormatter.TryProcessExpression(PlayerList.Title.Value, "player list title", out result))
        ServerConsole.AddLog(result);
      else
        this.NetworksyncServerName = result;
    }
  }

  public void RefreshTitle()
  {
    this.NetworksyncServerName = string.IsNullOrEmpty(PlayerList.Title.Value) ? ServerConsole.singleton.RefreshServerName() : ServerConsole.singleton.NameFormatter.ProcessExpression(PlayerList.Title.Value);
  }

  private IEnumerator<float> _RefreshTitleLoop()
  {
    PlayerList playerList = this;
    while ((UnityEngine.Object) playerList != (UnityEngine.Object) null)
    {
      playerList.RefreshTitleSafe();
      for (ushort i = 0; (double) i < 50.0 * (double) PlayerList._refreshRate.Value; ++i)
        yield return 0.0f;
    }
  }

  private void MirrorProcessed()
  {
  }

  public int NetworkRoundStartTime
  {
    get
    {
      return this.RoundStartTime;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.RoundStartTime, 1UL);
    }
  }

  public string NetworksyncServerName
  {
    get
    {
      return this.syncServerName;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.syncServerName, 2UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32(this.RoundStartTime);
      writer.WriteString(this.syncServerName);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.RoundStartTime);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteString(this.syncServerName);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworkRoundStartTime = reader.ReadPackedInt32();
      this.NetworksyncServerName = reader.ReadString();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
        this.NetworkRoundStartTime = reader.ReadPackedInt32();
      if ((num & 2L) == 0L)
        return;
      this.NetworksyncServerName = reader.ReadString();
    }
  }

  [Serializable]
  public class Instance
  {
    public GameObject owner;
    public PlayerListElement listElementReference;
  }
}
