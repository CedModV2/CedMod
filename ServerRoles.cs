// Decompiled with JetBrains decompiler
// Type: ServerRoles
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using GameCore;
using MEC;
using Mirror;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class ServerRoles : NetworkBehaviour
{
  public ServerRoles.NamedColor CurrentColor;
  public ServerRoles.NamedColor[] NamedColors;
  [NonSerialized]
  internal bool PublicKeyAccepted;
  [NonSerialized]
  public Dictionary<string, string> FirstVerResult;
  internal AsymmetricKeyParameter PublicKey;
  [NonSerialized]
  public bool BypassMode;
  [NonSerialized]
  public bool LocalRemoteAdmin;
  private bool _authorizeBadge;
  internal bool OverwatchPermitted;
  internal bool OverwatchEnabled;
  internal bool AmIInOverwatch;
  internal string PrevBadge;
  internal UserGroup Group;
  private CharacterClassManager _ccm;
  private string _globalBadgeUnconfirmed;
  private string _prevColor;
  private string _prevText;
  private string _prevBadge;
  private string _badgeUserChallenge;
  private string _authChallenge;
  private string _badgeChallenge;
  private string _bgc;
  private string _bgt;
  private string _fixedBadge;
  private bool _hideLocalBadge;
  private bool _neverHideLocalBadge;
  private bool _neverCover;
  private bool _prefSet;
  private bool _badgeCover;
  private bool _requested;
  private bool _publicPartRequested;
  private bool _badgeRequested;
  private bool _authRequested;
  private bool _noclipReady;
  private bool _publicInfoDirty;
  [SyncVar(hook = "SetColor")]
  public string MyColor;
  [SyncVar(hook = "SetText")]
  public string MyText;
  [SyncVar(hook = "SetPublicInfo")]
  public string PublicPlayerInfoToken;
  [SyncVar]
  public string GlobalBadge;
  [NonSerialized]
  public bool GlobalSet;
  [NonSerialized]
  public int GlobalBadgeType;
  [NonSerialized]
  public bool RemoteAdmin;
  [NonSerialized]
  public bool Staff;
  [NonSerialized]
  public bool BypassStaff;
  [NonSerialized]
  public bool RaEverywhere;
  [NonSerialized]
  public ulong Permissions;
  [NonSerialized]
  public string HiddenBadge;
  [NonSerialized]
  public bool DoNotTrack;
  [NonSerialized]
  public ServerRoles.AccessMode RemoteAdminMode;
  [NonSerialized]
  internal Dictionary<string, int> PlayerSkins;
  private RateLimit _commandRateLimit;

  internal byte KickPower
  {
    get
    {
      if (this.RaEverywhere)
        return byte.MaxValue;
      return this.Group != null ? this.Group.KickPower : (byte) 0;
    }
  }

  internal bool NoclipReady
  {
    get
    {
      return this._noclipReady;
    }
    set
    {
      if (this._noclipReady == value)
        return;
      this._noclipReady = value;
      if (!NetworkServer.active)
        return;
      if (!this._noclipReady)
        this._ccm.SetNoclip(false);
      this.TargetSetNoclipReady(this.connectionToClient, this._noclipReady);
    }
  }

  public void Awake()
  {
    this.PlayerSkins = new Dictionary<string, int>();
  }

  public void Start()
  {
    this._commandRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
    this._ccm = this.GetComponent<CharacterClassManager>();
  }

  [TargetRpc]
  public void TargetSetHiddenRole(NetworkConnection connection, string role)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(role);
    this.SendTargetRPCInternal(connection, typeof (ServerRoles), nameof (TargetSetHiddenRole), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcResetFixed()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (ServerRoles), nameof (RpcResetFixed), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  public void CmdRequestBadge(string token)
  {
    if (this.isServer)
    {
      this.CallCmdRequestBadge(token);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(token);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdRequestBadge), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdSetPublicPart(string token)
  {
    if (this.isServer)
    {
      this.CallCmdSetPublicPart(token);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(token);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdSetPublicPart), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdDoNotTrack()
  {
    if (this.isServer)
    {
      this.CallCmdDoNotTrack();
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdDoNotTrack), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdSetLocalTagPreferences(bool hide, bool neverHide, bool neverCover)
  {
    if (this.isServer)
    {
      this.CallCmdSetLocalTagPreferences(hide, neverHide, neverCover);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteBoolean(hide);
      writer.WriteBoolean(neverHide);
      writer.WriteBoolean(neverCover);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdSetLocalTagPreferences), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ServerCallback]
  public void SetDoNotTrack()
  {
    if (!NetworkServer.active || this.DoNotTrack)
      return;
    this.DoNotTrack = true;
    if (!string.IsNullOrEmpty(this.GetComponent<NicknameSync>().MyNick))
      this.LogDNT();
    if (this.isLocalPlayer)
      return;
    this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "Your \"Do not track\" request has been received.", "green");
  }

  [ServerCallback]
  public void LogDNT()
  {
    if (!NetworkServer.active || this._ccm.UserId == null)
      return;
    ServerLogs.AddLog(ServerLogs.Modules.Networking, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") connected from IP " + this.connectionToClient.address + " sent Do Not Track signal.", ServerLogs.ServerLogType.ConnectionUpdate);
    this._ccm.RefreshSyncedId();
  }

  [ServerCallback]
  public void RefreshPermissions(bool disp = false)
  {
    if (!NetworkServer.active)
      return;
    UserGroup userGroup1 = ServerStatic.PermissionsHandler.GetUserGroup(this._ccm.UserId);
    if (userGroup1 != null)
      this.SetGroup(userGroup1, false, false, disp);
    else if (this._ccm.UserId2 != null)
    {
      UserGroup userGroup2 = ServerStatic.PermissionsHandler.GetUserGroup(this._ccm.UserId2);
      if (userGroup2 != null)
        this.SetGroup(userGroup2, false, false, disp);
    }
    this.GetComponent<QueryProcessor>().GameplayData = PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.GameplayData);
  }

  [ServerCallback]
  public void SetGroup(UserGroup group, bool ovr, bool byAdmin = false, bool disp = false)
  {
    if (!NetworkServer.active)
      return;
    if (group == null)
    {
      if (this.RaEverywhere && (long) this.Permissions == (long) ServerStatic.PermissionsHandler.FullPerm)
        return;
      this.RemoteAdmin = false;
      this.Permissions = 0UL;
      this.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
      this.Group = (UserGroup) null;
      this.SetColor((string) null);
      this.SetText((string) null);
      this._badgeCover = false;
      if (!string.IsNullOrEmpty(this.PrevBadge))
        this.NetworkGlobalBadge = this.PrevBadge;
      this.TargetCloseRemoteAdmin(this.connectionToClient);
      this._ccm.TargetConsolePrint(this.connectionToClient, "Your local permissions has been revoked by server administrator.", "red");
    }
    else
    {
      this._ccm.TargetConsolePrint(this.connectionToClient, !byAdmin ? "Updating your group on server (local permissions)..." : "Updating your group on server (set by server administrator)...", "cyan");
      this.Group = group;
      this._badgeCover = group.Cover;
      if (!this.OverwatchPermitted && PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
        this.OverwatchPermitted = true;
      if (group.Permissions > 0UL && (long) this.Permissions != (long) ServerStatic.PermissionsHandler.FullPerm && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions))
      {
        this.RemoteAdmin = true;
        this.Permissions = group.Permissions;
        this.RemoteAdminMode = ovr ? ServerRoles.AccessMode.PasswordOverride : ServerRoles.AccessMode.LocalAccess;
        this.GetComponent<QueryProcessor>().PasswordTries = 0;
        this.TargetOpenRemoteAdmin(this.connectionToClient, ovr);
        this._ccm.TargetConsolePrint(this.connectionToClient, !byAdmin ? "Your remote admin access has been granted (local permissions)." : "Your remote admin access has been granted (set by server administrator).", "cyan");
        if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.ViewHiddenBadges))
        {
          foreach (GameObject player in PlayerManager.players)
          {
            ServerRoles component = player.GetComponent<ServerRoles>();
            if (!string.IsNullOrEmpty(component.HiddenBadge))
              component.TargetSetHiddenRole(this.connectionToClient, component.HiddenBadge);
          }
          this._ccm.TargetConsolePrint(this.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
        }
      }
      else if (!this.RaEverywhere && (long) this.Permissions != (long) ServerStatic.PermissionsHandler.FullPerm)
      {
        this.RemoteAdmin = false;
        this.Permissions = 0UL;
        this.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
        this.TargetCloseRemoteAdmin(this.connectionToClient);
      }
      ServerLogs.AddLog(ServerLogs.Modules.Permissions, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") has been assigned to group " + group.BadgeText + ".", ServerLogs.ServerLogType.ConnectionUpdate);
      if (group.BadgeColor == "none")
        return;
      if (this._hideLocalBadge || group.HiddenByDefault && !disp && !this._neverHideLocalBadge)
      {
        this._badgeCover = false;
        if (!string.IsNullOrEmpty(this.MyText))
          return;
        this.GlobalSet = false;
        this.NetworkMyText = (string) null;
        this.NetworkMyColor = (string) null;
        this.HiddenBadge = group.BadgeText;
        this.RefreshHiddenTag();
        this.TargetSetHiddenRole(this.connectionToClient, group.BadgeText);
        if (!byAdmin)
          this._ccm.TargetConsolePrint(this.connectionToClient, "Your role has been granted, but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "yellow");
        else
          this._ccm.TargetConsolePrint(this.connectionToClient, "Your role has been granted to you (set by server administrator), but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "cyan");
      }
      else
      {
        this.GlobalSet = false;
        this.HiddenBadge = (string) null;
        this.RpcResetFixed();
        this.NetworkMyText = group.BadgeText;
        this.NetworkMyColor = group.BadgeColor;
        if (!byAdmin)
          this._ccm.TargetConsolePrint(this.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (local permissions).", "cyan");
        else
          this._ccm.TargetConsolePrint(this.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (set by server administrator).", "cyan");
      }
    }
  }

  [ServerCallback]
  public void RefreshHiddenTag()
  {
    if (!NetworkServer.active)
      return;
    foreach (GameObject player in PlayerManager.players)
    {
      ServerRoles component = player.GetComponent<ServerRoles>();
      if (PermissionsHandler.IsPermitted(component.Permissions, PlayerPermissions.ViewHiddenBadges) || component.Staff)
        this.TargetSetHiddenRole(component.connectionToClient, this.HiddenBadge);
    }
  }

  private IEnumerator<float> _RequestRoleFromServer(string token)
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    ServerRoles serverRoles = this;
    if (num != 0)
      return false;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    if (!serverRoles._ccm.IsVerified && string.IsNullOrEmpty(serverRoles._ccm.UserId) || CentralAuth.ValidatePartialAuthToken(token, serverRoles._ccm.SaltedUserId, serverRoles.GetComponent<NicknameSync>().MyNick, serverRoles._ccm.AuthTokenSerial, "Badge request") == null)
      return false;
    serverRoles._globalBadgeUnconfirmed = token;
    serverRoles.StartServerChallenge(1);
    return false;
  }

  private IEnumerator<float> SetPublicTokenOnServer(string token)
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    ServerRoles serverRoles = this;
    if (num != 0)
      return false;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    if (!serverRoles._ccm.IsVerified && string.IsNullOrEmpty(serverRoles._ccm.UserId))
      return false;
    Dictionary<string, string> dict = CentralAuth.ValidatePartialAuthToken(token, serverRoles._ccm.SaltedUserId, serverRoles.GetComponent<NicknameSync>().MyNick, serverRoles._ccm.AuthTokenSerial, "Public player info");
    if (dict == null)
      return false;
    serverRoles._ccm.TargetConsolePrint(serverRoles.connectionToClient, "Your public player info has been accepted.", "cyan");
    serverRoles.NetworkPublicPlayerInfoToken = token;
    serverRoles.ProcessSkins(ref dict);
    return false;
  }

  public string GetColoredRoleString(bool newLine = false)
  {
    if (string.IsNullOrEmpty(this.MyColor) || string.IsNullOrEmpty(this.MyText) || this.CurrentColor == null || (this.CurrentColor.Restricted || this.MyText.Contains("[") || (this.MyText.Contains("]") || this.MyText.Contains("<")) || this.MyText.Contains(">")) && !this._authorizeBadge)
      return string.Empty;
    foreach (ServerRoles.NamedColor namedColor in this.NamedColors)
    {
      if (namedColor.Name == this.MyColor)
        return (newLine ? "\n" : string.Empty) + "<color=#" + namedColor.ColorHex + ">" + this.MyText + "</color>";
    }
    return string.Empty;
  }

  public string GetUncoloredRoleString()
  {
    return string.IsNullOrEmpty(this.MyColor) || string.IsNullOrEmpty(this.MyText) || this.CurrentColor == null || (this.CurrentColor.Restricted || this.MyText.Contains("[") || (this.MyText.Contains("]") || this.MyText.Contains("<")) || this.MyText.Contains(">")) && !this._authorizeBadge ? string.Empty : this.MyText;
  }

  private void Update()
  {
    if (this.CurrentColor == null)
      return;
    if (!string.IsNullOrEmpty(this._fixedBadge) && this.MyText != this._fixedBadge)
    {
      this.SetText(this._fixedBadge);
      this.SetColor("silver");
    }
    else if (!string.IsNullOrEmpty(this._fixedBadge) && this.CurrentColor.Name != "silver")
    {
      this.SetColor("silver");
    }
    else
    {
      if (this.GlobalBadge != this._prevBadge)
      {
        this._prevBadge = this.GlobalBadge;
        if (string.IsNullOrEmpty(this.GlobalBadge))
        {
          this._bgc = (string) null;
          this._bgt = (string) null;
          this._authorizeBadge = false;
          this._prevColor += ".";
          this._prevText += ".";
          return;
        }
        GameCore.Console.AddDebugLog("SDAUTH", "Validating global badge of user " + this.GetComponent<NicknameSync>().MyNick, MessageImportance.LessImportant, false);
        Dictionary<string, string> dictionary = CentralAuth.ValidatePartialAuthToken(this.GlobalBadge, this._ccm.SaltedUserId, this.GetComponent<NicknameSync>().MyNick, (string) null, "Badge request");
        if (dictionary == null)
        {
          GameCore.Console.AddDebugLog("SDAUTH", "<color=red>Validation of global badge of user " + this.GetComponent<NicknameSync>().MyNick + " failed - invalid digital signature.</color>", MessageImportance.Normal, false);
          this._bgc = (string) null;
          this._bgt = (string) null;
          this._authorizeBadge = false;
          this._prevColor += ".";
          this._prevText += ".";
          return;
        }
        GameCore.Console.AddDebugLog("SDAUTH", "Validation of global badge of user " + this.GetComponent<NicknameSync>().MyNick + " complete - badge signed by central server " + dictionary["Issued by"] + ".", MessageImportance.LessImportant, false);
        if (dictionary["Badge text"] == "(none)" && dictionary["Badge color"] == "(none)")
        {
          this._bgc = (string) null;
          this._bgt = (string) null;
          this._authorizeBadge = false;
        }
        else
        {
          this._bgc = dictionary["Badge color"];
          this._bgt = dictionary["Badge text"];
          this.NetworkMyColor = dictionary["Badge color"];
          this.NetworkMyText = dictionary["Badge text"];
          this._authorizeBadge = true;
        }
      }
      if (this._prevColor == this.MyColor && this._prevText == this.MyText)
        return;
      if (this.CurrentColor.Restricted && (this.MyText != this._bgt || this.MyColor != this._bgc))
      {
        GameCore.Console.AddLog("TAG FAIL 1 - " + this.MyText + " - " + this._bgt + " /-/ " + this.MyColor + " - " + this._bgc, Color.gray, false);
        this._authorizeBadge = false;
        this.NetworkMyColor = (string) null;
        this.NetworkMyText = (string) null;
        this._prevColor = (string) null;
        this._prevText = (string) null;
        PlayerList.UpdatePlayerRole(this.gameObject);
      }
      else if (this.MyText != null && this.MyText != this._bgt && (this.MyText.Contains("[") || this.MyText.Contains("]") || (this.MyText.Contains("<") || this.MyText.Contains(">"))))
      {
        GameCore.Console.AddLog("TAG FAIL 2 - " + this.MyText + " - " + this._bgt + " /-/ " + this.MyColor + " - " + this._bgc, Color.gray, false);
        this._authorizeBadge = false;
        this.NetworkMyColor = (string) null;
        this.NetworkMyText = (string) null;
        this._prevColor = (string) null;
        this._prevText = (string) null;
        PlayerList.UpdatePlayerRole(this.gameObject);
      }
      else
      {
        this._prevColor = this.MyColor;
        this._prevText = this.MyText;
        this._prevBadge = this.GlobalBadge;
        PlayerList.UpdatePlayerRole(this.gameObject);
      }
    }
  }

  private void ProcessSkins(ref Dictionary<string, string> dict)
  {
    this.PlayerSkins.Clear();
    foreach (KeyValuePair<string, string> keyValuePair in dict)
    {
      int result;
      if (keyValuePair.Key.StartsWith("_") && int.TryParse(keyValuePair.Value, out result))
        this.PlayerSkins.Add(keyValuePair.Key.Substring(1), result);
    }
  }

  public void SetColor(string i)
  {
    if (i == string.Empty || i == "default")
      i = (string) null;
    this.NetworkMyColor = i;
    if (i == null)
      return;
    ServerRoles.NamedColor namedColor = ((IEnumerable<ServerRoles.NamedColor>) this.NamedColors).FirstOrDefault<ServerRoles.NamedColor>((Func<ServerRoles.NamedColor, bool>) (row => row.Name == this.MyColor));
    if (namedColor == null && i != "default")
      this.SetColor((string) null);
    else
      this.CurrentColor = namedColor;
  }

  public void SetText(string i)
  {
    if (i == string.Empty)
      i = (string) null;
    this.NetworkMyText = i;
    ServerRoles.NamedColor namedColor = ((IEnumerable<ServerRoles.NamedColor>) this.NamedColors).FirstOrDefault<ServerRoles.NamedColor>((Func<ServerRoles.NamedColor, bool>) (row => row.Name == this.MyColor));
    if (namedColor == null)
      return;
    this.CurrentColor = namedColor;
  }

  public void SetPublicInfo(string i)
  {
    if (!(i == string.Empty))
      return;
    this.NetworkPublicPlayerInfoToken = (string) null;
  }

  [ServerCallback]
  public void StartServerChallenge(int selector)
  {
    if (!NetworkServer.active || selector == 0 && !string.IsNullOrEmpty(this._authChallenge) || (selector == 1 && !string.IsNullOrEmpty(this._badgeChallenge) || (selector > 1 || selector < 0)))
      return;
    byte[] numArray;
    using (RandomNumberGenerator randomNumberGenerator = (RandomNumberGenerator) new RNGCryptoServiceProvider())
    {
      numArray = new byte[32];
      randomNumberGenerator.GetBytes(numArray);
    }
    string base64String = Convert.ToBase64String(numArray);
    if (selector == 0)
    {
      this._authChallenge = "auth-" + base64String;
      this.TargetSignServerChallenge(this.connectionToClient, this._authChallenge);
    }
    else
    {
      this._badgeChallenge = "badge-server-" + base64String;
      this.TargetSignServerChallenge(this.connectionToClient, this._badgeChallenge);
    }
  }

  [TargetRpc]
  public void TargetSignServerChallenge(NetworkConnection target, string challenge)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(challenge);
    this.SendTargetRPCInternal(target, typeof (ServerRoles), nameof (TargetSignServerChallenge), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  public void CmdServerSignatureComplete(
    string challenge,
    string response,
    string publickey,
    bool hide)
  {
    if (this.isServer)
    {
      this.CallCmdServerSignatureComplete(challenge, response, publickey, hide);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WriteString(challenge);
      writer.WriteString(response);
      writer.WriteString(publickey);
      writer.WriteBoolean(hide);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdServerSignatureComplete), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [TargetRpc]
  internal void TargetOpenRemoteAdmin(NetworkConnection connection, bool password)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(password);
    this.SendTargetRPCInternal(connection, typeof (ServerRoles), nameof (TargetOpenRemoteAdmin), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  private void TargetCloseRemoteAdmin(NetworkConnection connection)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(connection, typeof (ServerRoles), nameof (TargetCloseRemoteAdmin), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  private void TargetSetNoclipReady(NetworkConnection connection, bool state)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(state);
    this.SendTargetRPCInternal(connection, typeof (ServerRoles), nameof (TargetSetNoclipReady), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [Command]
  public void CmdSetOverwatchStatus(byte status)
  {
    if (this.isServer)
    {
      this.CallCmdSetOverwatchStatus(status);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      NetworkWriterExtensions.WriteByte(writer, status);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdSetOverwatchStatus), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [Command]
  public void CmdSetNoclipStatus(byte status)
  {
    if (this.isServer)
    {
      this.CallCmdSetNoclipStatus(status);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      NetworkWriterExtensions.WriteByte(writer, status);
      this.SendCommandInternal(typeof (ServerRoles), nameof (CmdSetNoclipStatus), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  [ServerCallback]
  private void SetOverwatchStatus(bool status)
  {
    if (!NetworkServer.active)
      return;
    this.OverwatchEnabled = status;
    if (status && this._ccm.CurClass != RoleType.Spectator && this._ccm.Classes.CheckBounds(this._ccm.CurClass))
      this._ccm.SetClassID(RoleType.Spectator);
    this.TargetSetOverwatch(this.connectionToClient, this.OverwatchEnabled);
  }

  public void RequestBadge(string token)
  {
    this.CmdRequestBadge(token);
  }

  public void SetPublicPart(string token)
  {
    this.CmdSetPublicPart(token);
  }

  [TargetRpc]
  public void TargetPublicKeyAccepted(NetworkConnection conn)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(conn, typeof (ServerRoles), nameof (TargetPublicKeyAccepted), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  public void TargetSetOverwatch(NetworkConnection conn, bool s)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteBoolean(s);
    this.SendTargetRPCInternal(conn, typeof (ServerRoles), nameof (TargetSetOverwatch), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  public string NetworkMyColor
  {
    get
    {
      return this.MyColor;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(1UL))
      {
        this.setSyncVarHookGuard(1UL, true);
        this.SetColor(value);
        this.setSyncVarHookGuard(1UL, false);
      }
      this.SetSyncVar<string>(value, ref this.MyColor, 1UL);
    }
  }

  public string NetworkMyText
  {
    get
    {
      return this.MyText;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(2UL))
      {
        this.setSyncVarHookGuard(2UL, true);
        this.SetText(value);
        this.setSyncVarHookGuard(2UL, false);
      }
      this.SetSyncVar<string>(value, ref this.MyText, 2UL);
    }
  }

  public string NetworkPublicPlayerInfoToken
  {
    get
    {
      return this.PublicPlayerInfoToken;
    }
    [param: In] set
    {
      if (NetworkServer.localClientActive && !this.getSyncVarHookGuard(4UL))
      {
        this.setSyncVarHookGuard(4UL, true);
        this.SetPublicInfo(value);
        this.setSyncVarHookGuard(4UL, false);
      }
      this.SetSyncVar<string>(value, ref this.PublicPlayerInfoToken, 4UL);
    }
  }

  public string NetworkGlobalBadge
  {
    get
    {
      return this.GlobalBadge;
    }
    [param: In] set
    {
      this.SetSyncVar<string>(value, ref this.GlobalBadge, 8UL);
    }
  }

  protected static void InvokeCmdCmdRequestBadge(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdRequestBadge called on client.");
    else
      ((ServerRoles) obj).CallCmdRequestBadge(reader.ReadString());
  }

  protected static void InvokeCmdCmdSetPublicPart(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetPublicPart called on client.");
    else
      ((ServerRoles) obj).CallCmdSetPublicPart(reader.ReadString());
  }

  protected static void InvokeCmdCmdDoNotTrack(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdDoNotTrack called on client.");
    else
      ((ServerRoles) obj).CallCmdDoNotTrack();
  }

  protected static void InvokeCmdCmdSetLocalTagPreferences(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetLocalTagPreferences called on client.");
    else
      ((ServerRoles) obj).CallCmdSetLocalTagPreferences(reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdServerSignatureComplete(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdServerSignatureComplete called on client.");
    else
      ((ServerRoles) obj).CallCmdServerSignatureComplete(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
  }

  protected static void InvokeCmdCmdSetOverwatchStatus(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetOverwatchStatus called on client.");
    else
      ((ServerRoles) obj).CallCmdSetOverwatchStatus(NetworkReaderExtensions.ReadByte(reader));
  }

  protected static void InvokeCmdCmdSetNoclipStatus(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetNoclipStatus called on client.");
    else
      ((ServerRoles) obj).CallCmdSetNoclipStatus(NetworkReaderExtensions.ReadByte(reader));
  }

  public void CallCmdRequestBadge(string token)
  {
    if (this._requested || token == null)
      return;
    this._requested = true;
    Timing.RunCoroutine(this._RequestRoleFromServer(token), Segment.FixedUpdate);
  }

  public void CallCmdSetPublicPart(string token)
  {
    if (this._publicPartRequested || token == null)
      return;
    this._publicPartRequested = true;
    Timing.RunCoroutine(this.SetPublicTokenOnServer(token), Segment.FixedUpdate);
  }

  public void CallCmdDoNotTrack()
  {
    if (this.DoNotTrack)
      return;
    this.SetDoNotTrack();
  }

  public void CallCmdSetLocalTagPreferences(bool hide, bool neverHide, bool neverCover)
  {
    if (this._prefSet)
      return;
    this._prefSet = true;
    this._hideLocalBadge = hide;
    this._neverHideLocalBadge = neverHide;
    this._neverCover = neverCover;
  }

  public void CallCmdServerSignatureComplete(
    string challenge,
    string response,
    string publickey,
    bool hide)
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    if (this.FirstVerResult == null)
      this.FirstVerResult = CentralAuth.ValidatePartialAuthToken(this._globalBadgeUnconfirmed, this._ccm.SaltedUserId, this.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Badge request");
    if (this.FirstVerResult == null)
      return;
    if (this.FirstVerResult["Public key"] != Misc.Base64Encode(Sha.HashToString(Sha.Sha256(publickey))))
    {
      GameCore.Console.AddLog("Rejected signature of challenge " + challenge + " due to public key hash mismatch.", Color.red, false);
      this._ccm.TargetConsolePrint(this.connectionToClient, "Challenge signature rejected due to public key mismatch.", "red");
    }
    else
    {
      if (this.PublicKey == null)
        this.PublicKey = ECDSA.PublicKeyFromString(publickey);
      if (!ECDSA.Verify(challenge, response, this.PublicKey))
      {
        GameCore.Console.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.", Color.red, false);
        this._ccm.TargetConsolePrint(this.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
      }
      else if (challenge.StartsWith("auth-") && challenge == this._authChallenge)
      {
        this._ccm.UserId = this.FirstVerResult["User ID"];
        this._ccm.UserId2 = this.FirstVerResult.ContainsKey("User ID 2") ? this.FirstVerResult["User ID 2"] : (string) null;
        this.GetComponent<NicknameSync>().UpdateNickname(Misc.Base64Decode(this.FirstVerResult["Nickname"]));
        if (this.DoNotTrack)
          this.LogDNT();
        ServerConsole.NewPlayers.Add(this._ccm);
        this._ccm.TargetConsolePrint(this.connectionToClient, "Hi " + Misc.Base64Decode(this.FirstVerResult["Nickname"]) + "! Your challenge signature has been accepted.", "green");
        this.PublicKeyAccepted = true;
        this.TargetPublicKeyAccepted(this.connectionToClient);
        this.GetComponent<RemoteAdminCryptographicManager>().StartExchange();
        this.RefreshPermissions(false);
        this._authChallenge = (string) null;
      }
      else
      {
        if (!challenge.StartsWith("badge-server-") || !(challenge == this._badgeChallenge))
          return;
        Dictionary<string, string> dictionary = CentralAuth.ValidatePartialAuthToken(this._globalBadgeUnconfirmed, this._ccm.SaltedUserId, this.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Badge request");
        if (dictionary == null)
        {
          ServerConsole.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.");
          this._ccm.TargetConsolePrint(this.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
        }
        else
        {
          this.PrevBadge = this._globalBadgeUnconfirmed;
          if (dictionary["Staff"] == "YES")
            this.Staff = true;
          if (dictionary["Management"] == "YES" || dictionary["Global banning"] == "YES")
            this.RaEverywhere = true;
          if (dictionary["Overwatch mode"] == "YES")
            this.OverwatchPermitted = true;
          ulong result1 = ServerStatic.PermissionsHandler.FullPerm;
          if (dictionary.ContainsKey("RA Permissions"))
          {
            result1 = 0UL;
            ulong.TryParse(dictionary["RA Permissions"], out result1);
          }
          if (dictionary["Remote admin"] == "YES" && ServerStatic.PermissionsHandler.StaffAccess)
          {
            this.RemoteAdmin = true;
            this.Permissions = result1;
            this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
            this.GetComponent<QueryProcessor>().PasswordTries = 0;
            this.TargetOpenRemoteAdmin(this.connectionToClient, false);
            this._ccm.TargetConsolePrint(this.connectionToClient, "Your remote admin access has been granted (global permissions - staff).", "cyan");
          }
          else if (dictionary["Management"] == "YES" && ServerStatic.PermissionsHandler.ManagersAccess)
          {
            this.RemoteAdmin = true;
            this.Permissions = result1;
            this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
            this.GetComponent<QueryProcessor>().PasswordTries = 0;
            this.TargetOpenRemoteAdmin(this.connectionToClient, false);
            this._ccm.TargetConsolePrint(this.connectionToClient, "Your remote admin access has been granted (global permissions - management).", "cyan");
          }
          else if (dictionary["Global banning"] == "YES" && ServerStatic.PermissionsHandler.BanningTeamAccess)
          {
            this.RemoteAdmin = true;
            this.Permissions = result1;
            this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
            this.GetComponent<QueryProcessor>().PasswordTries = 0;
            this.TargetOpenRemoteAdmin(this.connectionToClient, false);
            this._ccm.TargetConsolePrint(this.connectionToClient, "Your remote admin access has been granted (global permissions - banning team).", "cyan");
          }
          if (dictionary["Badge text"] != "(none)" || dictionary["Badge color"] != "(none)")
          {
            if (this._neverCover || !this._badgeCover || (string.IsNullOrEmpty(this.MyText) || string.IsNullOrEmpty(this.MyColor)))
            {
              switch (dictionary["Badge type"])
              {
                case "3":
                  hide = true;
                  break;
                case "4":
                  if (ConfigFile.ServerConfig.GetBool("hide_banteam_badges_by_default", false))
                    goto case "3";
                  else
                    break;
                case "1":
                  if (ConfigFile.ServerConfig.GetBool("hide_staff_badges_by_default", false))
                    goto case "3";
                  else
                    break;
                case "2":
                  if (ConfigFile.ServerConfig.GetBool("hide_management_badges_by_default", false))
                    goto case "3";
                  else
                    break;
                case "0":
                  if (!ConfigFile.ServerConfig.GetBool("hide_patreon_badges_by_default", false) || ServerStatic.PermissionsHandler.IsVerified)
                    break;
                  goto case "3";
              }
              this.GlobalSet = true;
              int result2;
              if (int.TryParse(dictionary["Badge type"], out result2))
                this.GlobalBadgeType = result2;
              if (hide)
              {
                this.HiddenBadge = dictionary["Badge text"];
                this.RefreshHiddenTag();
                this._ccm.TargetConsolePrint(this.connectionToClient, "Your global badge has been granted, but it's hidden. Use \"gtag\" command in the game console to show your global badge.", "yellow");
              }
              else
              {
                this.HiddenBadge = (string) null;
                this.RpcResetFixed();
                this.NetworkGlobalBadge = this._globalBadgeUnconfirmed;
                this._ccm.TargetConsolePrint(this.connectionToClient, "Your global badge has been granted.", "cyan");
              }
            }
            else
              this._ccm.TargetConsolePrint(this.connectionToClient, "Your global badge is covered by server badge. Use \"gtag\" command in the game console to show your global badge.", "yellow");
          }
          this._badgeChallenge = (string) null;
          this._globalBadgeUnconfirmed = (string) null;
          this.GetComponent<QueryProcessor>().GameplayData = PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.GameplayData);
          if (!this.Staff)
            return;
          foreach (GameObject player in PlayerManager.players)
          {
            ServerRoles component = player.GetComponent<ServerRoles>();
            if (!string.IsNullOrEmpty(component.HiddenBadge))
              component.TargetSetHiddenRole(this.connectionToClient, component.HiddenBadge);
          }
          this._ccm.TargetConsolePrint(this.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
        }
      }
    }
  }

  public void CallCmdSetOverwatchStatus(byte status)
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    switch (status)
    {
      case 0:
        this.SetOverwatchStatus(false);
        return;
      case 1:
        if (this.OverwatchPermitted)
        {
          this.SetOverwatchStatus(true);
          return;
        }
        break;
      case 2:
        if (this.OverwatchEnabled)
        {
          this.SetOverwatchStatus(false);
          return;
        }
        if (this.OverwatchPermitted)
        {
          this.SetOverwatchStatus(true);
          return;
        }
        break;
    }
    this._ccm.TargetConsolePrint(this.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
  }

  public void CallCmdSetNoclipStatus(byte status)
  {
    if (!this._commandRateLimit.CanExecute(true))
      return;
    switch (status)
    {
      case 0:
        if (!this.NoclipReady)
          return;
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") disabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        this.NoclipReady = false;
        return;
      case 1:
        if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.Noclip))
        {
          this.NoclipReady = true;
          ServerLogs.AddLog(ServerLogs.Modules.Administrative, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") enabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
          return;
        }
        break;
      case 2:
        if (this.NoclipReady)
        {
          this.NoclipReady = false;
          ServerLogs.AddLog(ServerLogs.Modules.Administrative, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") disabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
          return;
        }
        if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.Noclip))
        {
          this.NoclipReady = true;
          ServerLogs.AddLog(ServerLogs.Modules.Administrative, this.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") enabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
          return;
        }
        break;
    }
    this._ccm.TargetConsolePrint(this.connectionToClient, "You don't have permissions to enable noclip mode!", "red");
  }

  protected static void InvokeRpcRpcResetFixed(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcResetFixed called on server.");
    else
      ((ServerRoles) obj).CallRpcResetFixed();
  }

  protected static void InvokeRpcTargetSetHiddenRole(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSetHiddenRole called on server.");
    else
      ((ServerRoles) obj).CallTargetSetHiddenRole(ClientScene.readyConnection, reader.ReadString());
  }

  protected static void InvokeRpcTargetSignServerChallenge(
    NetworkBehaviour obj,
    NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSignServerChallenge called on server.");
    else
      ((ServerRoles) obj).CallTargetSignServerChallenge(ClientScene.readyConnection, reader.ReadString());
  }

  protected static void InvokeRpcTargetOpenRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetOpenRemoteAdmin called on server.");
    else
      ((ServerRoles) obj).CallTargetOpenRemoteAdmin(ClientScene.readyConnection, reader.ReadBoolean());
  }

  protected static void InvokeRpcTargetCloseRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetCloseRemoteAdmin called on server.");
    else
      ((ServerRoles) obj).CallTargetCloseRemoteAdmin(ClientScene.readyConnection);
  }

  protected static void InvokeRpcTargetSetNoclipReady(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSetNoclipReady called on server.");
    else
      ((ServerRoles) obj).CallTargetSetNoclipReady(ClientScene.readyConnection, reader.ReadBoolean());
  }

  protected static void InvokeRpcTargetPublicKeyAccepted(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetPublicKeyAccepted called on server.");
    else
      ((ServerRoles) obj).CallTargetPublicKeyAccepted(ClientScene.readyConnection);
  }

  protected static void InvokeRpcTargetSetOverwatch(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetSetOverwatch called on server.");
    else
      ((ServerRoles) obj).CallTargetSetOverwatch(ClientScene.readyConnection, reader.ReadBoolean());
  }

  public void CallRpcResetFixed()
  {
    this._fixedBadge = (string) null;
  }

  public void CallTargetSetHiddenRole(NetworkConnection connection, string role)
  {
    if (this.isServer)
      return;
    if (string.IsNullOrEmpty(role))
    {
      this.GlobalSet = false;
      this.SetColor((string) null);
      this.SetText((string) null);
      this._fixedBadge = (string) null;
      this.SetText((string) null);
    }
    else
    {
      this.GlobalSet = true;
      this.SetColor("silver");
      this._fixedBadge = role.Replace("[", string.Empty).Replace("]", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty) + " (hidden)";
      this.SetText(this._fixedBadge);
    }
  }

  public void CallTargetSignServerChallenge(NetworkConnection target, string challenge)
  {
  }

  public void CallTargetOpenRemoteAdmin(NetworkConnection connection, bool password)
  {
  }

  public void CallTargetCloseRemoteAdmin(NetworkConnection connection)
  {
  }

  public void CallTargetSetNoclipReady(NetworkConnection connection, bool state)
  {
  }

  public void CallTargetPublicKeyAccepted(NetworkConnection conn)
  {
  }

  public void CallTargetSetOverwatch(NetworkConnection conn, bool s)
  {
  }

  static ServerRoles()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdRequestBadge", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdRequestBadge));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdSetPublicPart", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetPublicPart));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdDoNotTrack", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdDoNotTrack));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdSetLocalTagPreferences", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetLocalTagPreferences));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdServerSignatureComplete", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdServerSignatureComplete));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdSetOverwatchStatus", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetOverwatchStatus));
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerRoles), "CmdSetNoclipStatus", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetNoclipStatus));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "RpcResetFixed", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcRpcResetFixed));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetSetHiddenRole", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetHiddenRole));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetSignServerChallenge", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSignServerChallenge));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetOpenRemoteAdmin", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetOpenRemoteAdmin));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetCloseRemoteAdmin", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetCloseRemoteAdmin));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetSetNoclipReady", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetNoclipReady));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetPublicKeyAccepted", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetPublicKeyAccepted));
    NetworkBehaviour.RegisterRpcDelegate(typeof (ServerRoles), "TargetSetOverwatch", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetOverwatch));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WriteString(this.MyColor);
      writer.WriteString(this.MyText);
      writer.WriteString(this.PublicPlayerInfoToken);
      writer.WriteString(this.GlobalBadge);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WriteString(this.MyColor);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 2L) != 0L)
    {
      writer.WriteString(this.MyText);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 4L) != 0L)
    {
      writer.WriteString(this.PublicPlayerInfoToken);
      flag = true;
    }
    if (((long) this.syncVarDirtyBits & 8L) != 0L)
    {
      writer.WriteString(this.GlobalBadge);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      string i1 = reader.ReadString();
      this.SetColor(i1);
      this.NetworkMyColor = i1;
      string i2 = reader.ReadString();
      this.SetText(i2);
      this.NetworkMyText = i2;
      string i3 = reader.ReadString();
      this.SetPublicInfo(i3);
      this.NetworkPublicPlayerInfoToken = i3;
      this.NetworkGlobalBadge = reader.ReadString();
    }
    else
    {
      long num = (long) reader.ReadPackedUInt64();
      if ((num & 1L) != 0L)
      {
        string i = reader.ReadString();
        this.SetColor(i);
        this.NetworkMyColor = i;
      }
      if ((num & 2L) != 0L)
      {
        string i = reader.ReadString();
        this.SetText(i);
        this.NetworkMyText = i;
      }
      if ((num & 4L) != 0L)
      {
        string i = reader.ReadString();
        this.SetPublicInfo(i);
        this.NetworkPublicPlayerInfoToken = i;
      }
      if ((num & 8L) == 0L)
        return;
      this.NetworkGlobalBadge = reader.ReadString();
    }
  }

  [Serializable]
  public class NamedColor
  {
    public string Name;
    public Gradient SpeakingColorIn;
    public Gradient SpeakingColorOut;
    public string ColorHex;
    public bool Restricted;
  }

  [Serializable]
  public enum AccessMode : byte
  {
    LocalAccess = 1,
    GlobalAccess = 2,
    PasswordOverride = 3,
  }
}
