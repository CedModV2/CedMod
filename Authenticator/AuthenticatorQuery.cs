// Decompiled with JetBrains decompiler
// Type: Authenticator.AuthenticatorQuery
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Authenticator
{
  public static class AuthenticatorQuery
  {
    public static bool SendData(IEnumerable<string> data)
    {
      try
      {
        string response = HttpQuery.Post(CentralServer.MasterUrl + "v2/authenticator.php", HttpQuery.ToPostArgs(data));
        return response.StartsWith("{\"") ? AuthenticatorQuery.ProcessResponse(response) : AuthenticatorQuery.ProcessLegacyResponse(response);
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("Could not update server data on server list - (LOCAL EXCEPTION) " + ex.Message + "LOGTYPE-4");
        return false;
      }
    }

    private static void SendContactAddress()
    {
      try
      {
        List<string> stringList = new List<string>()
        {
          "ip=" + ServerConsole.Ip,
          "port=" + (object) ServerConsole.Port,
          "version=2",
          "address=" + Misc.Base64Encode(ConfigFile.ServerConfig.GetString("contact_email", ""))
        };
        if (!string.IsNullOrEmpty(ServerConsole.Password))
          stringList.Add("passcode=" + ServerConsole.Password);
        HttpQuery.Post(CentralServer.MasterUrl + "v2/contactaddress.php", HttpQuery.ToPostArgs((IEnumerable<string>) stringList));
      }
      catch
      {
      }
    }

    public static bool ProcessResponse(string response)
    {
      try
      {
        AuthenticatorResponse authenticatorResponse = JsonSerialize.FromJson<AuthenticatorResponse>(response);
        if (!authenticatorResponse.success)
        {
          ServerConsole.AddLog("Could not update server data on server list - " + authenticatorResponse.error + "LOGTYPE-4");
          return false;
        }
        if (!string.IsNullOrEmpty(authenticatorResponse.token))
        {
          ServerConsole.AddLog("Received verification token from central server.");
          AuthenticatorQuery.SaveNewToken(authenticatorResponse.token);
        }
        if (authenticatorResponse.actions != null && authenticatorResponse.actions.Length != 0)
        {
          foreach (string action in authenticatorResponse.actions)
            AuthenticatorQuery.HandleAction(action);
        }
        if (authenticatorResponse.messages != null && authenticatorResponse.messages.Length != 0)
        {
          foreach (string message in authenticatorResponse.messages)
            ServerConsole.AddLog("[MESSAGE FROM CENTRAL SERVER] " + message + " LOGTYPE-3");
        }
        if (authenticatorResponse.authAccepted != null && authenticatorResponse.authAccepted.Length != 0)
        {
          foreach (string str in authenticatorResponse.authAccepted)
            ServerConsole.AddLog("Authentication token of player " + str + " has been confirmed by central server.");
        }
        if (authenticatorResponse.authRejected != null && authenticatorResponse.authRejected.Length != 0)
        {
          foreach (GameObject player1 in PlayerManager.players)
          {
            GameObject player = player1;
            if (!((IEnumerable<AuthenticatiorAuthReject>) authenticatorResponse.authRejected).All<AuthenticatiorAuthReject>((Func<AuthenticatiorAuthReject, bool>) (rj => rj.Id != player.GetComponent<CharacterClassManager>().UserId)))
            {
              CharacterClassManager ccm = player.GetComponent<CharacterClassManager>();
              string reason = ((IEnumerable<AuthenticatiorAuthReject>) authenticatorResponse.authRejected).FirstOrDefault<AuthenticatiorAuthReject>((Func<AuthenticatiorAuthReject, bool>) (rj => rj.Id == ccm.UserId)).Reason;
              ServerConsole.AddLog("Authentication token of player " + ccm.UserId + " has been REJECTED by central server with reason: " + reason + ".");
              ccm.GetComponent<GameConsoleTransmission>().SendToClient(ccm.connectionToClient, "Auth token rejected by central server, with reason: " + reason + ".", "red");
              ServerConsole.Disconnect(ccm.connectionToClient, "Reason: Auth token rejected by central server, with reason: " + reason + ".");
            }
          }
        }
        return authenticatorResponse.verified;
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("Could not update server data on server list - (LOCAL EXCEPTION) " + ex.Message + "LOGTYPE-4");
        return false;
      }
    }

    public static bool ProcessLegacyResponse(string response)
    {
      if (response == "YES")
        return true;
      if (response.StartsWith("New code generated:"))
      {
        ServerStatic.PermissionsHandler.SetServerAsVerified();
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
        try
        {
          File.Delete(path);
        }
        catch
        {
          ServerConsole.AddLog("New password could not be saved.LOGTYPE-4");
        }
        try
        {
          StreamWriter streamWriter = new StreamWriter(path);
          string str = response.Remove(0, response.IndexOf(":", StringComparison.Ordinal)).Remove(response.IndexOf(":", StringComparison.Ordinal));
          while (str.Contains(":"))
            str = str.Replace(":", string.Empty);
          streamWriter.WriteLine(str);
          streamWriter.Close();
          ServerConsole.AddLog("New password saved.LOGTYPE-3");
          ServerConsole.Update = true;
        }
        catch
        {
          ServerConsole.AddLog("New password could not be saved.LOGTYPE-4");
        }
      }
      else if (response.Contains(":Restart:"))
        AuthenticatorQuery.HandleAction("Restart");
      else if (response.Contains(":RoundRestart:"))
        AuthenticatorQuery.HandleAction("RoundRestart");
      else if (response.Contains(":UpdateData:"))
        AuthenticatorQuery.HandleAction("UpdateData");
      else if (response.Contains(":RefreshKey:"))
        AuthenticatorQuery.HandleAction("RefreshKey");
      else if (response.Contains(":Message - "))
      {
        string str = response.Substring(response.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
        ServerConsole.AddLog("[MESSAGE FROM CENTRAL SERVER] " + str.Substring(0, str.IndexOf(":::", StringComparison.Ordinal)) + " LOGTYPE-3");
      }
      else if (response.Contains(":GetContactAddress:"))
      {
        AuthenticatorQuery.HandleAction("GetContactAddress");
      }
      else
      {
        if (response.Contains("Server is not verified."))
          return false;
        ServerConsole.AddLog("Could not update data on server list (legacy)- " + response + "LOGTYPE-4");
      }
      return true;
    }

    internal static void HandleAction(string action)
    {
      if (!(action == "Restart"))
      {
        if (!(action == "RoundRestart"))
        {
          if (!(action == "UpdateData"))
          {
            if (!(action == "RefreshKey"))
            {
              if (!(action == "GetContactAddress"))
                return;
              new Thread(new ThreadStart(AuthenticatorQuery.SendContactAddress))
              {
                Name = "SCP:SL Response to central servers (contact data request)",
                Priority = System.Threading.ThreadPriority.BelowNormal,
                IsBackground = true
              }.Start();
            }
            else
              ServerConsole.RunRefreshPublicKeyOnce();
          }
          else
            ServerConsole.Update = true;
        }
        else
        {
          ServerConsole.AddLog("Round restart requested by central server.LOGTYPE-3");
          PlayerStats component = PlayerManager.localPlayer.GetComponent<PlayerStats>();
          if (!component.isServer)
            return;
          component.Roundrestart();
        }
      }
      else
      {
        ServerConsole.AddLog("Server restart requested by central server.LOGTYPE-3");
        Application.Quit();
      }
    }

    private static void SaveNewToken(string token)
    {
      ServerStatic.PermissionsHandler.SetServerAsVerified();
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
      try
      {
        File.Delete(path);
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("New verification token could not be saved (1): " + ex.Message + "LOGTYPE-4");
      }
      try
      {
        StreamWriter streamWriter = new StreamWriter(path);
        streamWriter.WriteLine(token);
        streamWriter.Close();
        ServerConsole.AddLog("New verification token saved.LOGTYPE-3");
        ServerConsole.Update = true;
        ServerConsole.ScheduleTokenRefresh = true;
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("New verification token could not be saved (2): " + ex.Message + "LOGTYPE-4");
      }
    }
  }
}
