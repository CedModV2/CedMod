using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cryptography;
using GameCore;
using MEC;
using Mirror;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using Security;
using UnityEngine;

// Token: 0x0200030A RID: 778
public class ServerRoles : NetworkBehaviour
{
    // Token: 0x17000262 RID: 610
    // (get) Token: 0x060012FF RID: 4863 RVA: 0x0001584D File Offset: 0x00013A4D
    internal byte KickPower
    {
        get
        {
            if (this.RaEverywhere)
            {
                return byte.MaxValue;
            }
            if (this.Group != null)
            {
                return this.Group.KickPower;
            }
            return 0;
        }
    }

    // Token: 0x17000263 RID: 611
    // (get) Token: 0x06001300 RID: 4864 RVA: 0x00015872 File Offset: 0x00013A72
    // (set) Token: 0x06001301 RID: 4865 RVA: 0x0006FB60 File Offset: 0x0006DD60
    internal bool NoclipReady
    {
        get
        {
            return this._noclipReady;
        }
        set
        {
            if (this._noclipReady == value)
            {
                return;
            }
            this._noclipReady = value;
            if (!NetworkServer.active)
            {
                return;
            }
            if (!this._noclipReady)
            {
                this._ccm.SetNoclip(false);
            }
            this.TargetSetNoclipReady(base.connectionToClient, this._noclipReady);
        }
    }

    // Token: 0x06001302 RID: 4866 RVA: 0x0001587A File Offset: 0x00013A7A
    public void Awake()
    {
        this.PlayerSkins = new Dictionary<string, int>();
    }

    // Token: 0x06001303 RID: 4867 RVA: 0x00015887 File Offset: 0x00013A87
    public void Start()
    {
        this._commandRateLimit = base.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
        this._ccm = base.GetComponent<CharacterClassManager>();
    }

