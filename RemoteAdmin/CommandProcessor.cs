// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.CommandProcessor
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using Utils.CommandInterpolation;

namespace RemoteAdmin
{
  public static class CommandProcessor
  {
    internal static void ProcessQuery(string q, CommandSender sender)
    {
      string[] query = q.Split(' ');
      string str1 = sender.Nickname;
      QueryProcessor queryProcessor = sender is PlayerCommandSender playerCommandSender ? playerCommandSender.Processor : (QueryProcessor) null;
      if (playerCommandSender != null)
        str1 = str1 + " (" + playerCommandSender.CCM.UserId + ")";
      string upper1 = query[0].ToUpper();
      uint stringHash1 = PrivateImplementationDetails.ComputeStringHash(upper1);
      if (stringHash1 <= 2129901492U)
      {
        if (stringHash1 <= 934975720U)
        {
          if (stringHash1 <= 501868943U)
          {
            if (stringHash1 <= 275074824U)
            {
              if (stringHash1 <= 43150473U)
              {
                if (stringHash1 != 27894855U)
                {
                  if ((stringHash1 != 43150473U ? 1 : (!(upper1 == "PING") ? 1 : 0)) == 0)
                  {
                    if (query.Length == 1)
                    {
                      if ((UnityEngine.Object) queryProcessor == (UnityEngine.Object) null)
                      {
                        sender.RaReply(query[0].ToUpper() + "#This command is only available for players!", false, true, "");
                        return;
                      }
                      int connectionId = queryProcessor.connectionToClient.connectionId;
                      if (connectionId == 0)
                      {
                        sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", false, true, "");
                        return;
                      }
                      sender.RaReply(query[0].ToUpper() + "#Your ping: " + (object) LiteNetLib4MirrorServer.Peers[connectionId].Ping + "ms", true, true, "");
                      return;
                    }
                    if (query.Length != 2)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Too many arguments! (expected 0 or 1)", false, true, "");
                      return;
                    }
                    int id2;
                    if (!int.TryParse(query[1], out id2))
                    {
                      sender.RaReply(query[0].ToUpper() + "#Invalid player id!", false, true, "");
                      return;
                    }
                    GameObject gameObject = PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (pl => pl.GetComponent<QueryProcessor>().PlayerId == id2));
                    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Invalid player id!", false, true, "");
                      return;
                    }
                    int connectionId1 = gameObject.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
                    if (connectionId1 == 0)
                    {
                      sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", false, true, "");
                      return;
                    }
                    sender.RaReply(query[0].ToUpper() + "#Ping: " + (object) LiteNetLib4MirrorServer.Peers[connectionId1].Ping + "ms", true, true, "");
                    return;
                  }
                  goto label_958;
                }
                else if (upper1 == "CONFIG")
                  goto label_816;
                else
                  goto label_958;
              }
              else if (stringHash1 != 170942193U)
              {
                if (stringHash1 != 219017314U)
                {
                  if (stringHash1 != 275074824U || !(upper1 == "FORCESTART"))
                    goto label_958;
                  else
                    goto label_363;
                }
                else if (upper1 == "SNR")
                  goto label_806;
                else
                  goto label_958;
              }
              else if (upper1 == "SLML_STYLE")
                goto label_809;
              else
                goto label_958;
            }
            else if (stringHash1 <= 371615861U)
            {
              if (stringHash1 != 290479497U)
              {
                if (stringHash1 != 329200923U)
                {
                  if (stringHash1 != 371615861U || !(upper1 == "DOORLIST"))
                    goto label_958;
                  else
                    goto label_950;
                }
                else if (upper1 == "UNMUTE")
                  goto label_898;
                else
                  goto label_958;
              }
              else if (upper1 == "OPEN")
                goto label_940;
              else
                goto label_958;
            }
            else if (stringHash1 != 427453584U)
            {
              if (stringHash1 != 501045426U)
              {
                if (stringHash1 != 501868943U || !(upper1 == "DTP"))
                  goto label_958;
              }
              else if (upper1 == "BC")
                goto label_789;
              else
                goto label_958;
            }
            else if (upper1 == "CLEARALERT")
              goto label_796;
            else
              goto label_958;
          }
          else if (stringHash1 <= 754810370U)
          {
            if (stringHash1 <= 543160318U)
            {
              if (stringHash1 != 517638783U)
              {
                if (stringHash1 != 534600664U)
                {
                  if (stringHash1 != 543160318U || !(upper1 == "ALERTMONO"))
                    goto label_958;
                  else
                    goto label_789;
                }
                else if (upper1 == "BM")
                  goto label_914;
                else
                  goto label_958;
              }
              else if (upper1 == "REQUEST_DATA")
              {
                if (query.Length >= 2)
                {
                  string upper2 = query[1].ToUpper();
                  if (!(upper2 == "PLAYER_LIST"))
                  {
                    if ((upper2 == "PLAYER" ? 0 : (!(upper2 == "SHORT-PLAYER") ? 1 : 0)) != 0)
                    {
                      if (!(upper2 == "AUTH"))
                      {
                        sender.RaReply(query[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", false, true, "PlayerInfo");
                        return;
                      }
                      if ((playerCommandSender == null || !playerCommandSender.SR.Staff ? (CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess, "", true) ? 1 : 0) : 1) == 0)
                        return;
                      if (query.Length >= 3)
                      {
                        try
                        {
                          GameObject gameObject = (GameObject) null;
                          foreach (NetworkConnection conn in NetworkServer.connections.Values)
                          {
                            GameObject connectedRoot = GameCore.Console.FindConnectedRoot(conn);
                            if (query[2].Contains("."))
                              query[2] = query[2].Split('.')[0];
                            if ((!((UnityEngine.Object) connectedRoot != (UnityEngine.Object) null) ? 0 : (connectedRoot.GetComponent<QueryProcessor>().PlayerId.ToString() == query[2] ? 1 : 0)) != 0)
                              gameObject = connectedRoot;
                          }
                          if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
                          {
                            sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", false, true, "");
                            return;
                          }
                          if (string.IsNullOrEmpty(gameObject.GetComponent<CharacterClassManager>().AuthToken))
                          {
                            sender.RaReply(query[0].ToUpper() + ":PLAYER#Can't obtain auth token. Is server using offline mode or you selected the host?", false, true, "PlayerInfo");
                            return;
                          }
                          ServerLogs.AddLog(ServerLogs.Modules.DataAccess, str1 + " accessed authentication token of player " + (object) gameObject.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                          if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
                          {
                            string str2 = "<color=white>Authentication token of player " + gameObject.GetComponent<NicknameSync>().MyNick + "(" + (object) gameObject.GetComponent<QueryProcessor>().PlayerId + "):\n" + gameObject.GetComponent<CharacterClassManager>().AuthToken + "</color>";
                            sender.RaReply(query[0].ToUpper() + ":PLAYER#" + str2, true, true, "null");
                            sender.RaReply("BigQR#" + gameObject.GetComponent<CharacterClassManager>().AuthToken, true, false, "null");
                            return;
                          }
                          sender.RaReply("StaffTokenReply#" + gameObject.GetComponent<CharacterClassManager>().AuthToken, true, false, "null");
                          return;
                        }
                        catch (Exception ex)
                        {
                          sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, false, true, "PlayerInfo");
                          throw;
                        }
                      }
                      else
                      {
                        sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, "");
                        return;
                      }
                    }
                    else if (query.Length >= 3)
                    {
                      if ((!string.Equals(query[1], "PLAYER", StringComparison.OrdinalIgnoreCase) || playerCommandSender != null && playerCommandSender.SR.Staff ? 0 : (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess, "", true) ? 1 : 0)) != 0)
                        return;
                      try
                      {
                        GameObject gameObject = (GameObject) null;
                        NetworkConnection networkConnection = (NetworkConnection) null;
                        foreach (NetworkConnection conn in NetworkServer.connections.Values)
                        {
                          GameObject connectedRoot = GameCore.Console.FindConnectedRoot(conn);
                          if (query[2].Contains("."))
                            query[2] = query[2].Split('.')[0];
                          if (((UnityEngine.Object) connectedRoot == (UnityEngine.Object) null ? 0 : (!(connectedRoot.GetComponent<QueryProcessor>().PlayerId.ToString() != query[2]) ? 1 : 0)) != 0)
                          {
                            gameObject = connectedRoot;
                            networkConnection = conn;
                          }
                        }
                        if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
                        {
                          sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", false, true, "");
                          return;
                        }
                        bool flag = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, "", false);
                        if (sender is PlayerCommandSender playerCommandSender)
                          playerCommandSender.Processor.GameplayData = flag;
                        CharacterClassManager component1 = gameObject.GetComponent<CharacterClassManager>();
                        ServerRoles component2 = gameObject.GetComponent<ServerRoles>();
                        if (query[1].ToUpper() == "PLAYER")
                          ServerLogs.AddLog(ServerLogs.Modules.DataAccess, str1 + " accessed IP address of player " + (object) gameObject.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                        StringBuilder stringBuilder = new StringBuilder("<color=white>");
                        stringBuilder.Append("Nickname: " + gameObject.GetComponent<NicknameSync>().MyNick);
                        stringBuilder.Append("\nPlayer ID: " + (object) gameObject.GetComponent<QueryProcessor>().PlayerId);
                        stringBuilder.Append("\nIP: " + (networkConnection == null ? "null" : (query[1].ToUpper() == "PLAYER" ? networkConnection.address : "[REDACTED]")));
                        stringBuilder.Append("\nUser ID: " + (string.IsNullOrEmpty(component1.UserId) ? "(none)" : component1.UserId));
                        if ((component1.SaltedUserId == null ? 0 : (component1.SaltedUserId.Contains("$") ? 1 : 0)) != 0)
                          stringBuilder.Append("\nSalted User ID: " + component1.SaltedUserId);
                        if (!string.IsNullOrEmpty(component1.UserId2))
                          stringBuilder.Append("\nUser ID 2: " + component1.UserId2);
                        stringBuilder.Append("\nServer role: " + component2.GetColoredRoleString(false));
                        if (!string.IsNullOrEmpty(component2.HiddenBadge))
                          stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + component2.HiddenBadge);
                        if (component2.RaEverywhere)
                          stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
                        else if (component2.Staff)
                          stringBuilder.Append("\nActive flag: Studio Staff");
                        if (component1.Muted)
                          stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
                        else if (component1.IntercomMuted)
                          stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
                        if (component1.GodMode)
                          stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
                        if (component1.NoclipEnabled)
                          stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
                        else if (component2.NoclipReady)
                          stringBuilder.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
                        if (component2.DoNotTrack)
                          stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
                        if (component2.BypassMode)
                          stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
                        if (component2.RemoteAdmin)
                          stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
                        if (component2.OverwatchEnabled)
                        {
                          stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
                        }
                        else
                        {
                          stringBuilder.Append("\nClass: " + (!flag ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (component1.Classes.CheckBounds(component1.CurClass) ? component1.Classes.SafeGet(component1.CurClass).fullName : "None")));
                          stringBuilder.Append("\nHP: " + (flag ? gameObject.GetComponent<PlayerStats>().HealthToString() : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
                          if (!flag)
                            stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
                        }
                        stringBuilder.Append("</color>");
                        sender.RaReply(query[0].ToUpper() + ":PLAYER#" + (object) stringBuilder, true, true, "PlayerInfo");
                        sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(component1.UserId) ? "(no User ID)" : component1.UserId), true, false, "PlayerInfo");
                        return;
                      }
                      catch (Exception ex)
                      {
                        sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, false, true, "PlayerInfo");
                        throw;
                      }
                    }
                    else
                    {
                      sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, "");
                      return;
                    }
                  }
                  else
                  {
                    try
                    {
                      string str2 = "\n";
                      bool flag1 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, string.Empty, false);
                      if (sender is PlayerCommandSender playerCommandSender)
                        playerCommandSender.Processor.GameplayData = flag1;
                      bool flag2 = q.Contains("STAFF", StringComparison.OrdinalIgnoreCase);
                      foreach (GameObject player in PlayerManager.players)
                      {
                        QueryProcessor component1 = player.GetComponent<QueryProcessor>();
                        if (!flag2)
                        {
                          string str3 = string.Empty;
                          bool flag3 = false;
                          ServerRoles component2 = component1.GetComponent<ServerRoles>();
                          try
                          {
                            str3 = component2.RaEverywhere ? "[~] " : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty));
                            flag3 = component2.OverwatchEnabled;
                          }
                          catch
                          {
                          }
                          str2 = str2 + str3 + "(" + (object) component1.PlayerId + ") " + component1.GetComponent<NicknameSync>().MyNick.Replace("\n", "").Replace("<", "").Replace(">", "") + (flag3 ? (object) "<OVRM>" : (object) "");
                        }
                        else
                          str2 = str2 + (object) component1.PlayerId + ";" + component1.GetComponent<NicknameSync>().MyNick;
                        str2 += "\n";
                      }
                      if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
                      {
                        sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + str2, true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
                        return;
                      }
                      sender.RaReply("StaffPlayerListReply#" + str2, true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
                      return;
                    }
                    catch (Exception ex)
                    {
                      sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, false, true, "");
                      throw;
                    }
                  }
                }
                else
                {
                  sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "PlayerInfo");
                  return;
                }
              }
              else
                goto label_958;
            }
            else if (stringHash1 != 603976806U)
            {
              if (stringHash1 != 644288100U)
              {
                if (stringHash1 != 754810370U || !(upper1 == "LOCK"))
                  goto label_958;
                else
                  goto label_945;
              }
              else if (upper1 == "SUDO")
                goto label_799;
              else
                goto label_958;
            }
            else
            {
              if (upper1 == "PM")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement, "", true))
                  return;
                Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
                List<string> allPermissions = ServerStatic.PermissionsHandler.GetAllPermissions();
                if (query.Length == 1)
                {
                  sender.RaReply(query[0].ToUpper() + "#Permissions manager help:\nSyntax: " + query[0] + " action\n\nAvailable actions:\ngroups - lists all groups\ngroup info <group name> - prints group info\ngroup grant <group name> <permission name> - grants permission to a group\ngroup revoke <group name> <permission name> - revokes permission from a group\ngroup setcolor <group name> <color name> - sets group color\ngroup settag <group name> <tag> - sets group tag\ngroup enablecover <group name> - enables badge cover for group\ngroup disablecover <group name> - disables badge cover for group\n\nusers - lists all privileged users\nsetgroup <UserID> <group name> - sets membership of user (use group name \"-1\" to remove user from group)\nreload - reloads permission file\n\n\"< >\" are only used to indicate the arguments, don't put them\nMore commands will be added in the next versions of the game", true, true, "");
                  return;
                }
                if (string.Equals(query[1], "groups", StringComparison.OrdinalIgnoreCase))
                {
                  string str2 = "\n";
                  string str3 = "";
                  string[] strArray = new string[23]
                  {
                    "BN1",
                    "BN2",
                    "BN3",
                    "FSE",
                    "FSP",
                    "FWR",
                    "GIV",
                    "EWA",
                    "ERE",
                    "ERO",
                    "SGR",
                    "GMD",
                    "OVR",
                    "FCM",
                    "PLM",
                    "PRM",
                    "SSC",
                    "VHB",
                    "CFG",
                    "BRC",
                    "CDA",
                    "NCP",
                    "AFK"
                  };
                  int num1 = (int) Math.Ceiling((double) allPermissions.Count / 12.0);
                  for (int index = 0; index < num1; ++index)
                  {
                    int num2 = 0;
                    str2 = str2 + "\n-----" + ((IEnumerable<string>) strArray).Skip<string>(index * 12).Take<string>(12).Aggregate<string>((Func<string, string, string>) ((current, adding) => current + " " + adding));
                    foreach (KeyValuePair<string, UserGroup> keyValuePair in allGroups)
                    {
                      if (index == 0)
                        str3 = str3 + "\n" + (object) num2 + " - " + keyValuePair.Key;
                      string str4 = num2.ToString();
                      for (int length = str4.Length; length < 5; ++length)
                        str4 += " ";
                      foreach (string check in allPermissions.Skip<string>(index * 12).Take<string>(12))
                        str4 = str4 + "  " + (ServerStatic.PermissionsHandler.IsPermitted(keyValuePair.Value.Permissions, check) ? "X" : " ") + " ";
                      ++num2;
                      str2 = str2 + "\n" + str4;
                    }
                  }
                  sender.RaReply(query[0].ToUpper() + "#All defined groups: " + str2 + "\n" + str3, true, true, "");
                  return;
                }
                if ((!string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 2 ? 1 : 0)) != 0)
                {
                  sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                  return;
                }
                if ((!string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length > 2 ? 1 : 0)) != 0)
                {
                  if ((!string.Equals(query[2], "info", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 4 ? 1 : 0)) != 0)
                  {
                    KeyValuePair<string, UserGroup> keyValuePair = allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3]));
                    if (keyValuePair.Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    sender.RaReply(query[0].ToUpper() + "#Details of group " + keyValuePair.Key + "\nTag text: " + keyValuePair.Value.BadgeText + "\nTag color: " + keyValuePair.Value.BadgeColor + "\nPermissions: " + (object) keyValuePair.Value.Permissions + "\nCover: " + (keyValuePair.Value.Cover ? (object) "YES" : (object) "NO") + "\nHidden by default: " + (keyValuePair.Value.HiddenByDefault ? (object) "YES" : (object) "NO") + "\nKick power: " + (object) keyValuePair.Value.KickPower + "\nRequired kick power: " + (object) keyValuePair.Value.RequiredKickPower, true, true, "");
                    return;
                  }
                  if ((string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase) || string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase) ? (query.Length == 5 ? 1 : 0) : 0) != 0)
                  {
                    if (allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3])).Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    if (!allPermissions.Contains(query[4]))
                    {
                      sender.RaReply(query[0].ToUpper() + "#Permission can't be found.", false, true, "");
                      return;
                    }
                    Dictionary<string, string> stringDictionary = ServerStatic.RolesConfig.GetStringDictionary("Permissions");
                    List<string> stringList = (List<string>) null;
                    foreach (string key in stringDictionary.Keys)
                    {
                      if (!(key != query[4]))
                        stringList = ((IEnumerable<string>) YamlConfig.ParseCommaSeparatedString(stringDictionary[key])).ToList<string>();
                    }
                    if (stringList == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Permission can't be found in the config.", false, true, "");
                      return;
                    }
                    if ((!stringList.Contains(query[3]) ? 0 : (string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase) ? 1 : 0)) != 0)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group already has that permission.", false, true, "");
                      return;
                    }
                    if ((stringList.Contains(query[3]) ? 0 : (string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase) ? 1 : 0)) != 0)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group already doesn't have that permission.", false, true, "");
                      return;
                    }
                    if (string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
                      stringList.Add(query[3]);
                    else
                      stringList.Remove(query[3]);
                    stringList.Sort();
                    string str2 = "[";
                    foreach (string str3 in stringList)
                    {
                      if (str2 != "[")
                        str2 += ", ";
                      str2 += str3;
                    }
                    string str4 = str2 + "]";
                    ServerStatic.RolesConfig.SetStringDictionaryItem("Permissions", query[4], str4);
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    sender.RaReply(query[0].ToUpper() + "#Permissions updated.", true, true, "");
                    return;
                  }
                  if ((!string.Equals(query[2], "setcolor", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 5 ? 1 : 0)) != 0)
                  {
                    if (allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3])).Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    ServerStatic.RolesConfig.SetString(query[3] + "_color", query[4]);
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    sender.RaReply(query[0].ToUpper() + "#Group color updated.", true, true, "");
                    return;
                  }
                  if ((!string.Equals(query[2], "settag", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 5 ? 1 : 0)) != 0)
                  {
                    if (allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3])).Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    ServerStatic.RolesConfig.SetString(query[3] + "_badge", query[4]);
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    sender.RaReply(query[0].ToUpper() + "#Group tag updated.", true, true, "");
                    return;
                  }
                  if ((!string.Equals(query[2], "enablecover", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 4 ? 1 : 0)) != 0)
                  {
                    if (allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3])).Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    ServerStatic.RolesConfig.SetString(query[3] + "_cover", "true");
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", true, true, "");
                    return;
                  }
                  if ((!(query[2].ToLower() == "disablecover") ? 1 : (query.Length != 4 ? 1 : 0)) != 0)
                  {
                    sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                    return;
                  }
                  if (allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3])).Key == null)
                  {
                    sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                    return;
                  }
                  ServerStatic.RolesConfig.SetString(query[3] + "_cover", "false");
                  ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                  sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", true, true, "");
                  return;
                }
                if (string.Equals(query[1], "users", StringComparison.OrdinalIgnoreCase))
                {
                  Dictionary<string, string> stringDictionary1 = ServerStatic.RolesConfig.GetStringDictionary("Members");
                  Dictionary<string, string> stringDictionary2 = ServerStatic.SharedGroupsMembersConfig?.GetStringDictionary("SharedMembers");
                  string str2 = "Players with assigned groups:";
                  foreach (KeyValuePair<string, string> keyValuePair in stringDictionary1)
                    str2 = str2 + "\n" + keyValuePair.Key + " - " + keyValuePair.Value;
                  if (stringDictionary2 != null)
                  {
                    foreach (KeyValuePair<string, string> keyValuePair in stringDictionary2)
                      str2 = str2 + "\n" + keyValuePair.Key + " - " + keyValuePair.Value + " <color=#FFD700>[SHARED MEMBERSHIP]</color>";
                  }
                  sender.RaReply(query[0].ToUpper() + "#" + str2, true, true, "");
                  return;
                }
                if ((!string.Equals(query[1], "setgroup", StringComparison.OrdinalIgnoreCase) ? 0 : (query.Length == 4 ? 1 : 0)) != 0)
                {
                  string str2;
                  if (query[3] == "-1")
                  {
                    str2 = (string) null;
                  }
                  else
                  {
                    KeyValuePair<string, UserGroup> keyValuePair = allGroups.FirstOrDefault<KeyValuePair<string, UserGroup>>((Func<KeyValuePair<string, UserGroup>, bool>) (gr => gr.Key == query[3]));
                    if (keyValuePair.Key == null)
                    {
                      sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                      return;
                    }
                    str2 = keyValuePair.Key;
                  }
                  ServerStatic.RolesConfig.SetStringDictionaryItem("Members", query[2], str2);
                  ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                  sender.RaReply(query[0].ToUpper() + "#User permissions updated. If user is online, please use \"setgroup\" command to change it now (without this command, new role will be applied during next round).", true, true, "");
                  return;
                }
                if (string.Equals(query[1], "reload", StringComparison.OrdinalIgnoreCase))
                {
                  ConfigFile.ReloadGameConfigs(false);
                  ServerStatic.RolesConfig.Reload();
                  ServerStatic.SharedGroupsConfig = ConfigSharing.Paths[4] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt");
                  ServerStatic.SharedGroupsMembersConfig = ConfigSharing.Paths[5] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt");
                  ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                  sender.RaReply(query[0].ToUpper() + "#Permission file reloaded.", true, true, "");
                  return;
                }
                sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                return;
              }
              goto label_958;
            }
          }
          else if (stringHash1 <= 873487573U)
          {
            if (stringHash1 != 844380939U)
            {
              if (stringHash1 != 858808885U)
              {
                if (stringHash1 != 873487573U || !(upper1 == "UNLOCK"))
                  goto label_958;
                else
                  goto label_368;
              }
              else if (!(upper1 == "DOORTP"))
                goto label_958;
            }
            else
            {
              if (upper1 == "HELLO")
              {
                sender.RaReply(query[0].ToUpper() + "#Hello World!", true, true, "");
                return;
              }
              goto label_958;
            }
          }
          else if (stringHash1 != 904296662U)
          {
            if (stringHash1 != 912447482U)
            {
              if (stringHash1 == 934975720U && upper1 == "INTERCOM-TIMEOUT")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[6]
                {
                  PlayerPermissions.KickingAndShortTermBanning,
                  PlayerPermissions.BanningUpToDay,
                  PlayerPermissions.LongTermBanning,
                  PlayerPermissions.RoundEvents,
                  PlayerPermissions.FacilityManagement,
                  PlayerPermissions.PlayersManagement
                }, "ServerEvents", true))
                  return;
                if (!Intercom.host.speaking)
                {
                  sender.RaReply(query[0].ToUpper() + "#Intercom is not being used.", false, true, "ServerEvents");
                  return;
                }
                if ((double) Intercom.host.speechRemainingTime == -77.0)
                {
                  sender.RaReply(query[0].ToUpper() + "#Intercom is being used by player with bypass mode enabled.", false, true, "ServerEvents");
                  return;
                }
                Intercom.host.speechRemainingTime = -1f;
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " timeouted the intercom speaker.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                sender.RaReply(query[0].ToUpper() + "#Done! Intercom speaker timeouted.", true, true, "ServerEvents");
                return;
              }
              goto label_958;
            }
            else
            {
              if (upper1 == "HELP")
              {
                sender.RaReply(query[0].ToUpper() + "#This should be useful!", true, true, "");
                return;
              }
              goto label_958;
            }
          }
          else if (upper1 == "NC")
            goto label_872;
          else
            goto label_958;
          if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "DoorsManagement", true))
            return;
          if (query.Length != 3)
          {
            sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " PlayerIDs DoorName", false, true, "");
            return;
          }
          ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the DoorTp command (Door: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
          int failures;
          int successes;
          string error;
          bool replySent;
          CommandProcessor.StandardizedQueryModel1(sender, "DOORTP", query[1], query[2], out failures, out successes, out error, out replySent, "");
          if (replySent)
            return;
          if (failures == 0)
          {
            sender.RaReply(query[0] + "#Done! The request affected " + (object) successes + " player(s)!", true, true, "DoorsManagement");
            return;
          }
          sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "DoorsManagement");
          return;
        }
        if (stringHash1 <= 1443004851U)
        {
          if (stringHash1 <= 1159084506U)
          {
            if (stringHash1 <= 991654772U)
            {
              if (stringHash1 != 945458267U)
              {
                if (stringHash1 != 971901278U)
                {
                  if (stringHash1 != 991654772U || !(upper1 == "RTIME"))
                    goto label_958;
                  else
                    goto label_360;
                }
                else if (upper1 == "OVR")
                  goto label_887;
                else
                  goto label_958;
              }
              else
              {
                if (upper1 == "HEAL")
                {
                  if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true))
                    return;
                  if (query.Length < 2)
                  {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                    return;
                  }
                  int result = query.Length < 3 || !int.TryParse(query[2], out result) ? 0 : result;
                  ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the heal command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                  int failures;
                  int successes;
                  string error;
                  bool replySent;
                  CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], result.ToString(), out failures, out successes, out error, out replySent, "");
                  if (replySent)
                    return;
                  if (failures == 0)
                  {
                    sender.RaReply(query[0] + "#Done! The request affected " + (object) successes + " player(s)!", true, true, "");
                    return;
                  }
                  sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "AdminTools");
                  return;
                }
                goto label_958;
              }
            }
            else if (stringHash1 != 1094345139U)
            {
              if (stringHash1 != 1154386829U)
              {
                if (stringHash1 == 1159084506U && upper1 == "SETGROUP")
                {
                  if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.SetGroup, "", true))
                    return;
                  if (!ConfigFile.ServerConfig.GetBool("online_mode", true))
                  {
                    sender.RaReply(query[0] + "#This command requires the server to operate in online mode!", false, true, "");
                    return;
                  }
                  if (query.Length < 3)
                  {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                    return;
                  }
                  ServerLogs.AddLog(ServerLogs.Modules.Permissions, str1 + " ran the setgroup command (new group: " + query[2] + " min) on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                  int failures;
                  int successes;
                  string error;
                  bool replySent;
                  CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent, "");
                  if (replySent)
                    return;
                  if (failures == 0)
                  {
                    sender.RaReply(query[0] + "#Done! The request affected " + (object) successes + " player(s)!", true, true, "");
                    return;
                  }
                  sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "");
                  return;
                }
                goto label_958;
              }
              else if (upper1 == "DL")
                goto label_950;
              else
                goto label_958;
            }
            else if (!(upper1 == "SETHP"))
              goto label_958;
          }
          else if (stringHash1 <= 1226143193U)
          {
            if (stringHash1 != 1160557609U)
            {
              if (stringHash1 != 1173502427U)
              {
                if (stringHash1 != 1226143193U || !(upper1 == "ALERT"))
                  goto label_958;
                else
                  goto label_789;
              }
              else if (upper1 == "BCCLEAR")
                goto label_796;
              else
                goto label_958;
            }
            else if (upper1 == "SPEAK")
              goto label_907;
            else
              goto label_958;
          }
          else if (stringHash1 != 1302443662U)
          {
            if (stringHash1 != 1442280307U)
            {
              if (stringHash1 != 1443004851U || !(upper1 == "SC"))
                goto label_958;
              else
                goto label_816;
            }
            else if (upper1 == "CASSIE_SN")
              goto label_784;
            else
              goto label_958;
          }
          else if (upper1 == "ALERTCLEAR")
            goto label_796;
          else
            goto label_958;
        }
        else if (stringHash1 <= 1761926707U)
        {
          if (stringHash1 <= 1630279262U)
          {
            if (stringHash1 != 1475835545U)
            {
              if (stringHash1 != 1521082795U)
              {
                if (stringHash1 != 1630279262U || !(upper1 == "IUNMUTE"))
                  goto label_958;
                else
                  goto label_898;
              }
              else
              {
                if (upper1 == "PERM")
                {
                  if (!CommandProcessor.IsPlayer(sender, query[0], "") || (UnityEngine.Object) queryProcessor == (UnityEngine.Object) null)
                    return;
                  ulong permissions = queryProcessor.Roles.Permissions;
                  string str2 = "Your permissions:";
                  foreach (string allPermission in ServerStatic.PermissionsHandler.GetAllPermissions())
                  {
                    string str3 = ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler.GetPermissionValue(allPermission)) ? "*" : "";
                    str2 = str2 + "\n" + allPermission + str3 + " (" + (object) ServerStatic.PermissionsHandler.GetPermissionValue(allPermission) + "): " + (ServerStatic.PermissionsHandler.IsPermitted(permissions, allPermission) ? (object) "YES" : (object) "NO");
                  }
                  sender.RaReply(query[0].ToUpper() + "#" + str2, true, true, "");
                  return;
                }
                goto label_958;
              }
            }
            else if (upper1 == "CASSIE_SL")
              goto label_784;
            else
              goto label_958;
          }
          else if (stringHash1 != 1692550565U)
          {
            if (stringHash1 != 1723521268U)
            {
              if (stringHash1 != 1761926707U || !(upper1 == "RT"))
                goto label_958;
              else
                goto label_360;
            }
            else if (upper1 == "ROUNDLOCK")
              goto label_953;
            else
              goto label_958;
          }
          else if (upper1 == "LL")
            goto label_956;
          else
            goto label_958;
        }
        else if (stringHash1 <= 1951805507U)
        {
          if (stringHash1 != 1826771517U)
          {
            if (stringHash1 != 1894470373U)
            {
              if (stringHash1 != 1951805507U || !(upper1 == "CASSIE_SILENTNOISE"))
                goto label_958;
              else
                goto label_784;
            }
            else if (!(upper1 == "HP"))
              goto label_958;
          }
          else if (upper1 == "LD")
            goto label_925;
          else
            goto label_958;
        }
        else if (stringHash1 != 1976784350U)
        {
          if (stringHash1 != 1990398098U)
          {
            if (stringHash1 != 2129901492U || !(upper1 == "UL"))
              goto label_958;
            else
              goto label_368;
          }
          else if (upper1 == "ROUNDTIME")
            goto label_360;
          else
            goto label_958;
        }
        else if (upper1 == "FS")
          goto label_363;
        else
          goto label_958;
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true))
          return;
        if (query.Length < 3)
        {
          sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
          return;
        }
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the sethp command on " + query[1] + " players (HP: " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        int failures1;
        int successes1;
        string error1;
        bool replySent1;
        CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures1, out successes1, out error1, out replySent1, "");
        if (replySent1)
          return;
        if (failures1 == 0)
        {
          sender.RaReply(query[0] + "#Done! The request affected " + (object) successes1 + " player(s)!", true, true, "");
          return;
        }
        sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures1 + "\nLast error log:\n" + error1, false, true, "");
        return;
