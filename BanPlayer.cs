// Decompiled with JetBrains decompiler
// Type: BanPlayer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using GameCore;
using Mirror;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class BanPlayer : NetworkBehaviour
{
  public bool KickUser(GameObject user, string reason, string issuer)
  {
    return this.BanUser(user, 0, reason, issuer);
  }

  public bool KickUser(GameObject user, string reason, string issuer, bool isGlobalBan)
  {
    return this.BanUser(user, 0, reason, issuer, isGlobalBan);
  }

  public bool BanUser(GameObject user, int duration, string reason, string issuer)
  {
    return this.BanUser(user, duration, reason, issuer, false);
  }

  public bool BanUser(
    GameObject user,
    int duration,
    string reason,
    string issuer,
    bool isGlobalBan)
  {
    string str1 = "0";
    if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
      duration = int.MaxValue;
    string str2 = (string) null;
    string address = user.GetComponent<NetworkIdentity>().connectionToClient.address;
    CharacterClassManager characterClassManager = (CharacterClassManager) null;
    try
    {
      if (ConfigFile.ServerConfig.GetBool("online_mode", false))
      {
        characterClassManager = user.GetComponent<CharacterClassManager>();
        str2 = characterClassManager.UserId;
      }
    }
    catch
    {
      Initializer.logger.Error("BANSYSTEM", "Failed during issue of User ID ban (1)!");
    }
    if (duration > 0 && (!ServerStatic.PermissionsHandler.IsVerified || !user.GetComponent<ServerRoles>().BypassStaff))
    {
      if (!string.IsNullOrEmpty(user.GetComponent<NicknameSync>().MyNick))
      {
        string myNick = user.GetComponent<NicknameSync>().MyNick;
      }
      TimeBehaviour.CurrentTimestamp();
      TimeBehaviour.GetBanExpieryTime((uint) duration);
      try
      {
        if (str2 != null)
        {
          if (!isGlobalBan)
          {
            if (duration >= 1)
            {
              try
              {
                using (WebClient webClient = new WebClient())
                {
                  webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                  webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                  str1 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/ban.php?id=" + str2 + "&reason=" + reason + "&aname=" + issuer + "&bd=" + (object) duration + ("&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")));
                  Initializer.logger.Info("BANSYSTEM", "User: " + str2 + " has been banned by: " + issuer + " for the reason: " + reason + " duration: " + (object) duration);
                  Initializer.logger.Debug("BANSYSTEM", "Response from ban API: " + str1);
                }
              }
              catch (WebException ex)
              {
                HttpWebResponse response = (HttpWebResponse) ex.Response;
                Initializer.logger.Error("BANSYSTEM", "An error occured: " + ex.Message + " " + (object) ex.Status);
              }
              if (!string.IsNullOrEmpty(characterClassManager.UserId2))
              {
                if (duration >= 1)
                {
                  try
                  {
                    using (WebClient webClient = new WebClient())
                    {
                      webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                      webClient.Headers.Add("user-agent", "Cedmod Client build: " + Initializer.GetCedModVersion());
                      str1 = webClient.DownloadString("http://83.82.126.185/scpserverbans/scpplugin/ban.php?id=" + str2 + "&reason=" + reason + "&aname=" + issuer + "&bd=" + (object) duration + ("&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none") + "&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none")));
                      Initializer.logger.Info("BANSYSTEM", "User: " + str2 + " has been banned by: " + issuer + " for the reason: " + reason + " duration: " + (object) duration);
                      Initializer.logger.Debug("BANSYSTEM", "Response from ban API: " + str1);
                    }
                  }
                  catch (WebException ex)
                  {
                    HttpWebResponse response = (HttpWebResponse) ex.Response;
                    Initializer.logger.Error("BANSYSTEM", "An error occured: " + ex.Message + " " + (object) ex.Status);
                  }
                }
              }
            }
          }
        }
      }
      catch
      {
        Initializer.logger.Error("BANSYSTEM", "Failed during issue of User ID ban (2)!");
        return false;
      }
      try
      {
        ConfigFile.ServerConfig.GetBool("ip_banning", false);
      }
      catch
      {
        Initializer.logger.Error("BANSYSTEM", "Failed during issue of IP ban!");
        return false;
      }
    }
    string message;
    if ((duration > 0 ? "banned" : "kicked").Contains("kicked"))
      message = "You have been kicked By: " + issuer + "\n" + " Ban Duration in minutes: " + (object) duration + "\n";
    else
      message = str1;
    if (!string.IsNullOrEmpty(reason) && duration <= 0)
      message = message + " Reason: " + reason;
    HashSet<GameObject> gameObjectSet = new HashSet<GameObject>();
    foreach (GameObject player in PlayerManager.players)
    {
      CharacterClassManager component = player.GetComponent<CharacterClassManager>();
      if (str2 != null && component.UserId == str2 || address != null && component.connectionToClient.address == address)
        gameObjectSet.Add(user);
    }
    foreach (GameObject player in gameObjectSet)
      ServerConsole.Disconnect(player, message);
    return true;
  }

  private void MirrorProcessed()
  {
  }
}