    // Token: 0x06001304 RID: 4868 RVA: 0x0006FBAC File Offset: 0x0006DDAC
    [TargetRpc]
    public void TargetSetHiddenRole(NetworkConnection connection, string role)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(role);
        this.SendTargetRPCInternal(connection, typeof(ServerRoles), "TargetSetHiddenRole", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001305 RID: 4869 RVA: 0x0006FBEC File Offset: 0x0006DDEC
    [ClientRpc]
    public void RpcResetFixed()
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        this.SendRPCInternal(typeof(ServerRoles), "RpcResetFixed", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001306 RID: 4870 RVA: 0x0006FC20 File Offset: 0x0006DE20
    [Command]
    public void CmdRequestBadge(string token)
    {
        if (base.isServer)
        {
            this.CallCmdRequestBadge(token);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(token);
        base.SendCommandInternal(typeof(ServerRoles), "CmdRequestBadge", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001307 RID: 4871 RVA: 0x0006FC78 File Offset: 0x0006DE78
    [Command]
    public void CmdSetPublicPart(string token)
    {
        if (base.isServer)
        {
            this.CallCmdSetPublicPart(token);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(token);
        base.SendCommandInternal(typeof(ServerRoles), "CmdSetPublicPart", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001308 RID: 4872 RVA: 0x0006FCD0 File Offset: 0x0006DED0
    [Command]
    public void CmdDoNotTrack()
    {
        if (base.isServer)
        {
            this.CallCmdDoNotTrack();
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        base.SendCommandInternal(typeof(ServerRoles), "CmdDoNotTrack", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001309 RID: 4873 RVA: 0x0006FD18 File Offset: 0x0006DF18
    [Command]
    public void CmdSetLocalTagPreferences(bool hide, bool neverHide, bool neverCover)
    {
        if (base.isServer)
        {
            this.CallCmdSetLocalTagPreferences(hide, neverHide, neverCover);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(hide);
        writer.WriteBoolean(neverHide);
        writer.WriteBoolean(neverCover);
        base.SendCommandInternal(typeof(ServerRoles), "CmdSetLocalTagPreferences", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600130A RID: 4874 RVA: 0x0006FD8C File Offset: 0x0006DF8C
    [ServerCallback]
    public void SetDoNotTrack()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (this.DoNotTrack)
        {
            return;
        }
        this.DoNotTrack = true;
        if (!string.IsNullOrEmpty(base.GetComponent<NicknameSync>().MyNick))
        {
            this.LogDNT();
        }
        if (base.isLocalPlayer)
        {
            return;
        }
        base.GetComponent<GameConsoleTransmission>().SendToClient(base.connectionToClient, "Your \"Do not track\" request has been received.", "green");
    }

    // Token: 0x0600130B RID: 4875 RVA: 0x0006FDF0 File Offset: 0x0006DFF0
    [ServerCallback]
    public void LogDNT()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (this._ccm.UserId == null)
        {
            return;
        }
        ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Concat(new string[]
        {
            base.GetComponent<NicknameSync>().MyNick,
            " (",
            this._ccm.UserId,
            ") connected from IP ",
            base.connectionToClient.address,
            " sent Do Not Track signal."
        }), ServerLogs.ServerLogType.ConnectionUpdate);
        this._ccm.RefreshSyncedId();
    }

    // Token: 0x0600130C RID: 4876 RVA: 0x0006FE78 File Offset: 0x0006E078
    [ServerCallback]
    public void RefreshPermissions(bool disp = false)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        UserGroup userGroup = ServerStatic.PermissionsHandler.GetUserGroup(this._ccm.UserId);
        if (userGroup != null)
        {
            this.SetGroup(userGroup, false, false, disp);
        }
        else if (this._ccm.UserId2 != null)
        {
            userGroup = ServerStatic.PermissionsHandler.GetUserGroup(this._ccm.UserId2);
            if (userGroup != null)
            {
                this.SetGroup(userGroup, false, false, disp);
            }
        }
        base.GetComponent<QueryProcessor>().GameplayData = PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.GameplayData);
    }

    // Token: 0x0600130D RID: 4877 RVA: 0x0006FF04 File Offset: 0x0006E104
    [ServerCallback]
    public void SetGroup(UserGroup group, bool ovr, bool byAdmin = false, bool disp = false)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if (group == null)
        {
            if (this.RaEverywhere && this.Permissions == ServerStatic.PermissionsHandler.FullPerm)
            {
                return;
            }
            this.RemoteAdmin = false;
            this.Permissions = 0UL;
            this.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
            this.Group = null;
            this.SetColor(null);
            this.SetText(null);
            this._badgeCover = false;
            if (!string.IsNullOrEmpty(this.PrevBadge))
            {
                this.NetworkGlobalBadge = this.PrevBadge;
            }
            this.TargetCloseRemoteAdmin(base.connectionToClient);
            this._ccm.TargetConsolePrint(base.connectionToClient, "Your local permissions has been revoked by server administrator.", "red");
            return;
        }
        else
        {
            this._ccm.TargetConsolePrint(base.connectionToClient, (!byAdmin) ? "Updating your group on server (local permissions)..." : "Updating your group on server (set by server administrator)...", "cyan");
            this.Group = group;
            this._badgeCover = group.Cover;
            if (!this.OverwatchPermitted && PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
            {
                this.OverwatchPermitted = true;
            }
            if (group.Permissions > 0UL && this.Permissions != ServerStatic.PermissionsHandler.FullPerm && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions))
            {
                this.RemoteAdmin = true;
                this.Permissions = group.Permissions;
                this.RemoteAdminMode = (ovr ? ServerRoles.AccessMode.PasswordOverride : ServerRoles.AccessMode.LocalAccess);
                base.GetComponent<QueryProcessor>().PasswordTries = 0;
                this.TargetOpenRemoteAdmin(base.connectionToClient, ovr);
                this._ccm.TargetConsolePrint(base.connectionToClient, (!byAdmin) ? "Your remote admin access has been granted (local permissions)." : "Your remote admin access has been granted (set by server administrator).", "cyan");
                if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.ViewHiddenBadges))
                {
                    foreach (GameObject gameObject in PlayerManager.players)
                    {
                        ServerRoles component = gameObject.GetComponent<ServerRoles>();
                        if (!string.IsNullOrEmpty(component.HiddenBadge))
                        {
                            component.TargetSetHiddenRole(base.connectionToClient, component.HiddenBadge);
                        }
                    }
                    this._ccm.TargetConsolePrint(base.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
                }
            }
            else if (!this.RaEverywhere && this.Permissions != ServerStatic.PermissionsHandler.FullPerm)
            {
                this.RemoteAdmin = false;
                this.Permissions = 0UL;
                this.RemoteAdminMode = ServerRoles.AccessMode.LocalAccess;
                this.TargetCloseRemoteAdmin(base.connectionToClient);
            }
            ServerLogs.AddLog(ServerLogs.Modules.Permissions, string.Concat(new string[]
            {
                base.GetComponent<NicknameSync>().MyNick,
                " (",
                this._ccm.UserId,
                ") has been assigned to group ",
                group.BadgeText,
                "."
            }), ServerLogs.ServerLogType.ConnectionUpdate);
            if (group.BadgeColor == "none")
            {
                return;
            }
            if (this._hideLocalBadge || (group.HiddenByDefault && !disp && !this._neverHideLocalBadge))
            {
                this._badgeCover = false;
                if (!string.IsNullOrEmpty(this.MyText))
                {
                    return;
                }
                this.GlobalSet = false;
                this.NetworkMyText = null;
                this.NetworkMyColor = null;
                this.HiddenBadge = group.BadgeText;
                this.RefreshHiddenTag();
                this.TargetSetHiddenRole(base.connectionToClient, group.BadgeText);
                if (!byAdmin)
                {
                    this._ccm.TargetConsolePrint(base.connectionToClient, "Your role has been granted, but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "yellow");
                    return;
                }
                this._ccm.TargetConsolePrint(base.connectionToClient, "Your role has been granted to you (set by server administrator), but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "cyan");
                return;
            }
            else
            {
                this.GlobalSet = false;
                this.HiddenBadge = null;
                this.RpcResetFixed();
                this.NetworkMyText = group.BadgeText;
                this.NetworkMyColor = group.BadgeColor;
                if (!byAdmin)
                {
                    this._ccm.TargetConsolePrint(base.connectionToClient, string.Concat(new string[]
                    {
                        "Your role \"",
                        group.BadgeText,
                        "\" with color ",
                        group.BadgeColor,
                        " has been granted to you (local permissions)."
                    }), "cyan");
                    return;
                }
                this._ccm.TargetConsolePrint(base.connectionToClient, string.Concat(new string[]
                {
                    "Your role \"",
                    group.BadgeText,
                    "\" with color ",
                    group.BadgeColor,
                    " has been granted to you (set by server administrator)."
                }), "cyan");
                return;
            }
        }
    }

    // Token: 0x0600130E RID: 4878 RVA: 0x00070348 File Offset: 0x0006E548
    [ServerCallback]
    public void RefreshHiddenTag()
    {
        if (!NetworkServer.active)
        {
            return;
        }
        foreach (GameObject gameObject in PlayerManager.players)
        {
            ServerRoles component = gameObject.GetComponent<ServerRoles>();
            if (PermissionsHandler.IsPermitted(component.Permissions, PlayerPermissions.ViewHiddenBadges) || component.Staff)
            {
                this.TargetSetHiddenRole(component.connectionToClient, this.HiddenBadge);
            }
        }
    }

    // Token: 0x0600130F RID: 4879 RVA: 0x000158A8 File Offset: 0x00013AA8
    private IEnumerator<float> _RequestRoleFromServer(string token)
    {
        if (!this._ccm.IsVerified && string.IsNullOrEmpty(this._ccm.UserId))
        {
            yield break;
        }
        if (CentralAuth.ValidatePartialAuthToken(token, this._ccm.SaltedUserId, base.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Badge request") == null)
        {
            yield break;
        }
        this._globalBadgeUnconfirmed = token;
        this.StartServerChallenge(1);
        yield break;
    }

    // Token: 0x06001310 RID: 4880 RVA: 0x000158BE File Offset: 0x00013ABE
    private IEnumerator<float> SetPublicTokenOnServer(string token)
    {
        if (!this._ccm.IsVerified && string.IsNullOrEmpty(this._ccm.UserId))
        {
            yield break;
        }
        Dictionary<string, string> dictionary = CentralAuth.ValidatePartialAuthToken(token, this._ccm.SaltedUserId, base.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Public player info");
        if (dictionary == null)
        {
            yield break;
        }
        this._ccm.TargetConsolePrint(base.connectionToClient, "Your public player info has been accepted.", "cyan");
        this.NetworkPublicPlayerInfoToken = token;
        this.ProcessSkins(ref dictionary);
        yield break;
    }

    // Token: 0x06001311 RID: 4881 RVA: 0x000703D0 File Offset: 0x0006E5D0
    public string GetColoredRoleString(bool newLine = false)
    {
        if (string.IsNullOrEmpty(this.MyColor) || string.IsNullOrEmpty(this.MyText) || this.CurrentColor == null)
        {
            return string.Empty;
        }
        if ((this.CurrentColor.Restricted || this.MyText.Contains("[") || this.MyText.Contains("]") || this.MyText.Contains("<") || this.MyText.Contains(">")) && !this._authorizeBadge)
        {
            return string.Empty;
        }
        foreach (ServerRoles.NamedColor namedColor in this.NamedColors)
        {
            if (namedColor.Name == this.MyColor)
            {
                return string.Concat(new string[]
                {
                    newLine ? "\n" : string.Empty,
                    "<color=#",
                    namedColor.ColorHex,
                    ">",
                    this.MyText,
                    "</color>"
                });
            }
        }
        return string.Empty;
    }

    // Token: 0x06001312 RID: 4882 RVA: 0x000704E4 File Offset: 0x0006E6E4
    public string GetUncoloredRoleString()
    {
        if (string.IsNullOrEmpty(this.MyColor) || string.IsNullOrEmpty(this.MyText) || this.CurrentColor == null)
        {
            return string.Empty;
        }
        if ((this.CurrentColor.Restricted || this.MyText.Contains("[") || this.MyText.Contains("]") || this.MyText.Contains("<") || this.MyText.Contains(">")) && !this._authorizeBadge)
        {
            return string.Empty;
        }
        return this.MyText;
    }

    // Token: 0x06001313 RID: 4883 RVA: 0x00070584 File Offset: 0x0006E784
    private void Update()
    {
        if (this.CurrentColor == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(this._fixedBadge) && this.MyText != this._fixedBadge)
        {
            this.SetText(this._fixedBadge);
            this.SetColor("silver");
            return;
        }
        if (!string.IsNullOrEmpty(this._fixedBadge) && this.CurrentColor.Name != "silver")
        {
            this.SetColor("silver");
            return;
        }
        if (this.GlobalBadge != this._prevBadge)
        {
            this._prevBadge = this.GlobalBadge;
            if (string.IsNullOrEmpty(this.GlobalBadge))
            {
                this._bgc = null;
                this._bgt = null;
                this._authorizeBadge = false;
                this._prevColor += ".";
                this._prevText += ".";
                return;
            }
            GameCore.Console.AddDebugLog("SDAUTH", "Validating global badge of user " + base.GetComponent<NicknameSync>().MyNick, MessageImportance.LessImportant, false);
            Dictionary<string, string> dictionary = CentralAuth.ValidatePartialAuthToken(this.GlobalBadge, this._ccm.SaltedUserId, base.GetComponent<NicknameSync>().MyNick, null, "Badge request");
            if (dictionary == null)
            {
                GameCore.Console.AddDebugLog("SDAUTH", "<color=red>Validation of global badge of user " + base.GetComponent<NicknameSync>().MyNick + " failed - invalid digital signature.</color>", MessageImportance.Normal, false);
                this._bgc = null;
                this._bgt = null;
                this._authorizeBadge = false;
                this._prevColor += ".";
                this._prevText += ".";
                return;
            }
            GameCore.Console.AddDebugLog("SDAUTH", string.Concat(new string[]
            {
                "Validation of global badge of user ",
                base.GetComponent<NicknameSync>().MyNick,
                " complete - badge signed by central server ",
                dictionary["Issued by"],
                "."
            }), MessageImportance.LessImportant, false);
            if (dictionary["Badge text"] == "(none)" && dictionary["Badge color"] == "(none)")
            {
                this._bgc = null;
                this._bgt = null;
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
        {
            return;
        }
        if (this.CurrentColor.Restricted && (this.MyText != this._bgt || this.MyColor != this._bgc))
        {
            GameCore.Console.AddLog(string.Concat(new string[]
            {
                "TAG FAIL 1 - ",
                this.MyText,
                " - ",
                this._bgt,
                " /-/ ",
                this.MyColor,
                " - ",
                this._bgc
            }), Color.gray, false);
            this._authorizeBadge = false;
            this.NetworkMyColor = null;
            this.NetworkMyText = null;
            this._prevColor = null;
            this._prevText = null;
            PlayerList.UpdatePlayerRole(base.gameObject);
            return;
        }
        if (this.MyText != null && this.MyText != this._bgt && (this.MyText.Contains("[") || this.MyText.Contains("]") || this.MyText.Contains("<") || this.MyText.Contains(">")))
        {
            GameCore.Console.AddLog(string.Concat(new string[]
            {
                "TAG FAIL 2 - ",
                this.MyText,
                " - ",
                this._bgt,
                " /-/ ",
                this.MyColor,
                " - ",
                this._bgc
            }), Color.gray, false);
            this._authorizeBadge = false;
            this.NetworkMyColor = null;
            this.NetworkMyText = null;
            this._prevColor = null;
            this._prevText = null;
            PlayerList.UpdatePlayerRole(base.gameObject);
            return;
        }
        this._prevColor = this.MyColor;
        this._prevText = this.MyText;
        this._prevBadge = this.GlobalBadge;
        PlayerList.UpdatePlayerRole(base.gameObject);
    }

    // Token: 0x06001314 RID: 4884 RVA: 0x00070A10 File Offset: 0x0006EC10
    private void ProcessSkins(ref Dictionary<string, string> dict)
    {
        this.PlayerSkins.Clear();
        foreach (KeyValuePair<string, string> keyValuePair in dict)
        {
            int value;
            if (keyValuePair.Key.StartsWith("_") && int.TryParse(keyValuePair.Value, out value))
            {
                this.PlayerSkins.Add(keyValuePair.Key.Substring(1), value);
            }
        }
    }

    // Token: 0x06001315 RID: 4885 RVA: 0x00070AA0 File Offset: 0x0006ECA0
    public void SetColor(string i)
    {
        if (i == string.Empty || i == "default")
        {
            i = null;
        }
        this.NetworkMyColor = i;
        if (i == null)
        {
            return;
        }
        ServerRoles.NamedColor namedColor = this.NamedColors.FirstOrDefault((ServerRoles.NamedColor row) => row.Name == this.MyColor);
        if (namedColor == null && i != "default")
        {
            this.SetColor(null);
            return;
        }
        this.CurrentColor = namedColor;
    }

    // Token: 0x06001316 RID: 4886 RVA: 0x00070B0C File Offset: 0x0006ED0C
    public void SetText(string i)
    {
        if (i == string.Empty)
        {
            i = null;
        }
        this.NetworkMyText = i;
        ServerRoles.NamedColor namedColor = this.NamedColors.FirstOrDefault((ServerRoles.NamedColor row) => row.Name == this.MyColor);
        if (namedColor == null)
        {
            return;
        }
        this.CurrentColor = namedColor;
    }

    // Token: 0x06001317 RID: 4887 RVA: 0x000158D4 File Offset: 0x00013AD4
    public void SetPublicInfo(string i)
    {
        if (i == string.Empty)
        {
            this.NetworkPublicPlayerInfoToken = null;
        }
    }

    // Token: 0x06001318 RID: 4888 RVA: 0x00070B54 File Offset: 0x0006ED54
    [ServerCallback]
    public void StartServerChallenge(int selector)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        if ((selector == 0 && !string.IsNullOrEmpty(this._authChallenge)) || (selector == 1 && !string.IsNullOrEmpty(this._badgeChallenge)) || selector > 1 || selector < 0)
        {
            return;
        }
        byte[] array;
        using (RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider())
        {
            array = new byte[32];
            randomNumberGenerator.GetBytes(array);
        }
        string str = Convert.ToBase64String(array);
        if (selector == 0)
        {
            this._authChallenge = "auth-" + str;
            this.TargetSignServerChallenge(base.connectionToClient, this._authChallenge);
            return;
        }
        this._badgeChallenge = "badge-server-" + str;
        this.TargetSignServerChallenge(base.connectionToClient, this._badgeChallenge);
    }

    // Token: 0x06001319 RID: 4889 RVA: 0x00070C18 File Offset: 0x0006EE18
    [TargetRpc]
    public void TargetSignServerChallenge(NetworkConnection target, string challenge)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(challenge);
        this.SendTargetRPCInternal(target, typeof(ServerRoles), "TargetSignServerChallenge", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131A RID: 4890 RVA: 0x00070C58 File Offset: 0x0006EE58
    [Command]
    public void CmdServerSignatureComplete(string challenge, string response, string publickey, bool hide)
    {
        if (base.isServer)
        {
            this.CallCmdServerSignatureComplete(challenge, response, publickey, hide);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteString(challenge);
        writer.WriteString(response);
        writer.WriteString(publickey);
        writer.WriteBoolean(hide);
        base.SendCommandInternal(typeof(ServerRoles), "CmdServerSignatureComplete", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131B RID: 4891 RVA: 0x00070CD8 File Offset: 0x0006EED8
    [TargetRpc]
    internal void TargetOpenRemoteAdmin(NetworkConnection connection, bool password)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(password);
        this.SendTargetRPCInternal(connection, typeof(ServerRoles), "TargetOpenRemoteAdmin", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131C RID: 4892 RVA: 0x00070D18 File Offset: 0x0006EF18
    [TargetRpc]
    private void TargetCloseRemoteAdmin(NetworkConnection connection)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        this.SendTargetRPCInternal(connection, typeof(ServerRoles), "TargetCloseRemoteAdmin", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131D RID: 4893 RVA: 0x00070D50 File Offset: 0x0006EF50
    [TargetRpc]
    private void TargetSetNoclipReady(NetworkConnection connection, bool state)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(state);
        this.SendTargetRPCInternal(connection, typeof(ServerRoles), "TargetSetNoclipReady", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131E RID: 4894 RVA: 0x00070D90 File Offset: 0x0006EF90
    [Command]
    public void CmdSetOverwatchStatus(byte status)
    {
        if (base.isServer)
        {
            this.CallCmdSetOverwatchStatus(status);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteByte(status);
        base.SendCommandInternal(typeof(ServerRoles), "CmdSetOverwatchStatus", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x0600131F RID: 4895 RVA: 0x00070DE8 File Offset: 0x0006EFE8
    [Command]
    public void CmdSetNoclipStatus(byte status)
    {
        if (base.isServer)
        {
            this.CallCmdSetNoclipStatus(status);
            return;
        }
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteByte(status);
        base.SendCommandInternal(typeof(ServerRoles), "CmdSetNoclipStatus", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001320 RID: 4896 RVA: 0x00070E40 File Offset: 0x0006F040
    [ServerCallback]
    private void SetOverwatchStatus(bool status)
    {
        if (!NetworkServer.active)
        {
            return;
        }
        this.OverwatchEnabled = status;
        if (status && this._ccm.CurClass != RoleType.Spectator && this._ccm.Classes.CheckBounds(this._ccm.CurClass))
        {
            this._ccm.SetClassID(RoleType.Spectator);
        }
        this.TargetSetOverwatch(base.connectionToClient, this.OverwatchEnabled);
    }

    // Token: 0x06001321 RID: 4897 RVA: 0x000158EA File Offset: 0x00013AEA
    public void RequestBadge(string token)
    {
        this.CmdRequestBadge(token);
    }

    // Token: 0x06001322 RID: 4898 RVA: 0x000158F3 File Offset: 0x00013AF3
    public void SetPublicPart(string token)
    {
        this.CmdSetPublicPart(token);
    }

    // Token: 0x06001323 RID: 4899 RVA: 0x00070EAC File Offset: 0x0006F0AC
    [TargetRpc]
    public void TargetPublicKeyAccepted(NetworkConnection conn)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        this.SendTargetRPCInternal(conn, typeof(ServerRoles), "TargetPublicKeyAccepted", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001324 RID: 4900 RVA: 0x00070EE4 File Offset: 0x0006F0E4
    [TargetRpc]
    public void TargetSetOverwatch(NetworkConnection conn, bool s)
    {
        NetworkWriter writer = NetworkWriterPool.GetWriter();
        writer.WriteBoolean(s);
        this.SendTargetRPCInternal(conn, typeof(ServerRoles), "TargetSetOverwatch", writer, 0);
        NetworkWriterPool.Recycle(writer);
    }

    // Token: 0x06001328 RID: 4904 RVA: 0x00002FD1 File Offset: 0x000011D1
    private void MirrorProcessed()
    {
    }

    // Token: 0x17000264 RID: 612
    // (get) Token: 0x06001329 RID: 4905 RVA: 0x00070F24 File Offset: 0x0006F124
    // (set) Token: 0x0600132A RID: 4906 RVA: 0x00070F38 File Offset: 0x0006F138
    public string NetworkMyColor
    {
        get
        {
            return this.MyColor;
        }
        [param: In]
        set
        {
            if (NetworkServer.localClientActive && !base.getSyncVarHookGuard(1UL))
            {
                base.setSyncVarHookGuard(1UL, true);
                this.SetColor(value);
                base.setSyncVarHookGuard(1UL, false);
            }
            base.SetSyncVar<string>(value, ref this.MyColor, 1UL);
        }
    }

    // Token: 0x17000265 RID: 613
    // (get) Token: 0x0600132B RID: 4907 RVA: 0x00070FA4 File Offset: 0x0006F1A4
    // (set) Token: 0x0600132C RID: 4908 RVA: 0x00070FB8 File Offset: 0x0006F1B8
    public string NetworkMyText
    {
        get
        {
            return this.MyText;
        }
        [param: In]
        set
        {
            if (NetworkServer.localClientActive && !base.getSyncVarHookGuard(2UL))
            {
                base.setSyncVarHookGuard(2UL, true);
                this.SetText(value);
                base.setSyncVarHookGuard(2UL, false);
            }
            base.SetSyncVar<string>(value, ref this.MyText, 2UL);
        }
    }

    // Token: 0x17000266 RID: 614
    // (get) Token: 0x0600132D RID: 4909 RVA: 0x00071024 File Offset: 0x0006F224
    // (set) Token: 0x0600132E RID: 4910 RVA: 0x00071038 File Offset: 0x0006F238
    public string NetworkPublicPlayerInfoToken
    {
        get
        {
            return this.PublicPlayerInfoToken;
        }
        [param: In]
        set
        {
            if (NetworkServer.localClientActive && !base.getSyncVarHookGuard(4UL))
            {
                base.setSyncVarHookGuard(4UL, true);
                this.SetPublicInfo(value);
                base.setSyncVarHookGuard(4UL, false);
            }
            base.SetSyncVar<string>(value, ref this.PublicPlayerInfoToken, 4UL);
        }
    }

    // Token: 0x17000267 RID: 615
    // (get) Token: 0x0600132F RID: 4911 RVA: 0x000710A4 File Offset: 0x0006F2A4
    // (set) Token: 0x06001330 RID: 4912 RVA: 0x0001590F File Offset: 0x00013B0F
    public string NetworkGlobalBadge
    {
        get
        {
            return this.GlobalBadge;
        }
        [param: In]
        set
        {
            base.SetSyncVar<string>(value, ref this.GlobalBadge, 8UL);
        }
    }

    // Token: 0x06001331 RID: 4913 RVA: 0x00015927 File Offset: 0x00013B27
    protected static void InvokeCmdCmdRequestBadge(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdRequestBadge called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdRequestBadge(reader.ReadString());
    }

    // Token: 0x06001332 RID: 4914 RVA: 0x00015950 File Offset: 0x00013B50
    protected static void InvokeCmdCmdSetPublicPart(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdSetPublicPart called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdSetPublicPart(reader.ReadString());
    }

    // Token: 0x06001333 RID: 4915 RVA: 0x00015979 File Offset: 0x00013B79
    protected static void InvokeCmdCmdDoNotTrack(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdDoNotTrack called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdDoNotTrack();
    }

    // Token: 0x06001334 RID: 4916 RVA: 0x0001599C File Offset: 0x00013B9C
    protected static void InvokeCmdCmdSetLocalTagPreferences(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdSetLocalTagPreferences called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdSetLocalTagPreferences(reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean());
    }

    // Token: 0x06001335 RID: 4917 RVA: 0x000159D1 File Offset: 0x00013BD1
    protected static void InvokeCmdCmdServerSignatureComplete(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdServerSignatureComplete called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdServerSignatureComplete(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadBoolean());
    }

    // Token: 0x06001336 RID: 4918 RVA: 0x00015A0C File Offset: 0x00013C0C
    protected static void InvokeCmdCmdSetOverwatchStatus(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdSetOverwatchStatus called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdSetOverwatchStatus(reader.ReadByte());
    }

    // Token: 0x06001337 RID: 4919 RVA: 0x00015A35 File Offset: 0x00013C35
    protected static void InvokeCmdCmdSetNoclipStatus(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Command CmdSetNoclipStatus called on client.");
            return;
        }
        ((ServerRoles)obj).CallCmdSetNoclipStatus(reader.ReadByte());
    }

    // Token: 0x06001338 RID: 4920 RVA: 0x00015A5E File Offset: 0x00013C5E
    public void CallCmdRequestBadge(string token)
    {
        if (this._requested || token == null)
        {
            return;
        }
        this._requested = true;
        Timing.RunCoroutine(this._RequestRoleFromServer(token), Segment.FixedUpdate);
    }

    // Token: 0x06001339 RID: 4921 RVA: 0x00015A81 File Offset: 0x00013C81
    public void CallCmdSetPublicPart(string token)
    {
        if (this._publicPartRequested || token == null)
        {
            return;
        }
        this._publicPartRequested = true;
        Timing.RunCoroutine(this.SetPublicTokenOnServer(token), Segment.FixedUpdate);
    }

    // Token: 0x0600133A RID: 4922 RVA: 0x00015AA4 File Offset: 0x00013CA4
    public void CallCmdDoNotTrack()
    {
        if (this.DoNotTrack)
        {
            return;
        }
        this.SetDoNotTrack();
    }

    // Token: 0x0600133B RID: 4923 RVA: 0x00015AB5 File Offset: 0x00013CB5
    public void CallCmdSetLocalTagPreferences(bool hide, bool neverHide, bool neverCover)
    {
        if (this._prefSet)
        {
            return;
        }
        this._prefSet = true;
        this._hideLocalBadge = hide;
        this._neverHideLocalBadge = neverHide;
        this._neverCover = neverCover;
    }

    // Token: 0x0600133C RID: 4924 RVA: 0x000710B8 File Offset: 0x0006F2B8
    public void CallCmdServerSignatureComplete(string challenge, string response, string publickey, bool hide)
    {
        if (!this._commandRateLimit.CanExecute(true))
        {
            return;
        }
        if (this.FirstVerResult == null)
        {
            this.FirstVerResult = CentralAuth.ValidatePartialAuthToken(this._globalBadgeUnconfirmed, this._ccm.SaltedUserId, base.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Badge request");
        }
        if (this.FirstVerResult == null)
        {
            return;
        }
        if (this.FirstVerResult["Public key"] != Misc.Base64Encode(Sha.HashToString(Sha.Sha256(publickey))))
        {
            GameCore.Console.AddLog("Rejected signature of challenge " + challenge + " due to public key hash mismatch.", Color.red, false);
            this._ccm.TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to public key mismatch.", "red");
            return;
        }
        if (this.PublicKey == null)
        {
            this.PublicKey = ECDSA.PublicKeyFromString(publickey);
        }
        if (!ECDSA.Verify(challenge, response, this.PublicKey))
        {
            GameCore.Console.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.", Color.red, false);
            this._ccm.TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
            return;
        }
        if (challenge.StartsWith("auth-") && challenge == this._authChallenge)
        {
            this._ccm.UserId = this.FirstVerResult["User ID"];
            this._ccm.UserId2 = (this.FirstVerResult.ContainsKey("User ID 2") ? this.FirstVerResult["User ID 2"] : null);
            base.GetComponent<NicknameSync>().UpdateNickname(Misc.Base64Decode(this.FirstVerResult["Nickname"]));
            if (this.DoNotTrack)
            {
                this.LogDNT();
            }
            ServerConsole.NewPlayers.Add(this._ccm);
            this._ccm.TargetConsolePrint(base.connectionToClient, "Hi " + Misc.Base64Decode(this.FirstVerResult["Nickname"]) + "! Your challenge signature has been accepted.", "green");
            this.PublicKeyAccepted = true;
            this.TargetPublicKeyAccepted(base.connectionToClient);
            base.GetComponent<RemoteAdminCryptographicManager>().StartExchange();
            this.RefreshPermissions(false);
            this._authChallenge = null;
            return;
        }
        if (challenge.StartsWith("badge-server-") && challenge == this._badgeChallenge)
        {
            Dictionary<string, string> dictionary = CentralAuth.ValidatePartialAuthToken(this._globalBadgeUnconfirmed, this._ccm.SaltedUserId, base.GetComponent<NicknameSync>().MyNick, this._ccm.AuthTokenSerial, "Badge request");
            if (dictionary == null)
            {
                ServerConsole.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.");
                this._ccm.TargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
                return;
            }
            this.PrevBadge = this._globalBadgeUnconfirmed;
            if (dictionary["Staff"] == "YES")
            {
                this.Staff = true;
            }
            if (dictionary["Management"] == "YES" || dictionary["Global banning"] == "YES")
            {
                this.RaEverywhere = true;
            }
            if (dictionary["Overwatch mode"] == "YES")
            {
                this.OverwatchPermitted = true;
            }
            ulong permissions = ServerStatic.PermissionsHandler.FullPerm;
            if (dictionary.ContainsKey("RA Permissions"))
            {
                permissions = 0UL;
                ulong.TryParse(dictionary["RA Permissions"], out permissions);
            }
            if (dictionary["Remote admin"] == "YES" && ServerStatic.PermissionsHandler.StaffAccess)
            {
                this.RemoteAdmin = true;
                this.Permissions = permissions;
                this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
                base.GetComponent<QueryProcessor>().PasswordTries = 0;
                this.TargetOpenRemoteAdmin(base.connectionToClient, false);
                this._ccm.TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - staff).", "cyan");
            }
            else if (dictionary["Management"] == "YES" && ServerStatic.PermissionsHandler.ManagersAccess)
            {
                this.RemoteAdmin = true;
                this.Permissions = permissions;
                this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
                base.GetComponent<QueryProcessor>().PasswordTries = 0;
                this.TargetOpenRemoteAdmin(base.connectionToClient, false);
                this._ccm.TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - management).", "cyan");
            }
            else if (dictionary["Global banning"] == "YES" && ServerStatic.PermissionsHandler.BanningTeamAccess)
            {
                this.RemoteAdmin = true;
                this.Permissions = permissions;
                this.RemoteAdminMode = ServerRoles.AccessMode.GlobalAccess;
                base.GetComponent<QueryProcessor>().PasswordTries = 0;
                this.TargetOpenRemoteAdmin(base.connectionToClient, false);
                this._ccm.TargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - banning team).", "cyan");
            }
            if (dictionary["Badge text"] != "(none)" || dictionary["Badge color"] != "(none)")
            {
                if (this._neverCover || !this._badgeCover || string.IsNullOrEmpty(this.MyText) || string.IsNullOrEmpty(this.MyColor))
                {
                    string text = dictionary["Badge type"];
                    if (text != null)
                    {
                        if (!(text == "3"))
                        {
                            if (!(text == "4"))
                            {
                                if (!(text == "1"))
                                {
                                    if (!(text == "2"))
                                    {
                                        if (!(text == "0"))
                                        {
                                            goto IL_5B0;
                                        }
                                        if (!ConfigFile.ServerConfig.GetBool("hide_patreon_badges_by_default", false) || ServerStatic.PermissionsHandler.IsVerified)
                                        {
                                            goto IL_5B0;
                                        }
                                    }
                                    else if (!ConfigFile.ServerConfig.GetBool("hide_management_badges_by_default", false))
                                    {
                                        goto IL_5B0;
                                    }
                                }
                                else if (!ConfigFile.ServerConfig.GetBool("hide_staff_badges_by_default", false))
                                {
                                    goto IL_5B0;
                                }
                            }
                            else if (!ConfigFile.ServerConfig.GetBool("hide_banteam_badges_by_default", false))
                            {
                                goto IL_5B0;
                            }
                        }
                        hide = true;
                    }
                IL_5B0:
                    this.GlobalSet = true;
                    int globalBadgeType;
                    if (int.TryParse(dictionary["Badge type"], out globalBadgeType))
                    {
                        this.GlobalBadgeType = globalBadgeType;
                    }
                    if (hide)
                    {
                        this.HiddenBadge = dictionary["Badge text"];
                        this.RefreshHiddenTag();
                        this._ccm.TargetConsolePrint(base.connectionToClient, "Your global badge has been granted, but it's hidden. Use \"gtag\" command in the game console to show your global badge.", "yellow");
                    }
                    else
                    {
                        this.HiddenBadge = null;
                        this.RpcResetFixed();
                        this.NetworkGlobalBadge = this._globalBadgeUnconfirmed;
                        this._ccm.TargetConsolePrint(base.connectionToClient, "Your global badge has been granted.", "cyan");
                    }
                }
                else
                {
                    this._ccm.TargetConsolePrint(base.connectionToClient, "Your global badge is covered by server badge. Use \"gtag\" command in the game console to show your global badge.", "yellow");
                }
            }
            this._badgeChallenge = null;
            this._globalBadgeUnconfirmed = null;
            base.GetComponent<QueryProcessor>().GameplayData = PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.GameplayData);
            if (!this.Staff)
            {
                return;
            }
            foreach (GameObject gameObject in PlayerManager.players)
            {
                ServerRoles component = gameObject.GetComponent<ServerRoles>();
                if (!string.IsNullOrEmpty(component.HiddenBadge))
                {
                    component.TargetSetHiddenRole(base.connectionToClient, component.HiddenBadge);
                }
            }
            this._ccm.TargetConsolePrint(base.connectionToClient, "Hidden badges have been displayed for you (if there are any).", "gray");
        }
    }

    // Token: 0x0600133D RID: 4925 RVA: 0x000717D8 File Offset: 0x0006F9D8
    public void CallCmdSetOverwatchStatus(byte status)
    {
        if (!this._commandRateLimit.CanExecute(true))
        {
            return;
        }
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
        this._ccm.TargetConsolePrint(base.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
    }

    // Token: 0x0600133E RID: 4926 RVA: 0x00071860 File Offset: 0x0006FA60
    public void CallCmdSetNoclipStatus(byte status)
    {
        if (!this._commandRateLimit.CanExecute(true))
        {
            return;
        }
        switch (status)
        {
            case 0:
                if (this.NoclipReady)
                {
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, base.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") disabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    this.NoclipReady = false;
                    return;
                }
                return;
            case 1:
                if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.Noclip))
                {
                    this.NoclipReady = true;
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, base.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") enabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    return;
                }
                break;
            case 2:
                if (this.NoclipReady)
                {
                    this.NoclipReady = false;
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, base.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") disabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    return;
                }
                if (PermissionsHandler.IsPermitted(this.Permissions, PlayerPermissions.Noclip))
                {
                    this.NoclipReady = true;
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, base.GetComponent<NicknameSync>().MyNick + " (" + this._ccm.UserId + ") enabled noclip for self.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    return;
                }
                break;
        }
        this._ccm.TargetConsolePrint(base.connectionToClient, "You don't have permissions to enable noclip mode!", "red");
    }

    // Token: 0x0600133F RID: 4927 RVA: 0x00015ADC File Offset: 0x00013CDC
    protected static void InvokeRpcRpcResetFixed(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("RPC RpcResetFixed called on server.");
            return;
        }
        ((ServerRoles)obj).CallRpcResetFixed();
    }

    // Token: 0x06001340 RID: 4928 RVA: 0x00015AFF File Offset: 0x00013CFF
    protected static void InvokeRpcTargetSetHiddenRole(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetSetHiddenRole called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetSetHiddenRole(ClientScene.readyConnection, reader.ReadString());
    }

    // Token: 0x06001341 RID: 4929 RVA: 0x00015B2D File Offset: 0x00013D2D
    protected static void InvokeRpcTargetSignServerChallenge(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetSignServerChallenge called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetSignServerChallenge(ClientScene.readyConnection, reader.ReadString());
    }

    // Token: 0x06001342 RID: 4930 RVA: 0x00015B5B File Offset: 0x00013D5B
    protected static void InvokeRpcTargetOpenRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetOpenRemoteAdmin called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetOpenRemoteAdmin(ClientScene.readyConnection, reader.ReadBoolean());
    }

    // Token: 0x06001343 RID: 4931 RVA: 0x00015B89 File Offset: 0x00013D89
    protected static void InvokeRpcTargetCloseRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetCloseRemoteAdmin called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetCloseRemoteAdmin(ClientScene.readyConnection);
    }

    // Token: 0x06001344 RID: 4932 RVA: 0x00015BB1 File Offset: 0x00013DB1
    protected static void InvokeRpcTargetSetNoclipReady(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetSetNoclipReady called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetSetNoclipReady(ClientScene.readyConnection, reader.ReadBoolean());
    }

    // Token: 0x06001345 RID: 4933 RVA: 0x00015BDF File Offset: 0x00013DDF
    protected static void InvokeRpcTargetPublicKeyAccepted(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetPublicKeyAccepted called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetPublicKeyAccepted(ClientScene.readyConnection);
    }

    // Token: 0x06001346 RID: 4934 RVA: 0x00015C07 File Offset: 0x00013E07
    protected static void InvokeRpcTargetSetOverwatch(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("TargetRPC TargetSetOverwatch called on server.");
            return;
        }
        ((ServerRoles)obj).CallTargetSetOverwatch(ClientScene.readyConnection, reader.ReadBoolean());
    }

    // Token: 0x06001347 RID: 4935 RVA: 0x00015C35 File Offset: 0x00013E35
    public void CallRpcResetFixed()
    {
        this._fixedBadge = null;
    }

    // Token: 0x06001348 RID: 4936 RVA: 0x000719BC File Offset: 0x0006FBBC
    public void CallTargetSetHiddenRole(NetworkConnection connection, string role)
    {
        if (base.isServer)
        {
            return;
        }
        if (string.IsNullOrEmpty(role))
        {
            this.GlobalSet = false;
            this.SetColor(null);
            this.SetText(null);
            this._fixedBadge = null;
            this.SetText(null);
            return;
        }
        this.GlobalSet = true;
        this.SetColor("silver");
        this._fixedBadge = role.Replace("[", string.Empty).Replace("]", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty) + " (hidden)";
        this.SetText(this._fixedBadge);
    }

    // Token: 0x06001349 RID: 4937 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetSignServerChallenge(NetworkConnection target, string challenge)
    {
    }

    // Token: 0x0600134A RID: 4938 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetOpenRemoteAdmin(NetworkConnection connection, bool password)
    {
    }

    // Token: 0x0600134B RID: 4939 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetCloseRemoteAdmin(NetworkConnection connection)
    {
    }

    // Token: 0x0600134C RID: 4940 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetSetNoclipReady(NetworkConnection connection, bool state)
    {
    }

    // Token: 0x0600134D RID: 4941 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetPublicKeyAccepted(NetworkConnection conn)
    {
    }

    // Token: 0x0600134E RID: 4942 RVA: 0x00002FD1 File Offset: 0x000011D1
    public void CallTargetSetOverwatch(NetworkConnection conn, bool s)
    {
    }

    // Token: 0x0600134F RID: 4943 RVA: 0x00071A6C File Offset: 0x0006FC6C
    static ServerRoles()
    {
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdRequestBadge", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdRequestBadge));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdSetPublicPart", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetPublicPart));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdDoNotTrack", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdDoNotTrack));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdSetLocalTagPreferences", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetLocalTagPreferences));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdServerSignatureComplete", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdServerSignatureComplete));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdSetOverwatchStatus", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetOverwatchStatus));
        NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), "CmdSetNoclipStatus", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeCmdCmdSetNoclipStatus));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "RpcResetFixed", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcRpcResetFixed));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetSetHiddenRole", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetHiddenRole));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetSignServerChallenge", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSignServerChallenge));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetOpenRemoteAdmin", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetOpenRemoteAdmin));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetCloseRemoteAdmin", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetCloseRemoteAdmin));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetSetNoclipReady", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetNoclipReady));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetPublicKeyAccepted", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetPublicKeyAccepted));
        NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), "TargetSetOverwatch", new NetworkBehaviour.CmdDelegate(ServerRoles.InvokeRpcTargetSetOverwatch));
    }

    // Token: 0x06001350 RID: 4944 RVA: 0x00071C5C File Offset: 0x0006FE5C
    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
        bool result = base.OnSerialize(writer, forceAll);
        if (forceAll)
        {
            writer.WriteString(this.MyColor);
            writer.WriteString(this.MyText);
            writer.WriteString(this.PublicPlayerInfoToken);
            writer.WriteString(this.GlobalBadge);
            return true;
        }
        writer.WritePackedUInt64(base.syncVarDirtyBits);
        if ((base.syncVarDirtyBits & 1UL) != 0UL)
        {
            writer.WriteString(this.MyColor);
            result = true;
        }
        if ((base.syncVarDirtyBits & 2UL) != 0UL)
        {
            writer.WriteString(this.MyText);
            result = true;
        }
        if ((base.syncVarDirtyBits & 4UL) != 0UL)
        {
            writer.WriteString(this.PublicPlayerInfoToken);
            result = true;
        }
        if ((base.syncVarDirtyBits & 8UL) != 0UL)
        {
            writer.WriteString(this.GlobalBadge);
            result = true;
        }
        return result;
    }

    // Token: 0x06001351 RID: 4945 RVA: 0x00071D48 File Offset: 0x0006FF48
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
        if (initialState)
        {
            string text = reader.ReadString();
            this.SetColor(text);
            this.NetworkMyColor = text;
            string text2 = reader.ReadString();
            this.SetText(text2);
            this.NetworkMyText = text2;
            string text3 = reader.ReadString();
            this.SetPublicInfo(text3);
            this.NetworkPublicPlayerInfoToken = text3;
            string networkGlobalBadge = reader.ReadString();
            this.NetworkGlobalBadge = networkGlobalBadge;
            return;
        }
        long num = (long)reader.ReadPackedUInt64();
        if ((num & 1L) != 0L)
        {
            string text4 = reader.ReadString();
            this.SetColor(text4);
            this.NetworkMyColor = text4;
        }
        if ((num & 2L) != 0L)
        {
            string text5 = reader.ReadString();
            this.SetText(text5);
            this.NetworkMyText = text5;
        }
        if ((num & 4L) != 0L)
        {
            string text6 = reader.ReadString();
            this.SetPublicInfo(text6);
            this.NetworkPublicPlayerInfoToken = text6;
        }
        if ((num & 8L) != 0L)
        {
            string networkGlobalBadge2 = reader.ReadString();
            this.NetworkGlobalBadge = networkGlobalBadge2;
        }
    }

    // Token: 0x04001440 RID: 5184
    public ServerRoles.NamedColor CurrentColor;

    // Token: 0x04001441 RID: 5185
    public ServerRoles.NamedColor[] NamedColors;

    // Token: 0x04001442 RID: 5186
    [NonSerialized]
    internal bool PublicKeyAccepted;

    // Token: 0x04001443 RID: 5187
    [NonSerialized]
    public Dictionary<string, string> FirstVerResult;

    // Token: 0x04001444 RID: 5188
    internal AsymmetricKeyParameter PublicKey;

    // Token: 0x04001445 RID: 5189
    [NonSerialized]
    public bool BypassMode;

    // Token: 0x04001446 RID: 5190
    [NonSerialized]
    public bool LocalRemoteAdmin;

    // Token: 0x04001447 RID: 5191
    private bool _authorizeBadge;

    // Token: 0x04001448 RID: 5192
    internal bool OverwatchPermitted;

    // Token: 0x04001449 RID: 5193
    internal bool OverwatchEnabled;

    // Token: 0x0400144A RID: 5194
    internal bool AmIInOverwatch;

    // Token: 0x0400144B RID: 5195
    internal string PrevBadge;

    // Token: 0x0400144C RID: 5196
    internal UserGroup Group;

    // Token: 0x0400144D RID: 5197
    private CharacterClassManager _ccm;

    // Token: 0x0400144E RID: 5198
    private string _globalBadgeUnconfirmed;

    // Token: 0x0400144F RID: 5199
    private string _prevColor;

    // Token: 0x04001450 RID: 5200
    private string _prevText;

    // Token: 0x04001451 RID: 5201
    private string _prevBadge;

    // Token: 0x04001452 RID: 5202
    private string _badgeUserChallenge;

    // Token: 0x04001453 RID: 5203
    private string _authChallenge;

    // Token: 0x04001454 RID: 5204
    private string _badgeChallenge;

    // Token: 0x04001455 RID: 5205
    private string _bgc;

    // Token: 0x04001456 RID: 5206
    private string _bgt;

    // Token: 0x04001457 RID: 5207
    private string _fixedBadge;

    // Token: 0x04001458 RID: 5208
    private bool _hideLocalBadge;

    // Token: 0x04001459 RID: 5209
    private bool _neverHideLocalBadge;

    // Token: 0x0400145A RID: 5210
    private bool _neverCover;

    // Token: 0x0400145B RID: 5211
    private bool _prefSet;

    // Token: 0x0400145C RID: 5212
    private bool _badgeCover;

    // Token: 0x0400145D RID: 5213
    private bool _requested;

    // Token: 0x0400145E RID: 5214
    private bool _publicPartRequested;

    // Token: 0x0400145F RID: 5215
    private bool _badgeRequested;

    // Token: 0x04001460 RID: 5216
    private bool _authRequested;

    // Token: 0x04001461 RID: 5217
    private bool _noclipReady;

    // Token: 0x04001462 RID: 5218
    private bool _publicInfoDirty;

    // Token: 0x04001463 RID: 5219
    [SyncVar(hook = "SetColor")]
    public string MyColor;

    // Token: 0x04001464 RID: 5220
    [SyncVar(hook = "SetText")]
    public string MyText;

    // Token: 0x04001465 RID: 5221
    [SyncVar(hook = "SetPublicInfo")]
    public string PublicPlayerInfoToken;

    // Token: 0x04001466 RID: 5222
    [SyncVar]
    public string GlobalBadge;

    // Token: 0x04001467 RID: 5223
    [NonSerialized]
    public bool GlobalSet;

    // Token: 0x04001468 RID: 5224
    [NonSerialized]
    public int GlobalBadgeType;

    // Token: 0x04001469 RID: 5225
    [NonSerialized]
    public bool RemoteAdmin;

    // Token: 0x0400146A RID: 5226
    [NonSerialized]
    public bool Staff;

    // Token: 0x0400146B RID: 5227
    [NonSerialized]
    public bool BypassStaff;

    // Token: 0x0400146C RID: 5228
    [NonSerialized]
    public bool RaEverywhere;

    // Token: 0x0400146D RID: 5229
    [NonSerialized]
    public ulong Permissions;

    // Token: 0x0400146E RID: 5230
    [NonSerialized]
    public string HiddenBadge;

    // Token: 0x0400146F RID: 5231
    [NonSerialized]
    public bool DoNotTrack;

    // Token: 0x04001470 RID: 5232
    [NonSerialized]
    public ServerRoles.AccessMode RemoteAdminMode;

    // Token: 0x04001471 RID: 5233
    [NonSerialized]
    internal Dictionary<string, int> PlayerSkins;

    // Token: 0x04001472 RID: 5234
    private RateLimit _commandRateLimit;

    // Token: 0x0200030B RID: 779
    [Serializable]
    public class NamedColor
    {
        // Token: 0x04001473 RID: 5235
        public string Name;

        // Token: 0x04001474 RID: 5236
        public Gradient SpeakingColorIn;

        // Token: 0x04001475 RID: 5237
        public Gradient SpeakingColorOut;

        // Token: 0x04001476 RID: 5238
        public string ColorHex;

        // Token: 0x04001477 RID: 5239
        public bool Restricted;
    }

    // Token: 0x0200030C RID: 780
    [Serializable]
    public enum AccessMode : byte
    {
        // Token: 0x04001479 RID: 5241
        LocalAccess = 1,
        // Token: 0x0400147A RID: 5242
        GlobalAccess,
        // Token: 0x0400147B RID: 5243
        PasswordOverride
    }
}