label_360:
        if (RoundStart.RoundLenght.Ticks == 0L)
        {
          sender.RaReply(query[0].ToUpper() + "#The round has not yet started!", false, true, "");
          return;
        }
        sender.RaReply(query[0].ToUpper() + "#Round time: " + RoundStart.RoundLenght.ToString("hh\\:mm\\:ss\\.fff", (IFormatProvider) CultureInfo.InvariantCulture), true, true, "");
        return;
label_363:
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true))
          return;
        bool success = CharacterClassManager.ForceRoundStart();
        if (success)
          ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " forced round start.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        sender.RaReply(query[0] + "#" + (success ? "Done! Forced round start." : "Failed to force start."), success, true, "ServerEvents");
        return;
label_368:
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true))
          return;
        if (query.Length != 2)
        {
          sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
          return;
        }
        CommandProcessor.ProcessDoorQuery(sender, "UNLOCK", query[1]);
        return;
      }
      if (stringHash1 <= 3373006507U)
      {
        if (stringHash1 <= 2409015043U)
        {
          if (stringHash1 <= 2245226254U)
          {
            if (stringHash1 <= 2184885908U)
            {
              if (stringHash1 != 2163566540U)
              {
                if (stringHash1 != 2164589563U)
                {
                  if (stringHash1 != 2184885908U || !(upper1 == "MUTE"))
                    goto label_958;
                  else
                    goto label_898;
                }
                else if (upper1 == "RL")
                  goto label_953;
                else
                  goto label_958;
              }
              else if (upper1 == "NOCLIP")
                goto label_872;
              else
                goto label_958;
            }
            else if (stringHash1 != 2194134619U)
            {
              if (stringHash1 != 2214547930U)
              {
                if (stringHash1 != 2245226254U || !(upper1 == "FC"))
                  goto label_958;
                else
                  goto label_756;
              }
              else if (upper1 == "LOCKDOWN")
                goto label_925;
              else
                goto label_958;
            }
            else if (upper1 == "BROADCASTMONO")
              goto label_789;
            else
              goto label_958;
          }
          else if (stringHash1 <= 2331358749U)
          {
            if (stringHash1 != 2246992121U)
            {
              if (stringHash1 != 2262052351U)
              {
                if (stringHash1 == 2331358749U && upper1 == "GOD")
                {
                  if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true))
                    return;
                  if (query.Length < 2)
                  {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
                    return;
                  }
                  if (query.Length == 2)
                    query = new string[3]
                    {
                      query[0],
                      query[1],
                      ""
                    };
                  ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the god command (new status: " + (query[2] == "" ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                  int failures;
                  int successes;
                  string error;
                  bool replySent;
                  CommandProcessor.StandardizedQueryModel1(sender, "GOD", query[1], query[2], out failures, out successes, out error, out replySent, "");
                  if (replySent)
                    return;
                  if (failures == 0)
                  {
                    sender.RaReply("OVERWATCH#Done! The request affected " + (object) successes + " player(s)!", true, true, "AdminTools");
                    return;
                  }
                  sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "AdminTools");
                  return;
                }
                goto label_958;
              }
              else
              {
                if (upper1 == "UNBAN")
                {
                  if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning, "", true))
                    return;
                  string str2 = string.Empty;
                  if (query.Length > 3)
                    str2 = ((IEnumerable<string>) query).Skip<string>(3).Aggregate<string>((Func<string, string, string>) ((current, n) => current + " " + n));
                  if (query.Length < 4)
                  {
                    sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere ReasonHere OR unban ip IpAddressHere ReasonHere", false, true, "");
                    return;
                  }
                  if (str2.Contains("&"))
                    sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                  ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the unban " + query[1] + " command on " + query[2] + ".", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                  string lower = query[1].ToLower();
                  if (lower == "id" || lower == "playerid" || lower == "player")
                  {
                    BanHandler.RemoveBan(query[2], BanHandler.BanType.UserId);
                    using (WebClient webClient = new WebClient())
                    {
                      webClient.Credentials = (ICredentials) new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                      webClient.DownloadString("http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=" + query[2] + "&reason=" + str2 + "&aname=" + sender.Nickname + ("&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none")));
                      ServerConsole.AddLog("User " + query[2] + " Unbanned by RA user " + sender.Nickname);
                    }
                    sender.RaReply(query[0] + "#Done!", true, true, "");
                    return;
                  }
                  if (!(lower == "ip") && !(lower == "address"))
                  {
                    sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere OR unban ip IpAddressHere", false, true, "");
                    return;
                  }
                  BanHandler.RemoveBan(query[2], BanHandler.BanType.IP);
                  sender.RaReply(query[0] + "#Done!", true, true, "");
                  return;
                }
                goto label_958;
              }
            }
            else if (upper1 == "CLEARBC")
              goto label_796;
            else
              goto label_958;
          }
          else if (stringHash1 != 2377374125U)
          {
            if (stringHash1 != 2390379445U)
            {
              if (stringHash1 != 2409015043U || !(upper1 == "CLOSE"))
                goto label_958;
            }
            else if (upper1 == "BCMONO")
              goto label_789;
            else
              goto label_958;
          }
          else
          {
            if (upper1 == "GROUPS")
            {
              string str2 = "Groups defined on this server:";
              Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
              ServerRoles.NamedColor[] namedColors = QueryProcessor.Localplayer.GetComponent<ServerRoles>().NamedColors;
              foreach (KeyValuePair<string, UserGroup> keyValuePair in allGroups)
              {
                KeyValuePair<string, UserGroup> permentry = keyValuePair;
                try
                {
                  str2 = str2 + "\n" + permentry.Key + " (" + (object) permentry.Value.Permissions + ") - <color=#" + ((IEnumerable<ServerRoles.NamedColor>) namedColors).FirstOrDefault<ServerRoles.NamedColor>((Func<ServerRoles.NamedColor, bool>) (y => y.Name == permentry.Value.BadgeColor)).ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
                }
                catch
                {
                  str2 = str2 + "\n" + permentry.Key + " (" + (object) permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
                }
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
                  str2 += " BN1";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
                  str2 += " BN2";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
                  str2 += " BN3";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
                  str2 += " FSE";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
                  str2 += " FSP";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
                  str2 += " FWR";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
                  str2 += " GIV";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
                  str2 += " EWA";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
                  str2 += " ERE";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
                  str2 += " ERO";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
                  str2 += " SGR";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData))
                  str2 += " GMD";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch))
                  str2 += " OVR";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
                  str2 += " FCM";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement))
                  str2 += " PLM";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement))
                  str2 += " PRM";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands))
                  str2 += " SCC";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges))
                  str2 += " VHB";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs))
                  str2 += " CFG";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Broadcasting))
                  str2 += " BRC";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayerSensitiveDataAccess))
                  str2 += " CDA";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Noclip))
                  str2 += " NCP";
                if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AFKImmunity))
                  str2 += " AFK";
              }
              sender.RaReply(query[0].ToUpper() + "#" + str2, true, true, "");
              return;
            }
            goto label_958;
          }
        }
        else if (stringHash1 <= 2881770144U)
        {
          if (stringHash1 <= 2674786406U)
          {
            if (stringHash1 != 2425129245U)
            {
              if (stringHash1 != 2453427760U)
              {
                if (stringHash1 == 2674786406U && upper1 == "BAN")
                {
                  if (query.Length < 3)
                  {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                    return;
                  }
                  string str2 = string.Empty;
                  if (str2.Contains("&"))
                    sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                  if (query.Length > 3)
                    str2 = ((IEnumerable<string>) query).Skip<string>(3).Aggregate<string>((Func<string, string, string>) ((current, n) => current + " " + n));
                  if (str2 == "")
                  {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban " + query[1] + " " + query[2] + " ReasonHere", false, true, "");
                    return;
                  }
                  ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the ban command (duration: " + query[2] + " min) on " + query[1] + " players. Reason: " + (str2 == string.Empty ? "(none)" : str2) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                  int failures;
                  int successes;
                  string error;
                  bool replySent;
                  CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent, str2);
                  if (replySent)
                    return;
                  if (failures == 0)
                  {
                    string str3 = "Banned";
                    int result;
                    if (int.TryParse(query[2], out result))
                      str3 = result > 0 ? "Banned" : "Kicked";
                    sender.RaReply(query[0] + "#Done! " + str3 + " " + (object) successes + " player(s)!", true, true, "");
                    return;
                  }
                  sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "");
                  return;
                }
                goto label_958;
              }
              else if (upper1 == "STOPNEXTROUND")
                goto label_806;
              else
                goto label_958;
            }
            else
            {
              if (upper1 == "CASSIE")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
                  return;
                if (query.Length > 1)
                {
                  ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                  PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), false, true);
                  return;
                }
                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                return;
              }
              goto label_958;
            }
          }
          else if (stringHash1 != 2790969783U)
          {
            if (stringHash1 != 2796389774U)
            {
              if (stringHash1 == 2881770144U && upper1 == "INTERCOM-RESET")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
                {
                  PlayerPermissions.RoundEvents,
                  PlayerPermissions.FacilityManagement,
                  PlayerPermissions.PlayersManagement
                }, "ServerEvents", true))
                  return;
                if ((double) Intercom.host.remainingCooldown <= 0.0)
                {
                  sender.RaReply(query[0].ToUpper() + "#Intercom is already ready to use.", false, true, "ServerEvents");
                  return;
                }
                Intercom.host.remainingCooldown = -1f;
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " reset the intercom cooldown.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                sender.RaReply(query[0].ToUpper() + "#Done! Intercom cooldown reset.", true, true, "ServerEvents");
                return;
              }
              goto label_958;
            }
            else if (upper1 == "DOORS")
              goto label_950;
            else
              goto label_958;
          }
          else if (upper1 == "CASSIE_SILENT")
            goto label_784;
          else
            goto label_958;
        }
        else if (stringHash1 <= 3216141499U)
        {
          if (stringHash1 != 2943495851U)
          {
            if (stringHash1 != 3182344701U)
            {
              if (stringHash1 != 3216141499U || !(upper1 == "RCON"))
                goto label_958;
              else
                goto label_799;
            }
            else if (upper1 == "IMUTE")
              goto label_898;
            else
              goto label_958;
          }
          else
          {
            if (upper1 == "HIDETAG")
            {
              if (!CommandProcessor.IsPlayer(sender, query[0], ""))
                return;
              queryProcessor.GetComponent<ServerRoles>().HiddenBadge = queryProcessor.GetComponent<ServerRoles>().MyText;
              queryProcessor.GetComponent<ServerRoles>().NetworkGlobalBadge = (string) null;
              queryProcessor.GetComponent<ServerRoles>().SetText((string) null);
              queryProcessor.GetComponent<ServerRoles>().SetColor((string) null);
              queryProcessor.GetComponent<ServerRoles>().GlobalSet = false;
              queryProcessor.GetComponent<ServerRoles>().RefreshHiddenTag();
              sender.RaReply(query[0].ToUpper() + "#Tag hidden!", true, true, "");
              return;
            }
            goto label_958;
          }
        }
        else if (stringHash1 != 3234565675U)
        {
          if (stringHash1 != 3322673650U)
          {
            if (stringHash1 != 3373006507U || !(upper1 == "L"))
              goto label_958;
            else
              goto label_945;
          }
          else if (!(upper1 == "C"))
            goto label_958;
        }
        else
        {
          if (upper1 == "BRING")
          {
            if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools", true) || !CommandProcessor.IsPlayer(sender, query[0], "AdminTools"))
              return;
            if (query.Length != 2)
            {
              sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "AdminTools");
              return;
            }
            if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.GetComponent<CharacterClassManager>().CurClass < RoleType.Scp173)
            {
              sender.RaReply("BRING#Command disabled when you are spectator!", false, true, "AdminTools");
              return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the bring command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            int failures;
            int successes;
            string error;
            bool replySent;
            CommandProcessor.StandardizedQueryModel1(sender, "BRING", query[1], "", out failures, out successes, out error, out replySent, "");
            if (replySent)
              return;
            if (failures == 0)
            {
              sender.RaReply("BRING#Done! The request affected " + (object) successes + " player(s)!", true, true, "AdminTools");
              return;
            }
            sender.RaReply("BRING#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "AdminTools");
            return;
          }
          goto label_958;
        }
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true))
          return;
        if (query.Length != 2)
        {
          sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
          return;
        }
        CommandProcessor.ProcessDoorQuery(sender, "CLOSE", query[1]);
        return;
      }
      bool replySent2;
      if (stringHash1 <= 3715648436U)
      {
        if (stringHash1 <= 3554601228U)
        {
          if (stringHash1 <= 3440740129U)
          {
            if (stringHash1 != 3389784126U)
            {
              if (stringHash1 != 3406561745U)
              {
                if (stringHash1 != 3440740129U || !(upper1 == "SETCONFIG"))
                  goto label_958;
                else
                  goto label_816;
              }
              else if (upper1 == "N")
                goto label_872;
              else
                goto label_958;
            }
            else if (upper1 == "O")
              goto label_940;
            else
              goto label_958;
          }
          else if (stringHash1 != 3494757443U)
          {
            if (stringHash1 != 3510393926U)
            {
              if (stringHash1 != 3554601228U || !(upper1 == "FORCECLASS"))
                goto label_958;
              else
                goto label_756;
            }
            else if (upper1 == "OVERWATCH")
              goto label_887;
            else
              goto label_958;
          }
          else
          {
            if (upper1 == "CONTACT")
            {
              sender.RaReply(query[0].ToUpper() + "#Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", ""), false, true, "");
              return;
            }
            goto label_958;
          }
        }
        else if (stringHash1 <= 3611046620U)
        {
          if (stringHash1 != 3555373166U)
          {
            if (stringHash1 != 3556468092U)
            {
              if (stringHash1 == 3611046620U && upper1 == "GIVE")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GivingItems, "", true))
                  return;
                if (query.Length < 3)
                {
                  sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                  return;
                }
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the give command (ID: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                int failures;
                int successes;
                string error;
                bool replySent1;
                CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent1, "");
                if (replySent1)
                  return;
                if (failures == 0)
                {
                  sender.RaReply(query[0] + "#Done! The request affected " + (object) successes + " player(s)!", true, true, "");
                  return;
                }
                sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures + "\nLast error log:\n" + error, false, true, "");
                return;
              }
              goto label_958;
            }
            else if (upper1 == "LOBBYLOCK")
              goto label_956;
            else
              goto label_958;
          }
          else if (!(upper1 == "GLOBALTAG"))
            goto label_958;
        }
        else if (stringHash1 != 3612018453U)
        {
          if (stringHash1 != 3709327981U)
          {
            if (stringHash1 != 3715648436U || !(upper1 == "BROADCAST"))
              goto label_958;
            else
              goto label_789;
          }
          else if (upper1 == "BYPASS")
            goto label_914;
          else
            goto label_958;
        }
        else
        {
          if (upper1 == "SERVER_EVENT")
          {
            if (query.Length < 2)
            {
              sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
              return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " forced a server event: " + query[1].ToUpper(), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            GameObject gameObject = GameObject.Find("Host");
            MTFRespawn component1 = gameObject.GetComponent<MTFRespawn>();
            AlphaWarheadController component2 = gameObject.GetComponent<AlphaWarheadController>();
            bool flag = true;
            string upper2 = query[1].ToUpper();
            uint stringHash2 = PrivateImplementationDetails.ComputeStringHash(upper2);
            if (stringHash2 <= 893689630U)
            {
              if (stringHash2 <= 687634301U)
              {
                if (stringHash2 != 368174935U)
                {
                  if (stringHash2 == 687634301U && upper2 == "FORCE_MTF_RESPAWN")
                  {
                    if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents", true))
                      return;
                    component1.nextWaveIsCI = false;
                    component1.timeToNextRespawn = 0.1f;
                    goto label_631;
                  }
                  else
                    goto label_630;
                }
                else if (!(upper2 == "ROUND_RESTART"))
                  goto label_630;
              }
              else if (stringHash2 != 734536883U)
              {
                if (stringHash2 != 892818482U)
                {
                  if (stringHash2 == 893689630U && upper2 == "FORCE_CI_RESPAWN")
                  {
                    if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents", true))
                      return;
                    component1.nextWaveIsCI = true;
                    component1.timeToNextRespawn = 0.1f;
                    goto label_631;
                  }
                  else
                    goto label_630;
                }
                else if (!(upper2 == "ROUNDRESTART"))
                  goto label_630;
              }
              else if (upper2 == "DETONATION_CANCEL")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true))
                  return;
                component2.CancelDetonation();
                goto label_631;
              }
              else
                goto label_630;
            }
            else if (stringHash2 <= 1369092108U)
            {
              if (stringHash2 != 1240338700U)
              {
                if (stringHash2 != 1369092108U || !(upper2 == "RESTART"))
                  goto label_630;
              }
              else if (upper2 == "TERMINATE_UNCONN")
              {
                if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true))
                  return;
                using (Dictionary<int, NetworkConnection>.ValueCollection.Enumerator enumerator = NetworkServer.connections.Values.GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    NetworkConnection current = enumerator.Current;
                    if ((UnityEngine.Object) GameCore.Console.FindConnectedRoot(current) == (UnityEngine.Object) null)
                    {
                      current.Disconnect();
                      current.Dispose();
                    }
                  }
                  goto label_631;
                }
              }
              else
                goto label_630;
            }
            else if (stringHash2 != 1615199367U)
            {
              if (stringHash2 != 1862592421U)
              {
                if (stringHash2 == 1877204362U && upper2 == "DETONATION_INSTANT")
                {
                  if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true))
                    return;
                  component2.InstantPrepare();
                  component2.StartDetonation();
                  component2.NetworktimeToDetonation = 5f;
                  goto label_631;
                }
                else
                  goto label_630;
              }
              else if (!(upper2 == "RR"))
                goto label_630;
            }
            else if (upper2 == "DETONATION_START")
            {
              if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true))
                return;
              component2.InstantPrepare();
              component2.StartDetonation();
              goto label_631;
            }
            else
              goto label_630;
            if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true))
              return;
            PlayerStats component3 = PlayerManager.localPlayer.GetComponent<PlayerStats>();
            if (component3.isServer)
            {
              component3.Roundrestart();
              goto label_631;
            }
            else
              goto label_631;
