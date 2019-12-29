// Decompiled with JetBrains decompiler
// Type: CustomNetworkManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Integrations.MirrorIgnorance;
using GameCore;
using LiteNetLib;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Mono.Nat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : LiteNetLib4MirrorNetworkManager
{
  private readonly HashSet<IPEndPoint> _dictToRemove = new HashSet<IPEndPoint>();
  public string disconnectMessage = "";
  public static string Ip = "";
  public static bool Modded = false;
  public static readonly bool isPrivateBeta = false;
  public static readonly bool isStreamingAllowed = false;
  public static readonly string[] CompatibleVersions = new string[1]
  {
    "9.1.3"
  };
  private static readonly int _expectedGameFilesVersion = 4;
  public GameObject popup;
  public GameObject createpop;
  public RectTransform contSize;
  private static QueryServer _queryserver;
  private List<INatDevice> _mappedDevices;
  public CustomNetworkManager.DisconnectLog[] logs;
  private int _curLogId;
  private int _queryPort;
  public float reconnectTime;
  private bool _queryEnabled;
  private bool _configLoaded;
  private bool _activated;
  private float _dictCleanupTime;
  private float _ipRateLimitTime;
  private float _userIdRateLimitTime;
  private static ushort _ipRateLimitWindow;
  private static ushort _userIdLimitWindow;
  public static string ConnectionIp;
  [Space(30f)]
  public int GameFilesVersion;
  public static readonly byte Major;
  public static readonly byte Minor;
  public static int slots;
  public static int reservedSlots;

  public static CustomNetworkManager TypedSingleton
  {
    get
    {
      return (CustomNetworkManager) LiteNetLib4MirrorNetworkManager.singleton;
    }
  }

  static CustomNetworkManager()
  {
    Misc.ParseVersion(out CustomNetworkManager.Major, out CustomNetworkManager.Minor);
  }

  public int MaxPlayers
  {
    get
    {
      return this.maxConnections;
    }
    set
    {
      this.maxConnections = value;
      LiteNetLib4MirrorTransport.Singleton.maxConnections = (ushort) value;
    }
  }

  private void Update()
  {
  }

  private void FixedUpdate()
  {
    if (!NetworkServer.active)
      return;
    this._dictCleanupTime += Time.fixedDeltaTime;
    this._ipRateLimitTime += Time.fixedDeltaTime;
    this._userIdRateLimitTime += Time.fixedDeltaTime;
    if ((double) this._ipRateLimitTime >= (double) CustomNetworkManager._ipRateLimitWindow)
    {
      this._ipRateLimitTime = 0.0f;
      CustomLiteNetLib4MirrorTransport.IpRateLimit.Clear();
    }
    if ((double) this._userIdRateLimitTime >= (double) CustomNetworkManager._userIdLimitWindow)
    {
      this._userIdRateLimitTime = 0.0f;
      CustomLiteNetLib4MirrorTransport.UserRateLimit.Clear();
    }
    if ((double) this._dictCleanupTime <= 30.0)
      return;
    this._dictCleanupTime = 0.0f;
    DateTime dateTime = DateTime.Now;
    dateTime = dateTime.AddSeconds(-90.0);
    long ticks = dateTime.Ticks;
    foreach (KeyValuePair<IPEndPoint, PreauthItem> userId in CustomLiteNetLib4MirrorTransport.UserIds)
    {
      if (userId.Value.Added <= ticks)
        this._dictToRemove.Add(userId.Key);
    }
    foreach (IPEndPoint key in this._dictToRemove)
    {
      if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(key))
        CustomLiteNetLib4MirrorTransport.UserIds.Remove(key);
    }
    this._dictToRemove.Clear();
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
    base.OnClientDisconnect(conn);
    SteamManager.CancelTicket();
    switch (LiteNetLib4MirrorCore.LastDisconnectReason)
    {
      case DisconnectReason.ConnectionFailed:
        this.ShowLog(1, "", "", "");
        break;
      case DisconnectReason.Timeout:
        this.ShowLog(6, "", "", "");
        break;
      case DisconnectReason.HostUnreachable:
      case DisconnectReason.NetworkUnreachable:
        this.ShowLog(1, "", "", "");
        break;
      case DisconnectReason.ConnectionRejected:
        switch (CustomLiteNetLib4MirrorTransport.LastRejectionReason)
        {
          case RejectionReason.ServerFull:
            this.ShowLog(4, "", "", "");
            return;
          case RejectionReason.InvalidToken:
            this.ShowLog(30, "", "", "");
            return;
          case RejectionReason.VersionMismatch:
            this.ShowLog(9, "", "", "");
            return;
          case RejectionReason.Error:
            this.ShowLog(19, "", "", "");
            return;
          case RejectionReason.AuthenticationRequired:
            this.ShowLog(20, "", "", "");
            return;
          case RejectionReason.Banned:
            string str1 = string.IsNullOrWhiteSpace(CustomLiteNetLib4MirrorTransport.LastCustomReason) ? (string) null : CustomLiteNetLib4MirrorTransport.LastCustomReason.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
            if (str1 != null && str1.Length > 400)
              str1 = str1.Substring(0, 400);
            if (CustomLiteNetLib4MirrorTransport.LastBanExpiration == 0L)
            {
              if (str1 == null)
              {
                this.ShowLog(24, "", "", "");
                return;
              }
              this.ShowLog(31, str1, "", "");
              return;
            }
            try
            {
              DateTime dateTime = new DateTime(CustomLiteNetLib4MirrorTransport.LastBanExpiration);
              if (str1 == null)
              {
                this.ShowLog(25, dateTime.ToShortDateString(), dateTime.ToLongTimeString(), "");
                return;
              }
              this.ShowLog(32, dateTime.ToShortDateString(), dateTime.ToLongTimeString(), str1);
              return;
            }
            catch
            {
              if (str1 == null)
              {
                this.ShowLog(24, "", "", "");
                return;
              }
              this.ShowLog(31, str1, "", "");
              return;
            }
          case RejectionReason.NotWhitelisted:
            this.ShowLog(21, "", "", "");
            return;
          case RejectionReason.Geoblocked:
            this.ShowLog(28, "", "", "");
            return;
          case RejectionReason.Custom:
            string str2 = string.IsNullOrEmpty(CustomLiteNetLib4MirrorTransport.LastCustomReason) ? "(no reason)" : CustomLiteNetLib4MirrorTransport.LastCustomReason.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty);
            if (str2.Length > 400)
              str2 = str2.Substring(0, 400);
            this.ShowLog(29, str2, "", "");
            return;
          case RejectionReason.ExpiredAuth:
            this.ShowLog(26, "", "", "");
            return;
          case RejectionReason.RateLimit:
            this.ShowLog(27, "", "", "");
            return;
          default:
            this.ShowLog(0, "", "", "");
            return;
        }
      default:
        this.ShowLog(0, "", "", "");
        break;
    }
  }

  public override void OnClientError(NetworkConnection conn, int errorCode)
  {
    this.ShowLog(errorCode, "", "", "");
  }

  public override void OnStartClient()
  {
    base.OnStartClient();
    CharacterClassManager.ResetHostFound();
    this.StartCoroutine((IEnumerator) this._ConnectToServer());
  }

  private IEnumerator<float> _ConnectToServer()
  {
    while (LiteNetLib4MirrorCore.State == LiteNetLib4MirrorCore.States.ClientConnecting || LiteNetLib4MirrorCore.State == LiteNetLib4MirrorCore.States.ClientConnected)
    {
      if (NetworkClient.isConnected)
      {
        this.ShowLog(17, "", "", "");
        break;
      }
      yield return 0.0f;
    }
  }

  public bool IsFacilityLoading()
  {
    return this._curLogId == 17;
  }

  public override void OnServerDisconnect(NetworkConnection conn)
  {
    base.OnServerDisconnect(conn);
    MirrorIgnoranceServer.ForceDisconnectClient(conn);
    conn.Disconnect();
    conn.Dispose();
  }

  public static void PlayerDisconnect(NetworkConnection conn)
  {
    MirrorIgnoranceServer.ForceDisconnectClient(conn);
  }

  private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
  {
    if (scene.name.Contains("menu", StringComparison.OrdinalIgnoreCase))
    {
      this._curLogId = 0;
      if (!this._activated)
        this._activated = true;
    }
    if ((double) this.reconnectTime <= 0.0)
      return;
    this.ShowLog(14, "", "", "");
    this.Invoke("Reconnect", 3f);
  }

  public override void OnClientSceneChanged(NetworkConnection conn)
  {
    base.OnClientSceneChanged(conn);
    CharacterClassManager.ResetHostFound();
    if ((double) this.reconnectTime > 0.0 || !this.logs[this._curLogId].autoHideOnSceneLoad)
      return;
    this.popup.SetActive(false);
  }

  public bool ShouldPlayIntensive()
  {
    return this._curLogId == 13 || this.IsFacilityLoading();
  }

  private void Reconnect()
  {
    if ((double) this.reconnectTime <= 0.0)
      return;
    this.Invoke("StartClient", this.reconnectTime);
    this.reconnectTime = 0.0f;
  }

  public void StopReconnecting()
  {
    this.reconnectTime = 0.0f;
  }

  public void ShowLog(int id, string obj1 = "", string obj2 = "", string obj3 = "")
  {
  }

  public int ReservedMaxPlayers
  {
    get
    {
      return CustomNetworkManager.slots;
    }
  }

  public void LoadConfigs(bool firstTime = false)
  {
    if (this._configLoaded)
      return;
    this._configLoaded = true;
    ConfigFile.HosterPolicy = !System.IO.File.Exists("hoster_policy.txt") ? (!System.IO.File.Exists(FileManager.GetAppFolder(true, false, "") + "hoster_policy.txt") ? new YamlConfig() : new YamlConfig(FileManager.GetAppFolder(true, false, "") + "hoster_policy.txt")) : new YamlConfig("hoster_policy.txt");
    FileManager.RefreshAppFolder();
    if (ServerStatic.IsDedicated)
      return;
    ServerConsole.AddLog("Loading configs...");
    ConfigFile.ReloadGameConfigs(firstTime);
    ServerConsole.AddLog("Config file loaded!");
  }

  public override void ConfigureServerFrameRate()
  {
  }

  public override void Start()
  {
    base.Start();
    this.LoadConfigs(true);
    if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux || System.IO.File.Exists("/etc/ssl/certs/ca-certificates.crt"))
      return;
    if (System.IO.File.Exists("/etc/pki/tls/certs/ca-bundle.crt"))
      ServerConsole.AddLog("System CA Cert store not available! Unity expects it to be in /etc/ssl/certs/ca-certificates.crt, but we've detected it's present in /etc/pki/tls/certs/ca-bundle.crt on your system, please symlink your store to the required location!");
    else if (System.IO.File.Exists("/etc/ssl/ca-bundle.pem"))
      ServerConsole.AddLog("System CA Cert store not available! Unity expects it to be in /etc/ssl/certs/ca-certificates.crt, but we've detected it's present in /etc/ssl/ca-bundle.pem on your system, please symlink your store to the required location!");
    else if (System.IO.File.Exists("/etc/pki/tls/cacert.pem"))
      ServerConsole.AddLog("System CA Cert store not available! Unity expects it to be in /etc/ssl/certs/ca-certificates.crt, but we've detected it's present in /etc/pki/tls/cacert.pem on your system, please symlink your store to the required location!");
    else if (System.IO.File.Exists("/etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem"))
      ServerConsole.AddLog("System CA Cert store not available! Unity expects it to be in /etc/ssl/certs/ca-certificates.crt, but we've detected it's present in /etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem on your system, please symlink your store to the required location!");
    else
      ServerConsole.AddLog("System CA Cert store not available! Unity expects it to be in /etc/ssl/certs/ca-certificates.crt and we couldn't detect its location! Please provide access to it in the specified path!");
  }

  public void CreateMatch()
  {
    this.LoadConfigs(false);
    this.ShowLog(13, "", "", "");
    this.createpop.SetActive(false);
    NetworkServer.Reset();
    ServerConsole.AddLog("Loading configs...");
    ConfigFile.ReloadGameConfigs(false);
    LiteNetLib4MirrorTransport.Singleton.port = ServerStatic.IsDedicated ? ServerStatic.ServerPort : this.GetFreePort();
    LiteNetLib4MirrorTransport.Singleton.useUpnP = ConfigFile.ServerConfig.GetBool("forward_ports", true);
    CustomNetworkManager.slots = ConfigFile.ServerConfig.GetInt("max_players", 20);
    CustomNetworkManager.reservedSlots = Mathf.Max(ConfigFile.ServerConfig.GetInt("reserved_slots", ReservedSlot.Users.Count), 0);
    this.MaxPlayers = (CustomNetworkManager.slots + CustomNetworkManager.reservedSlots) * 2 + 50;
    int num = ConfigFile.HosterPolicy.GetInt("players_limit", -1);
    if (num > 0 && CustomNetworkManager.slots + CustomNetworkManager.reservedSlots > num)
    {
      this.MaxPlayers = num * 2 + 50;
      ServerConsole.AddLog("You have exceeded players limit set by your hosting provider. Max players value set to " + (object) num);
    }
    ServerConsole.Port = (int) LiteNetLib4MirrorTransport.Singleton.port;
    ServerConsole.AddLog("Config files loaded from " + FileManager.GetAppFolder(true, true, ""));
    this._queryEnabled = ConfigFile.ServerConfig.GetBool("enable_query", false);
    string str1 = FileManager.GetAppFolder(true, true, "") + "config_remoteadmin.txt";
    if (!System.IO.File.Exists(str1))
      System.IO.File.Copy("ConfigTemplates/config_remoteadmin.template.txt", str1);
    ServerStatic.RolesConfigPath = str1;
    ServerStatic.RolesConfig = new YamlConfig(str1);
    ServerStatic.SharedGroupsConfig = ConfigSharing.Paths[4] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt");
    ServerStatic.SharedGroupsMembersConfig = ConfigSharing.Paths[5] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt");
    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
    ServerConsole.FriendlyFire = ConfigFile.ServerConfig.GetBool("friendly_fire", false);
    ServerConsole.WhiteListEnabled = ConfigFile.ServerConfig.GetBool("enable_whitelist", false) || ConfigFile.ServerConfig.GetBool("custom_whitelist", false);
    ServerConsole.AccessRestriction = ConfigFile.ServerConfig.GetBool("server_access_restriction", false);
    ServerConsole.RateLimitKick = ConfigFile.ServerConfig.GetBool("ratelimit_kick", true);
    ServerConsole.EnforceSameIp = ConfigFile.ServerConfig.GetBool("enforce_same_ip", true);
    ServerConsole.EnforceSameAsn = ConfigFile.ServerConfig.GetBool("enforce_same_asn", true);
    ServerConsole.SkipEnforcementForLocalAddresses = ConfigFile.ServerConfig.GetBool("no_enforcement_for_local_ip_addresses", true);
    CustomLiteNetLib4MirrorTransport.IpRateLimiting = ConfigFile.ServerConfig.GetBool("enable_ip_ratelimit", true);
    CustomLiteNetLib4MirrorTransport.UserRateLimiting = ConfigFile.ServerConfig.GetBool("enable_userid_ratelimit", true);
    CustomLiteNetLib4MirrorTransport.UseGlobalBans = ConfigFile.ServerConfig.GetBool("use_global_bans", true);
    CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted = ConfigFile.ServerConfig.GetBool("geoblocking_ignore_whitelisted", true);
    CustomLiteNetLib4MirrorTransport.GeoblockingList.Clear();
    string str2 = ConfigFile.ServerConfig.GetString("geoblocking_mode", "none");
    if (!(str2 == "whitelist"))
    {
      if (str2 == "blacklist")
      {
        CustomLiteNetLib4MirrorTransport.Geoblocking = GeoblockingMode.Blacklist;
        foreach (string str3 in ConfigFile.ServerConfig.GetStringList("geoblocking_blacklist"))
          CustomLiteNetLib4MirrorTransport.GeoblockingList.Add(str3.ToUpper());
      }
    }
    else
    {
      CustomLiteNetLib4MirrorTransport.Geoblocking = GeoblockingMode.Whitelist;
      foreach (string str3 in ConfigFile.ServerConfig.GetStringList("geoblocking_whitelist"))
        CustomLiteNetLib4MirrorTransport.GeoblockingList.Add(str3.ToUpper());
    }
    CustomNetworkManager._ipRateLimitWindow = ConfigFile.ServerConfig.GetUShort("ip_ratelimit_window", (ushort) 3);
    CustomNetworkManager._userIdLimitWindow = ConfigFile.ServerConfig.GetUShort("userid_ratelimit_window", (ushort) 5);
    if (CustomNetworkManager._ipRateLimitWindow == (ushort) 0)
      CustomNetworkManager._ipRateLimitWindow = (ushort) 1;
    if (CustomNetworkManager._userIdLimitWindow == (ushort) 0)
      CustomNetworkManager._userIdLimitWindow = (ushort) 1;
    Timing.RunCoroutine(this._CreateLobby());
  }

  private IEnumerator<float> _CreateLobby()
  {
    CustomNetworkManager customNetworkManager = this;
    if (customNetworkManager.GameFilesVersion != CustomNetworkManager._expectedGameFilesVersion)
    {
      ServerConsole.AddLog("This source code file is made for different version of the game!");
      ServerConsole.AddLog("Please validate game files integrity using steam!");
      ServerConsole.AddLog("Aborting server startup.");
    }
    else
    {
      ServerConsole.AddLog("Game version: " + CustomNetworkManager.CompatibleVersions[0]);
      if (CustomNetworkManager.isPrivateBeta)
        ServerConsole.AddLog("PRIVATE BETA VERSION - DO NOT SHARE");
      yield return float.NegativeInfinity;
      ServerConsole.AddLog(ConfigFile.ServerConfig.GetBool("online_mode", true) ? "Online mode is ENABLED." : "Online mode is DISABLED - SERVER CANNOT VALIDATE USER ID OF CONNECTING PLAYERS!!! Features like User ID admin authentication won't work.");
      ServerConsole.RunRefreshPublicKey();
      ServerConsole.RunRefreshCentralServers();
      if (customNetworkManager._queryEnabled)
      {
        customNetworkManager._queryPort = (int) LiteNetLib4MirrorTransport.Singleton.port + ConfigFile.ServerConfig.GetInt("query_port_shift", 0);
        ServerConsole.AddLog("Query port will be enabled on port " + (object) customNetworkManager._queryPort + " TCP.");
        CustomNetworkManager._queryserver = new QueryServer(customNetworkManager._queryPort, ConfigFile.ServerConfig.GetBool("query_use_IPv6", true));
        CustomNetworkManager._queryserver.StartServer();
      }
      else
        ServerConsole.AddLog("Query port disabled in config!");
      ServerConsole.AddLog("Starting server...");
      if (ConfigFile.HosterPolicy.GetString("server_ip", "none") != "none")
      {
        CustomNetworkManager.Ip = ConfigFile.HosterPolicy.GetString("server_ip", "none");
        ServerConsole.AddLog("Server IP set to " + CustomNetworkManager.Ip + " by your hosting provider.");
      }
      else if (ConfigFile.ServerConfig.GetBool("online_mode", true) && ServerStatic.IsDedicated)
      {
        if (ConfigFile.ServerConfig.GetString("server_ip", "auto") != "auto")
        {
          CustomNetworkManager.Ip = ConfigFile.ServerConfig.GetString("server_ip", "auto");
          ServerConsole.AddLog("Custom config detected. Your game-server IP will be " + CustomNetworkManager.Ip);
        }
        else
        {
          ServerConsole.AddLog("Obtaining your external IP address...");
          using (UnityWebRequest www = UnityWebRequest.Get(CentralServer.StandardUrl + "ip.php"))
          {
            yield return Timing.WaitUntilDone((AsyncOperation) www.SendWebRequest());
            if (string.IsNullOrEmpty(www.error))
            {
              CustomNetworkManager.Ip = www.downloadHandler.text.EndsWith(".") ? www.downloadHandler.text.Remove(www.downloadHandler.text.Length - 1) : www.downloadHandler.text;
              ServerConsole.AddLog("Done, your game-server IP will be " + CustomNetworkManager.Ip);
            }
            else
            {
              ServerConsole.AddLog("Error: connection to " + CentralServer.StandardUrl + " failed. Website returned: " + www.error + " | Aborting startup... LOGTYPE-8");
              yield break;
            }
          }
        }
      }
      else
        CustomNetworkManager.Ip = "127.0.0.1";
      ServerConsole.Ip = CustomNetworkManager.Ip;
      ServerConsole.AddLog("Initializing game server...");
      if (ServerStatic.IsDedicated)
      {
        if (ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "none") != "none")
        {
          LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress = ConfigFile.HosterPolicy.GetString("ipv4_bind_ip", "0.0.0.0");
          if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
            ServerConsole.AddLog("Server starting at all IPv4 addresses and port " + (object) LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.");
          else
            ServerConsole.AddLog("Server starting at IPv4 " + LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress + " and port " + (object) LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.");
        }
        else
        {
          LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress = ConfigFile.ServerConfig.GetString("ipv4_bind_ip", "0.0.0.0");
          if (LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress == "0.0.0.0")
            ServerConsole.AddLog("Server starting at all IPv4 addresses and port " + (object) LiteNetLib4MirrorTransport.Singleton.port);
          else
            ServerConsole.AddLog("Server starting at IPv4 " + LiteNetLib4MirrorTransport.Singleton.serverIPv4BindAddress + " and port " + (object) LiteNetLib4MirrorTransport.Singleton.port);
        }
        if (ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "none") != "none")
        {
          LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = ConfigFile.HosterPolicy.GetString("ipv6_bind_ip", "::");
          if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
            ServerConsole.AddLog("Server starting at all IPv6 addresses and port " + (object) LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.");
          else
            ServerConsole.AddLog("Server starting at IPv6 " + LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress + " and port " + (object) LiteNetLib4MirrorTransport.Singleton.port + " - set by your hosting provider.");
        }
        else
        {
          LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress = ConfigFile.ServerConfig.GetString("ipv6_bind_ip", "::");
          if (LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress == "::")
            ServerConsole.AddLog("Server starting at all IPv6 addresses and port " + (object) LiteNetLib4MirrorTransport.Singleton.port);
          else
            ServerConsole.AddLog("Server starting at IPv6 " + LiteNetLib4MirrorTransport.Singleton.serverIPv6BindAddress + " and port " + (object) LiteNetLib4MirrorTransport.Singleton.port);
        }
        customNetworkManager.StartHost();
        while (SceneManager.GetActiveScene().name != "Facility")
          yield return float.NegativeInfinity;
        ServerConsole.AddLog("Level loaded. Creating match...");
        if (!ConfigFile.ServerConfig.GetBool("online_mode", true))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to online_mode turned off in server configuration.LOGTYPE-8");
        else if (!ConfigFile.ServerConfig.GetBool("use_vac", true))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to use_vac turned off in server configuration.LOGTYPE-8");
        else if (!ConfigFile.ServerConfig.GetBool("use_global_bans", true))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to use_global_bans turned off in server configuration.LOGTYPE-8");
        else if (ConfigFile.ServerConfig.GetBool("disable_global_badges", false))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to disable_global_badges turned on in server configuration (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
        else if (ConfigFile.ServerConfig.GetBool("hide_global_badges", false))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to hide_global_badges turned on in server configuration. You can still disable specific badges instead of using this command. (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
        else if (ConfigFile.ServerConfig.GetBool("disable_ban_bypass", false))
          ServerConsole.AddLog("Server WON'T be visible on the public list due to disable_ban_bypass turned on in server configuration. (this is servermod function - if you are not using servermod, you can safely remove this config value, it won't change anything).LOGTYPE-8");
        else if (ConfigFile.ServerConfig.GetBool("hide_from_public_list", false))
        {
          ServerConsole.AddLog("Server WON'T be visible on the public list due to hide_from_public_list enabled in server configuration.LOGTYPE-8");
        }
        else
        {
          if (ConfigFile.ServerConfig.GetBool("hide_patreon_badges_by_default", false) || ConfigFile.ServerConfig.GetBool("block_gtag_patreon_badges", false) || (ConfigFile.ServerConfig.GetBool("block_gtag_banteam_badges", false) || ConfigFile.ServerConfig.GetBool("block_gtag_management_badges", false)))
            ServerConsole.AddLog("If your server is verified (put in the official server list) some badge settings enabled in your config will be ignored. If your server isn't on the public list - ignore this message.LOGTYPE-8");
          UnityEngine.Object.FindObjectOfType<ServerConsole>().RunServer();
        }
      }
    }
  }

  public ushort GetFreePort()
  {
    return ServerStatic.ServerPort;
  }

  [Serializable]
  public class DisconnectLog
  {
    [Multiline]
    public string msg_en;
    public CustomNetworkManager.DisconnectLog.LogButton button;
    public bool autoHideOnSceneLoad;

    [Serializable]
    public class LogButton
    {
      public ConnInfoButton[] actions;
    }
  }
}
