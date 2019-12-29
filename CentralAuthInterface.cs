// Decompiled with JetBrains decompiler
// Type: CentralAuthInterface
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using GameCore;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

internal class CentralAuthInterface : ICentralAuth
{
  private readonly CharacterClassManager _s;
  private readonly bool _is;

  public CharacterClassManager GetCcm()
  {
    return this._s;
  }

  public CentralAuthInterface(CharacterClassManager sync, bool server)
  {
    this._s = sync;
    this._is = server;
  }

  public void TokenGenerated(string token)
  {
    GameCore.Console.AddLog("Authentication token obtained from central server.", Color.green, false);
    this._s.CmdSendToken(token);
  }

  public void RequestBadge(string token)
  {
    this._s.SrvRoles.RequestBadge(token);
  }

  public void RequestPublicPart(string token)
  {
    this._s.SrvRoles.SetPublicPart(token);
  }

  public void Fail()
  {
    if (this._is)
    {
      ServerConsole.AddLog("Failed to validate authentication token.");
      ServerConsole.Disconnect(this._s.connectionToClient, "Failed to validate authentication token.");
    }
    else
    {
      GameCore.Console.AddLog("Failed to obtain authentication token from central server.", Color.red, false);
      this._s.connectionToServer.Disconnect();
      this._s.connectionToServer.Dispose();
    }
  }