label_630:
            flag = false;
label_631:
            if (flag)
            {
              sender.RaReply(query[0].ToUpper() + "#Started event: " + query[1].ToUpper(), true, true, "ServerEvents");
              return;
            }
            sender.RaReply(query[0].ToUpper() + "#Incorrect event! (Doesn't exist)", false, true, "ServerEvents");
            return;
          }
          goto label_958;
        }
      }
      else if (stringHash1 <= 3878768297U)
      {
        if (stringHash1 <= 3754455428U)
        {
          if (stringHash1 != 3730152828U)
          {
            if (stringHash1 != 3739414038U)
            {
              if (stringHash1 != 3754455428U || !(upper1 == "LLOCK"))
                goto label_958;
              else
                goto label_956;
            }
            else if (!(upper1 == "GTAG"))
              goto label_958;
          }
          else
          {
            if (upper1 == "GBAN-KICK")
            {
              if ((playerCommandSender == null || !playerCommandSender.SR.RaEverywhere && !playerCommandSender.SR.Staff) && CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.KickingAndShortTermBanning, "", true) && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement, "", true))
                return;
              if (query.Length != 2)
              {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 1 argument! (some parameters are missing)", false, true, "");
                return;
              }
              ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " globally banned and kicked " + query[1] + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
              CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], "0", out int _, out int _, out string _, out replySent2, "");
              return;
            }
            goto label_958;
          }
        }
        else if (stringHash1 != 3828307698U)
        {
          if (stringHash1 != 3856939955U)
          {
            if (stringHash1 != 3878768297U || !(upper1 == "ICOM"))
              goto label_958;
            else
              goto label_907;
          }
          else
          {
            if (upper1 == "OBAN")
            {
              if (query.Length < 4)
              {
                sender.RaReply(query[0].ToUpper() + "#OBAN [NAME] [IP/SteamID/DiscordID] [MINUTES] (OPTIONAL REASON)\nOBAN [NAME] [MINUTES] [REASON]", false, true, "");
                return;
              }
              string issuer = sender == null || string.IsNullOrEmpty(sender.SenderId) ? "Server" : sender.Nickname;
              string str2 = query[1];
              string s1 = query[2];
              int result1;
              int num = int.TryParse(query[3], out result1) ? result1 : -1;
              if (num == -1)
              {
                string reason = query.Length > 3 ? string.Join(" ", ((IEnumerable<string>) query).Skip<string>(3)) : string.Empty;
                int result2;
                int duration = int.TryParse(s1, out result2) ? result2 : 0;
                foreach (GameObject player in PlayerManager.players)
                {
                  if (player.GetComponent<NicknameSync>().MyNick.Contains(str2, StringComparison.OrdinalIgnoreCase))
                  {
                    PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(player, duration, reason, issuer);
                    string str3 = "\nPlayer with name: " + str2 + "\nWas banned for: " + (object) duration + " minutes \nBy: " + issuer;
                    sender.RaReply(query[0].ToUpper() + "#" + str3, true, true, "");
                    return;
                  }
                }
                sender.RaReply(query[0].ToUpper() + "#Jugador con ese nombre no encontrado lol", false, true, "");
                return;
              }
              string str4 = query.Length > 4 ? string.Join(" ", ((IEnumerable<string>) query).Skip<string>(4)) : string.Empty;
              if (num < 0)
              {
                sender.RaReply(query[0].ToUpper() + "#Creo que eres autista y no sabes leer. Pon bien los minutos", false, true, "");
                return;
              }
              string s2 = s1.Trim();
              bool flag = long.TryParse(s2, out long _);
              if (s1.Contains("."))
              {
                if (s1.Split('.').Length != 4)
                {
                  sender.RaReply("Invalid IP: " + s1, false, true, "");
                  return;
                }
                string str3 = s1.Contains("::ffff:") ? s1 : "::ffff:" + s1;
                BanHandler.IssueBan(new BanDetails()
                {
                  OriginalName = str2,
                  Id = s1,
                  IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                  Expires = DateTime.UtcNow.AddMinutes((double) num).Ticks,
                  Reason = str4,
                  Issuer = issuer
                }, BanHandler.BanType.IP);
                string str5 = "\nPlayer with name: " + str2 + "\nIP: " + str3 + "\nWas banned for: " + (object) num + " minutes \nBy: " + issuer;
                sender.RaReply(query[0].ToUpper() + "#" + str5, true, true, "");
                return;
              }
              if (s2.Length == 17 & flag)
              {
                BanHandler.IssueBan(new BanDetails()
                {
                  OriginalName = str2,
                  Id = s2 + "@steam",
                  IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                  Expires = DateTime.UtcNow.AddMinutes((double) num).Ticks,
                  Reason = str4,
                  Issuer = issuer
                }, BanHandler.BanType.UserId);
                string str3 = "\nPlayer with name: " + str2 + "\nSteamID64: " + s2 + "\nWas banned for: " + (object) num + " minutes \nBy: " + issuer;
                sender.RaReply(query[0].ToUpper() + "#" + str3, true, true, "");
                return;
              }
              if (s2.Length == 18 & flag)
              {
                BanHandler.IssueBan(new BanDetails()
                {
                  OriginalName = str2,
                  Id = s2 + "@discord",
                  IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                  Expires = DateTime.UtcNow.AddMinutes((double) num).Ticks,
                  Reason = str4,
                  Issuer = issuer
                }, BanHandler.BanType.UserId);
                string str3 = "\nPlayer with name: " + str2 + "\nDiscordID: " + s2 + "\nWas banned for: " + (object) num + " minutes \nBy: " + issuer;
                sender.RaReply(query[0].ToUpper() + "#" + str3, true, true, "");
                return;
              }
              if (s2.Contains("@discord") || s2.Contains("@steam") || s2.Contains("@northwood"))
              {
                BanHandler.IssueBan(new BanDetails()
                {
                  OriginalName = str2,
                  Id = s2,
                  IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                  Expires = DateTime.UtcNow.AddMinutes((double) num).Ticks,
                  Reason = str4,
                  Issuer = issuer
                }, BanHandler.BanType.UserId);
                string str3 = "\nPlayer with name: " + str2 + "\nUserID: " + s2 + "\nWas banned for: " + (object) num + " minutes \nBy: " + issuer;
                sender.RaReply(query[0].ToUpper() + "#" + str3, true, true, "");
                return;
              }
              sender.RaReply(query[0].ToUpper() + "#Invalid ID. The fuck you wrote, lad.", false, true, "");
              return;
            }
            goto label_958;
          }
        }
        else if (upper1 == "RLOCK")
          goto label_953;
        else
          goto label_958;
      }
      else if (stringHash1 <= 4024269157U)
      {
        if (stringHash1 != 3974983732U)
        {
          if (stringHash1 != 4000555060U)
          {
            if (stringHash1 == 4024269157U && upper1 == "DESTROY")
            {
              if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true))
                return;
              if (query.Length != 2)
              {
                sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                return;
              }
              CommandProcessor.ProcessDoorQuery(sender, "DESTROY", query[1]);
              return;
            }
            goto label_958;
          }
          else if (upper1 == "SLML_TAG")
            goto label_809;
          else
            goto label_958;
        }
        else
        {
          if (upper1 == "SHOWTAG")
          {
            if (!CommandProcessor.IsPlayer(sender, query[0], "") || (UnityEngine.Object) queryProcessor == (UnityEngine.Object) null)
              return;
            queryProcessor.Roles.HiddenBadge = (string) null;
            queryProcessor.Roles.RpcResetFixed();
            queryProcessor.Roles.RefreshPermissions(true);
            sender.RaReply(query[0].ToUpper() + "#Local tag refreshed!", true, true, "");
            return;
          }
          goto label_958;
        }
      }
      else if (stringHash1 != 4043923228U)
      {
        if (stringHash1 != 4076134438U)
        {
          if (stringHash1 == 4209683619U && upper1 == "WARHEAD")
          {
            if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "", true))
              return;
            if (query.Length == 1)
            {
              sender.RaReply("Syntax: warhead (status|detonate|instant|cancel|enable|disable)", false, true, string.Empty);
              return;
            }
            string lower = query[1].ToLower();
            if (!(lower == "status"))
            {
              if (lower == "detonate")
              {
                AlphaWarheadController.Host.StartDetonation();
                sender.RaReply("Detonation sequence started.", true, true, string.Empty);
                return;
              }
              if (lower == "instant")
              {
                AlphaWarheadController.Host.InstantPrepare();
                AlphaWarheadController.Host.StartDetonation();
                AlphaWarheadController.Host.NetworktimeToDetonation = 5f;
                sender.RaReply("Instant detonation started.", true, true, string.Empty);
                return;
              }
              if (lower == "cancel")
              {
                AlphaWarheadController.Host.CancelDetonation((GameObject) null);
                sender.RaReply("Detonation has been canceled.", true, true, string.Empty);
                return;
              }
              if (lower == "enable")
              {
                AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
                sender.RaReply("Warhead has been enabled.", true, true, string.Empty);
                return;
              }
              if (!(lower == "disable"))
              {
                sender.RaReply("WARHEAD: Unknown subcommand.", false, true, string.Empty);
                return;
              }
              AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
              sender.RaReply("Warhead has been disabled.", true, true, string.Empty);
              return;
            }
            if (AlphaWarheadController.Host.detonated || (double) Math.Abs(AlphaWarheadController.Host.timeToDetonation) < 1.0 / 1000.0)
            {
              sender.RaReply("Warhead has been detonated.", true, true, string.Empty);
              return;
            }
            if (AlphaWarheadController.Host.inProgress)
            {
              sender.RaReply("Detonation is in progress.", true, true, string.Empty);
              return;
            }
            if (!AlphaWarheadOutsitePanel.nukeside.enabled)
            {
              sender.RaReply("Warhead is disabled.", true, true, string.Empty);
              return;
            }
            if ((double) AlphaWarheadController.Host.timeToDetonation > (double) AlphaWarheadController.Host.RealDetonationTime())
            {
              sender.RaReply("Warhead is restarting.", true, true, string.Empty);
              return;
            }
            sender.RaReply("Warhead is ready to detonation.", true, true, string.Empty);
            return;
          }
          goto label_958;
        }
        else
        {
          if (upper1 == "GOTO")
          {
            if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools", true) || !CommandProcessor.IsPlayer(sender, query[0], "AdminTools"))
              return;
            if (query.Length != 2)
            {
              sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "AdminTools");
              return;
            }
            if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.CurClass < RoleType.Scp173)
            {
              sender.RaReply("GOTO#Command disabled when you are spectator!", false, true, "AdminTools");
              return;
            }
            int id;
            if (!int.TryParse(query[1], out id))
            {
              sender.RaReply("GOTO#Player ID must be an integer.", false, true, "AdminTools");
              return;
            }
            if (query[1].Contains("."))
            {
              sender.RaReply("GOTO#Goto command requires exact one selected player.", false, true, "AdminTools");
              return;
            }
            GameObject gameObject = PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (pl => pl.GetComponent<QueryProcessor>().PlayerId == id));
            if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
            {
              sender.RaReply("GOTO#Can't find requested player.", false, true, "AdminTools");
              return;
            }
            if (gameObject.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || gameObject.GetComponent<CharacterClassManager>().CurClass < RoleType.None)
            {
              sender.RaReply("GOTO#Requested player is a spectator!", false, true, "AdminTools");
              return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the goto command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            queryProcessor.GetComponent<PlyMovementSync>().OverridePosition(gameObject.GetComponent<PlyMovementSync>().RealModelPosition, 0.0f, false);
            sender.RaReply("GOTO#Done!", true, true, "AdminTools");
            return;
          }
          goto label_958;
        }
      }
      else
      {
        if (upper1 == "PBC")
        {
          if (query.Length < 4)
          {
            sender.RaReply(query[0].ToUpper() + "#Usage: PBC <PLAYER> <TIME> <MESSAGE>", false, true, "");
            return;
          }
          if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
          {
            sender.RaReply(query[0].ToUpper() + "#No perms to PBC bro.", false, true, "");
            return;
          }
          uint result;
          if (!uint.TryParse(query[2], out result) || result < 1U)
          {
            sender.RaReply(query[0].ToUpper() + "#Argument after the name must be a positive integer.", false, true, "");
            return;
          }
          string data = query[3];
          for (int index = 4; index < query.Length; ++index)
            data = data + " " + query[index];
          foreach (GameObject player in PlayerManager.players)
          {
            if (player.GetComponent<NicknameSync>().MyNick.Contains(query[1], StringComparison.OrdinalIgnoreCase))
            {
              NetworkConnection connectionToClient = player.GetComponent<NetworkIdentity>().connectionToClient;
              if (connectionToClient != null)
              {
                QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(connectionToClient, data, result, false);
                sender.RaReply(query[0].ToUpper() + "#Sent: " + data + " to: " + player.GetComponent<NicknameSync>().MyNick, true, true, "");
                ServerLogs.AddLog(ServerLogs.Modules.DataAccess, "Broadcasted: " + data + " to: " + player.GetComponent<NicknameSync>().MyNick + " by " + sender.Nickname, ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
              }
            }
          }
          sender.RaReply(query[0].ToUpper() + "#PBC command sent.", true, true, "");
          return;
        }
        goto label_958;
      }
      if (!CommandProcessor.IsPlayer(sender, query[0], "ServerEvents") || (UnityEngine.Object) queryProcessor == (UnityEngine.Object) null)
        return;
      if (string.IsNullOrEmpty(queryProcessor.Roles.PrevBadge))
      {
        sender.RaReply(query[0].ToUpper() + "#You don't have global tag.", false, true, "");
        return;
      }
      queryProcessor.Roles.HiddenBadge = (string) null;
      queryProcessor.Roles.RpcResetFixed();
      queryProcessor.Roles.NetworkGlobalBadge = queryProcessor.Roles.PrevBadge;
      queryProcessor.Roles.GlobalSet = true;
      sender.RaReply(query[0].ToUpper() + "#Global tag refreshed!", true, true, "");
      return;
label_756:
      if (query.Length < 3)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
        return;
      }
      int result3 = 0;
      if (!int.TryParse(query[2], out result3) || result3 < 0 || result3 >= QueryProcessor.LocalCCM.Classes.Length)
      {
        sender.RaReply(query[0].ToUpper() + "#Invalid class ID.", false, true, "");
        return;
      }
      string fullName = QueryProcessor.LocalCCM.Classes.SafeGet(result3).fullName;
      GameObject gameObject1 = GameObject.Find("Host");
      if ((UnityEngine.Object) gameObject1 == (UnityEngine.Object) null)
      {
        sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", false, true, "");
        return;
      }
      CharacterClassManager component = gameObject1.GetComponent<CharacterClassManager>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null || !component.isLocalPlayer || !component.isServer || !component.RoundStarted)
      {
        sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", false, true, "");
        return;
      }
      bool flag4 = sender is PlayerCommandSender playerCommandSender && (query[1] == playerCommandSender.PlayerId.ToString() || query[1] == playerCommandSender.PlayerId.ToString() + ".");
      bool flag5 = result3 == 2;
      if (flag4 & flag5)
      {
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
        {
          PlayerPermissions.ForceclassWithoutRestrictions,
          PlayerPermissions.ForceclassToSpectator,
          PlayerPermissions.ForceclassSelf
        }, "", true))
          goto label_773;
      }
      if (flag4 && !flag5)
      {
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
        {
          PlayerPermissions.ForceclassWithoutRestrictions,
          PlayerPermissions.ForceclassSelf
        }, "", true))
          goto label_773;
      }
      if (!flag4 & flag5)
      {
        if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
        {
          PlayerPermissions.ForceclassWithoutRestrictions,
          PlayerPermissions.ForceclassToSpectator
        }, "", true))
          goto label_773;
      }
      int num3;
      if (!flag4 && !flag5)
      {
        num3 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[1]
        {
          PlayerPermissions.ForceclassWithoutRestrictions
        }, "", true) ? 1 : 0;
        goto label_774;
      }
      else
      {
        num3 = 0;
        goto label_774;
      }
