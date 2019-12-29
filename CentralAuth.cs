// Decompiled with JetBrains decompiler
// Type: CentralAuth
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using MEC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using UnityEngine;

public class CentralAuth : MonoBehaviour
{
  public static bool GlobalBadgeIssued;
  private ICentralAuth _ica;
  private static bool _awaitingTicket;
  private static bool _awaitingRequest;
  public static CentralAuth singleton;

  private void Awake()
  {
    CentralAuth.singleton = this;
  }

  private static void AuthDebug(string msg, string color = "blue")
  {
    GameCore.Console.AddDebugLog("SDAUTH", "<color=" + color + ">" + msg + "</color>", MessageImportance.Normal, false);
  }

  private void Update()
  {
  }

  internal void GenerateToken(ICentralAuth icaa)
  {
  }

  public void StartValidateToken(ICentralAuth icaa, string token, IPEndPoint endpoint)
  {
    Timing.RunCoroutine(this._ValidateToken(icaa, token, endpoint), Segment.FixedUpdate);
  }

  private IEnumerator<float> _ValidateToken(
    ICentralAuth icaa,
    string token,
    IPEndPoint endpoint)
  {
    if (string.IsNullOrEmpty(token) || !token.Contains("<br>Signature: "))
      icaa.FailToken("Malformed token.");
    try
    {
      string data = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
      string signature = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15).Replace("<br>", "");
      if (!ECDSA.Verify(data, signature, ServerConsole.PublicKey))
      {
        ServerConsole.AddLog("Authentication token signature mismatch.");
        icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to signature mismatch.", "red");
        icaa.FailToken("Failed to validate authentication token signature.");
      }
      else
      {
        Dictionary<string, string> dictionary = ((IEnumerable<string>) data.Split(new string[1]
        {
          "<br>"
        }, StringSplitOptions.None)).Select<string, string[]>((Func<string, string[]>) (rwr => rwr.Split(new string[1]
        {
          ": "
        }, StringSplitOptions.None))).ToDictionary<string[], string, string>((Func<string[], string>) (split => split[0]), (Func<string[], string>) (split => split[1]));
        if (dictionary["Usage"] != "Authentication")
        {
          ServerConsole.AddLog("Player tried to use token not issued to authentication purposes.");
          icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to invalid purpose of signature.", "red");
          this._ica.FailToken("Token supplied by your game can't be used for authentication purposes.");
        }
        else if (endpoint != null && (!CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(endpoint) || CustomLiteNetLib4MirrorTransport.UserIds[endpoint].UserId != CentralAuth.RemoveSalt(dictionary["User ID"])))
        {
          ServerConsole.AddLog("Player tried to use token issued to a different user than the preauthentication token.");
          icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "UserID mismatch between authentication and preauthentication token.", "red");
          this._ica.FailToken("UserID mismatch between authentication and preauthentication token.");
        }
        else if (dictionary["Test signature"] != "NO" && !CentralServer.TestServer)
        {
          ServerConsole.AddLog("Player tried to use authentication token issued only for testing. Server: " + dictionary["Issued by"] + ".");
          icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to testing signature.", "red");
          this._ica.FailToken("Your authentication token is issued only for testing purposes.");
        }
        else if (!dictionary.ContainsKey("Auth Version") || dictionary["Auth Version"] != "2")
        {
          ServerConsole.AddLog("Player used invalid version of authentication token. Server: " + dictionary["Issued by"] + ".");
          icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token version mismatch. Required version: 2.", "red");
          this._ica.FailToken("This version of game requires authentication token version 2.");
        }
        else
        {
          DateTime exact1 = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture);
          DateTime exact2 = DateTime.ParseExact(dictionary["Issuance time"], "yyyy-MM-dd HH:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture);
          DateTime utcNow = DateTime.UtcNow;
          if (exact1 < utcNow)
          {
            ServerConsole.AddLog("Player tried to use expired authentication token. Server: " + dictionary["Issued by"] + ".");
            ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
            icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to expired signature.", "red");
            this._ica.FailToken("Your authentication token has expired.");
          }
          else if (exact2 > DateTime.UtcNow.AddMinutes(20.0))
          {
            ServerConsole.AddLog("Player tried to use non-issued authentication token. Server: " + dictionary["Issued by"] + ".");
            ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
            icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to non-issued signature.", "red");
            this._ica.FailToken("Your authentication token has invalid issuance date.");
          }
          else if (CustomNetworkManager.isPrivateBeta && (!dictionary.ContainsKey("Private beta ownership") || dictionary["Private beta ownership"] != "YES"))
          {
            ServerConsole.AddLog("Player " + dictionary["User ID"] + " tried to join this server, but is not Private Beta DLC owner. Server: " + dictionary["Issued by"] + ".");
            icaa.GetCcm().TargetConsolePrint(icaa.GetCcm().connectionToClient, "Private Beta DLC ownership is required to join private beta server.", "red");
            this._ica.FailToken("Private Beta DLC ownership is required to join private beta server.");
          }
          else
          {
            icaa.GetCcm().GetComponent<ServerRoles>().FirstVerResult = dictionary;
            icaa.Ok(dictionary["User ID"], dictionary.ContainsKey("User ID 2") ? dictionary["User ID 2"] : (string) null, dictionary["Global ban"], dictionary["Issued by"], dictionary["Bypass bans"] == "YES", dictionary["Bypass WL"] == "YES", dictionary.ContainsKey("Do Not Track") && dictionary["Do Not Track"] == "YES", dictionary.ContainsKey("Serial") ? dictionary["Serial"] : "N/A", dictionary.ContainsKey("VAC session") ? dictionary["VAC session"] : "N/A", dictionary.ContainsKey("Request IP") ? dictionary["Request IP"] : "N/A", dictionary.ContainsKey("ASN") ? dictionary["ASN"] : "N/A", dictionary.ContainsKey("Skip IP Check") && dictionary["Skip IP Check"] == "YES");
            string str = CentralAuth.RemoveSalt(dictionary["User ID"]) + " authenticated from endpoint " + (endpoint == null ? "(null)" : endpoint.ToString()) + ". Auth token serial number: " + (dictionary.ContainsKey("Serial") ? dictionary["Serial"] : "N/A");
            ServerConsole.AddLog(str);
            ServerLogs.AddLog(ServerLogs.Modules.Networking, str, ServerLogs.ServerLogType.ConnectionUpdate);
          }
          if (endpoint != null)
          {
            if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(endpoint))
              CustomLiteNetLib4MirrorTransport.UserIds.Remove(endpoint);
          }
        }
      }
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Error during authentication token verification: " + ex.Message);
      icaa.Fail();
    }
    yield return float.NegativeInfinity;
  }

  internal static Dictionary<string, string> ValidatePartialAuthToken(
    string token,
    string userId,
    string nickname,
    string authSerial,
    string usage)
  {
    if (!string.IsNullOrEmpty(token))
    {
      if (token.Contains("<br>Signature: "))
      {
        try
        {
          string data = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
          string signature = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15).Replace("<br>", "");
          if (!ECDSA.Verify(data, signature, ServerConsole.PublicKey))
          {
            ServerConsole.AddLog("Partial auth token signature mismatch.");
            return (Dictionary<string, string>) null;
          }
          Dictionary<string, string> dictionary = ((IEnumerable<string>) data.Split(new string[1]
          {
            "<br>"
          }, StringSplitOptions.None)).Select<string, string[]>((Func<string, string[]>) (rwr => rwr.Split(new string[1]
          {
            ": "
          }, StringSplitOptions.None))).ToDictionary<string[], string, string>((Func<string[], string>) (split => split[0]), (Func<string[], string>) (split => split[1]));
          if (dictionary["Usage"] != usage)
          {
            ServerConsole.AddLog("Player tried to use a partial auth token issued to a different purpose.");
            return (Dictionary<string, string>) null;
          }
          if (authSerial != null && dictionary["Serial"] != authSerial)
          {
            ServerConsole.AddLog("Partial auth token serial mismatch.");
            return (Dictionary<string, string>) null;
          }
          if (dictionary["Test signature"] != "NO")
          {
            ServerConsole.AddLog("Player tried to use a partial auth token issued only for testing. Server: " + dictionary["Issued by"] + ".");
            return (Dictionary<string, string>) null;
          }
          if (string.IsNullOrEmpty(userId))
          {
            ServerConsole.AddLog("Player tried to use a partial auth token issued for different user (User ID mismatch - empty). Server: " + dictionary["Issued by"] + ".");
            return (Dictionary<string, string>) null;
          }
          string str1 = Sha.HashToString(Sha.Sha512(userId));
          string str2 = Sha.HashToString(Sha.Sha512(dictionary["User ID"]));
          if (dictionary["User ID"] != userId && dictionary["User ID"] != str1 && (str2 != userId && str2 != str1))
          {
            ServerConsole.AddLog("Player tried to use a partial auth token issued for different user (User ID mismatch). Server: " + dictionary["Issued by"] + ".");
            return (Dictionary<string, string>) null;
          }
          if (Misc.Base64Decode(dictionary["Nickname"]) != nickname)
          {
            ServerConsole.AddLog("Player tried to use a partial auth token issued for different user (nickname mismatch). Server: " + dictionary["Issued by"] + ".");
            return (Dictionary<string, string>) null;
          }
          DateTime exact1 = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture);
          DateTime exact2 = DateTime.ParseExact(dictionary["Issuance time"], "yyyy-MM-dd HH:mm:ss", (IFormatProvider) CultureInfo.InvariantCulture);
          DateTime utcNow = DateTime.UtcNow;
          if (exact1 < utcNow)
          {
            ServerConsole.AddLog("Player tried to use expired partial auth request token. Server: " + dictionary["Issued by"] + ".");
            ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
            return (Dictionary<string, string>) null;
          }
          if (!(exact2 > DateTime.UtcNow.AddMinutes(20.0)))
            return dictionary;
          ServerConsole.AddLog("Player tried to use non-issued partial auth token. Server: " + dictionary["Issued by"] + ".");
          ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
          return (Dictionary<string, string>) null;
        }
        catch (Exception ex)
        {
          ServerConsole.AddLog("Error during partial auth token verification: " + ex.Message);
          Debug.Log((object) ("Error during partial auth token verification: " + ex.Message + " StackTrace: " + ex.StackTrace));
          return (Dictionary<string, string>) null;
        }
      }
    }
    return (Dictionary<string, string>) null;
  }

  public static string RemoveSalt(string userId)
  {
    return userId.Contains("$") ? userId.Substring(0, userId.IndexOf("$", StringComparison.Ordinal)) : userId;
  }
}