  public void Ok(
    string userId,
    string userId2,
    string ban,
    string server,
    bool bypass,
    bool bypassWl,
    bool DNT,
    string serial,
    string vacSession,
    string rqIp,
    string Asn,
    bool BypassIpCheck)
  {
    if (!(ban == "NO"))
    {
      if (!(ban == "M1"))
      {
        if (!(ban == "M2"))
          ServerConsole.AddLog("Accepted authentication token of user " + userId + " signed by " + server + ". Active global ban present in the token.");
        else
          ServerConsole.AddLog("Accepted authentication token of user " + userId + " signed by " + server + ". Player is globally muted on intercom.");
      }
      else
        ServerConsole.AddLog("Accepted authentication token of user " + userId + " signed by " + server + ". Player is globally muted.");
    }
    else
      ServerConsole.AddLog("Accepted authentication token of user " + userId + " signed by " + server + ". No active global bans.");
    this._s.TargetConsolePrint(this._s.connectionToClient, "Accepted your authentication token (your user id is " + userId + ") " + (ban == "NO" ? "without any global bans present" : "with global ban status " + ban) + " signed by " + server + " server.", "green");
    this._s.AuthTokenSerial = serial;
    this._s.RequestIp = rqIp;
    this._s.VacSession = vacSession;
    this._s.Asn = Asn;
    ServerRoles component = this._s.GetComponent<ServerRoles>();
    if (DNT)
      component.SetDoNotTrack();
    string userId1 = userId.Contains("$") ? userId.Substring(0, userId.IndexOf("$", StringComparison.Ordinal)) : userId;
    string str1;
    using (WebClient webClient = new WebClient())
    {
      webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
      webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
      str1 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/check.php?id=" + userId1 + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
    }
    Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + userId1 + " Response from API: " + str1);
    if (str1 == "1")
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
        webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
        string str2 = webClient.DownloadString("http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=" + userId + "&reason=Expired&aname=Server&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
        Initializer.logger.Info("BANSYSTEM", "user: " + userId1 + " Ban expired attempting unban, Response from API: " + str2);
      }
    }
    string str3 = "0";
    bool flag1 = true;
    bool flag2 = false;
    try
    {
      using (WebClient webClient = new WebClient())
      {
        webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", ""), ConfigFile.ServerConfig.GetString("bansystem_apikey", ""));
        webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
        str3 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/reason_request.php?id=" + userId + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none"));
        Initializer.logger.Debug("BANSYSTEM", "Checking ban status of user: " + userId + " Response from API: " + str3);
        if (str3 == "0")
        {
          flag1 = false;
          flag2 = false;
          Initializer.logger.Debug("BANSYSTEM", "User is not banned");
        }
      }
    }
    catch (WebException ex)
    {
      flag1 = false;
    }
    if (flag1)
    {
      Initializer.logger.Debug("BANSYSTEM", "Data found for " + userId + "setting banned value to true");
      flag2 = true;
    }
    if (flag2)
    {
      Initializer.logger.Info("BANSYSTEM", "user: " + userId + " attempted connection with active ban disconnecting");
      ServerConsole.Disconnect(this._s.connectionToClient, " " + str3 + " You can fill in a ban appeal here: " + ConfigFile.ServerConfig.GetString("bansystem_banappealurl", "none"));
    }
    KeyValuePair<BanDetails, BanDetails> keyValuePair;
    if (!bypass || !ServerStatic.GetPermissionsHandler().IsVerified)
    {
      keyValuePair = BanHandler.QueryBan(userId1, (string) null);
      if (keyValuePair.Key != null)
        return;
    }
    if (userId2 != null && (!bypass || !ServerStatic.GetPermissionsHandler().IsVerified))
    {
      keyValuePair = BanHandler.QueryBan(userId2, (string) null);
      if (keyValuePair.Key != null)
        return;
    }
    if ((ConfigFile.ServerConfig.GetBool("use_global_bans", true) || ServerStatic.PermissionsHandler.IsVerified) && (ban != "NO" && ban != "M1") && ban != "M2")
    {
      this._s.TargetConsolePrint(this._s.connectionToClient, ban, "red");
      ServerConsole.AddLog("Player with ID " + userId + " kicked due to an active global ban: " + ban + ".");
      ServerConsole.Disconnect(this._s.connectionToClient, ban);
    }
    else
    {
      if (!BypassIpCheck && rqIp != "N/A" && ServerConsole.EnforceSameIp)
      {
        string address = this.GetCcm().connectionToClient.address;
        if (address.Contains(".") && rqIp.Contains(".") || address.Contains(":") && rqIp.Contains(":"))
        {
          bool flag3 = false;
          if (ServerConsole.SkipEnforcementForLocalAddresses)
          {
            flag3 = address == "127.0.0.1" || address.StartsWith("10.") || address.StartsWith("192.168.");
            if (!flag3 && address.StartsWith("172."))
            {
              string[] strArray = address.Split('.');
              byte result;
              if (strArray.Length == 4 && byte.TryParse(strArray[1], out result) && (result >= (byte) 16 && result <= (byte) 31))
                flag3 = true;
            }
          }
          if (!flag3 && address != rqIp)
          {
            this._s.TargetConsolePrint(this._s.connectionToClient, "Authentication token has been issued to a different IP address.", "red");
            this._s.TargetConsolePrint(this._s.connectionToClient, "Your IP address: " + address, "red");
            this._s.TargetConsolePrint(this._s.connectionToClient, "Issued to: " + rqIp, "red");
            ServerConsole.AddLog("Player kicked due to IP addresses mismatch.");
            ServerConsole.Disconnect(this._s.connectionToClient, "Authentication token has been issued to a different IP address.");
            return;
          }
        }
      }
      if (MuteHandler.QueryPersistantMute(userId1) || userId2 != null && MuteHandler.QueryPersistantMute(userId2))
      {
        this._s.NetworkMuted = true;
        this._s.NetworkIntercomMuted = true;
        this._s.TargetConsolePrint(this._s.connectionToClient, "You are muted on the voice chat by the server administrator.", "red");
      }
      else if ((ConfigFile.ServerConfig.GetBool("global_mutes_voicechat", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "M1")
      {
        this._s.NetworkMuted = true;
        this._s.NetworkIntercomMuted = true;
        this._s.TargetConsolePrint(this._s.connectionToClient, "You are globally muted on the voice chat.", "red");
      }
      else if (MuteHandler.QueryPersistantMute("ICOM-" + userId1) || userId2 != null && MuteHandler.QueryPersistantMute("ICOM-" + userId2))
      {
        this._s.NetworkIntercomMuted = true;
        this._s.TargetConsolePrint(this._s.connectionToClient, "You are muted on the intercom by the server administrator.", "red");
      }
      else if ((ConfigFile.ServerConfig.GetBool("global_mutes_intercom", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "M2")
      {
        this._s.NetworkIntercomMuted = true;
        this._s.TargetConsolePrint(this._s.connectionToClient, "You are globally muted on the intercom.", "red");
      }
      component.BypassStaff |= bypass;
      this._s.NetworkMuted = this._s.Muted && !component.BypassStaff;
      this._s.NetworkIntercomMuted = this._s.IntercomMuted && !component.BypassStaff;
      if (component.BypassStaff)
        component.GetComponent<CharacterClassManager>().TargetConsolePrint(this._s.connectionToClient, "You have the ban bypass flag, so you can't be banned from this server.", "cyan");
      component.StartServerChallenge(0);
    }
  }

  public void FailToken(string reason)
  {
    this._s.TargetConsolePrint(this._s.connectionToClient, "Your authentication token is invalid - " + reason, "red");
    ServerConsole.AddLog("Rejected invalid authentication token.");
    ServerConsole.Disconnect(this._s.connectionToClient, reason);
  }
}