label_773:
      num3 = 1;
label_774:
      if (num3 != 0)
        return;
      if (string.Equals(query[0], "role", StringComparison.OrdinalIgnoreCase))
        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, str1 + " ran the role command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      else
        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, str1 + " ran the forceclass command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      int failures2;
      int successes2;
      string error2;
      bool replySent3;
      CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures2, out successes2, out error2, out replySent3, "");
      if (replySent3)
        return;
      if (failures2 == 0)
      {
        sender.RaReply(query[0] + "#Done! The request affected " + (object) successes2 + " player(s)!", true, true, "");
        return;
      }
      sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures2 + "\nLast error log:\n" + error2, false, true, "");
      return;
label_784:
      if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
        return;
      if (query.Length > 1)
      {
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), false, false);
        return;
      }
      sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
      return;
label_789:
      if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
        return;
      if (query.Length < 2)
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
      uint result4 = 0;
      if (!uint.TryParse(query[1], out result4) || result4 < 1U)
        sender.RaReply(query[0].ToUpper() + "#First argument must be a positive integer.", false, true, "");
      string data1 = q.Substring(query[0].Length + query[1].Length + 2);
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the broadcast command (duration: " + query[1] + " seconds) with text \"" + data1 + "\" players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(data1, result4, query[0].Contains("mono", StringComparison.OrdinalIgnoreCase));
      sender.RaReply(query[0].ToUpper() + "#Broadcast sent.", false, true, "");
      return;
label_796:
      if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
        return;
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcClearElements();
      sender.RaReply(query[0].ToUpper() + "#All broadcasts cleared.", false, true, "");
      return;
label_799:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands, "", true))
        return;
      if (query.Length < 2)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 1 argument! (some parameters are missing)", false, true, "");
        return;
      }
      if (query[1].StartsWith("!") && !ServerStatic.RolesConfig.GetBool("allow_central_server_commands_as_ServerConsoleCommands", false))
      {
        sender.RaReply(query[0] + "#Running central server commands in Remote Admin is disabled in RA config file!", false, true, "");
        return;
      }
      string str6 = ((IEnumerable<string>) query).Skip<string>(1).Aggregate<string, string>("", (Func<string, string, string>) ((current, arg) => current + arg + " "));
      string cmds = str6.Substring(0, str6.Length - 1);
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " executed command as server console: " + cmds + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      ServerConsole.EnterCommand(cmds, sender);
      sender.RaReply(query[0] + "#Command \"" + cmds + "\" executed in server console!", true, true, "");
      return;
label_806:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands, "", true))
        return;
      ServerStatic.StopNextRound = !ServerStatic.StopNextRound;
      sender.RaReply(query[0] + "#Server " + (ServerStatic.StopNextRound ? "WILL" : "WON'T") + " stop after next round.", true, true, "");
      return;
label_809:
      if (query.Length < 3)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
        return;
      }
      ServerLogs.AddLog(ServerLogs.Modules.Logger, str1 + " Requested a download of " + query[2] + " on " + query[1] + " players' computers.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
      int failures3;
      int successes3;
      string error3;
      bool replySent4;
      CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures3, out successes3, out error3, out replySent4, "");
      if (replySent4)
        return;
      if (failures3 == 0)
      {
        sender.RaReply(query[0] + "#Done! " + (object) successes3 + " player(s) affected!", true, true, "");
        return;
      }
      sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + (object) failures3 + "\nLast error log:\n" + error3, false, true, "");
      return;
label_816:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs, "ServerConfigs", true))
        return;
      if (query.Length >= 3)
      {
        if (query.Length > 3)
        {
          string str2 = query[2];
          for (int index = 3; index < query.Length; ++index)
            str2 = str2 + " " + query[index];
          query = new string[3]{ query[0], query[1], str2 };
        }
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the setconfig command (" + query[1] + ": " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        string upper2 = query[1].ToUpper();
        uint stringHash2 = PrivateImplementationDetails.ComputeStringHash(upper2);
        if (stringHash2 <= 1283525746U)
        {
          if (stringHash2 != 1107074724U)
          {
            if (stringHash2 != 1119736759U)
            {
              if (stringHash2 == 1283525746U && upper2 == "PD_REFRESH_EXIT")
              {
                bool result1;
                if (bool.TryParse(query[2], out result1))
                {
                  UnityEngine.Object.FindObjectOfType<PocketDimensionTeleport>().RefreshExit = result1;
                  sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result1.ToString() + "]!", true, true, "ServerConfigs");
                  return;
                }
                sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", false, true, "ServerConfigs");
                return;
              }
            }
            else if (upper2 == "HUMAN_GRENADE_MULTIPLIER")
            {
              float result1;
              if (float.TryParse(query[2], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1))
              {
                ConfigFile.ServerConfig.SetString("human_grenade_multiplier", result1.ToString());
                sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + (object) result1 + "]!", true, true, "ServerConfigs");
                return;
              }
              sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", false, true, "ServerConfigs");
              return;
            }
          }
          else if (upper2 == "PLAYER_LIST_TITLE")
          {
            string str2 = query[2] ?? string.Empty;
            PlayerList.Title.Value = str2;
            try
            {
              PlayerList.singleton.RefreshTitle();
            }
            catch (Exception ex)
            {
              if (!(ex is CommandInputException) && !(ex is InvalidOperationException))
              {
                throw;
              }
              else
              {
                sender.RaReply(query[0].ToUpper() + "#Could not set player list title [" + str2 + "]:\n" + ex.Message, false, true, "ServerConfigs");
                return;
              }
            }
            sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + PlayerList.singleton.syncServerName + "]!", true, true, "ServerConfigs");
            return;
          }
        }
        else if (stringHash2 <= 3161611648U)
        {
          if (stringHash2 != 1585466007U)
          {
            if (stringHash2 == 3161611648U && upper2 == "SPAWN_PROTECT_TIME")
            {
              int result1;
              if (int.TryParse(query[2], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1))
              {
                foreach (CharacterClassManager characterClassManager in UnityEngine.Object.FindObjectsOfType<CharacterClassManager>())
                  characterClassManager.SProtectedDuration = (float) result1;
                sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + (object) result1 + "]!", true, true, "ServerConfigs");
                return;
              }
              sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid integer!", false, true, "ServerConfigs");
              return;
            }
          }
          else if (upper2 == "SPAWN_PROTECT_DISABLE")
          {
            bool result1;
            if (bool.TryParse(query[2], out result1))
            {
              foreach (CharacterClassManager characterClassManager in UnityEngine.Object.FindObjectsOfType<CharacterClassManager>())
                characterClassManager.EnableSP = !result1;
              sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result1.ToString() + "]!", true, true, "ServerConfigs");
              return;
            }
            sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", false, true, "ServerConfigs");
            return;
          }
        }
        else if (stringHash2 != 3269110662U)
        {
          if (stringHash2 == 4162371295U && upper2 == "FRIENDLY_FIRE")
          {
            bool result1;
            if (bool.TryParse(query[2], out result1))
            {
              ServerConsole.FriendlyFire = result1;
              foreach (WeaponManager weaponManager in UnityEngine.Object.FindObjectsOfType<WeaponManager>())
                weaponManager.NetworkfriendlyFire = result1;
              sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result1.ToString() + "]!", true, true, "ServerConfigs");
              return;
            }
            sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", false, true, "ServerConfigs");
            return;
          }
        }
        else if (upper2 == "SCP_GRENADE_MULTIPLIER")
        {
          float result1;
          if (float.TryParse(query[2], NumberStyles.Any, (IFormatProvider) CultureInfo.InvariantCulture, out result1))
          {
            ConfigFile.ServerConfig.SetString("scp_grenade_multiplier", result1.ToString());
            sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + (object) result1 + "]!", true, true, "ServerConfigs");
            return;
          }
          sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", false, true, "ServerConfigs");
          return;
        }
        sender.RaReply(query[0].ToUpper() + "#Invalid config " + query[1], false, true, "ServerConfigs");
        return;
      }
      sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "ServerConfigs");
      return;
label_872:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Noclip, "", true))
        return;
      if (query.Length >= 2)
      {
        if (query.Length == 2)
          query = new string[3]{ query[0], query[1], "" };
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the noclip command (new status: " + (query[2] == "" ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        int failures1;
        int successes1;
        string error1;
        bool replySent1;
        CommandProcessor.StandardizedQueryModel1(sender, "NOCLIP", query[1], query[2], out failures1, out successes1, out error1, out replySent1, "");
        if (replySent1)
          return;
        if (failures1 == 0)
        {
          sender.RaReply("NOCLIP#Done! The request affected " + (object) successes1 + " player(s)!", true, true, "AdminTools");
          return;
        }
        sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + (object) failures1 + "\nLast error log:\n" + error1, false, true, "AdminTools");
        return;
      }
      if (!(sender is PlayerCommandSender playerCommandSender))
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
        return;
      }
      int failures4;
      int successes4;
      string error4;
      CommandProcessor.StandardizedQueryModel1(sender, "NOCLIP", playerCommandSender.PlayerId.ToString(), "", out failures4, out successes4, out error4, out replySent2, "");
      if (failures4 == 0)
      {
        sender.RaReply("NOCLIP#Done! The request affected " + (object) successes4 + " player(s)!", true, true, "AdminTools");
        return;
      }
      sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + (object) failures4 + "\nLast error log:\n" + error4, false, true, "AdminTools");
      return;
label_887:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Overwatch, "", true))
        return;
      if (query.Length < 2)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
        return;
      }
      if (query.Length == 2)
        query = new string[3]{ query[0], query[1], "" };
      ServerLogs.AddLog(ServerLogs.Modules.ClassChange, str1 + " ran the overwatch command (new status: " + (query[2] == "" ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      int failures5;
      int successes5;
      string error5;
      bool replySent5;
      CommandProcessor.StandardizedQueryModel1(sender, "OVERWATCH", query[1], query[2], out failures5, out successes5, out error5, out replySent5, "");
      if (replySent5)
        return;
      if (failures5 == 0)
      {
        sender.RaReply("OVERWATCH#Done! The request affected " + (object) successes5 + " player(s)!", true, true, "AdminTools");
        return;
      }
      sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + (object) failures5 + "\nLast error log:\n" + error5, false, true, "AdminTools");
      return;
label_898:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
      {
        PlayerPermissions.BanningUpToDay,
        PlayerPermissions.LongTermBanning,
        PlayerPermissions.PlayersManagement
      }, "", true))
        return;
      if (query.Length != 2)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "PlayersManagement");
        return;
      }
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the " + query[0].ToLower() + " command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
      int failures6;
      int successes6;
      string error6;
      bool replySent6;
      CommandProcessor.StandardizedQueryModel1(sender, query[0].ToUpper(), query[1], (string) null, out failures6, out successes6, out error6, out replySent6, "");
      if (replySent6)
        return;
      if (failures6 == 0)
      {
        sender.RaReply(query[0].ToUpper() + "#Done! The request affected " + (object) successes6 + " player(s)!", true, true, "PlayersManagement");
        return;
      }
      sender.RaReply(query[0].ToUpper() + "#The proccess has occured an issue! Failures: " + (object) failures6 + "\nLast error log:\n" + error6, false, true, "PlayersManagement");
      return;
label_907:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Broadcasting, "ServerEvents", true) || !CommandProcessor.IsPlayer(sender, query[0], "ServerEvents"))
        return;
      if (Intercom.AdminSpeaking)
      {
        Intercom.AdminSpeaking = false;
        Intercom.host.RequestTransmission((GameObject) null);
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ended global intercom transmission.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
        sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom revoked.", true, true, "ServerEvents");
        return;
      }
      if (Intercom.host.speaking)
      {
        sender.RaReply(query[0].ToUpper() + "#Intercom is being used by someone else.", false, true, "ServerEvents");
        return;
      }
      Intercom.AdminSpeaking = true;
      Intercom.host.RequestTransmission(queryProcessor.GetComponent<Intercom>().gameObject);
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " requested global voice over the intercom.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
      sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom granted.", true, true, "ServerEvents");
      return;
label_914:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true))
        return;
      if (query.Length < 2)
      {
        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
        return;
      }
      if (query.Length == 2)
        query = new string[3]{ query[0], query[1], "" };
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " ran the bypass mode command (new status: " + (query[2] == "" ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      int failures7;
      int successes7;
      string error7;
      bool replySent7;
      CommandProcessor.StandardizedQueryModel1(sender, "BYPASS", query[1], query[2], out failures7, out successes7, out error7, out replySent7, "");
      if (replySent7)
        return;
      if (failures7 == 0)
      {
        sender.RaReply("BYPASS#Done! The request affected " + (object) successes7 + " player(s)!", true, true, "AdminTools");
        return;
      }
      sender.RaReply("BYPASS#The proccess has occured an issue! Failures: " + (object) failures7 + "\nLast error log:\n" + error7, false, true, "AdminTools");
      return;
label_925:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true))
        return;
      if (!QueryProcessor.Lockdown)
      {
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " enabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
        foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
        {
          if (!door.locked)
          {
            door.lockdown = true;
            door.UpdateLock();
          }
        }
        QueryProcessor.Lockdown = true;
        sender.RaReply(query[0] + "#Lockdown enabled!", true, true, "AdminTools");
        return;
      }
      ServerLogs.AddLog(ServerLogs.Modules.Administrative, str1 + " disabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
      {
        if (door.lockdown)
        {
          door.lockdown = false;
          door.UpdateLock();
        }
      }
      QueryProcessor.Lockdown = false;
      sender.RaReply(query[0] + "#Lockdown disabled!", true, true, "AdminTools");
      return;
label_940:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true))
        return;
      if (query.Length != 2)
      {
        sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
        return;
      }
      CommandProcessor.ProcessDoorQuery(sender, "OPEN", query[1]);
      return;
label_945:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true))
        return;
      if (query.Length != 2)
      {
        sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
        return;
      }
      CommandProcessor.ProcessDoorQuery(sender, "LOCK", query[1]);
      return;
label_950:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true))
        return;
      string str7 = "List of named doors in the facility:\n";
      List<string> list = ((IEnumerable<Door>) UnityEngine.Object.FindObjectsOfType<Door>()).Where<Door>((Func<Door, bool>) (item => !string.IsNullOrEmpty(item.DoorName))).Select<Door, string>((Func<Door, string>) (item => item.DoorName + " - " + (item.isOpen ? "<color=green>OPENED</color>" : "<color=orange>CLOSED</color>") + (item.locked ? " <color=red>[LOCKED]</color>" : "") + (string.IsNullOrEmpty(item.permissionLevel) ? "" : " <color=blue>[CARD REQUIRED]</color>"))).ToList<string>();
      list.Sort();
      string str8 = str7 + list.Aggregate<string>((Func<string, string, string>) ((current, adding) => current + "\n" + adding));
      sender.RaReply(query[0] + "#" + str8, true, true, "");
      return;
label_953:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "", true))
        return;
      RoundSummary.RoundLock = !RoundSummary.RoundLock;
      sender.RaReply(query[0].ToUpper() + "#Round lock " + (RoundSummary.RoundLock ? "enabled!" : "disabled!"), true, true, "ServerEvents");
      return;
label_956:
      if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "", true))
        return;
      RoundStart.LobbyLock = !RoundStart.LobbyLock;
      sender.RaReply(query[0].ToUpper() + "#Lobby lock " + (RoundStart.LobbyLock ? "enabled!" : "disabled!"), true, true, "ServerEvents");
      return;
label_958:
      sender.RaReply("SYSTEM#Unknown command!", false, true, "");
    }

    private static void ProcessDoorQuery(CommandSender sender, string command, string door)
    {
      if (!CommandProcessor.CheckPermissions(sender, command.ToUpper(), PlayerPermissions.FacilityManagement, "", true))
        return;
      if (string.IsNullOrEmpty(door))
      {
        sender.RaReply(command + "#Please select door first.", false, true, "DoorsManagement");
      }
      else
      {
        bool flag = false;
        door = door.ToUpper();
        byte num1 = command == "OPEN" ? (byte) 1 : (command == "LOCK" ? (byte) 2 : (command == "UNLOCK" ? (byte) 3 : (command == "DESTROY" ? (byte) 4 : (byte) 0)));
        foreach (Door door1 in UnityEngine.Object.FindObjectsOfType<Door>())
        {
          if (!(door1.DoorName.ToUpper() != door) || !(door != "**") && !(door1.permissionLevel == "UNACCESSIBLE") || !(door != "!*") && string.IsNullOrEmpty(door1.DoorName) || !(door != "*") && !string.IsNullOrEmpty(door1.DoorName) && !(door1.permissionLevel == "UNACCESSIBLE"))
          {
            switch (num1)
            {
              case 0:
                door1.SetStateWithSound(false);
                break;
              case 1:
                door1.SetStateWithSound(true);
                break;
              case 2:
                door1.commandlock = true;
                door1.UpdateLock();
                break;
              case 3:
                door1.commandlock = false;
                door1.UpdateLock();
                break;
              case 4:
                door1.DestroyDoor(true);
                break;
            }
            flag = true;
          }
        }
        CommandSender commandSender = sender;
        string str1 = command;
        string str2;
        if (!flag)
          str2 = "Can't find door " + door + ".";
        else
          str2 = "Door " + door + " " + command.ToLower() + "ed.";
        string text = str1 + "#" + str2;
        int num2 = flag ? 1 : 0;
        commandSender.RaReply(text, num2 != 0, true, "DoorsManagement");
        if (!flag)
          return;
        ServerLogs.AddLog(ServerLogs.Modules.Administrative, sender.Nickname + " " + (sender is PlayerCommandSender ? "(" + sender.SenderId + ") " : "") + command.ToLower() + (command.ToLower().EndsWith("e") ? "d" : "ed") + " door " + door + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
      }
    }

    private static void StandardizedQueryModel1(
      CommandSender sender,
      string programName,
      string playerIds,
      string xValue,
      out int failures,
      out int successes,
      out string error,
      out bool replySent,
      string arg1 = "")
    {
      error = string.Empty;
      failures = 0;
      successes = 0;
      replySent = false;
      programName = programName.ToUpper();
      int result;
      if (int.TryParse(xValue, out result) || programName.StartsWith("SLML") || (programName == "SETGROUP" || programName == "OVERWATCH") || (programName == "NOCLIP" || programName == "BYPASS" || (programName == "HEAL" || programName == "GOD")) || (programName == "BRING" || programName == "MUTE" || (programName == "UNMUTE" || programName == "IMUTE") || (programName == "IUNMUTE" || programName == "DOORTP")))
      {
        List<int> intList = new List<int>();
        try
        {
          string[] strArray = playerIds.Split('.');
          intList.AddRange(((IEnumerable<string>) strArray).Where<string>((Func<string, bool>) (item => !string.IsNullOrEmpty(item))).Select<string, int>(new Func<string, int>(int.Parse)));
          UserGroup group1 = (UserGroup) null;
          Vector3 pos = Vector3.down;
          if (programName == "BAN")
          {
            replySent = true;
            if (result < 0)
              result = 0;
            if (result == 0)
            {
              if (!CommandProcessor.CheckPermissions(sender, programName, new PlayerPermissions[3]
              {
                PlayerPermissions.KickingAndShortTermBanning,
                PlayerPermissions.BanningUpToDay,
                PlayerPermissions.LongTermBanning
              }, "", true))
                return;
            }
            if (result > 0 && result <= 60 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.KickingAndShortTermBanning, "", true) || result > 60 && result <= 1440 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.BanningUpToDay, "", true) || result > 1440 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.LongTermBanning, "", true))
              return;
            replySent = false;
          }
          else if (programName.StartsWith("SLML"))
          {
            MarkupTransceiver objectOfType = UnityEngine.Object.FindObjectOfType<MarkupTransceiver>();
            if (programName.Contains("_STYLE"))
              objectOfType.RequestStyleDownload(xValue, intList.ToArray());
            else if (programName.Contains("_TAG"))
              objectOfType.Transmit(xValue, intList.ToArray());
          }
          else if (!(programName == "SETGROUP"))
          {
            if (programName == "DOORTP")
            {
              xValue = xValue.ToUpper();
              Door door = ((IEnumerable<Door>) UnityEngine.Object.FindObjectsOfType<Door>()).FirstOrDefault<Door>((Func<Door, bool>) (dr => dr.DoorName.ToUpper() == xValue));
              if ((UnityEngine.Object) door == (UnityEngine.Object) null)
              {
                replySent = true;
                sender.RaReply(programName + "#Can't find door " + xValue + ".", false, true, "DoorsManagement");
                return;
              }
              pos = door.transform.position;
              pos.y += 2.5f;
              for (byte index = 0; index < (byte) 21; ++index)
              {
                if (index == (byte) 0)
                  pos.x += 1.5f;
                else if (index < (byte) 3)
                  ++pos.x;
                else if (index == (byte) 4)
                {
                  pos = door.transform.position;
                  pos.y += 2.5f;
                  pos.z += 1.5f;
                }
                else if (index < (byte) 10 && (int) index % 2 == 0)
                  ++pos.z;
                else if (index < (byte) 10)
                  ++pos.x;
                else if (index == (byte) 10)
                {
                  pos = door.transform.position;
                  pos.y += 2.5f;
                  pos.x -= 1.5f;
                }
                else if (index < (byte) 13)
                  --pos.x;
                else if (index == (byte) 14)
                {
                  pos = door.transform.position;
                  pos.y += 2.5f;
                  pos.z -= 1.5f;
                }
                else if ((int) index % 2 == 0)
                  --pos.z;
                else
                  --pos.x;
                if (!FallDamage.CheckUnsafePosition(pos))
                {
                  if (index == (byte) 20)
                    pos = Vector3.zero;
                }
                else
                  break;
              }
              if (pos == Vector3.zero)
              {
                replySent = true;
                sender.RaReply(programName + "#Can't find safe place to teleport to door " + xValue + ".", false, true, "DoorsManagement");
                return;
              }
            }
          }
          else if (xValue != "-1")
          {
            group1 = ServerStatic.PermissionsHandler.GetGroup(xValue);
            if (group1 == null)
            {
              replySent = true;
              sender.RaReply(programName + "#Requested group doesn't exist! Use group \"-1\" to remove user group.", false, true, "");
              return;
            }
          }
          bool isVerified = ServerStatic.PermissionsHandler.IsVerified;
          string nickname = sender.Nickname;
          foreach (int num1 in intList)
          {
            try
            {
              foreach (GameObject player in PlayerManager.players)
              {
                if (num1 == player.GetComponent<QueryProcessor>().PlayerId)
                {
                  // ISSUE: reference to a compiler-generated method
                  switch (<PrivateImplementationDetails>.ComputeStringHash(programName))
                  {
                    case 329200923:
                      if (programName == "UNMUTE")
                      {
                        MuteHandler.RevokePersistantMute(player.GetComponent<CharacterClassManager>().UserId);
                        player.GetComponent<CharacterClassManager>().SetMuted(false);
                        goto default;
                      }
                      else
                        goto default;
                    case 858808885:
                      if (programName == "DOORTP")
                      {
                        player.GetComponent<PlyMovementSync>().OverridePosition(pos, 0.0f, false);
                        goto default;
                      }
                      else
                        goto default;
                    case 945458267:
                      if (programName == "HEAL")
                      {
                        PlayerStats component = player.GetComponent<PlayerStats>();
                        if (xValue != null && result > 0)
                        {
                          component.HealHPAmount((float) result);
                          goto default;
                        }
                        else
                        {
                          component.SetHPAmount(component.ccm.Classes.SafeGet(component.ccm.CurClass).maxHP);
                          goto default;
                        }
                      }
                      else
                        goto default;
                    case 1094345139:
                      if (programName == "SETHP")
                        goto label_100;
                      else
                        goto default;
                    case 1159084506:
                      if (programName == "SETGROUP")
                      {
                        ServerRoles component = player.GetComponent<ServerRoles>();
                        if (component.PublicKeyAccepted)
                        {
                          component.SetGroup(group1, false, true, false);
                          goto default;
                        }
                        else
                        {
                          ++failures;
                          goto default;
                        }
                      }
                      else
                        goto default;
                    case 1297180441:
                      if (programName == "ROLE")
                      {
                        QueryProcessor.LocalCCM.SetPlayersClass((RoleType) result, player, true, false);
                        goto default;
                      }
                      else
                        goto default;
                    case 1630279262:
                      if (programName == "IUNMUTE")
                      {
                        MuteHandler.RevokePersistantMute("ICOM-" + player.GetComponent<CharacterClassManager>().UserId);
                        player.GetComponent<CharacterClassManager>().NetworkIntercomMuted = false;
                        goto default;
                      }
                      else
                        goto default;
                    case 1894470373:
                      if (programName == "HP")
                        goto label_100;
                      else
                        goto default;
                    case 2163566540:
                      if (programName == "NOCLIP")
                      {
                        ServerRoles component = player.GetComponent<ServerRoles>();
                        if (string.IsNullOrEmpty(xValue))
                        {
                          component.NoclipReady = !component.NoclipReady;
                          goto default;
                        }
                        else if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase)))
                        {
                          component.NoclipReady = true;
                          goto default;
                        }
                        else if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase)))
                        {
                          component.NoclipReady = false;
                          goto default;
                        }
                        else
                        {
                          replySent = true;
                          sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                          return;
                        }
                      }
                      else
                        goto default;
                    case 2184885908:
                      if (programName == "MUTE")
                      {
                        MuteHandler.IssuePersistantMute(player.GetComponent<CharacterClassManager>().UserId);
                        player.GetComponent<CharacterClassManager>().SetMuted(true);
                        goto default;
                      }
                      else
                        goto default;
                    case 2245226254:
                      if (programName == "FC")
                        break;
                      goto default;
                    case 2331358749:
                      if (programName == "GOD")
                      {
                        if (string.IsNullOrEmpty(xValue))
                        {
                          player.GetComponent<CharacterClassManager>().GodMode = !player.GetComponent<CharacterClassManager>().GodMode;
                          goto default;
                        }
                        else if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<CharacterClassManager>().GodMode = true;
                          goto default;
                        }
                        else if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<CharacterClassManager>().GodMode = false;
                          goto default;
                        }
                        else
                        {
                          replySent = true;
                          sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                          return;
                        }
                      }
                      else
                        goto default;
                    case 2674786406:
                      if (programName == "BAN")
                      {
                        string myNick = player.GetComponent<NicknameSync>().MyNick;
                        if (!sender.FullPermissions)
                        {
                          UserGroup group2 = player.GetComponent<ServerRoles>().Group;
                          byte num2 = group2 != null ? group2.RequiredKickPower : (byte) 0;
                          if ((int) num2 > (int) sender.KickPower)
                          {
                            ++failures;
                            string text = string.Format("You can't kick/ban {0}. Required kick power: {1}, your kick power: {2}.", (object) myNick, (object) num2, (object) sender.KickPower);
                            error = text;
                            sender.RaReply(text, false, true, string.Empty);
                            continue;
                          }
                        }
                        if (isVerified && player.GetComponent<ServerRoles>().BypassStaff)
                        {
                          QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(player, 0, arg1, nickname);
                          goto default;
                        }
                        else
                        {
                          if (result == 0 && ConfigFile.ServerConfig.GetBool("broadcast_kicks", false))
                            QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_kick_text", "%nick% has been kicked from this server.").Replace("%nick%", myNick), (uint) ConfigFile.ServerConfig.GetInt("broadcast_kick_duration", 5), false);
                          else if (result != 0 && ConfigFile.ServerConfig.GetBool("broadcast_bans", true))
                            QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", myNick), (uint) ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), false);
                          QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(player, result, arg1, nickname);
                          goto default;
                        }
                      }
                      else
                        goto default;
                    case 3182344701:
                      if (programName == "IMUTE")
                      {
                        MuteHandler.IssuePersistantMute("ICOM-" + player.GetComponent<CharacterClassManager>().UserId);
                        player.GetComponent<CharacterClassManager>().NetworkIntercomMuted = true;
                        goto default;
                      }
                      else
                        goto default;
                    case 3234565675:
                      if (programName == "BRING")
                      {
                        if (!(sender is PlayerCommandSender))
                          return;
                        if (player.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || player.GetComponent<CharacterClassManager>().CurClass == RoleType.None)
                        {
                          ++failures;
                          continue;
                        }
                        Vector3 realModelPosition = ((PlayerCommandSender) sender).Processor.GetComponent<PlyMovementSync>().RealModelPosition;
                        player.GetComponent<PlyMovementSync>().OverridePosition(realModelPosition, 0.0f, false);
                        goto default;
                      }
                      else
                        goto default;
                    case 3510393926:
                      if (programName == "OVERWATCH")
                      {
                        if (string.IsNullOrEmpty(xValue))
                        {
                          player.GetComponent<ServerRoles>().CmdSetOverwatchStatus((byte) 2);
                          goto default;
                        }
                        else if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<ServerRoles>().CmdSetOverwatchStatus((byte) 1);
                          goto default;
                        }
                        else if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<ServerRoles>().CmdSetOverwatchStatus((byte) 0);
                          goto default;
                        }
                        else
                        {
                          replySent = true;
                          sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                          return;
                        }
                      }
                      else
                        goto default;
                    case 3554601228:
                      if (programName == "FORCECLASS")
                        break;
                      goto default;
                    case 3611046620:
                      if (programName == "GIVE")
                      {
                        try
                        {
                          player.GetComponent<Inventory>().AddNewItem((ItemType) result, -4.656647E+11f, 0, 0, 0);
                          goto default;
                        }
                        catch (Exception ex)
                        {
                          ++failures;
                          error = ex.Message;
                          continue;
                        }
                      }
                      else
                        goto default;
                    case 3709327981:
                      if (programName == "BYPASS")
                      {
                        if (string.IsNullOrEmpty(xValue))
                        {
                          player.GetComponent<ServerRoles>().BypassMode = !player.GetComponent<ServerRoles>().BypassMode;
                          goto default;
                        }
                        else if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<ServerRoles>().BypassMode = true;
                          goto default;
                        }
                        else if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || (string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase)))
                        {
                          player.GetComponent<ServerRoles>().BypassMode = false;
                          goto default;
                        }
                        else
                        {
                          replySent = true;
                          sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                          return;
                        }
                      }
                      else
                        goto default;
                    case 3730152828:
                      if (programName == "GBAN-KICK")
                      {
                        QueryProcessor.Localplayer.GetComponent<BanPlayer>().KickUser(player, "Globally Banned", nickname, true);
                        goto default;
                      }
                      else
                        goto default;
                    default:
label_138:
                      ++successes;
                      continue;
                  }
                  QueryProcessor.LocalCCM.SetPlayersClass((RoleType) result, player, false, false);
                  goto label_138;
label_100:
                  player.GetComponent<PlayerStats>().SetHPAmount(result);
                  goto label_138;
                }
              }
            }
            catch (Exception ex)
            {
              ++failures;
              error = ex.Message + "\nStackTrace:\n" + ex.StackTrace;
            }
          }
        }
        catch (Exception ex)
        {
          replySent = true;
          sender.RaReply(programName + "#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source + "\nMost likely the PlayerId array was not in the correct format.", false, true, "");
          throw;
        }
      }
      else
      {
        replySent = true;
        sender.RaReply(programName + "#The third parameter has to be an integer!", false, true, "");
      }
    }

    private static bool CheckPermissions(
      CommandSender sender,
      string queryZero,
      PlayerPermissions[] perm,
      string replyScreen = "",
      bool reply = true)
    {
      if (ServerStatic.IsDedicated && sender.FullPermissions || ServerStatic.PermissionsHandler.IsPermitted(sender.Permissions, perm))
        return true;
      if (!reply)
        return false;
      string str = ((IEnumerable<PlayerPermissions>) perm).Aggregate<PlayerPermissions, string>("", (Func<string, PlayerPermissions, string>) ((current, p) => current + "\n- " + (object) p));
      str.Remove(str.Length - 3);
      sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nYou need at least one of following permissions: " + str, false, true, replyScreen);
      return false;
    }

    private static bool CheckPermissions(
      CommandSender sender,
      string queryZero,
      PlayerPermissions perm,
      string replyScreen = "",
      bool reply = true)
    {
      if (ServerStatic.IsDedicated && sender.FullPermissions || PermissionsHandler.IsPermitted(sender.Permissions, perm))
        return true;
      if (reply)
        sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + (object) perm, false, true, replyScreen);
      return false;
    }

    private static bool IsPlayer(CommandSender sender, string queryZero, string replyScreen = "")
    {
      if (sender is PlayerCommandSender)
        return true;
      sender.RaReply(queryZero + "#This command can be executed only from the game level.", false, true, replyScreen);
      return false;
    }
  }
}
