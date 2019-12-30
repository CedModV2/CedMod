using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using GameCore;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Utils.CommandInterpolation;

namespace RemoteAdmin
{
    // Token: 0x020003E6 RID: 998
    public static class CommandProcessor
    {
        // Token: 0x060018A6 RID: 6310 RVA: 0x0008536C File Offset: 0x0008356C
        internal static void ProcessQuery(string q, CommandSender sender)
        {
            string[] query = q.Split(new char[]
            {
                ' '
            });
            string text = sender.Nickname;
            PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
            QueryProcessor queryProcessor = (playerCommandSender != null) ? playerCommandSender.Processor : null;
            if (playerCommandSender != null)
            {
                text = text + " (" + playerCommandSender.CCM.UserId + ")";
            }
            string text2 = query[0].ToUpper();
            uint num = PrivateImplementationDetails.ComputeStringHash(text2);
            int num5;
            int num6;
            string text11;
            bool flag41;
            if (num <= 2129901492U)
            {
                if (num <= 934975720U)
                {
                    if (num <= 501868943U)
                    {
                        if (num <= 275074824U)
                        {
                            if (num <= 43150473U)
                            {
                                if (num != 27894855U)
                                {
                                    if (num != 43150473U || !(text2 == "PING"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    if (query.Length == 1)
                                    {
                                        if (queryProcessor == null)
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
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0].ToUpper(),
                                            "#Your ping: ",
                                            LiteNetLib4MirrorServer.Peers[connectionId].Ping,
                                            "ms"
                                        }), true, true, "");
                                        return;
                                    }
                                    else
                                    {
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
                                        GameObject gameObject = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id2);
                                        if (gameObject == null)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Invalid player id!", false, true, "");
                                            return;
                                        }
                                        int connectionId2 = gameObject.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
                                        if (connectionId2 == 0)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", false, true, "");
                                            return;
                                        }
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0].ToUpper(),
                                            "#Ping: ",
                                            LiteNetLib4MirrorServer.Peers[connectionId2].Ping,
                                            "ms"
                                        }), true, true, "");
                                        return;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "CONFIG"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9905;
                                }
                            }
                            else if (num != 170942193U)
                            {
                                if (num != 219017314U)
                                {
                                    if (num != 275074824U)
                                    {
                                        goto IL_B3F2;
                                    }
                                    if (!(text2 == "FORCESTART"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_39C4;
                                }
                                else
                                {
                                    if (!(text2 == "SNR"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9740;
                                }
                            }
                            else
                            {
                                if (!(text2 == "SLML_STYLE"))
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_97B3;
                            }
                        }
                        else if (num <= 371615861U)
                        {
                            if (num != 290479497U)
                            {
                                if (num != 329200923U)
                                {
                                    if (num != 371615861U)
                                    {
                                        goto IL_B3F2;
                                    }
                                    if (!(text2 == "DOORLIST"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_B20C;
                                }
                                else
                                {
                                    if (!(text2 == "UNMUTE"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_AA7B;
                                }
                            }
                            else
                            {
                                if (!(text2 == "OPEN"))
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_B0DC;
                            }
                        }
                        else if (num != 427453584U)
                        {
                            if (num != 501045426U)
                            {
                                if (num != 501868943U)
                                {
                                    goto IL_B3F2;
                                }
                                if (!(text2 == "DTP"))
                                {
                                    goto IL_B3F2;
                                }
                            }
                            else
                            {
                                if (!(text2 == "BC"))
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_93C6;
                            }
                        }
                        else
                        {
                            if (!(text2 == "CLEARALERT"))
                            {
                                goto IL_B3F2;
                            }
                            goto IL_9577;
                        }
                    }
                    else if (num <= 754810370U)
                    {
                        if (num <= 543160318U)
                        {
                            if (num != 517638783U)
                            {
                                if (num != 534600664U)
                                {
                                    if (num != 543160318U)
                                    {
                                        goto IL_B3F2;
                                    }
                                    if (!(text2 == "ALERTMONO"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_93C6;
                                }
                                else
                                {
                                    if (!(text2 == "BM"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_AD4C;
                                }
                            }
                            else
                            {
                                if (!(text2 == "REQUEST_DATA"))
                                {
                                    goto IL_B3F2;
                                }
                                if (query.Length >= 2)
                                {
                                    text2 = query[1].ToUpper();
                                    if (!(text2 == "PLAYER_LIST"))
                                    {
                                        if (!(text2 == "PLAYER") && !(text2 == "SHORT-PLAYER"))
                                        {
                                            if (!(text2 == "AUTH"))
                                            {
                                                sender.RaReply(query[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", false, true, "PlayerInfo");
                                                return;
                                            }
                                            if ((playerCommandSender != null && playerCommandSender.SR.Staff) || CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess, "", true))
                                            {
                                                if (query.Length >= 3)
                                                {
                                                    try
                                                    {
                                                        GameObject gameObject2 = null;
                                                        foreach (NetworkConnection conn in NetworkServer.connections.Values)
                                                        {
                                                            GameObject gameObject3 = GameCore.Console.FindConnectedRoot(conn);
                                                            if (query[2].Contains("."))
                                                            {
                                                                query[2] = query[2].Split(new char[]
                                                                {
                                                                    '.'
                                                                })[0];
                                                            }
                                                            if (gameObject3 != null && gameObject3.GetComponent<QueryProcessor>().PlayerId.ToString() == query[2])
                                                            {
                                                                gameObject2 = gameObject3;
                                                            }
                                                        }
                                                        if (gameObject2 == null)
                                                        {
                                                            sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", false, true, "");
                                                        }
                                                        else if (string.IsNullOrEmpty(gameObject2.GetComponent<CharacterClassManager>().AuthToken))
                                                        {
                                                            sender.RaReply(query[0].ToUpper() + ":PLAYER#Can't obtain auth token. Is server using offline mode or you selected the host?", false, true, "PlayerInfo");
                                                        }
                                                        else
                                                        {
                                                            ServerLogs.AddLog(ServerLogs.Modules.DataAccess, string.Concat(new object[]
                                                            {
                                                                text,
                                                                " accessed authentication token of player ",
                                                                gameObject2.GetComponent<QueryProcessor>().PlayerId,
                                                                " (",
                                                                gameObject2.GetComponent<NicknameSync>().MyNick,
                                                                ")."
                                                            }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                                            if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                string myNick = gameObject2.GetComponent<NicknameSync>().MyNick;
                                                                string str = string.Concat(new object[]
                                                                {
                                                                    "<color=white>Authentication token of player ",
                                                                    myNick,
                                                                    "(",
                                                                    gameObject2.GetComponent<QueryProcessor>().PlayerId,
                                                                    "):\n",
                                                                    gameObject2.GetComponent<CharacterClassManager>().AuthToken,
                                                                    "</color>"
                                                                });
                                                                sender.RaReply(query[0].ToUpper() + ":PLAYER#" + str, true, true, "null");
                                                                sender.RaReply("BigQR#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, true, false, "null");
                                                            }
                                                            else
                                                            {
                                                                sender.RaReply("StaffTokenReply#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, true, false, "null");
                                                            }
                                                        }
                                                        return;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        sender.RaReply(string.Concat(new string[]
                                                        {
                                                            query[0].ToUpper(),
                                                            "#An unexpected problem has occurred!\nMessage: ",
                                                            ex.Message,
                                                            "\nStackTrace: ",
                                                            ex.StackTrace,
                                                            "\nAt: ",
                                                            ex.Source
                                                        }), false, true, "PlayerInfo");
                                                        throw;
                                                    }
                                                }
                                                sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, "");
                                                return;
                                            }
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            string text3 = "\n";
                                            bool gameplayData = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, string.Empty, false);
                                            PlayerCommandSender playerCommandSender2;
                                            if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
                                            {
                                                playerCommandSender2.Processor.GameplayData = gameplayData;
                                            }
                                            bool flag = q.Contains("STAFF", StringComparison.OrdinalIgnoreCase);
                                            foreach (GameObject gameObject4 in PlayerManager.players)
                                            {
                                                QueryProcessor component = gameObject4.GetComponent<QueryProcessor>();
                                                if (!flag)
                                                {
                                                    string text4 = string.Empty;
                                                    bool flag2 = false;
                                                    ServerRoles component2 = component.GetComponent<ServerRoles>();
                                                    try
                                                    {
                                                        text4 = (component2.RaEverywhere ? "[~] " : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty)));
                                                        flag2 = component2.OverwatchEnabled;
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    text3 = string.Concat(new object[]
                                                    {
                                                        text3,
                                                        text4,
                                                        "(",
                                                        component.PlayerId,
                                                        ") ",
                                                        component.GetComponent<NicknameSync>().MyNick.Replace("\n", "").Replace("<", "").Replace(">", ""),
                                                        flag2 ? "<OVRM>" : ""
                                                    });
                                                }
                                                else
                                                {
                                                    text3 = string.Concat(new object[]
                                                    {
                                                        text3,
                                                        component.PlayerId,
                                                        ";",
                                                        component.GetComponent<NicknameSync>().MyNick
                                                    });
                                                }
                                                text3 += "\n";
                                            }
                                            if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
                                            {
                                                sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + text3, true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
                                            }
                                            else
                                            {
                                                sender.RaReply("StaffPlayerListReply#" + text3, true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
                                            }
                                            return;
                                        }
                                        catch (Exception ex2)
                                        {
                                            sender.RaReply(string.Concat(new string[]
                                            {
                                                query[0].ToUpper(),
                                                ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: ",
                                                ex2.Message,
                                                "\nStackTrace: ",
                                                ex2.StackTrace,
                                                "\nAt: ",
                                                ex2.Source
                                            }), false, true, "");
                                            throw;
                                        }
                                    }
                                    if (query.Length >= 3)
                                    {
                                        if (string.Equals(query[1], "PLAYER", StringComparison.OrdinalIgnoreCase) && (playerCommandSender == null || !playerCommandSender.SR.Staff) && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess, "", true))
                                        {
                                            return;
                                        }
                                        try
                                        {
                                            GameObject gameObject5 = null;
                                            NetworkConnection networkConnection = null;
                                            foreach (NetworkConnection networkConnection2 in NetworkServer.connections.Values)
                                            {
                                                GameObject gameObject6 = GameCore.Console.FindConnectedRoot(networkConnection2);
                                                if (query[2].Contains("."))
                                                {
                                                    query[2] = query[2].Split(new char[]
                                                    {
                                                        '.'
                                                    })[0];
                                                }
                                                if (!(gameObject6 == null) && !(gameObject6.GetComponent<QueryProcessor>().PlayerId.ToString() != query[2]))
                                                {
                                                    gameObject5 = gameObject6;
                                                    networkConnection = networkConnection2;
                                                }
                                            }
                                            if (gameObject5 == null)
                                            {
                                                sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", false, true, "");
                                            }
                                            else
                                            {
                                                bool flag3 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, "", false);
                                                PlayerCommandSender playerCommandSender3;
                                                if ((playerCommandSender3 = (sender as PlayerCommandSender)) != null)
                                                {
                                                    playerCommandSender3.Processor.GameplayData = flag3;
                                                }
                                                CharacterClassManager component3 = gameObject5.GetComponent<CharacterClassManager>();
                                                ServerRoles component4 = gameObject5.GetComponent<ServerRoles>();
                                                if (query[1].ToUpper() == "PLAYER")
                                                {
                                                    ServerLogs.AddLog(ServerLogs.Modules.DataAccess, string.Concat(new object[]
                                                    {
                                                        text,
                                                        " accessed IP address of player ",
                                                        gameObject5.GetComponent<QueryProcessor>().PlayerId,
                                                        " (",
                                                        gameObject5.GetComponent<NicknameSync>().MyNick,
                                                        ")."
                                                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                                }
                                                StringBuilder stringBuilder = new StringBuilder("<color=white>");
                                                stringBuilder.Append("Nickname: " + gameObject5.GetComponent<NicknameSync>().MyNick);
                                                stringBuilder.Append("\nPlayer ID: " + gameObject5.GetComponent<QueryProcessor>().PlayerId);
                                                stringBuilder.Append("\nIP: " + ((networkConnection == null) ? "null" : ((query[1].ToUpper() == "PLAYER") ? networkConnection.address : "[REDACTED]")));
                                                stringBuilder.Append("\nUser ID: " + (string.IsNullOrEmpty(component3.UserId) ? "(none)" : component3.UserId));
                                                if (component3.SaltedUserId != null && component3.SaltedUserId.Contains("$"))
                                                {
                                                    stringBuilder.Append("\nSalted User ID: " + component3.SaltedUserId);
                                                }
                                                if (!string.IsNullOrEmpty(component3.UserId2))
                                                {
                                                    stringBuilder.Append("\nUser ID 2: " + component3.UserId2);
                                                }
                                                stringBuilder.Append("\nServer role: " + component4.GetColoredRoleString(false));
                                                if (!string.IsNullOrEmpty(component4.HiddenBadge))
                                                {
                                                    stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + component4.HiddenBadge);
                                                }
                                                if (component4.RaEverywhere)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
                                                }
                                                else if (component4.Staff)
                                                {
                                                    stringBuilder.Append("\nActive flag: Studio Staff");
                                                }
                                                if (component3.Muted)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
                                                }
                                                else if (component3.IntercomMuted)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
                                                }
                                                if (component3.GodMode)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
                                                }
                                                if (component3.NoclipEnabled)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
                                                }
                                                else if (component4.NoclipReady)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
                                                }
                                                if (component4.DoNotTrack)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
                                                }
                                                if (component4.BypassMode)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
                                                }
                                                if (component4.RemoteAdmin)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
                                                }
                                                if (component4.OverwatchEnabled)
                                                {
                                                    stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
                                                }
                                                else
                                                {
                                                    stringBuilder.Append("\nClass: " + ((!flag3) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (component3.Classes.CheckBounds(component3.CurClass) ? component3.Classes.SafeGet(component3.CurClass).fullName : "None")));
                                                    stringBuilder.Append("\nHP: " + (flag3 ? gameObject5.GetComponent<PlayerStats>().HealthToString() : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
                                                    if (!flag3)
                                                    {
                                                        stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
                                                    }
                                                }
                                                stringBuilder.Append("</color>");
                                                sender.RaReply(query[0].ToUpper() + ":PLAYER#" + stringBuilder, true, true, "PlayerInfo");
                                                sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(component3.UserId) ? "(no User ID)" : component3.UserId), true, false, "PlayerInfo");
                                            }
                                            return;
                                        }
                                        catch (Exception ex3)
                                        {
                                            sender.RaReply(string.Concat(new string[]
                                            {
                                                query[0].ToUpper(),
                                                "#An unexpected problem has occurred!\nMessage: ",
                                                ex3.Message,
                                                "\nStackTrace: ",
                                                ex3.StackTrace,
                                                "\nAt: ",
                                                ex3.Source
                                            }), false, true, "PlayerInfo");
                                            throw;
                                        }
                                    }
                                    sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, "");
                                    return;
                                }
                                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "PlayerInfo");
                                return;
                            }
                        }
                        else if (num != 603976806U)
                        {
                            if (num != 644288100U)
                            {
                                if (num != 754810370U)
                                {
                                    goto IL_B3F2;
                                }
                                if (!(text2 == "LOCK"))
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_B174;
                            }
                            else
                            {
                                if (!(text2 == "SUDO"))
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_95E6;
                            }
                        }
                        else
                        {
                            if (!(text2 == "PM"))
                            {
                                goto IL_B3F2;
                            }
                            if (!CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement, "", true))
                            {
                                return;
                            }
                            Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
                            List<string> allPermissions = ServerStatic.PermissionsHandler.GetAllPermissions();
                            if (query.Length == 1)
                            {
                                sender.RaReply(query[0].ToUpper() + "#Permissions manager help:\nSyntax: " + query[0] + " action\n\nAvailable actions:\ngroups - lists all groups\ngroup info <group name> - prints group info\ngroup grant <group name> <permission name> - grants permission to a group\ngroup revoke <group name> <permission name> - revokes permission from a group\ngroup setcolor <group name> <color name> - sets group color\ngroup settag <group name> <tag> - sets group tag\ngroup enablecover <group name> - enables badge cover for group\ngroup disablecover <group name> - disables badge cover for group\n\nusers - lists all privileged users\nsetgroup <UserID> <group name> - sets membership of user (use group name \"-1\" to remove user from group)\nreload - reloads permission file\n\n\"< >\" are only used to indicate the arguments, don't put them\nMore commands will be added in the next versions of the game", true, true, "");
                                return;
                            }
                            if (string.Equals(query[1], "groups", StringComparison.OrdinalIgnoreCase))
                            {
                                int num2 = 0;
                                string text5 = "\n";
                                string text6 = "";
                                string[] source = new string[]
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
                                int num3 = (int)Math.Ceiling((double)allPermissions.Count / 12.0);
                                int num4;
                                for (int i = 0; i < num3; i = num4 + 1)
                                {
                                    num2 = 0;
                                    text5 = text5 + "\n-----" + source.Skip(i * 12).Take(12).Aggregate((string current, string adding) => current + " " + adding);
                                    foreach (KeyValuePair<string, UserGroup> keyValuePair in allGroups)
                                    {
                                        if (i == 0)
                                        {
                                            text6 = string.Concat(new object[]
                                            {
                                                text6,
                                                "\n",
                                                num2,
                                                " - ",
                                                keyValuePair.Key
                                            });
                                        }
                                        string text7 = num2.ToString();
                                        for (int j = text7.Length; j < 5; j = num4 + 1)
                                        {
                                            text7 += " ";
                                            num4 = j;
                                        }
                                        foreach (string check in allPermissions.Skip(i * 12).Take(12))
                                        {
                                            text7 = text7 + "  " + (ServerStatic.PermissionsHandler.IsPermitted(keyValuePair.Value.Permissions, check) ? "X" : " ") + " ";
                                        }
                                        num4 = num2;
                                        num2 = num4 + 1;
                                        text5 = text5 + "\n" + text7;
                                    }
                                    num4 = i;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#All defined groups: ",
                                    text5,
                                    "\n",
                                    text6
                                }), true, true, "");
                                return;
                            }
                            if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length == 2)
                            {
                                sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                                return;
                            }
                            if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length > 2)
                            {
                                if (string.Equals(query[2], "info", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
                                {
                                    KeyValuePair<string, UserGroup> keyValuePair2 = allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
                                    if (keyValuePair2.Key == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                        return;
                                    }
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        query[0].ToUpper(),
                                        "#Details of group ",
                                        keyValuePair2.Key,
                                        "\nTag text: ",
                                        keyValuePair2.Value.BadgeText,
                                        "\nTag color: ",
                                        keyValuePair2.Value.BadgeColor,
                                        "\nPermissions: ",
                                        keyValuePair2.Value.Permissions,
                                        "\nCover: ",
                                        keyValuePair2.Value.Cover ? "YES" : "NO",
                                        "\nHidden by default: ",
                                        keyValuePair2.Value.HiddenByDefault ? "YES" : "NO",
                                        "\nKick power: ",
                                        keyValuePair2.Value.KickPower,
                                        "\nRequired kick power: ",
                                        keyValuePair2.Value.RequiredKickPower
                                    }), true, true, "");
                                    return;
                                }
                                else if ((string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase) || string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase)) && query.Length == 5)
                                {
                                    if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
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
                                    List<string> list = null;
                                    foreach (string text8 in stringDictionary.Keys)
                                    {
                                        if (!(text8 != query[4]))
                                        {
                                            list = YamlConfig.ParseCommaSeparatedString(stringDictionary[text8]).ToList<string>();
                                        }
                                    }
                                    if (list == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Permission can't be found in the config.", false, true, "");
                                        return;
                                    }
                                    if (list.Contains(query[3]) && string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group already has that permission.", false, true, "");
                                        return;
                                    }
                                    if (!list.Contains(query[3]) && string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase))
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group already doesn't have that permission.", false, true, "");
                                        return;
                                    }
                                    if (string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
                                    {
                                        list.Add(query[3]);
                                    }
                                    else
                                    {
                                        list.Remove(query[3]);
                                    }
                                    list.Sort();
                                    string text9 = "[";
                                    foreach (string str2 in list)
                                    {
                                        if (text9 != "[")
                                        {
                                            text9 += ", ";
                                        }
                                        text9 += str2;
                                    }
                                    text9 += "]";
                                    ServerStatic.RolesConfig.SetStringDictionaryItem("Permissions", query[4], text9);
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Permissions updated.", true, true, "");
                                    return;
                                }
                                else if (string.Equals(query[2], "setcolor", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
                                {
                                    if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                        return;
                                    }
                                    ServerStatic.RolesConfig.SetString(query[3] + "_color", query[4]);
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Group color updated.", true, true, "");
                                    return;
                                }
                                else if (string.Equals(query[2], "settag", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
                                {
                                    if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                        return;
                                    }
                                    ServerStatic.RolesConfig.SetString(query[3] + "_badge", query[4]);
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Group tag updated.", true, true, "");
                                    return;
                                }
                                else if (string.Equals(query[2], "enablecover", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
                                {
                                    if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                        return;
                                    }
                                    ServerStatic.RolesConfig.SetString(query[3] + "_cover", "true");
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", true, true, "");
                                    return;
                                }
                                else
                                {
                                    if (!(query[2].ToLower() == "disablecover") || query.Length != 4)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                                        return;
                                    }
                                    if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                        return;
                                    }
                                    ServerStatic.RolesConfig.SetString(query[3] + "_cover", "false");
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", true, true, "");
                                    return;
                                }
                            }
                            else
                            {
                                if (string.Equals(query[1], "users", StringComparison.OrdinalIgnoreCase))
                                {
                                    Dictionary<string, string> stringDictionary2 = ServerStatic.RolesConfig.GetStringDictionary("Members");
                                    YamlConfig sharedGroupsMembersConfig = ServerStatic.SharedGroupsMembersConfig;
                                    Dictionary<string, string> dictionary = (sharedGroupsMembersConfig != null) ? sharedGroupsMembersConfig.GetStringDictionary("SharedMembers") : null;
                                    string text10 = "Players with assigned groups:";
                                    foreach (KeyValuePair<string, string> keyValuePair3 in stringDictionary2)
                                    {
                                        text10 = string.Concat(new string[]
                                        {
                                            text10,
                                            "\n",
                                            keyValuePair3.Key,
                                            " - ",
                                            keyValuePair3.Value
                                        });
                                    }
                                    if (dictionary != null)
                                    {
                                        foreach (KeyValuePair<string, string> keyValuePair4 in dictionary)
                                        {
                                            text10 = string.Concat(new string[]
                                            {
                                                text10,
                                                "\n",
                                                keyValuePair4.Key,
                                                " - ",
                                                keyValuePair4.Value,
                                                " <color=#FFD700>[SHARED MEMBERSHIP]</color>"
                                            });
                                        }
                                    }
                                    sender.RaReply(query[0].ToUpper() + "#" + text10, true, true, "");
                                    return;
                                }
                                if (string.Equals(query[1], "setgroup", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
                                {
                                    string value;
                                    if (query[3] == "-1")
                                    {
                                        value = null;
                                    }
                                    else
                                    {
                                        KeyValuePair<string, UserGroup> keyValuePair5 = allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
                                        if (keyValuePair5.Key == null)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Group can't be found.", false, true, "");
                                            return;
                                        }
                                        value = keyValuePair5.Key;
                                    }
                                    ServerStatic.RolesConfig.SetStringDictionaryItem("Members", query[2], value);
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#User permissions updated. If user is online, please use \"setgroup\" command to change it now (without this command, new role will be applied during next round).", true, true, "");
                                    return;
                                }
                                if (string.Equals(query[1], "reload", StringComparison.OrdinalIgnoreCase))
                                {
                                    ConfigFile.ReloadGameConfigs(false);
                                    ServerStatic.RolesConfig.Reload();
                                    ServerStatic.SharedGroupsConfig = ((ConfigSharing.Paths[4] == null) ? null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt"));
                                    ServerStatic.SharedGroupsMembersConfig = ((ConfigSharing.Paths[5] == null) ? null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt"));
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    sender.RaReply(query[0].ToUpper() + "#Permission file reloaded.", true, true, "");
                                    return;
                                }
                                sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", false, true, "");
                                return;
                            }
                        }
                    }
                    else if (num <= 873487573U)
                    {
                        if (num != 844380939U)
                        {
                            bool flag4 = num != 858808885U;
                            if (flag4)
                            {
                                if (num != 873487573U)
                                {
                                    goto IL_B3F2;
                                }
                                bool flag5 = !(text2 == "UNLOCK");
                                bool flag6 = flag5;
                                bool flag7 = flag6;
                                if (flag7)
                                {
                                    goto IL_B3F2;
                                }
                                goto IL_3A77;
                            }
                            else
                            {
                                bool flag8 = !(text2 == "DOORTP");
                                bool flag9 = flag8;
                                bool flag10 = flag9;
                                if (flag10)
                                {
                                    goto IL_B3F2;
                                }
                            }
                        }
                        else
                        {
                            bool flag11 = text2 == "HELLO";
                            bool flag12 = flag11;
                            bool flag13 = flag12;
                            if (flag13)
                            {
                                sender.RaReply(query[0].ToUpper() + "#Hello World!", true, true, "");
                                return;
                            }
                            goto IL_B3F2;
                        }
                    }
                    else
                    {
                        bool flag14 = num != 904296662U;
                        bool flag15 = flag14;
                        bool flag16 = flag15;
                        if (flag16)
                        {
                            bool flag17 = num != 912447482U;
                            bool flag18 = flag17;
                            bool flag19 = flag18;
                            if (flag19)
                            {
                                if (num != 934975720U || !(text2 == "INTERCOM-TIMEOUT"))
                                {
                                    goto IL_B3F2;
                                }
                                bool flag20 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                                {
                                    PlayerPermissions.KickingAndShortTermBanning,
                                    PlayerPermissions.BanningUpToDay,
                                    PlayerPermissions.LongTermBanning,
                                    PlayerPermissions.RoundEvents,
                                    PlayerPermissions.FacilityManagement,
                                    PlayerPermissions.PlayersManagement
                                }, "ServerEvents", true);
                                bool flag21 = flag20;
                                bool flag22 = flag21;
                                if (flag22)
                                {
                                    return;
                                }
                                bool flag23 = !Intercom.host.speaking;
                                bool flag24 = flag23;
                                bool flag25 = flag24;
                                if (flag25)
                                {
                                    sender.RaReply(query[0].ToUpper() + "#Intercom is not being used.", false, true, "ServerEvents");
                                    return;
                                }
                                bool flag26 = Intercom.host.speechRemainingTime == -77f;
                                bool flag27 = flag26;
                                bool flag28 = flag27;
                                if (flag28)
                                {
                                    sender.RaReply(query[0].ToUpper() + "#Intercom is being used by player with bypass mode enabled.", false, true, "ServerEvents");
                                    return;
                                }
                                Intercom.host.speechRemainingTime = -1f;
                                ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " timeouted the intercom speaker.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                                sender.RaReply(query[0].ToUpper() + "#Done! Intercom speaker timeouted.", true, true, "ServerEvents");
                                return;
                            }
                            else
                            {
                                bool flag29 = text2 == "HELP";
                                bool flag30 = flag29;
                                bool flag31 = flag30;
                                if (flag31)
                                {
                                    sender.RaReply(query[0].ToUpper() + "#This should be useful!", true, true, "");
                                    return;
                                }
                                goto IL_B3F2;
                            }
                        }
                        else
                        {
                            bool flag32 = !(text2 == "NC");
                            bool flag33 = flag32;
                            bool flag34 = flag33;
                            if (flag34)
                            {
                                goto IL_B3F2;
                            }
                            goto IL_A5C5;
                        }
                    }
                    bool flag35 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "DoorsManagement", true);
                    bool flag36 = flag35;
                    bool flag37 = flag36;
                    if (flag37)
                    {
                        return;
                    }
                    bool flag38 = query.Length != 3;
                    bool flag39 = flag38;
                    bool flag40 = flag39;
                    if (flag40)
                    {
                        sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " PlayerIDs DoorName", false, true, "");
                        return;
                    }
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                    {
                        text,
                        " ran the DoorTp command (Door: ",
                        query[2],
                        ") on ",
                        query[1],
                        " players."
                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    CommandProcessor.StandardizedQueryModel1(sender, "DOORTP", query[1], query[2], out num5, out num6, out text11, out flag41, "");
                    bool flag42 = flag41;
                    bool flag43 = flag42;
                    bool flag44 = flag43;
                    if (flag44)
                    {
                        return;
                    }
                    bool flag45 = num5 == 0;
                    bool flag46 = flag45;
                    bool flag47 = flag46;
                    if (flag47)
                    {
                        sender.RaReply(string.Concat(new object[]
                        {
                            query[0],
                            "#Done! The request affected ",
                            num6,
                            " player(s)!"
                        }), true, true, "DoorsManagement");
                        return;
                    }
                    sender.RaReply(string.Concat(new object[]
                    {
                        query[0],
                        "#The proccess has occured an issue! Failures: ",
                        num5,
                        "\nLast error log:\n",
                        text11
                    }), false, true, "DoorsManagement");
                    return;
                }
                else
                {
                    bool flag48 = num <= 1443004851U;
                    bool flag49 = flag48;
                    bool flag50 = flag49;
                    if (flag50)
                    {
                        bool flag51 = num <= 1159084506U;
                        bool flag52 = flag51;
                        bool flag53 = flag52;
                        if (flag53)
                        {
                            bool flag54 = num <= 991654772U;
                            bool flag55 = flag54;
                            bool flag56 = flag55;
                            if (flag56)
                            {
                                bool flag57 = num != 945458267U;
                                bool flag58 = flag57;
                                bool flag59 = flag58;
                                if (flag59)
                                {
                                    bool flag60 = num != 971901278U;
                                    bool flag61 = flag60;
                                    bool flag62 = flag61;
                                    if (flag62)
                                    {
                                        if (num != 991654772U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag63 = !(text2 == "RTIME");
                                        bool flag64 = flag63;
                                        bool flag65 = flag64;
                                        if (flag65)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_391A;
                                    }
                                    else
                                    {
                                        bool flag66 = !(text2 == "OVR");
                                        bool flag67 = flag66;
                                        bool flag68 = flag67;
                                        if (flag68)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_A8C2;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "HEAL"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag69 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true);
                                    bool flag70 = flag69;
                                    bool flag71 = flag70;
                                    if (flag71)
                                    {
                                        return;
                                    }
                                    bool flag72 = query.Length < 2;
                                    bool flag73 = flag72;
                                    bool flag74 = flag73;
                                    if (flag74)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                                        return;
                                    }
                                    int num7 = (query.Length >= 3 && int.TryParse(query[2], out num7)) ? num7 : 0;
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the heal command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                    CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], num7.ToString(), out num5, out num6, out text11, out flag41, "");
                                    bool flag75 = flag41;
                                    bool flag76 = flag75;
                                    bool flag77 = flag76;
                                    if (flag77)
                                    {
                                        return;
                                    }
                                    bool flag78 = num5 == 0;
                                    bool flag79 = flag78;
                                    bool flag80 = flag79;
                                    if (flag80)
                                    {
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0],
                                            "#Done! The request affected ",
                                            num6,
                                            " player(s)!"
                                        }), true, true, "");
                                        return;
                                    }
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        query[0],
                                        "#The proccess has occured an issue! Failures: ",
                                        num5,
                                        "\nLast error log:\n",
                                        text11
                                    }), false, true, "AdminTools");
                                    return;
                                }
                            }
                            else
                            {
                                bool flag81 = num != 1094345139U;
                                bool flag82 = flag81;
                                bool flag83 = flag82;
                                if (flag83)
                                {
                                    bool flag84 = num != 1154386829U;
                                    bool flag85 = flag84;
                                    bool flag86 = flag85;
                                    if (flag86)
                                    {
                                        if (num != 1159084506U || !(text2 == "SETGROUP"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag87 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.SetGroup, "", true);
                                        bool flag88 = flag87;
                                        bool flag89 = flag88;
                                        if (flag89)
                                        {
                                            return;
                                        }
                                        bool flag90 = !ConfigFile.ServerConfig.GetBool("online_mode", true);
                                        bool flag91 = flag90;
                                        bool flag92 = flag91;
                                        if (flag92)
                                        {
                                            sender.RaReply(query[0] + "#This command requires the server to operate in online mode!", false, true, "");
                                            return;
                                        }
                                        bool flag93 = query.Length < 3;
                                        bool flag94 = flag93;
                                        bool flag95 = flag94;
                                        if (flag95)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                                            return;
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Permissions, string.Concat(new string[]
                                        {
                                            text,
                                            " ran the setgroup command (new group: ",
                                            query[2],
                                            " min) on ",
                                            query[1],
                                            " players."
                                        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, "");
                                        bool flag96 = flag41;
                                        bool flag97 = flag96;
                                        bool flag98 = flag97;
                                        if (flag98)
                                        {
                                            return;
                                        }
                                        bool flag99 = num5 == 0;
                                        bool flag100 = flag99;
                                        bool flag101 = flag100;
                                        if (flag101)
                                        {
                                            sender.RaReply(string.Concat(new object[]
                                            {
                                                query[0],
                                                "#Done! The request affected ",
                                                num6,
                                                " player(s)!"
                                            }), true, true, "");
                                            return;
                                        }
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0],
                                            "#The proccess has occured an issue! Failures: ",
                                            num5,
                                            "\nLast error log:\n",
                                            text11
                                        }), false, true, "");
                                        return;
                                    }
                                    else
                                    {
                                        bool flag102 = !(text2 == "DL");
                                        bool flag103 = flag102;
                                        bool flag104 = flag103;
                                        if (flag104)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B20C;
                                    }
                                }
                                else
                                {
                                    bool flag105 = !(text2 == "SETHP");
                                    bool flag106 = flag105;
                                    bool flag107 = flag106;
                                    if (flag107)
                                    {
                                        goto IL_B3F2;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bool flag108 = num <= 1226143193U;
                            bool flag109 = flag108;
                            bool flag110 = flag109;
                            if (flag110)
                            {
                                bool flag111 = num != 1160557609U;
                                bool flag112 = flag111;
                                bool flag113 = flag112;
                                if (flag113)
                                {
                                    bool flag114 = num != 1173502427U;
                                    bool flag115 = flag114;
                                    bool flag116 = flag115;
                                    if (flag116)
                                    {
                                        if (num != 1226143193U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag117 = !(text2 == "ALERT");
                                        bool flag118 = flag117;
                                        bool flag119 = flag118;
                                        if (flag119)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_93C6;
                                    }
                                    else
                                    {
                                        bool flag120 = !(text2 == "BCCLEAR");
                                        bool flag121 = flag120;
                                        bool flag122 = flag121;
                                        if (flag122)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9577;
                                    }
                                }
                                else
                                {
                                    bool flag123 = !(text2 == "SPEAK");
                                    bool flag124 = flag123;
                                    bool flag125 = flag124;
                                    if (flag125)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_AC19;
                                }
                            }
                            else
                            {
                                bool flag126 = num != 1302443662U;
                                bool flag127 = flag126;
                                bool flag128 = flag127;
                                if (flag128)
                                {
                                    bool flag129 = num != 1442280307U;
                                    bool flag130 = flag129;
                                    bool flag131 = flag130;
                                    if (flag131)
                                    {
                                        if (num != 1443004851U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag132 = !(text2 == "SC");
                                        bool flag133 = flag132;
                                        bool flag134 = flag133;
                                        if (flag134)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9905;
                                    }
                                    else
                                    {
                                        bool flag135 = !(text2 == "CASSIE_SN");
                                        bool flag136 = flag135;
                                        bool flag137 = flag136;
                                        if (flag137)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9323;
                                    }
                                }
                                else
                                {
                                    bool flag138 = !(text2 == "ALERTCLEAR");
                                    bool flag139 = flag138;
                                    bool flag140 = flag139;
                                    if (flag140)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9577;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag141 = num <= 1761926707U;
                        bool flag142 = flag141;
                        bool flag143 = flag142;
                        if (flag143)
                        {
                            bool flag144 = num <= 1630279262U;
                            bool flag145 = flag144;
                            bool flag146 = flag145;
                            if (flag146)
                            {
                                bool flag147 = num != 1475835545U;
                                bool flag148 = flag147;
                                bool flag149 = flag148;
                                if (flag149)
                                {
                                    bool flag150 = num != 1521082795U;
                                    bool flag151 = flag150;
                                    bool flag152 = flag151;
                                    if (flag152)
                                    {
                                        if (num != 1630279262U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag153 = !(text2 == "IUNMUTE");
                                        bool flag154 = flag153;
                                        bool flag155 = flag154;
                                        if (flag155)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AA7B;
                                    }
                                    else
                                    {
                                        if (!(text2 == "PERM"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag156 = CommandProcessor.IsPlayer(sender, query[0], "") && !(queryProcessor == null);
                                        bool flag157 = flag156;
                                        bool flag158 = flag157;
                                        if (flag158)
                                        {
                                            ulong permissions = queryProcessor.Roles.Permissions;
                                            string text12 = "Your permissions:";
                                            foreach (string text13 in ServerStatic.PermissionsHandler.GetAllPermissions())
                                            {
                                                string text14 = ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler.GetPermissionValue(text13)) ? "*" : "";
                                                text12 = string.Concat(new object[]
                                                {
                                                    text12,
                                                    "\n",
                                                    text13,
                                                    text14,
                                                    " (",
                                                    ServerStatic.PermissionsHandler.GetPermissionValue(text13),
                                                    "): ",
                                                    ServerStatic.PermissionsHandler.IsPermitted(permissions, text13) ? "YES" : "NO"
                                                });
                                            }
                                            sender.RaReply(query[0].ToUpper() + "#" + text12, true, true, "");
                                            return;
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    bool flag159 = !(text2 == "CASSIE_SL");
                                    bool flag160 = flag159;
                                    bool flag161 = flag160;
                                    if (flag161)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9323;
                                }
                            }
                            else
                            {
                                bool flag162 = num != 1692550565U;
                                bool flag163 = flag162;
                                bool flag164 = flag163;
                                if (flag164)
                                {
                                    bool flag165 = num != 1723521268U;
                                    bool flag166 = flag165;
                                    bool flag167 = flag166;
                                    if (flag167)
                                    {
                                        if (num != 1761926707U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag168 = !(text2 == "RT");
                                        bool flag169 = flag168;
                                        bool flag170 = flag169;
                                        if (flag170)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_391A;
                                    }
                                    else
                                    {
                                        bool flag171 = !(text2 == "ROUNDLOCK");
                                        bool flag172 = flag171;
                                        bool flag173 = flag172;
                                        if (flag173)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B30C;
                                    }
                                }
                                else
                                {
                                    bool flag174 = !(text2 == "LL");
                                    bool flag175 = flag174;
                                    bool flag176 = flag175;
                                    if (flag176)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_B37F;
                                }
                            }
                        }
                        else
                        {
                            bool flag177 = num <= 1951805507U;
                            bool flag178 = flag177;
                            bool flag179 = flag178;
                            if (flag179)
                            {
                                bool flag180 = num != 1826771517U;
                                bool flag181 = flag180;
                                bool flag182 = flag181;
                                if (flag182)
                                {
                                    bool flag183 = num != 1894470373U;
                                    bool flag184 = flag183;
                                    bool flag185 = flag184;
                                    if (flag185)
                                    {
                                        if (num != 1951805507U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag186 = !(text2 == "CASSIE_SILENTNOISE");
                                        bool flag187 = flag186;
                                        bool flag188 = flag187;
                                        if (flag188)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9323;
                                    }
                                    else
                                    {
                                        bool flag189 = !(text2 == "HP");
                                        bool flag190 = flag189;
                                        bool flag191 = flag190;
                                        if (flag191)
                                        {
                                            goto IL_B3F2;
                                        }
                                    }
                                }
                                else
                                {
                                    bool flag192 = !(text2 == "LD");
                                    bool flag193 = flag192;
                                    bool flag194 = flag193;
                                    if (flag194)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_AF05;
                                }
                            }
                            else
                            {
                                bool flag195 = num != 1976784350U;
                                bool flag196 = flag195;
                                bool flag197 = flag196;
                                if (flag197)
                                {
                                    bool flag198 = num != 1990398098U;
                                    bool flag199 = flag198;
                                    bool flag200 = flag199;
                                    if (flag200)
                                    {
                                        if (num != 2129901492U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag201 = !(text2 == "UL");
                                        bool flag202 = flag201;
                                        bool flag203 = flag202;
                                        if (flag203)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_3A77;
                                    }
                                    else
                                    {
                                        bool flag204 = !(text2 == "ROUNDTIME");
                                        bool flag205 = flag204;
                                        bool flag206 = flag205;
                                        if (flag206)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_391A;
                                    }
                                }
                                else
                                {
                                    bool flag207 = !(text2 == "FS");
                                    bool flag208 = flag207;
                                    bool flag209 = flag208;
                                    if (flag209)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_39C4;
                                }
                            }
                        }
                    }
                    bool flag210 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true);
                    bool flag211 = flag210;
                    bool flag212 = flag211;
                    if (flag212)
                    {
                        return;
                    }
                    bool flag213 = query.Length < 3;
                    bool flag214 = flag213;
                    bool flag215 = flag214;
                    if (flag215)
                    {
                        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                        return;
                    }
                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                    {
                        text,
                        " ran the sethp command on ",
                        query[1],
                        " players (HP: ",
                        query[2],
                        ")."
                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, "");
                    bool flag216 = flag41;
                    bool flag217 = flag216;
                    bool flag218 = flag217;
                    if (flag218)
                    {
                        return;
                    }
                    bool flag219 = num5 == 0;
                    bool flag220 = flag219;
                    bool flag221 = flag220;
                    if (flag221)
                    {
                        sender.RaReply(string.Concat(new object[]
                        {
                            query[0],
                            "#Done! The request affected ",
                            num6,
                            " player(s)!"
                        }), true, true, "");
                        return;
                    }
                    sender.RaReply(string.Concat(new object[]
                    {
                        query[0],
                        "#The proccess has occured an issue! Failures: ",
                        num5,
                        "\nLast error log:\n",
                        text11
                    }), false, true, "");
                    return;
                IL_391A:
                    bool flag222 = RoundStart.RoundLenght.Ticks == 0L;
                    bool flag223 = flag222;
                    bool flag224 = flag223;
                    if (flag224)
                    {
                        sender.RaReply(query[0].ToUpper() + "#The round has not yet started!", false, true, "");
                        return;
                    }
                    sender.RaReply(query[0].ToUpper() + "#Round time: " + RoundStart.RoundLenght.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture), true, true, "");
                    return;
                }
            IL_39C4:
                bool flag225 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true);
                bool flag226 = flag225;
                bool flag227 = flag226;
                if (flag227)
                {
                    bool flag228 = CharacterClassManager.ForceRoundStart();
                    bool flag229 = flag228;
                    bool flag230 = flag229;
                    bool flag231 = flag230;
                    if (flag231)
                    {
                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " forced round start.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                    }
                    sender.RaReply(query[0] + "#" + (flag228 ? "Done! Forced round start." : "Failed to force start."), flag228, true, "ServerEvents");
                    return;
                }
                return;
            IL_3A77:
                bool flag232 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true);
                bool flag233 = flag232;
                bool flag234 = flag233;
                if (flag234)
                {
                    return;
                }
                bool flag235 = query.Length != 2;
                bool flag236 = flag235;
                bool flag237 = flag236;
                if (flag237)
                {
                    sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                    return;
                }
                CommandProcessor.ProcessDoorQuery(sender, "UNLOCK", query[1]);
                return;
            }
            else
            {
                bool flag238 = num <= 3373006507U;
                bool flag239 = flag238;
                bool flag240 = flag239;
                if (flag240)
                {
                    bool flag241 = num <= 2409015043U;
                    bool flag242 = flag241;
                    bool flag243 = flag242;
                    if (flag243)
                    {
                        bool flag244 = num <= 2245226254U;
                        bool flag245 = flag244;
                        bool flag246 = flag245;
                        if (flag246)
                        {
                            bool flag247 = num <= 2184885908U;
                            bool flag248 = flag247;
                            bool flag249 = flag248;
                            if (flag249)
                            {
                                bool flag250 = num != 2163566540U;
                                bool flag251 = flag250;
                                bool flag252 = flag251;
                                if (flag252)
                                {
                                    bool flag253 = num != 2164589563U;
                                    bool flag254 = flag253;
                                    bool flag255 = flag254;
                                    if (flag255)
                                    {
                                        if (num != 2184885908U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag256 = !(text2 == "MUTE");
                                        bool flag257 = flag256;
                                        bool flag258 = flag257;
                                        if (flag258)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AA7B;
                                    }
                                    else
                                    {
                                        bool flag259 = !(text2 == "RL");
                                        bool flag260 = flag259;
                                        bool flag261 = flag260;
                                        if (flag261)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B30C;
                                    }
                                }
                                else
                                {
                                    bool flag262 = !(text2 == "NOCLIP");
                                    bool flag263 = flag262;
                                    bool flag264 = flag263;
                                    if (flag264)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_A5C5;
                                }
                            }
                            else
                            {
                                bool flag265 = num != 2194134619U;
                                bool flag266 = flag265;
                                bool flag267 = flag266;
                                if (flag267)
                                {
                                    bool flag268 = num != 2214547930U;
                                    bool flag269 = flag268;
                                    bool flag270 = flag269;
                                    if (flag270)
                                    {
                                        if (num != 2245226254U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag271 = !(text2 == "FC");
                                        bool flag272 = flag271;
                                        bool flag273 = flag272;
                                        if (flag273)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_8DA8;
                                    }
                                    else
                                    {
                                        bool flag274 = !(text2 == "LOCKDOWN");
                                        bool flag275 = flag274;
                                        bool flag276 = flag275;
                                        if (flag276)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AF05;
                                    }
                                }
                                else
                                {
                                    bool flag277 = !(text2 == "BROADCASTMONO");
                                    bool flag278 = flag277;
                                    bool flag279 = flag278;
                                    if (flag279)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_93C6;
                                }
                            }
                        }
                        else
                        {
                            bool flag280 = num <= 2331358749U;
                            bool flag281 = flag280;
                            bool flag282 = flag281;
                            if (flag282)
                            {
                                bool flag283 = num != 2246992121U;
                                bool flag284 = flag283;
                                bool flag285 = flag284;
                                if (flag285)
                                {
                                    bool flag286 = num != 2262052351U;
                                    bool flag287 = flag286;
                                    bool flag288 = flag287;
                                    if (flag288)
                                    {
                                        if (num != 2331358749U || !(text2 == "GOD"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag289 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "", true);
                                        bool flag290 = flag289;
                                        bool flag291 = flag290;
                                        if (flag291)
                                        {
                                            return;
                                        }
                                        bool flag292 = query.Length < 2;
                                        bool flag293 = flag292;
                                        bool flag294 = flag293;
                                        if (flag294)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
                                            return;
                                        }
                                        bool flag295 = query.Length == 2;
                                        bool flag296 = flag295;
                                        bool flag297 = flag296;
                                        if (flag297)
                                        {
                                            query = new string[]
                                            {
                                                query[0],
                                                query[1],
                                                ""
                                            };
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                                        {
                                            text,
                                            " ran the god command (new status: ",
                                            (query[2] == "") ? "TOGGLE" : query[2],
                                            ") on ",
                                            query[1],
                                            " players."
                                        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        CommandProcessor.StandardizedQueryModel1(sender, "GOD", query[1], query[2], out num5, out num6, out text11, out flag41, "");
                                        bool flag298 = flag41;
                                        bool flag299 = flag298;
                                        bool flag300 = flag299;
                                        if (flag300)
                                        {
                                            return;
                                        }
                                        bool flag301 = num5 == 0;
                                        bool flag302 = flag301;
                                        bool flag303 = flag302;
                                        if (flag303)
                                        {
                                            sender.RaReply("OVERWATCH#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                                            return;
                                        }
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            "OVERWATCH#The proccess has occured an issue! Failures: ",
                                            num5,
                                            "\nLast error log:\n",
                                            text11
                                        }), false, true, "AdminTools");
                                        return;
                                    }
                                    else
                                    {
                                        if (!(text2 == "UNBAN"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag304 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning, "", true);
                                        bool flag305 = flag304;
                                        bool flag306 = flag305;
                                        if (flag306)
                                        {
                                            return;
                                        }
                                        string text15 = string.Empty;
                                        bool flag307 = query.Length > 3;
                                        bool flag308 = flag307;
                                        bool flag309 = flag308;
                                        if (flag309)
                                        {
                                            text15 = query.Skip(3).Aggregate((string current, string n) => current + " " + n);
                                        }
                                        bool flag310 = query.Length < 4;
                                        bool flag311 = flag310;
                                        bool flag312 = flag311;
                                        if (flag312)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere ReasonHere OR unban ip IpAddressHere ReasonHere", false, true, "");
                                            return;
                                        }
                                        bool flag313 = text15.Contains("&");
                                        bool flag314 = flag313;
                                        bool flag315 = flag314;
                                        if (flag315)
                                        {
                                            sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                                        {
                                            text,
                                            " ran the unban ",
                                            query[1],
                                            " command on ",
                                            query[2],
                                            "."
                                        }), ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                                        text2 = query[1].ToLower();
                                        bool flag316 = text2 == "id" || text2 == "playerid" || text2 == "player";
                                        bool flag317 = flag316;
                                        bool flag318 = flag317;
                                        if (flag318)
                                        {
                                            BanHandler.RemoveBan(query[2], BanHandler.BanType.UserId);
                                            using (WebClient webClient = new WebClient())
                                            {
                                                webClient.Credentials = new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                                                webClient.DownloadString(string.Concat(new string[]
                                                {
                                                    "http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=",
                                                    query[2],
                                                    "&reason=",
                                                    text15,
                                                    "&aname=",
                                                    sender.Nickname,
                                                    "&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none")
                                                }));
                                                ServerConsole.AddLog("User " + query[2] + " Unbanned by RA user " + sender.Nickname);
                                            }
                                            sender.RaReply(query[0] + "#Done!", true, true, "");
                                            return;
                                        }
                                        bool flag319 = !(text2 == "ip") && !(text2 == "address");
                                        bool flag320 = flag319;
                                        bool flag321 = flag320;
                                        if (flag321)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere OR unban ip IpAddressHere", false, true, "");
                                            return;
                                        }
                                        BanHandler.RemoveBan(query[2], BanHandler.BanType.IP);
                                        sender.RaReply(query[0] + "#Done!", true, true, "");
                                        return;
                                    }
                                }
                                else
                                {
                                    bool flag322 = !(text2 == "CLEARBC");
                                    bool flag323 = flag322;
                                    bool flag324 = flag323;
                                    if (flag324)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9577;
                                }
                            }
                            else
                            {
                                bool flag325 = num != 2377374125U;
                                bool flag326 = flag325;
                                bool flag327 = flag326;
                                if (flag327)
                                {
                                    bool flag328 = num != 2390379445U;
                                    bool flag329 = flag328;
                                    bool flag330 = flag329;
                                    if (flag330)
                                    {
                                        if (num != 2409015043U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag331 = !(text2 == "CLOSE");
                                        bool flag332 = flag331;
                                        bool flag333 = flag332;
                                        if (flag333)
                                        {
                                            goto IL_B3F2;
                                        }
                                    }
                                    else
                                    {
                                        bool flag334 = !(text2 == "BCMONO");
                                        bool flag335 = flag334;
                                        bool flag336 = flag335;
                                        if (flag336)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_93C6;
                                    }
                                }
                                else
                                {
                                    bool flag337 = text2 == "GROUPS";
                                    bool flag338 = flag337;
                                    bool flag339 = flag338;
                                    if (flag339)
                                    {
                                        string text16 = "Groups defined on this server:";
                                        Dictionary<string, UserGroup> allGroups2 = ServerStatic.PermissionsHandler.GetAllGroups();
                                        ServerRoles.NamedColor[] namedColors = QueryProcessor.Localplayer.GetComponent<ServerRoles>().NamedColors;
                                        using (Dictionary<string, UserGroup>.Enumerator enumerator8 = allGroups2.GetEnumerator())
                                        {
                                            while (enumerator8.MoveNext())
                                            {
                                                KeyValuePair<string, UserGroup> permentry = enumerator8.Current;
                                                try
                                                {
                                                    text16 = string.Concat(new object[]
                                                    {
                                                        text16,
                                                        "\n",
                                                        permentry.Key,
                                                        " (",
                                                        permentry.Value.Permissions,
                                                        ") - <color=#",
                                                        namedColors.FirstOrDefault((ServerRoles.NamedColor y) => y.Name == permentry.Value.BadgeColor).ColorHex,
                                                        ">",
                                                        permentry.Value.BadgeText,
                                                        "</color> in color ",
                                                        permentry.Value.BadgeColor
                                                    });
                                                }
                                                catch
                                                {
                                                    text16 = string.Concat(new object[]
                                                    {
                                                        text16,
                                                        "\n",
                                                        permentry.Key,
                                                        " (",
                                                        permentry.Value.Permissions,
                                                        ") - ",
                                                        permentry.Value.BadgeText,
                                                        " in color ",
                                                        permentry.Value.BadgeColor
                                                    });
                                                }
                                                bool flag340 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning);
                                                bool flag341 = flag340;
                                                bool flag342 = flag341;
                                                if (flag342)
                                                {
                                                    text16 += " BN1";
                                                }
                                                bool flag343 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay);
                                                bool flag344 = flag343;
                                                bool flag345 = flag344;
                                                if (flag345)
                                                {
                                                    text16 += " BN2";
                                                }
                                                bool flag346 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning);
                                                bool flag347 = flag346;
                                                bool flag348 = flag347;
                                                if (flag348)
                                                {
                                                    text16 += " BN3";
                                                }
                                                bool flag349 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf);
                                                bool flag350 = flag349;
                                                bool flag351 = flag350;
                                                if (flag351)
                                                {
                                                    text16 += " FSE";
                                                }
                                                bool flag352 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator);
                                                bool flag353 = flag352;
                                                bool flag354 = flag353;
                                                if (flag354)
                                                {
                                                    text16 += " FSP";
                                                }
                                                bool flag355 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions);
                                                bool flag356 = flag355;
                                                bool flag357 = flag356;
                                                if (flag357)
                                                {
                                                    text16 += " FWR";
                                                }
                                                bool flag358 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems);
                                                bool flag359 = flag358;
                                                bool flag360 = flag359;
                                                if (flag360)
                                                {
                                                    text16 += " GIV";
                                                }
                                                bool flag361 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents);
                                                bool flag362 = flag361;
                                                bool flag363 = flag362;
                                                if (flag363)
                                                {
                                                    text16 += " EWA";
                                                }
                                                bool flag364 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents);
                                                bool flag365 = flag364;
                                                bool flag366 = flag365;
                                                if (flag366)
                                                {
                                                    text16 += " ERE";
                                                }
                                                bool flag367 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents);
                                                bool flag368 = flag367;
                                                bool flag369 = flag368;
                                                if (flag369)
                                                {
                                                    text16 += " ERO";
                                                }
                                                bool flag370 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup);
                                                bool flag371 = flag370;
                                                bool flag372 = flag371;
                                                if (flag372)
                                                {
                                                    text16 += " SGR";
                                                }
                                                bool flag373 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData);
                                                bool flag374 = flag373;
                                                bool flag375 = flag374;
                                                if (flag375)
                                                {
                                                    text16 += " GMD";
                                                }
                                                bool flag376 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch);
                                                bool flag377 = flag376;
                                                bool flag378 = flag377;
                                                if (flag378)
                                                {
                                                    text16 += " OVR";
                                                }
                                                bool flag379 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement);
                                                bool flag380 = flag379;
                                                bool flag381 = flag380;
                                                if (flag381)
                                                {
                                                    text16 += " FCM";
                                                }
                                                bool flag382 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement);
                                                bool flag383 = flag382;
                                                bool flag384 = flag383;
                                                if (flag384)
                                                {
                                                    text16 += " PLM";
                                                }
                                                bool flag385 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement);
                                                bool flag386 = flag385;
                                                bool flag387 = flag386;
                                                if (flag387)
                                                {
                                                    text16 += " PRM";
                                                }
                                                bool flag388 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands);
                                                bool flag389 = flag388;
                                                bool flag390 = flag389;
                                                if (flag390)
                                                {
                                                    text16 += " SCC";
                                                }
                                                bool flag391 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges);
                                                bool flag392 = flag391;
                                                bool flag393 = flag392;
                                                if (flag393)
                                                {
                                                    text16 += " VHB";
                                                }
                                                bool flag394 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs);
                                                bool flag395 = flag394;
                                                bool flag396 = flag395;
                                                if (flag396)
                                                {
                                                    text16 += " CFG";
                                                }
                                                bool flag397 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Broadcasting);
                                                bool flag398 = flag397;
                                                bool flag399 = flag398;
                                                if (flag399)
                                                {
                                                    text16 += " BRC";
                                                }
                                                bool flag400 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayerSensitiveDataAccess);
                                                bool flag401 = flag400;
                                                bool flag402 = flag401;
                                                if (flag402)
                                                {
                                                    text16 += " CDA";
                                                }
                                                bool flag403 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Noclip);
                                                bool flag404 = flag403;
                                                bool flag405 = flag404;
                                                if (flag405)
                                                {
                                                    text16 += " NCP";
                                                }
                                                bool flag406 = PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AFKImmunity);
                                                bool flag407 = flag406;
                                                bool flag408 = flag407;
                                                if (flag408)
                                                {
                                                    text16 += " AFK";
                                                }
                                            }
                                        }
                                        sender.RaReply(query[0].ToUpper() + "#" + text16, true, true, "");
                                        return;
                                    }
                                    goto IL_B3F2;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag409 = num <= 2881770144U;
                        bool flag410 = flag409;
                        bool flag411 = flag410;
                        if (flag411)
                        {
                            bool flag412 = num <= 2674786406U;
                            bool flag413 = flag412;
                            bool flag414 = flag413;
                            if (flag414)
                            {
                                bool flag415 = num != 2425129245U;
                                bool flag416 = flag415;
                                bool flag417 = flag416;
                                if (flag417)
                                {
                                    bool flag418 = num != 2453427760U;
                                    bool flag419 = flag418;
                                    bool flag420 = flag419;
                                    if (flag420)
                                    {
                                        if (num != 2674786406U || !(text2 == "BAN"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag421 = query.Length < 3;
                                        bool flag422 = flag421;
                                        bool flag423 = flag422;
                                        if (flag423)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                                            return;
                                        }
                                        string text17 = string.Empty;
                                        bool flag424 = text17.Contains("&");
                                        bool flag425 = flag424;
                                        bool flag426 = flag425;
                                        if (flag426)
                                        {
                                            sender.RaReply("The ban reason must not contain a & or else shit will hit the fan", false, false, "");
                                        }
                                        bool flag427 = query.Length > 3;
                                        bool flag428 = flag427;
                                        bool flag429 = flag428;
                                        if (flag429)
                                        {
                                            text17 = query.Skip(3).Aggregate((string current, string n) => current + " " + n);
                                        }
                                        bool flag430 = text17 == "";
                                        bool flag431 = flag430;
                                        bool flag432 = flag431;
                                        if (flag432)
                                        {
                                            sender.RaReply(string.Concat(new string[]
                                            {
                                                query[0].ToUpper(),
                                                "#To run this program, you must specify a reason use the text based RA console to do so, Autocorrection:   ban ",
                                                query[1],
                                                " ",
                                                query[2],
                                                " ReasonHere"
                                            }), false, true, "");
                                            return;
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                                        {
                                            text,
                                            " ran the ban command (duration: ",
                                            query[2],
                                            " min) on ",
                                            query[1],
                                            " players. Reason: ",
                                            (text17 == string.Empty) ? "(none)" : text17,
                                            "."
                                        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, text17);
                                        bool flag433 = flag41;
                                        bool flag434 = flag433;
                                        bool flag435 = flag434;
                                        if (flag435)
                                        {
                                            return;
                                        }
                                        bool flag436 = num5 == 0;
                                        bool flag437 = flag436;
                                        bool flag438 = flag437;
                                        if (flag438)
                                        {
                                            string text18 = "Banned";
                                            int num8;
                                            bool flag439 = int.TryParse(query[2], out num8);
                                            bool flag440 = flag439;
                                            bool flag441 = flag440;
                                            if (flag441)
                                            {
                                                text18 = ((num8 > 0) ? "Banned" : "Kicked");
                                            }
                                            sender.RaReply(string.Concat(new object[]
                                            {
                                                query[0],
                                                "#Done! ",
                                                text18,
                                                " ",
                                                num6,
                                                " player(s)!"
                                            }), true, true, "");
                                            return;
                                        }
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0],
                                            "#The proccess has occured an issue! Failures: ",
                                            num5,
                                            "\nLast error log:\n",
                                            text11
                                        }), false, true, "");
                                        return;
                                    }
                                    else
                                    {
                                        bool flag442 = !(text2 == "STOPNEXTROUND");
                                        bool flag443 = flag442;
                                        bool flag444 = flag443;
                                        if (flag444)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9740;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "CASSIE"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag445 = !CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true);
                                    bool flag446 = flag445;
                                    bool flag447 = flag446;
                                    if (flag447)
                                    {
                                        return;
                                    }
                                    bool flag448 = query.Length > 1;
                                    bool flag449 = flag448;
                                    bool flag450 = flag449;
                                    if (flag450)
                                    {
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), false, true);
                                        return;
                                    }
                                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                                    return;
                                }
                            }
                            else
                            {
                                bool flag451 = num != 2790969783U;
                                bool flag452 = flag451;
                                bool flag453 = flag452;
                                if (flag453)
                                {
                                    bool flag454 = num != 2796389774U;
                                    bool flag455 = flag454;
                                    bool flag456 = flag455;
                                    if (flag456)
                                    {
                                        if (num != 2881770144U || !(text2 == "INTERCOM-RESET"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag457 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                                        {
                                            PlayerPermissions.RoundEvents,
                                            PlayerPermissions.FacilityManagement,
                                            PlayerPermissions.PlayersManagement
                                        }, "ServerEvents", true);
                                        bool flag458 = flag457;
                                        bool flag459 = flag458;
                                        if (flag459)
                                        {
                                            return;
                                        }
                                        bool flag460 = Intercom.host.remainingCooldown <= 0f;
                                        bool flag461 = flag460;
                                        bool flag462 = flag461;
                                        if (flag462)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Intercom is already ready to use.", false, true, "ServerEvents");
                                            return;
                                        }
                                        Intercom.host.remainingCooldown = -1f;
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " reset the intercom cooldown.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                                        sender.RaReply(query[0].ToUpper() + "#Done! Intercom cooldown reset.", true, true, "ServerEvents");
                                        return;
                                    }
                                    else
                                    {
                                        bool flag463 = !(text2 == "DOORS");
                                        bool flag464 = flag463;
                                        bool flag465 = flag464;
                                        if (flag465)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B20C;
                                    }
                                }
                                else
                                {
                                    bool flag466 = !(text2 == "CASSIE_SILENT");
                                    bool flag467 = flag466;
                                    bool flag468 = flag467;
                                    if (flag468)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_9323;
                                }
                            }
                        }
                        else
                        {
                            bool flag469 = num <= 3216141499U;
                            bool flag470 = flag469;
                            bool flag471 = flag470;
                            if (flag471)
                            {
                                bool flag472 = num != 2943495851U;
                                bool flag473 = flag472;
                                bool flag474 = flag473;
                                if (flag474)
                                {
                                    bool flag475 = num != 3182344701U;
                                    bool flag476 = flag475;
                                    bool flag477 = flag476;
                                    if (flag477)
                                    {
                                        if (num != 3216141499U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag478 = !(text2 == "RCON");
                                        bool flag479 = flag478;
                                        bool flag480 = flag479;
                                        if (flag480)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_95E6;
                                    }
                                    else
                                    {
                                        bool flag481 = !(text2 == "IMUTE");
                                        bool flag482 = flag481;
                                        bool flag483 = flag482;
                                        if (flag483)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AA7B;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "HIDETAG"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag484 = CommandProcessor.IsPlayer(sender, query[0], "");
                                    bool flag485 = flag484;
                                    bool flag486 = flag485;
                                    if (flag486)
                                    {
                                        queryProcessor.GetComponent<ServerRoles>().HiddenBadge = queryProcessor.GetComponent<ServerRoles>().MyText;
                                        queryProcessor.GetComponent<ServerRoles>().NetworkGlobalBadge = null;
                                        queryProcessor.GetComponent<ServerRoles>().SetText(null);
                                        queryProcessor.GetComponent<ServerRoles>().SetColor(null);
                                        queryProcessor.GetComponent<ServerRoles>().GlobalSet = false;
                                        queryProcessor.GetComponent<ServerRoles>().RefreshHiddenTag();
                                        sender.RaReply(query[0].ToUpper() + "#Tag hidden!", true, true, "");
                                        return;
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                bool flag487 = num != 3234565675U;
                                bool flag488 = flag487;
                                bool flag489 = flag488;
                                if (flag489)
                                {
                                    bool flag490 = num != 3322673650U;
                                    bool flag491 = flag490;
                                    bool flag492 = flag491;
                                    if (flag492)
                                    {
                                        if (num != 3373006507U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag493 = !(text2 == "L");
                                        bool flag494 = flag493;
                                        bool flag495 = flag494;
                                        if (flag495)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B174;
                                    }
                                    else
                                    {
                                        bool flag496 = !(text2 == "C");
                                        bool flag497 = flag496;
                                        bool flag498 = flag497;
                                        if (flag498)
                                        {
                                            goto IL_B3F2;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "BRING"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag499 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools", true) || !CommandProcessor.IsPlayer(sender, query[0], "AdminTools");
                                    bool flag500 = flag499;
                                    bool flag501 = flag500;
                                    if (flag501)
                                    {
                                        return;
                                    }
                                    bool flag502 = query.Length != 2;
                                    bool flag503 = flag502;
                                    bool flag504 = flag503;
                                    if (flag504)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "AdminTools");
                                        return;
                                    }
                                    bool flag505 = playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.GetComponent<CharacterClassManager>().CurClass < RoleType.Scp173;
                                    bool flag506 = flag505;
                                    bool flag507 = flag506;
                                    if (flag507)
                                    {
                                        sender.RaReply("BRING#Command disabled when you are spectator!", false, true, "AdminTools");
                                        return;
                                    }
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the bring command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                    CommandProcessor.StandardizedQueryModel1(sender, "BRING", query[1], "", out num5, out num6, out text11, out flag41, "");
                                    bool flag508 = flag41;
                                    bool flag509 = flag508;
                                    bool flag510 = flag509;
                                    if (flag510)
                                    {
                                        return;
                                    }
                                    bool flag511 = num5 == 0;
                                    bool flag512 = flag511;
                                    bool flag513 = flag512;
                                    if (flag513)
                                    {
                                        sender.RaReply("BRING#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                                        return;
                                    }
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        "BRING#The proccess has occured an issue! Failures: ",
                                        num5,
                                        "\nLast error log:\n",
                                        text11
                                    }), false, true, "AdminTools");
                                    return;
                                }
                            }
                        }
                    }
                    bool flag514 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true);
                    bool flag515 = flag514;
                    bool flag516 = flag515;
                    if (flag516)
                    {
                        return;
                    }
                    bool flag517 = query.Length != 2;
                    bool flag518 = flag517;
                    bool flag519 = flag518;
                    if (flag519)
                    {
                        sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                        return;
                    }
                    CommandProcessor.ProcessDoorQuery(sender, "CLOSE", query[1]);
                    return;
                }
                else
                {
                    bool flag520 = num <= 3715648436U;
                    bool flag521 = flag520;
                    bool flag522 = flag521;
                    if (flag522)
                    {
                        bool flag523 = num <= 3554601228U;
                        bool flag524 = flag523;
                        bool flag525 = flag524;
                        if (flag525)
                        {
                            bool flag526 = num <= 3440740129U;
                            bool flag527 = flag526;
                            bool flag528 = flag527;
                            if (flag528)
                            {
                                bool flag529 = num != 3389784126U;
                                bool flag530 = flag529;
                                bool flag531 = flag530;
                                if (flag531)
                                {
                                    bool flag532 = num != 3406561745U;
                                    bool flag533 = flag532;
                                    bool flag534 = flag533;
                                    if (flag534)
                                    {
                                        if (num != 3440740129U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag535 = !(text2 == "SETCONFIG");
                                        bool flag536 = flag535;
                                        bool flag537 = flag536;
                                        if (flag537)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_9905;
                                    }
                                    else
                                    {
                                        bool flag538 = !(text2 == "N");
                                        bool flag539 = flag538;
                                        bool flag540 = flag539;
                                        if (flag540)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_A5C5;
                                    }
                                }
                                else
                                {
                                    bool flag541 = !(text2 == "O");
                                    bool flag542 = flag541;
                                    bool flag543 = flag542;
                                    if (flag543)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_B0DC;
                                }
                            }
                            else
                            {
                                bool flag544 = num != 3494757443U;
                                bool flag545 = flag544;
                                bool flag546 = flag545;
                                if (flag546)
                                {
                                    bool flag547 = num != 3510393926U;
                                    bool flag548 = flag547;
                                    bool flag549 = flag548;
                                    if (flag549)
                                    {
                                        if (num != 3554601228U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag550 = !(text2 == "FORCECLASS");
                                        bool flag551 = flag550;
                                        bool flag552 = flag551;
                                        if (flag552)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_8DA8;
                                    }
                                    else
                                    {
                                        bool flag553 = !(text2 == "OVERWATCH");
                                        bool flag554 = flag553;
                                        bool flag555 = flag554;
                                        if (flag555)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_A8C2;
                                    }
                                }
                                else
                                {
                                    bool flag556 = text2 == "CONTACT";
                                    bool flag557 = flag556;
                                    bool flag558 = flag557;
                                    if (flag558)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", ""), false, true, "");
                                        return;
                                    }
                                    goto IL_B3F2;
                                }
                            }
                        }
                        else
                        {
                            bool flag559 = num <= 3611046620U;
                            bool flag560 = flag559;
                            bool flag561 = flag560;
                            if (flag561)
                            {
                                bool flag562 = num != 3555373166U;
                                bool flag563 = flag562;
                                bool flag564 = flag563;
                                if (flag564)
                                {
                                    bool flag565 = num != 3556468092U;
                                    bool flag566 = flag565;
                                    bool flag567 = flag566;
                                    if (flag567)
                                    {
                                        if (num != 3611046620U || !(text2 == "GIVE"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag568 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GivingItems, "", true);
                                        bool flag569 = flag568;
                                        bool flag570 = flag569;
                                        if (flag570)
                                        {
                                            return;
                                        }
                                        bool flag571 = query.Length < 3;
                                        bool flag572 = flag571;
                                        bool flag573 = flag572;
                                        if (flag573)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                                            return;
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                                        {
                                            text,
                                            " ran the give command (ID: ",
                                            query[2],
                                            ") on ",
                                            query[1],
                                            " players."
                                        }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, "");
                                        bool flag574 = flag41;
                                        bool flag575 = flag574;
                                        bool flag576 = flag575;
                                        if (flag576)
                                        {
                                            return;
                                        }
                                        bool flag577 = num5 == 0;
                                        bool flag578 = flag577;
                                        bool flag579 = flag578;
                                        if (flag579)
                                        {
                                            sender.RaReply(string.Concat(new object[]
                                            {
                                                query[0],
                                                "#Done! The request affected ",
                                                num6,
                                                " player(s)!"
                                            }), true, true, "");
                                            return;
                                        }
                                        sender.RaReply(string.Concat(new object[]
                                        {
                                            query[0],
                                            "#The proccess has occured an issue! Failures: ",
                                            num5,
                                            "\nLast error log:\n",
                                            text11
                                        }), false, true, "");
                                        return;
                                    }
                                    else
                                    {
                                        bool flag580 = !(text2 == "LOBBYLOCK");
                                        bool flag581 = flag580;
                                        bool flag582 = flag581;
                                        if (flag582)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B37F;
                                    }
                                }
                                else
                                {
                                    bool flag583 = !(text2 == "GLOBALTAG");
                                    bool flag584 = flag583;
                                    bool flag585 = flag584;
                                    if (flag585)
                                    {
                                        goto IL_B3F2;
                                    }
                                }
                            }
                            else
                            {
                                bool flag586 = num != 3612018453U;
                                bool flag587 = flag586;
                                bool flag588 = flag587;
                                if (flag588)
                                {
                                    bool flag589 = num != 3709327981U;
                                    bool flag590 = flag589;
                                    bool flag591 = flag590;
                                    if (flag591)
                                    {
                                        if (num != 3715648436U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag592 = !(text2 == "BROADCAST");
                                        bool flag593 = flag592;
                                        bool flag594 = flag593;
                                        if (flag594)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_93C6;
                                    }
                                    else
                                    {
                                        bool flag595 = !(text2 == "BYPASS");
                                        bool flag596 = flag595;
                                        bool flag597 = flag596;
                                        if (flag597)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AD4C;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "SERVER_EVENT"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag598 = query.Length < 2;
                                    bool flag599 = flag598;
                                    bool flag600 = flag599;
                                    if (flag600)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                                        return;
                                    }
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " forced a server event: " + query[1].ToUpper(), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                    GameObject gameObject7 = GameObject.Find("Host");
                                    MTFRespawn component5 = gameObject7.GetComponent<MTFRespawn>();
                                    AlphaWarheadController component6 = gameObject7.GetComponent<AlphaWarheadController>();
                                    bool flag601 = true;
                                    text2 = query[1].ToUpper();
                                    num = PrivateImplementationDetails.ComputeStringHash(text2);
                                    bool flag602 = num <= 893689630U;
                                    bool flag603 = flag602;
                                    bool flag604 = flag603;
                                    if (flag604)
                                    {
                                        bool flag605 = num <= 687634301U;
                                        bool flag606 = flag605;
                                        bool flag607 = flag606;
                                        if (flag607)
                                        {
                                            bool flag608 = num != 368174935U;
                                            bool flag609 = flag608;
                                            bool flag610 = flag609;
                                            if (flag610)
                                            {
                                                if (num != 687634301U || !(text2 == "FORCE_MTF_RESPAWN"))
                                                {
                                                    goto IL_707E;
                                                }
                                                bool flag611 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents", true);
                                                bool flag612 = flag611;
                                                bool flag613 = flag612;
                                                if (flag613)
                                                {
                                                    return;
                                                }
                                                component5.nextWaveIsCI = false;
                                                component5.timeToNextRespawn = 0.1f;
                                                goto IL_7085;
                                            }
                                            else
                                            {
                                                bool flag614 = !(text2 == "ROUND_RESTART");
                                                bool flag615 = flag614;
                                                bool flag616 = flag615;
                                                if (flag616)
                                                {
                                                    goto IL_707E;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            bool flag617 = num != 734536883U;
                                            bool flag618 = flag617;
                                            bool flag619 = flag618;
                                            if (flag619)
                                            {
                                                bool flag620 = num != 892818482U;
                                                bool flag621 = flag620;
                                                bool flag622 = flag621;
                                                if (flag622)
                                                {
                                                    if (num != 893689630U || !(text2 == "FORCE_CI_RESPAWN"))
                                                    {
                                                        goto IL_707E;
                                                    }
                                                    bool flag623 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents", true);
                                                    bool flag624 = flag623;
                                                    bool flag625 = flag624;
                                                    if (flag625)
                                                    {
                                                        return;
                                                    }
                                                    component5.nextWaveIsCI = true;
                                                    component5.timeToNextRespawn = 0.1f;
                                                    goto IL_7085;
                                                }
                                                else
                                                {
                                                    bool flag626 = !(text2 == "ROUNDRESTART");
                                                    bool flag627 = flag626;
                                                    bool flag628 = flag627;
                                                    if (flag628)
                                                    {
                                                        goto IL_707E;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!(text2 == "DETONATION_CANCEL"))
                                                {
                                                    goto IL_707E;
                                                }
                                                bool flag629 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true);
                                                bool flag630 = flag629;
                                                bool flag631 = flag630;
                                                if (flag631)
                                                {
                                                    return;
                                                }
                                                component6.CancelDetonation();
                                                goto IL_7085;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool flag632 = num <= 1369092108U;
                                        bool flag633 = flag632;
                                        bool flag634 = flag633;
                                        if (flag634)
                                        {
                                            bool flag635 = num != 1240338700U;
                                            bool flag636 = flag635;
                                            bool flag637 = flag636;
                                            if (flag637)
                                            {
                                                if (num != 1369092108U)
                                                {
                                                    goto IL_707E;
                                                }
                                                bool flag638 = !(text2 == "RESTART");
                                                bool flag639 = flag638;
                                                bool flag640 = flag639;
                                                if (flag640)
                                                {
                                                    goto IL_707E;
                                                }
                                                goto IL_6FE7;
                                            }
                                            else
                                            {
                                                if (!(text2 == "TERMINATE_UNCONN"))
                                                {
                                                    goto IL_707E;
                                                }
                                                bool flag641 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true);
                                                bool flag642 = flag641;
                                                bool flag643 = flag642;
                                                if (flag643)
                                                {
                                                    return;
                                                }
                                                using (Dictionary<int, NetworkConnection>.ValueCollection.Enumerator enumerator9 = NetworkServer.connections.Values.GetEnumerator())
                                                {
                                                    while (enumerator9.MoveNext())
                                                    {
                                                        NetworkConnection networkConnection3 = enumerator9.Current;
                                                        bool flag644 = GameCore.Console.FindConnectedRoot(networkConnection3) == null;
                                                        bool flag645 = flag644;
                                                        bool flag646 = flag645;
                                                        if (flag646)
                                                        {
                                                            networkConnection3.Disconnect();
                                                            networkConnection3.Dispose();
                                                        }
                                                    }
                                                    goto IL_7085;
                                                }
                                            }
                                        }
                                        bool flag647 = num != 1615199367U;
                                        bool flag648 = flag647;
                                        bool flag649 = flag648;
                                        if (flag649)
                                        {
                                            bool flag650 = num != 1862592421U;
                                            bool flag651 = flag650;
                                            bool flag652 = flag651;
                                            if (flag652)
                                            {
                                                if (num != 1877204362U || !(text2 == "DETONATION_INSTANT"))
                                                {
                                                    goto IL_707E;
                                                }
                                                bool flag653 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true);
                                                bool flag654 = flag653;
                                                bool flag655 = flag654;
                                                if (flag655)
                                                {
                                                    return;
                                                }
                                                component6.InstantPrepare();
                                                component6.StartDetonation();
                                                component6.NetworktimeToDetonation = 5f;
                                                goto IL_7085;
                                            }
                                            else
                                            {
                                                bool flag656 = !(text2 == "RR");
                                                bool flag657 = flag656;
                                                bool flag658 = flag657;
                                                if (flag658)
                                                {
                                                    goto IL_707E;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (!(text2 == "DETONATION_START"))
                                            {
                                                goto IL_707E;
                                            }
                                            bool flag659 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents", true);
                                            bool flag660 = flag659;
                                            bool flag661 = flag660;
                                            if (flag661)
                                            {
                                                return;
                                            }
                                            component6.InstantPrepare();
                                            component6.StartDetonation();
                                            goto IL_7085;
                                        }
                                    }
                                IL_6FE7:
                                    bool flag662 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents", true);
                                    bool flag663 = flag662;
                                    bool flag664 = flag663;
                                    if (flag664)
                                    {
                                        return;
                                    }
                                    PlayerStats component7 = PlayerManager.localPlayer.GetComponent<PlayerStats>();
                                    bool isServer = component7.isServer;
                                    bool flag665 = isServer;
                                    bool flag666 = flag665;
                                    if (flag666)
                                    {
                                        component7.Roundrestart();
                                        goto IL_7085;
                                    }
                                    goto IL_7085;
                                IL_707E:
                                    flag601 = false;
                                IL_7085:
                                    bool flag667 = flag601;
                                    bool flag668 = flag667;
                                    bool flag669 = flag668;
                                    if (flag669)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Started event: " + query[1].ToUpper(), true, true, "ServerEvents");
                                        return;
                                    }
                                    sender.RaReply(query[0].ToUpper() + "#Incorrect event! (Doesn't exist)", false, true, "ServerEvents");
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        bool flag670 = num <= 3878768297U;
                        bool flag671 = flag670;
                        bool flag672 = flag671;
                        if (flag672)
                        {
                            bool flag673 = num <= 3754455428U;
                            bool flag674 = flag673;
                            bool flag675 = flag674;
                            if (flag675)
                            {
                                bool flag676 = num != 3730152828U;
                                bool flag677 = flag676;
                                bool flag678 = flag677;
                                if (flag678)
                                {
                                    bool flag679 = num != 3739414038U;
                                    bool flag680 = flag679;
                                    bool flag681 = flag680;
                                    if (flag681)
                                    {
                                        if (num != 3754455428U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag682 = !(text2 == "LLOCK");
                                        bool flag683 = flag682;
                                        bool flag684 = flag683;
                                        if (flag684)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_B37F;
                                    }
                                    else
                                    {
                                        bool flag685 = !(text2 == "GTAG");
                                        bool flag686 = flag685;
                                        bool flag687 = flag686;
                                        if (flag687)
                                        {
                                            goto IL_B3F2;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "GBAN-KICK"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag688 = (playerCommandSender == null || (!playerCommandSender.SR.RaEverywhere && !playerCommandSender.SR.Staff)) && CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.KickingAndShortTermBanning, "", true) && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement, "", true);
                                    bool flag689 = flag688;
                                    bool flag690 = flag689;
                                    if (flag690)
                                    {
                                        return;
                                    }
                                    bool flag691 = query.Length != 2;
                                    bool flag692 = flag691;
                                    bool flag693 = flag692;
                                    if (flag693)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 1 argument! (some parameters are missing)", false, true, "");
                                        return;
                                    }
                                    ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " globally banned and kicked " + query[1] + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                    CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], "0", out num5, out num6, out text11, out flag41, "");
                                    return;
                                }
                            }
                            else
                            {
                                bool flag694 = num != 3828307698U;
                                bool flag695 = flag694;
                                bool flag696 = flag695;
                                if (flag696)
                                {
                                    bool flag697 = num != 3856939955U;
                                    bool flag698 = flag697;
                                    bool flag699 = flag698;
                                    if (flag699)
                                    {
                                        if (num != 3878768297U)
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag700 = !(text2 == "ICOM");
                                        bool flag701 = flag700;
                                        bool flag702 = flag701;
                                        if (flag702)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_AC19;
                                    }
                                    else
                                    {
                                        if (!(text2 == "OBAN"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag703 = query.Length < 4;
                                        bool flag704 = flag703;
                                        bool flag705 = flag704;
                                        if (flag705)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#OBAN [NAME] [IP/SteamID/DiscordID] [MINUTES] (OPTIONAL REASON)\nOBAN [NAME] [MINUTES] [REASON]", false, true, "");
                                            return;
                                        }
                                        string text19 = (sender != null && !string.IsNullOrEmpty(sender.SenderId)) ? sender.Nickname : "Server";
                                        string text20 = query[1];
                                        string text21 = query[2];
                                        int num10;
                                        int num9 = int.TryParse(query[3], out num10) ? num10 : -1;
                                        bool flag706 = num9 == -1;
                                        bool flag707 = flag706;
                                        bool flag708 = flag707;
                                        string reason;
                                        if (flag708)
                                        {
                                            reason = ((query.Length > 3) ? string.Join(" ", query.Skip(3)) : string.Empty);
                                            int num11;
                                            num9 = (int.TryParse(text21, out num11) ? num11 : 0);
                                            foreach (GameObject gameObject8 in PlayerManager.players)
                                            {
                                                bool flag709 = gameObject8.GetComponent<NicknameSync>().MyNick.Contains(text20, StringComparison.OrdinalIgnoreCase);
                                                bool flag710 = flag709;
                                                bool flag711 = flag710;
                                                if (flag711)
                                                {
                                                    PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(gameObject8, num9, reason, text19);
                                                    string str3 = string.Concat(new object[]
                                                    {
                                                        "\nPlayer with name: ",
                                                        text20,
                                                        "\nWas banned for: ",
                                                        num9,
                                                        " minutes \nBy: ",
                                                        text19
                                                    });
                                                    sender.RaReply(query[0].ToUpper() + "#" + str3, true, true, "");
                                                    return;
                                                }
                                            }
                                            sender.RaReply(query[0].ToUpper() + "#Jugador con ese nombre no encontrado lol", false, true, "");
                                            return;
                                        }
                                        reason = ((query.Length > 4) ? string.Join(" ", query.Skip(4)) : string.Empty);
                                        bool flag712 = num9 < 0;
                                        bool flag713 = flag712;
                                        bool flag714 = flag713;
                                        if (flag714)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Creo que eres autista y no sabes leer. Pon bien los minutos", false, true, "");
                                            return;
                                        }
                                        string text22 = text21.Trim();
                                        long num12;
                                        bool flag715 = long.TryParse(text22, out num12);
                                        bool flag716 = text21.Contains(".");
                                        bool flag717 = flag716;
                                        bool flag718 = flag717;
                                        if (flag718)
                                        {
                                            bool flag719 = text21.Split(new char[]
                                            {
                                                '.'
                                            }).Length != 4;
                                            bool flag720 = flag719;
                                            bool flag721 = flag720;
                                            if (flag721)
                                            {
                                                sender.RaReply("Invalid IP: " + text21, false, true, "");
                                                return;
                                            }
                                            string text23 = text21.Contains("::ffff:") ? text21 : ("::ffff:" + text21);
                                            BanHandler.IssueBan(new BanDetails
                                            {
                                                OriginalName = text20,
                                                Id = text21,
                                                IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                                                Expires = DateTime.UtcNow.AddMinutes((double)num9).Ticks,
                                                Reason = reason,
                                                Issuer = text19
                                            }, BanHandler.BanType.IP);
                                            string str4 = string.Concat(new object[]
                                            {
                                                "\nPlayer with name: ",
                                                text20,
                                                "\nIP: ",
                                                text23,
                                                "\nWas banned for: ",
                                                num9,
                                                " minutes \nBy: ",
                                                text19
                                            });
                                            sender.RaReply(query[0].ToUpper() + "#" + str4, true, true, "");
                                            return;
                                        }
                                        else
                                        {
                                            bool flag722 = text22.Length == 17 && flag715;
                                            bool flag723 = flag722;
                                            bool flag724 = flag723;
                                            if (flag724)
                                            {
                                                BanHandler.IssueBan(new BanDetails
                                                {
                                                    OriginalName = text20,
                                                    Id = text22 + "@steam",
                                                    IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                                                    Expires = DateTime.UtcNow.AddMinutes((double)num9).Ticks,
                                                    Reason = reason,
                                                    Issuer = text19
                                                }, BanHandler.BanType.UserId);
                                                string str5 = string.Concat(new object[]
                                                {
                                                    "\nPlayer with name: ",
                                                    text20,
                                                    "\nSteamID64: ",
                                                    text22,
                                                    "\nWas banned for: ",
                                                    num9,
                                                    " minutes \nBy: ",
                                                    text19
                                                });
                                                sender.RaReply(query[0].ToUpper() + "#" + str5, true, true, "");
                                                return;
                                            }
                                            bool flag725 = text22.Length == 18 && flag715;
                                            bool flag726 = flag725;
                                            bool flag727 = flag726;
                                            if (flag727)
                                            {
                                                BanHandler.IssueBan(new BanDetails
                                                {
                                                    OriginalName = text20,
                                                    Id = text22 + "@discord",
                                                    IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                                                    Expires = DateTime.UtcNow.AddMinutes((double)num9).Ticks,
                                                    Reason = reason,
                                                    Issuer = text19
                                                }, BanHandler.BanType.UserId);
                                                string str6 = string.Concat(new object[]
                                                {
                                                    "\nPlayer with name: ",
                                                    text20,
                                                    "\nDiscordID: ",
                                                    text22,
                                                    "\nWas banned for: ",
                                                    num9,
                                                    " minutes \nBy: ",
                                                    text19
                                                });
                                                sender.RaReply(query[0].ToUpper() + "#" + str6, true, true, "");
                                                return;
                                            }
                                            bool flag728 = text22.Contains("@discord") || text22.Contains("@steam") || text22.Contains("@northwood");
                                            bool flag729 = flag728;
                                            bool flag730 = flag729;
                                            if (flag730)
                                            {
                                                BanHandler.IssueBan(new BanDetails
                                                {
                                                    OriginalName = text20,
                                                    Id = text22,
                                                    IssuanceTime = TimeBehaviour.CurrentTimestamp(),
                                                    Expires = DateTime.UtcNow.AddMinutes((double)num9).Ticks,
                                                    Reason = reason,
                                                    Issuer = text19
                                                }, BanHandler.BanType.UserId);
                                                string str7 = string.Concat(new object[]
                                                {
                                                    "\nPlayer with name: ",
                                                    text20,
                                                    "\nUserID: ",
                                                    text22,
                                                    "\nWas banned for: ",
                                                    num9,
                                                    " minutes \nBy: ",
                                                    text19
                                                });
                                                sender.RaReply(query[0].ToUpper() + "#" + str7, true, true, "");
                                                return;
                                            }
                                            sender.RaReply(query[0].ToUpper() + "#Invalid ID. The fuck you wrote, lad.", false, true, "");
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    bool flag731 = !(text2 == "RLOCK");
                                    bool flag732 = flag731;
                                    bool flag733 = flag732;
                                    if (flag733)
                                    {
                                        goto IL_B3F2;
                                    }
                                    goto IL_B30C;
                                }
                            }
                        }
                        else
                        {
                            bool flag734 = num <= 4024269157U;
                            bool flag735 = flag734;
                            bool flag736 = flag735;
                            if (flag736)
                            {
                                bool flag737 = num != 3974983732U;
                                bool flag738 = flag737;
                                bool flag739 = flag738;
                                if (flag739)
                                {
                                    bool flag740 = num != 4000555060U;
                                    bool flag741 = flag740;
                                    bool flag742 = flag741;
                                    if (flag742)
                                    {
                                        if (num != 4024269157U || !(text2 == "DESTROY"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag743 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true);
                                        bool flag744 = flag743;
                                        bool flag745 = flag744;
                                        if (flag745)
                                        {
                                            return;
                                        }
                                        bool flag746 = query.Length != 2;
                                        bool flag747 = flag746;
                                        bool flag748 = flag747;
                                        if (flag748)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                                            return;
                                        }
                                        CommandProcessor.ProcessDoorQuery(sender, "DESTROY", query[1]);
                                        return;
                                    }
                                    else
                                    {
                                        bool flag749 = !(text2 == "SLML_TAG");
                                        bool flag750 = flag749;
                                        bool flag751 = flag750;
                                        if (flag751)
                                        {
                                            goto IL_B3F2;
                                        }
                                        goto IL_97B3;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "SHOWTAG"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag752 = CommandProcessor.IsPlayer(sender, query[0], "") && !(queryProcessor == null);
                                    bool flag753 = flag752;
                                    bool flag754 = flag753;
                                    if (flag754)
                                    {
                                        queryProcessor.Roles.HiddenBadge = null;
                                        queryProcessor.Roles.RpcResetFixed();
                                        queryProcessor.Roles.RefreshPermissions(true);
                                        sender.RaReply(query[0].ToUpper() + "#Local tag refreshed!", true, true, "");
                                        return;
                                    }
                                    return;
                                }
                            }
                            else
                            {
                                bool flag755 = num != 4043923228U;
                                bool flag756 = flag755;
                                bool flag757 = flag756;
                                if (flag757)
                                {
                                    bool flag758 = num != 4076134438U;
                                    bool flag759 = flag758;
                                    bool flag760 = flag759;
                                    if (flag760)
                                    {
                                        if (num != 4209683619U || !(text2 == "WARHEAD"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag761 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "", true);
                                        bool flag762 = flag761;
                                        bool flag763 = flag762;
                                        if (flag763)
                                        {
                                            return;
                                        }
                                        bool flag764 = query.Length == 1;
                                        bool flag765 = flag764;
                                        bool flag766 = flag765;
                                        if (flag766)
                                        {
                                            sender.RaReply("Syntax: warhead (status|detonate|instant|cancel|enable|disable)", false, true, string.Empty);
                                            return;
                                        }
                                        text2 = query[1].ToLower();
                                        bool flag767 = !(text2 == "status");
                                        bool flag768 = flag767;
                                        bool flag769 = flag768;
                                        if (flag769)
                                        {
                                            bool flag770 = text2 == "detonate";
                                            bool flag771 = flag770;
                                            bool flag772 = flag771;
                                            if (flag772)
                                            {
                                                AlphaWarheadController.Host.StartDetonation();
                                                sender.RaReply("Detonation sequence started.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag773 = text2 == "instant";
                                            bool flag774 = flag773;
                                            bool flag775 = flag774;
                                            if (flag775)
                                            {
                                                AlphaWarheadController.Host.InstantPrepare();
                                                AlphaWarheadController.Host.StartDetonation();
                                                AlphaWarheadController.Host.NetworktimeToDetonation = 5f;
                                                sender.RaReply("Instant detonation started.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag776 = text2 == "cancel";
                                            bool flag777 = flag776;
                                            bool flag778 = flag777;
                                            if (flag778)
                                            {
                                                AlphaWarheadController.Host.CancelDetonation(null);
                                                sender.RaReply("Detonation has been canceled.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag779 = text2 == "enable";
                                            bool flag780 = flag779;
                                            bool flag781 = flag780;
                                            if (flag781)
                                            {
                                                AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
                                                sender.RaReply("Warhead has been enabled.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag782 = !(text2 == "disable");
                                            bool flag783 = flag782;
                                            bool flag784 = flag783;
                                            if (flag784)
                                            {
                                                sender.RaReply("WARHEAD: Unknown subcommand.", false, true, string.Empty);
                                                return;
                                            }
                                            AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
                                            sender.RaReply("Warhead has been disabled.", true, true, string.Empty);
                                            return;
                                        }
                                        else
                                        {
                                            bool flag785 = AlphaWarheadController.Host.detonated || Math.Abs(AlphaWarheadController.Host.timeToDetonation) < 0.001f;
                                            bool flag786 = flag785;
                                            bool flag787 = flag786;
                                            if (flag787)
                                            {
                                                sender.RaReply("Warhead has been detonated.", true, true, string.Empty);
                                                return;
                                            }
                                            bool inProgress = AlphaWarheadController.Host.inProgress;
                                            bool flag788 = inProgress;
                                            bool flag789 = flag788;
                                            if (flag789)
                                            {
                                                sender.RaReply("Detonation is in progress.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag790 = !AlphaWarheadOutsitePanel.nukeside.enabled;
                                            bool flag791 = flag790;
                                            bool flag792 = flag791;
                                            if (flag792)
                                            {
                                                sender.RaReply("Warhead is disabled.", true, true, string.Empty);
                                                return;
                                            }
                                            bool flag793 = AlphaWarheadController.Host.timeToDetonation > AlphaWarheadController.Host.RealDetonationTime();
                                            bool flag794 = flag793;
                                            bool flag795 = flag794;
                                            if (flag795)
                                            {
                                                sender.RaReply("Warhead is restarting.", true, true, string.Empty);
                                                return;
                                            }
                                            sender.RaReply("Warhead is ready to detonation.", true, true, string.Empty);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (!(text2 == "GOTO"))
                                        {
                                            goto IL_B3F2;
                                        }
                                        bool flag796 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools", true) || !CommandProcessor.IsPlayer(sender, query[0], "AdminTools");
                                        bool flag797 = flag796;
                                        bool flag798 = flag797;
                                        if (flag798)
                                        {
                                            return;
                                        }
                                        bool flag799 = query.Length != 2;
                                        bool flag800 = flag799;
                                        bool flag801 = flag800;
                                        if (flag801)
                                        {
                                            sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "AdminTools");
                                            return;
                                        }
                                        bool flag802 = playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.CurClass < RoleType.Scp173;
                                        bool flag803 = flag802;
                                        bool flag804 = flag803;
                                        if (flag804)
                                        {
                                            sender.RaReply("GOTO#Command disabled when you are spectator!", false, true, "AdminTools");
                                            return;
                                        }
                                        int id;
                                        bool flag805 = !int.TryParse(query[1], out id);
                                        bool flag806 = flag805;
                                        bool flag807 = flag806;
                                        if (flag807)
                                        {
                                            sender.RaReply("GOTO#Player ID must be an integer.", false, true, "AdminTools");
                                            return;
                                        }
                                        bool flag808 = query[1].Contains(".");
                                        bool flag809 = flag808;
                                        bool flag810 = flag809;
                                        if (flag810)
                                        {
                                            sender.RaReply("GOTO#Goto command requires exact one selected player.", false, true, "AdminTools");
                                            return;
                                        }
                                        GameObject gameObject9 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id);
                                        bool flag811 = gameObject9 == null;
                                        bool flag812 = flag811;
                                        bool flag813 = flag812;
                                        if (flag813)
                                        {
                                            sender.RaReply("GOTO#Can't find requested player.", false, true, "AdminTools");
                                            return;
                                        }
                                        bool flag814 = gameObject9.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || gameObject9.GetComponent<CharacterClassManager>().CurClass < RoleType.None;
                                        bool flag815 = flag814;
                                        bool flag816 = flag815;
                                        if (flag816)
                                        {
                                            sender.RaReply("GOTO#Requested player is a spectator!", false, true, "AdminTools");
                                            return;
                                        }
                                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the goto command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                                        queryProcessor.GetComponent<PlyMovementSync>().OverridePosition(gameObject9.GetComponent<PlyMovementSync>().RealModelPosition, 0f, false);
                                        sender.RaReply("GOTO#Done!", true, true, "AdminTools");
                                        return;
                                    }
                                }
                                else
                                {
                                    if (!(text2 == "PBC"))
                                    {
                                        goto IL_B3F2;
                                    }
                                    bool flag817 = query.Length < 4;
                                    bool flag818 = flag817;
                                    bool flag819 = flag818;
                                    if (flag819)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Usage: PBC <PLAYER> <TIME> <MESSAGE>", false, true, "");
                                        return;
                                    }
                                    bool flag820 = !CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true);
                                    bool flag821 = flag820;
                                    bool flag822 = flag821;
                                    if (flag822)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#No perms to PBC bro.", false, true, "");
                                        return;
                                    }
                                    uint num13;
                                    bool flag823 = !uint.TryParse(query[2], out num13) || num13 < 1U;
                                    bool flag824 = flag823;
                                    bool flag825 = flag824;
                                    if (flag825)
                                    {
                                        sender.RaReply(query[0].ToUpper() + "#Argument after the name must be a positive integer.", false, true, "");
                                        return;
                                    }
                                    string text24 = query[3];
                                    int num14;
                                    for (int k = 4; k < query.Length; k = num14 + 1)
                                    {
                                        text24 = text24 + " " + query[k];
                                        num14 = k;
                                    }
                                    foreach (GameObject gameObject10 in PlayerManager.players)
                                    {
                                        bool flag826 = gameObject10.GetComponent<NicknameSync>().MyNick.Contains(query[1], StringComparison.OrdinalIgnoreCase);
                                        bool flag827 = flag826;
                                        bool flag828 = flag827;
                                        if (flag828)
                                        {
                                            NetworkConnection connectionToClient = gameObject10.GetComponent<NetworkIdentity>().connectionToClient;
                                            bool flag829 = connectionToClient != null;
                                            bool flag830 = flag829;
                                            bool flag831 = flag830;
                                            if (flag831)
                                            {
                                                QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(connectionToClient, text24, num13, false);
                                                sender.RaReply(string.Concat(new string[]
                                                {
                                                    query[0].ToUpper(),
                                                    "#Sent: ",
                                                    text24,
                                                    " to: ",
                                                    gameObject10.GetComponent<NicknameSync>().MyNick
                                                }), true, true, "");
                                                ServerLogs.AddLog(ServerLogs.Modules.DataAccess, string.Concat(new string[]
                                                {
                                                    "Broadcasted: ",
                                                    text24,
                                                    " to: ",
                                                    gameObject10.GetComponent<NicknameSync>().MyNick,
                                                    " by ",
                                                    sender.Nickname
                                                }), ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                                            }
                                        }
                                    }
                                    sender.RaReply(query[0].ToUpper() + "#PBC command sent.", true, true, "");
                                    return;
                                }
                            }
                        }
                    }
                    bool flag832 = !CommandProcessor.IsPlayer(sender, query[0], "ServerEvents") || queryProcessor == null;
                    bool flag833 = flag832;
                    bool flag834 = flag833;
                    if (flag834)
                    {
                        return;
                    }
                    bool flag835 = string.IsNullOrEmpty(queryProcessor.Roles.PrevBadge);
                    bool flag836 = flag835;
                    bool flag837 = flag836;
                    if (flag837)
                    {
                        sender.RaReply(query[0].ToUpper() + "#You don't have global tag.", false, true, "");
                        return;
                    }
                    queryProcessor.Roles.HiddenBadge = null;
                    queryProcessor.Roles.RpcResetFixed();
                    queryProcessor.Roles.NetworkGlobalBadge = queryProcessor.Roles.PrevBadge;
                    queryProcessor.Roles.GlobalSet = true;
                    sender.RaReply(query[0].ToUpper() + "#Global tag refreshed!", true, true, "");
                    return;
                }
            IL_8DA8:
                bool flag838 = query.Length < 3;
                bool flag839 = flag838;
                bool flag840 = flag839;
                if (flag840)
                {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                    return;
                }
                int num15 = 0;
                bool flag841 = !int.TryParse(query[2], out num15) || num15 < 0 || num15 >= QueryProcessor.LocalCCM.Classes.Length;
                bool flag842 = flag841;
                bool flag843 = flag842;
                if (flag843)
                {
                    sender.RaReply(query[0].ToUpper() + "#Invalid class ID.", false, true, "");
                    return;
                }
                string fullName = QueryProcessor.LocalCCM.Classes.SafeGet(num15).fullName;
                GameObject gameObject11 = GameObject.Find("Host");
                bool flag844 = gameObject11 == null;
                bool flag845 = flag844;
                bool flag846 = flag845;
                if (flag846)
                {
                    sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", false, true, "");
                    return;
                }
                CharacterClassManager component8 = gameObject11.GetComponent<CharacterClassManager>();
                bool flag847 = component8 == null || !component8.isLocalPlayer || !component8.isServer || !component8.RoundStarted;
                bool flag848 = flag847;
                bool flag849 = flag848;
                if (flag849)
                {
                    sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", false, true, "");
                    return;
                }
                PlayerCommandSender playerCommandSender4;
                bool flag850 = (playerCommandSender4 = (sender as PlayerCommandSender)) != null && (query[1] == playerCommandSender4.PlayerId.ToString() || query[1] == playerCommandSender4.PlayerId + ".");
                bool flag851 = num15 == 2;
                bool flag852 = (flag850 && flag851 && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                {
                    PlayerPermissions.ForceclassWithoutRestrictions,
                    PlayerPermissions.ForceclassToSpectator,
                    PlayerPermissions.ForceclassSelf
                }, "", true)) || (flag850 && !flag851 && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                {
                    PlayerPermissions.ForceclassWithoutRestrictions,
                    PlayerPermissions.ForceclassSelf
                }, "", true)) || (!flag850 && flag851 && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                {
                    PlayerPermissions.ForceclassWithoutRestrictions,
                    PlayerPermissions.ForceclassToSpectator
                }, "", true)) || (!flag850 && !flag851 && !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
                {
                    PlayerPermissions.ForceclassWithoutRestrictions
                }, "", true));
                bool flag853 = flag852;
                bool flag854 = flag853;
                if (flag854)
                {
                    return;
                }
                bool flag855 = string.Equals(query[0], "role", StringComparison.OrdinalIgnoreCase);
                bool flag856 = flag855;
                bool flag857 = flag856;
                if (flag857)
                {
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                    {
                        text,
                        " ran the role command (ID: ",
                        query[2],
                        " - ",
                        fullName,
                        ") on ",
                        query[1],
                        " players."
                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                }
                else
                {
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                    {
                        text,
                        " ran the forceclass command (ID: ",
                        query[2],
                        " - ",
                        fullName,
                        ") on ",
                        query[1],
                        " players."
                    }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                }
                CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, "");
                bool flag858 = flag41;
                bool flag859 = flag858;
                bool flag860 = flag859;
                if (flag860)
                {
                    return;
                }
                bool flag861 = num5 == 0;
                bool flag862 = flag861;
                bool flag863 = flag862;
                if (flag863)
                {
                    sender.RaReply(string.Concat(new object[]
                    {
                        query[0],
                        "#Done! The request affected ",
                        num6,
                        " player(s)!"
                    }), true, true, "");
                    return;
                }
                sender.RaReply(string.Concat(new object[]
                {
                    query[0],
                    "#The proccess has occured an issue! Failures: ",
                    num5,
                    "\nLast error log:\n",
                    text11
                }), false, true, "");
                return;
            }
        IL_9323:
            bool flag864 = !CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true);
            bool flag865 = flag864;
            bool flag866 = flag865;
            if (flag866)
            {
                return;
            }
            bool flag867 = query.Length > 1;
            bool flag868 = flag867;
            bool flag869 = flag868;
            if (flag869)
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), false, false);
                return;
            }
            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
            return;
        IL_93C6:
            bool flag870 = CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true);
            bool flag871 = flag870;
            bool flag872 = flag871;
            if (flag872)
            {
                bool flag873 = query.Length < 2;
                bool flag874 = flag873;
                bool flag875 = flag874;
                if (flag875)
                {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "");
                }
                uint num16 = 0U;
                bool flag876 = !uint.TryParse(query[1], out num16) || num16 < 1U;
                bool flag877 = flag876;
                bool flag878 = flag877;
                if (flag878)
                {
                    sender.RaReply(query[0].ToUpper() + "#First argument must be a positive integer.", false, true, "");
                }
                string text25 = q.Substring(query[0].Length + query[1].Length + 2);
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                {
                    text,
                    " ran the broadcast command (duration: ",
                    query[1],
                    " seconds) with text \"",
                    text25,
                    "\" players."
                }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(text25, num16, query[0].Contains("mono", StringComparison.OrdinalIgnoreCase));
                sender.RaReply(query[0].ToUpper() + "#Broadcast sent.", false, true, "");
                return;
            }
            return;
        IL_9577:
            bool flag879 = CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true);
            bool flag880 = flag879;
            bool flag881 = flag880;
            if (flag881)
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcClearElements();
                sender.RaReply(query[0].ToUpper() + "#All broadcasts cleared.", false, true, "");
                return;
            }
            return;
        IL_95E6:
            bool flag882 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands, "", true);
            bool flag883 = flag882;
            bool flag884 = flag883;
            if (flag884)
            {
                return;
            }
            bool flag885 = query.Length < 2;
            bool flag886 = flag885;
            bool flag887 = flag886;
            if (flag887)
            {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 1 argument! (some parameters are missing)", false, true, "");
                return;
            }
            bool flag888 = query[1].StartsWith("!") && !ServerStatic.RolesConfig.GetBool("allow_central_server_commands_as_ServerConsoleCommands", false);
            bool flag889 = flag888;
            bool flag890 = flag889;
            if (flag890)
            {
                sender.RaReply(query[0] + "#Running central server commands in Remote Admin is disabled in RA config file!", false, true, "");
                return;
            }
            string text26 = query.Skip(1).Aggregate("", (string current, string arg) => current + arg + " ");
            text26 = text26.Substring(0, text26.Length - 1);
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " executed command as server console: " + text26 + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            ServerConsole.EnterCommand(text26, sender);
            sender.RaReply(query[0] + "#Command \"" + text26 + "\" executed in server console!", true, true, "");
            return;
        IL_9740:
            bool flag891 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands, "", true);
            bool flag892 = flag891;
            bool flag893 = flag892;
            if (flag893)
            {
                ServerStatic.StopNextRound = !ServerStatic.StopNextRound;
                sender.RaReply(query[0] + "#Server " + (ServerStatic.StopNextRound ? "WILL" : "WON'T") + " stop after next round.", true, true, "");
                return;
            }
            return;
        IL_97B3:
            bool flag894 = query.Length < 3;
            bool flag895 = flag894;
            bool flag896 = flag895;
            if (flag896)
            {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "");
                return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Logger, string.Concat(new string[]
            {
                text,
                " Requested a download of ",
                query[2],
                " on ",
                query[1],
                " players' computers."
            }), ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
            CommandProcessor.StandardizedQueryModel1(sender, query[0], query[1], query[2], out num5, out num6, out text11, out flag41, "");
            bool flag897 = flag41;
            bool flag898 = flag897;
            bool flag899 = flag898;
            if (flag899)
            {
                return;
            }
            bool flag900 = num5 == 0;
            bool flag901 = flag900;
            bool flag902 = flag901;
            if (flag902)
            {
                sender.RaReply(string.Concat(new object[]
                {
                    query[0],
                    "#Done! ",
                    num6,
                    " player(s) affected!"
                }), true, true, "");
                return;
            }
            sender.RaReply(string.Concat(new object[]
            {
                query[0],
                "#The proccess has occured an issue! Failures: ",
                num5,
                "\nLast error log:\n",
                text11
            }), false, true, "");
            return;
        IL_9905:
            bool flag903 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs, "ServerConfigs", true);
            bool flag904 = flag903;
            bool flag905 = flag904;
            if (flag905)
            {
                return;
            }
            bool flag906 = query.Length >= 3;
            bool flag907 = flag906;
            bool flag908 = flag907;
            if (flag908)
            {
                bool flag909 = query.Length > 3;
                bool flag910 = flag909;
                bool flag911 = flag910;
                if (flag911)
                {
                    string text27 = query[2];
                    int num17;
                    for (int l = 3; l < query.Length; l = num17 + 1)
                    {
                        text27 = text27 + " " + query[l];
                        num17 = l;
                    }
                    query = new string[]
                    {
                        query[0],
                        query[1],
                        text27
                    };
                }
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                {
                    text,
                    " ran the setconfig command (",
                    query[1],
                    ": ",
                    query[2],
                    ")."
                }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                text2 = query[1].ToUpper();
                num = PrivateImplementationDetails.ComputeStringHash(text2);
                bool flag912 = num <= 1283525746U;
                bool flag913 = flag912;
                bool flag914 = flag913;
                if (flag914)
                {
                    bool flag915 = num != 1107074724U;
                    bool flag916 = flag915;
                    bool flag917 = flag916;
                    if (flag917)
                    {
                        bool flag918 = num != 1119736759U;
                        bool flag919 = flag918;
                        bool flag920 = flag919;
                        if (flag920)
                        {
                            bool flag921 = num == 1283525746U && text2 == "PD_REFRESH_EXIT";
                            bool flag922 = flag921;
                            bool flag923 = flag922;
                            if (flag923)
                            {
                                bool refreshExit;
                                bool flag924 = bool.TryParse(query[2], out refreshExit);
                                bool flag925 = flag924;
                                bool flag926 = flag925;
                                if (flag926)
                                {
                                    UnityEngine.Object.FindObjectOfType<PocketDimensionTeleport>().RefreshExit = refreshExit;
                                    sender.RaReply(string.Concat(new string[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        refreshExit.ToString(),
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid bool!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                        else
                        {
                            bool flag927 = text2 == "HUMAN_GRENADE_MULTIPLIER";
                            bool flag928 = flag927;
                            bool flag929 = flag928;
                            if (flag929)
                            {
                                float num18;
                                bool flag930 = float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num18);
                                bool flag931 = flag930;
                                bool flag932 = flag931;
                                if (flag932)
                                {
                                    ConfigFile.ServerConfig.SetString("human_grenade_multiplier", num18.ToString());
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        num18,
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid float!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                    }
                    else
                    {
                        bool flag933 = text2 == "PLAYER_LIST_TITLE";
                        bool flag934 = flag933;
                        bool flag935 = flag934;
                        if (flag935)
                        {
                            string text28 = query[2] ?? string.Empty;
                            PlayerList.Title.Value = text28;
                            try
                            {
                                PlayerList.singleton.RefreshTitle();
                            }
                            catch (Exception ex4)
                            {
                                bool flag936 = !(ex4 is CommandInputException) && !(ex4 is InvalidOperationException);
                                bool flag937 = flag936;
                                bool flag938 = flag937;
                                if (flag938)
                                {
                                    throw;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#Could not set player list title [",
                                    text28,
                                    "]:\n",
                                    ex4.Message
                                }), false, true, "ServerConfigs");
                                return;
                            }
                            sender.RaReply(string.Concat(new string[]
                            {
                                query[0].ToUpper(),
                                "#Done! Config [",
                                query[1],
                                "] has been set to [",
                                PlayerList.singleton.syncServerName,
                                "]!"
                            }), true, true, "ServerConfigs");
                            return;
                        }
                    }
                }
                else
                {
                    bool flag939 = num <= 3161611648U;
                    bool flag940 = flag939;
                    bool flag941 = flag940;
                    if (flag941)
                    {
                        bool flag942 = num != 1585466007U;
                        bool flag943 = flag942;
                        bool flag944 = flag943;
                        if (flag944)
                        {
                            bool flag945 = num == 3161611648U && text2 == "SPAWN_PROTECT_TIME";
                            bool flag946 = flag945;
                            bool flag947 = flag946;
                            if (flag947)
                            {
                                int num19;
                                bool flag948 = int.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num19);
                                bool flag949 = flag948;
                                bool flag950 = flag949;
                                if (flag950)
                                {
                                    CharacterClassManager[] array = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
                                    int num20;
                                    for (int m = 0; m < array.Length; m = num20 + 1)
                                    {
                                        array[m].SProtectedDuration = (float)num19;
                                        num20 = m;
                                    }
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        num19,
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid integer!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                        else
                        {
                            bool flag951 = text2 == "SPAWN_PROTECT_DISABLE";
                            bool flag952 = flag951;
                            bool flag953 = flag952;
                            if (flag953)
                            {
                                bool flag955;
                                bool flag954 = bool.TryParse(query[2], out flag955);
                                bool flag956 = flag954;
                                bool flag957 = flag956;
                                if (flag957)
                                {
                                    CharacterClassManager[] array2 = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
                                    int num21;
                                    for (int n2 = 0; n2 < array2.Length; n2 = num21 + 1)
                                    {
                                        array2[n2].EnableSP = !flag955;
                                        num21 = n2;
                                    }
                                    sender.RaReply(string.Concat(new string[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        flag955.ToString(),
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid bool!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                    }
                    else
                    {
                        bool flag958 = num != 3269110662U;
                        bool flag959 = flag958;
                        bool flag960 = flag959;
                        if (flag960)
                        {
                            bool flag961 = num == 4162371295U && text2 == "FRIENDLY_FIRE";
                            bool flag962 = flag961;
                            bool flag963 = flag962;
                            if (flag963)
                            {
                                bool flag965;
                                bool flag964 = bool.TryParse(query[2], out flag965);
                                bool flag966 = flag964;
                                bool flag967 = flag966;
                                if (flag967)
                                {
                                    ServerConsole.FriendlyFire = flag965;
                                    WeaponManager[] array3 = UnityEngine.Object.FindObjectsOfType<WeaponManager>();
                                    int num23;
                                    for (int num22 = 0; num22 < array3.Length; num22 = num23 + 1)
                                    {
                                        array3[num22].NetworkfriendlyFire = flag965;
                                        num23 = num22;
                                    }
                                    sender.RaReply(string.Concat(new string[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        flag965.ToString(),
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid bool!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                        else
                        {
                            bool flag968 = text2 == "SCP_GRENADE_MULTIPLIER";
                            bool flag969 = flag968;
                            bool flag970 = flag969;
                            if (flag970)
                            {
                                float num24;
                                bool flag971 = float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num24);
                                bool flag972 = flag971;
                                bool flag973 = flag972;
                                if (flag973)
                                {
                                    ConfigFile.ServerConfig.SetString("scp_grenade_multiplier", num24.ToString());
                                    sender.RaReply(string.Concat(new object[]
                                    {
                                        query[0].ToUpper(),
                                        "#Done! Config [",
                                        query[1],
                                        "] has been set to [",
                                        num24,
                                        "]!"
                                    }), true, true, "ServerConfigs");
                                    return;
                                }
                                sender.RaReply(string.Concat(new string[]
                                {
                                    query[0].ToUpper(),
                                    "#",
                                    query[1],
                                    " has invalid value, ",
                                    query[2],
                                    " is not a valid float!"
                                }), false, true, "ServerConfigs");
                                return;
                            }
                        }
                    }
                }
                sender.RaReply(query[0].ToUpper() + "#Invalid config " + query[1], false, true, "ServerConfigs");
                return;
            }
            sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, "ServerConfigs");
            return;
        IL_A5C5:
            bool flag974 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Noclip, "", true);
            bool flag975 = flag974;
            bool flag976 = flag975;
            if (flag976)
            {
                return;
            }
            bool flag977 = query.Length >= 2;
            bool flag978 = flag977;
            bool flag979 = flag978;
            if (flag979)
            {
                bool flag980 = query.Length == 2;
                bool flag981 = flag980;
                bool flag982 = flag981;
                if (flag982)
                {
                    query = new string[]
                    {
                        query[0],
                        query[1],
                        ""
                    };
                }
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                {
                    text,
                    " ran the noclip command (new status: ",
                    (query[2] == "") ? "TOGGLE" : query[2],
                    ") on ",
                    query[1],
                    " players."
                }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                CommandProcessor.StandardizedQueryModel1(sender, "NOCLIP", query[1], query[2], out num5, out num6, out text11, out flag41, "");
                bool flag983 = flag41;
                bool flag984 = flag983;
                bool flag985 = flag984;
                if (flag985)
                {
                    return;
                }
                bool flag986 = num5 == 0;
                bool flag987 = flag986;
                bool flag988 = flag987;
                if (flag988)
                {
                    sender.RaReply("NOCLIP#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                    return;
                }
                sender.RaReply(string.Concat(new object[]
                {
                    "NOCLIP#The proccess has occured an issue! Failures: ",
                    num5,
                    "\nLast error log:\n",
                    text11
                }), false, true, "AdminTools");
                return;
            }
            else
            {
                PlayerCommandSender playerCommandSender5;
                bool flag989 = (playerCommandSender5 = (sender as PlayerCommandSender)) == null;
                bool flag990 = flag989;
                bool flag991 = flag990;
                if (flag991)
                {
                    sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
                    return;
                }
                CommandProcessor.StandardizedQueryModel1(sender, "NOCLIP", playerCommandSender5.PlayerId.ToString(), "", out num5, out num6, out text11, out flag41, "");
                bool flag992 = num5 == 0;
                bool flag993 = flag992;
                bool flag994 = flag993;
                if (flag994)
                {
                    sender.RaReply("NOCLIP#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                    return;
                }
                sender.RaReply(string.Concat(new object[]
                {
                    "NOCLIP#The proccess has occured an issue! Failures: ",
                    num5,
                    "\nLast error log:\n",
                    text11
                }), false, true, "AdminTools");
                return;
            }
        IL_A8C2:
            bool flag995 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Overwatch, "", true);
            bool flag996 = flag995;
            bool flag997 = flag996;
            if (flag997)
            {
                return;
            }
            bool flag998 = query.Length < 2;
            bool flag999 = flag998;
            bool flag1000 = flag999;
            if (flag1000)
            {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
                return;
            }
            bool flag1001 = query.Length == 2;
            bool flag1002 = flag1001;
            bool flag1003 = flag1002;
            if (flag1003)
            {
                query = new string[]
                {
                    query[0],
                    query[1],
                    ""
                };
            }
            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
            {
                text,
                " ran the overwatch command (new status: ",
                (query[2] == "") ? "TOGGLE" : query[2],
                ") on ",
                query[1],
                " players."
            }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            CommandProcessor.StandardizedQueryModel1(sender, "OVERWATCH", query[1], query[2], out num5, out num6, out text11, out flag41, "");
            bool flag1004 = flag41;
            bool flag1005 = flag1004;
            bool flag1006 = flag1005;
            if (flag1006)
            {
                return;
            }
            bool flag1007 = num5 == 0;
            bool flag1008 = flag1007;
            bool flag1009 = flag1008;
            if (flag1009)
            {
                sender.RaReply("OVERWATCH#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                return;
            }
            sender.RaReply(string.Concat(new object[]
            {
                "OVERWATCH#The proccess has occured an issue! Failures: ",
                num5,
                "\nLast error log:\n",
                text11
            }), false, true, "AdminTools");
            return;
        IL_AA7B:
            bool flag1010 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[]
            {
                PlayerPermissions.BanningUpToDay,
                PlayerPermissions.LongTermBanning,
                PlayerPermissions.PlayersManagement
            }, "", true);
            bool flag1011 = flag1010;
            bool flag1012 = flag1011;
            if (flag1012)
            {
                return;
            }
            bool flag1013 = query.Length != 2;
            bool flag1014 = flag1013;
            bool flag1015 = flag1014;
            if (flag1015)
            {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", false, true, "PlayersManagement");
                return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
            {
                text,
                " ran the ",
                query[0].ToLower(),
                " command on ",
                query[1],
                " players."
            }), ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
            CommandProcessor.StandardizedQueryModel1(sender, query[0].ToUpper(), query[1], null, out num5, out num6, out text11, out flag41, "");
            bool flag1016 = flag41;
            bool flag1017 = flag1016;
            bool flag1018 = flag1017;
            if (flag1018)
            {
                return;
            }
            bool flag1019 = num5 == 0;
            bool flag1020 = flag1019;
            bool flag1021 = flag1020;
            if (flag1021)
            {
                sender.RaReply(string.Concat(new object[]
                {
                    query[0].ToUpper(),
                    "#Done! The request affected ",
                    num6,
                    " player(s)!"
                }), true, true, "PlayersManagement");
                return;
            }
            sender.RaReply(string.Concat(new object[]
            {
                query[0].ToUpper(),
                "#The proccess has occured an issue! Failures: ",
                num5,
                "\nLast error log:\n",
                text11
            }), false, true, "PlayersManagement");
            return;
        IL_AC19:
            bool flag1022 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Broadcasting, "ServerEvents", true) || !CommandProcessor.IsPlayer(sender, query[0], "ServerEvents");
            bool flag1023 = flag1022;
            bool flag1024 = flag1023;
            if (flag1024)
            {
                return;
            }
            bool adminSpeaking = Intercom.AdminSpeaking;
            bool flag1025 = adminSpeaking;
            bool flag1026 = flag1025;
            if (flag1026)
            {
                Intercom.AdminSpeaking = false;
                Intercom.host.RequestTransmission(null);
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ended global intercom transmission.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom revoked.", true, true, "ServerEvents");
                return;
            }
            bool speaking = Intercom.host.speaking;
            bool flag1027 = speaking;
            bool flag1028 = flag1027;
            if (flag1028)
            {
                sender.RaReply(query[0].ToUpper() + "#Intercom is being used by someone else.", false, true, "ServerEvents");
                return;
            }
            Intercom.AdminSpeaking = true;
            Intercom.host.RequestTransmission(queryProcessor.GetComponent<Intercom>().gameObject);
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " requested global voice over the intercom.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
            sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom granted.", true, true, "ServerEvents");
            return;
        IL_AD4C:
            bool flag1029 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true);
            bool flag1030 = flag1029;
            bool flag1031 = flag1030;
            if (flag1031)
            {
                return;
            }
            bool flag1032 = query.Length < 2;
            bool flag1033 = flag1032;
            bool flag1034 = flag1033;
            if (flag1034)
            {
                sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
                return;
            }
            bool flag1035 = query.Length == 2;
            bool flag1036 = flag1035;
            bool flag1037 = flag1036;
            if (flag1037)
            {
                query = new string[]
                {
                    query[0],
                    query[1],
                    ""
                };
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
            {
                text,
                " ran the bypass mode command (new status: ",
                (query[2] == "") ? "TOGGLE" : query[2],
                ") on ",
                query[1],
                " players."
            }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            CommandProcessor.StandardizedQueryModel1(sender, "BYPASS", query[1], query[2], out num5, out num6, out text11, out flag41, "");
            bool flag1038 = flag41;
            bool flag1039 = flag1038;
            bool flag1040 = flag1039;
            if (flag1040)
            {
                return;
            }
            bool flag1041 = num5 == 0;
            bool flag1042 = flag1041;
            bool flag1043 = flag1042;
            if (flag1043)
            {
                sender.RaReply("BYPASS#Done! The request affected " + num6 + " player(s)!", true, true, "AdminTools");
                return;
            }
            sender.RaReply(string.Concat(new object[]
            {
                "BYPASS#The proccess has occured an issue! Failures: ",
                num5,
                "\nLast error log:\n",
                text11
            }), false, true, "AdminTools");
            return;
        IL_AF05:
            bool flag1044 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true);
            bool flag1045 = flag1044;
            bool flag1046 = flag1045;
            if (flag1046)
            {
                return;
            }
            bool flag1047 = !QueryProcessor.Lockdown;
            bool flag1048 = flag1047;
            bool flag1049 = flag1048;
            if (flag1049)
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " enabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
                foreach (Door door in UnityEngine.Object.FindObjectsOfType<Door>())
                {
                    bool flag1050 = !door.locked;
                    bool flag1051 = flag1050;
                    bool flag1052 = flag1051;
                    if (flag1052)
                    {
                        door.lockdown = true;
                        door.UpdateLock();
                    }
                }
                QueryProcessor.Lockdown = true;
                sender.RaReply(query[0] + "#Lockdown enabled!", true, true, "AdminTools");
                return;
            }
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " disabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            foreach (Door door2 in UnityEngine.Object.FindObjectsOfType<Door>())
            {
                bool lockdown = door2.lockdown;
                bool flag1053 = lockdown;
                bool flag1054 = flag1053;
                if (flag1054)
                {
                    door2.lockdown = false;
                    door2.UpdateLock();
                }
            }
            QueryProcessor.Lockdown = false;
            sender.RaReply(query[0] + "#Lockdown disabled!", true, true, "AdminTools");
            return;
        IL_B0DC:
            bool flag1055 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true);
            bool flag1056 = flag1055;
            bool flag1057 = flag1056;
            if (flag1057)
            {
                return;
            }
            bool flag1058 = query.Length != 2;
            bool flag1059 = flag1058;
            bool flag1060 = flag1059;
            if (flag1060)
            {
                sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                return;
            }
            CommandProcessor.ProcessDoorQuery(sender, "OPEN", query[1]);
            return;
        IL_B174:
            bool flag1061 = !CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement", true);
            bool flag1062 = flag1061;
            bool flag1063 = flag1062;
            if (flag1063)
            {
                return;
            }
            bool flag1064 = query.Length != 2;
            bool flag1065 = flag1064;
            bool flag1066 = flag1065;
            if (flag1066)
            {
                sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", false, true, "");
                return;
            }
            CommandProcessor.ProcessDoorQuery(sender, "LOCK", query[1]);
            return;
        IL_B20C:
            bool flag1067 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools", true);
            bool flag1068 = flag1067;
            bool flag1069 = flag1068;
            if (flag1069)
            {
                string text29 = "List of named doors in the facility:\n";
                List<string> list2 = (from item in UnityEngine.Object.FindObjectsOfType<Door>()
                                      where !string.IsNullOrEmpty(item.DoorName)
                                      select string.Concat(new string[]
                                      {
                    item.DoorName,
                    " - ",
                    item.isOpen ? "<color=green>OPENED</color>" : "<color=orange>CLOSED</color>",
                    item.locked ? " <color=red>[LOCKED]</color>" : "",
                    string.IsNullOrEmpty(item.permissionLevel) ? "" : " <color=blue>[CARD REQUIRED]</color>"
                                      })).ToList<string>();
                list2.Sort();
                text29 += list2.Aggregate((string current, string adding) => current + "\n" + adding);
                sender.RaReply(query[0] + "#" + text29, true, true, "");
                return;
            }
            return;
        IL_B30C:
            bool flag1070 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "", true);
            bool flag1071 = flag1070;
            bool flag1072 = flag1071;
            if (flag1072)
            {
                RoundSummary.RoundLock = !RoundSummary.RoundLock;
                sender.RaReply(query[0].ToUpper() + "#Round lock " + (RoundSummary.RoundLock ? "enabled!" : "disabled!"), true, true, "ServerEvents");
                return;
            }
            return;
        IL_B37F:
            bool flag1073 = CommandProcessor.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "", true);
            bool flag1074 = flag1073;
            bool flag1075 = flag1074;
            if (flag1075)
            {
                RoundStart.LobbyLock = !RoundStart.LobbyLock;
                sender.RaReply(query[0].ToUpper() + "#Lobby lock " + (RoundStart.LobbyLock ? "enabled!" : "disabled!"), true, true, "ServerEvents");
                return;
            }
            return;
        IL_B3F2:
            sender.RaReply("SYSTEM#Unknown command!", false, true, "");
        }

        // Token: 0x060018A7 RID: 6311 RVA: 0x0009097C File Offset: 0x0008EB7C
        private static void ProcessDoorQuery(CommandSender sender, string command, string door)
        {
            if (!CommandProcessor.CheckPermissions(sender, command.ToUpper(), PlayerPermissions.FacilityManagement, "", true))
            {
                return;
            }
            if (string.IsNullOrEmpty(door))
            {
                sender.RaReply(command + "#Please select door first.", false, true, "DoorsManagement");
                return;
            }
            bool flag = false;
            door = door.ToUpper();
            byte b;
            if (!(command == "OPEN"))
            {
                if (!(command == "LOCK"))
                {
                    if (!(command == "UNLOCK"))
                    {
                        if (!(command == "DESTROY"))
                        {
                            b = 0;
                        }
                        else
                        {
                            b = 4;
                        }
                    }
                    else
                    {
                        b = 3;
                    }
                }
                else
                {
                    b = 2;
                }
            }
            else
            {
                b = 1;
            }
            foreach (Door door2 in UnityEngine.Object.FindObjectsOfType<Door>())
            {
                if (!(door2.DoorName.ToUpper() != door) || (!(door != "**") && !(door2.permissionLevel == "UNACCESSIBLE")) || (!(door != "!*") && string.IsNullOrEmpty(door2.DoorName)) || (!(door != "*") && !string.IsNullOrEmpty(door2.DoorName) && !(door2.permissionLevel == "UNACCESSIBLE")))
                {
                    switch (b)
                    {
                        case 0:
                            door2.SetStateWithSound(false);
                            break;
                        case 1:
                            door2.SetStateWithSound(true);
                            break;
                        case 2:
                            door2.commandlock = true;
                            door2.UpdateLock();
                            break;
                        case 3:
                            door2.commandlock = false;
                            door2.UpdateLock();
                            break;
                        case 4:
                            door2.DestroyDoor(true);
                            break;
                    }
                    flag = true;
                }
            }
            sender.RaReply(command + "#" + (flag ? string.Concat(new string[]
            {
                "Door ",
                door,
                " ",
                command.ToLower(),
                "ed."
            }) : ("Can't find door " + door + ".")), flag, true, "DoorsManagement");
            if (flag)
            {
                ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
                {
                    sender.Nickname,
                    " ",
                    (sender is PlayerCommandSender) ? ("(" + sender.SenderId + ") ") : "",
                    command.ToLower(),
                    command.ToLower().EndsWith("e") ? "d" : "ed",
                    " door ",
                    door,
                    "."
                }), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
            }
        }

        // Token: 0x060018A8 RID: 6312 RVA: 0x00090BFC File Offset: 0x0008EDFC
        private static void StandardizedQueryModel1(CommandSender sender, string programName, string playerIds, string xValue, out int failures, out int successes, out string error, out bool replySent, string arg1 = "")
        {
            error = string.Empty;
            failures = 0;
            successes = 0;
            replySent = false;
            programName = programName.ToUpper();
            int num;
            if (int.TryParse(xValue, out num) || programName.StartsWith("SLML") || programName == "SETGROUP" || programName == "OVERWATCH" || programName == "NOCLIP" || programName == "BYPASS" || programName == "HEAL" || programName == "GOD" || programName == "BRING" || programName == "MUTE" || programName == "UNMUTE" || programName == "IMUTE" || programName == "IUNMUTE" || programName == "DOORTP")
            {
                List<int> list = new List<int>();
                try
                {
                    string[] source = playerIds.Split(new char[]
                    {
                        '.'
                    });
                    list.AddRange((from item in source
                                   where !string.IsNullOrEmpty(item)
                                   select item).Select(new Func<string, int>(int.Parse)));
                    UserGroup userGroup = null;
                    Vector3 vector = Vector3.down;
                    if (programName == "BAN")
                    {
                        replySent = true;
                        if (num < 0)
                        {
                            num = 0;
                        }
                        if (num == 0 && !CommandProcessor.CheckPermissions(sender, programName, new PlayerPermissions[]
                        {
                            PlayerPermissions.KickingAndShortTermBanning,
                            PlayerPermissions.BanningUpToDay,
                            PlayerPermissions.LongTermBanning
                        }, "", true))
                        {
                            return;
                        }
                        if (num > 0 && num <= 60 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.KickingAndShortTermBanning, "", true))
                        {
                            return;
                        }
                        if (num > 60 && num <= 1440 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.BanningUpToDay, "", true))
                        {
                            return;
                        }
                        if (num > 1440 && !CommandProcessor.CheckPermissions(sender, programName, PlayerPermissions.LongTermBanning, "", true))
                        {
                            return;
                        }
                        replySent = false;
                    }
                    else if (programName.StartsWith("SLML"))
                    {
                        MarkupTransceiver markupTransceiver = UnityEngine.Object.FindObjectOfType<MarkupTransceiver>();
                        if (programName.Contains("_STYLE"))
                        {
                            markupTransceiver.RequestStyleDownload(xValue, list.ToArray());
                        }
                        else if (programName.Contains("_TAG"))
                        {
                            markupTransceiver.Transmit(xValue, list.ToArray());
                        }
                    }
                    else if (!(programName == "SETGROUP"))
                    {
                        if (programName == "DOORTP")
                        {
                            xValue = xValue.ToUpper();
                            Door door = UnityEngine.Object.FindObjectsOfType<Door>().FirstOrDefault((Door dr) => dr.DoorName.ToUpper() == xValue);
                            if (door == null)
                            {
                                replySent = true;
                                sender.RaReply(programName + "#Can't find door " + xValue + ".", false, true, "DoorsManagement");
                                return;
                            }
                            vector = door.transform.position;
                            vector.y += 2.5f;
                            for (byte b = 0; b < 21; b += 1)
                            {
                                if (b == 0)
                                {
                                    vector.x += 1.5f;
                                }
                                else if (b < 3)
                                {
                                    vector.x += 1f;
                                }
                                else if (b == 4)
                                {
                                    vector = door.transform.position;
                                    vector.y += 2.5f;
                                    vector.z += 1.5f;
                                }
                                else if (b < 10 && b % 2 == 0)
                                {
                                    vector.z += 1f;
                                }
                                else if (b < 10)
                                {
                                    vector.x += 1f;
                                }
                                else if (b == 10)
                                {
                                    vector = door.transform.position;
                                    vector.y += 2.5f;
                                    vector.x -= 1.5f;
                                }
                                else if (b < 13)
                                {
                                    vector.x -= 1f;
                                }
                                else if (b == 14)
                                {
                                    vector = door.transform.position;
                                    vector.y += 2.5f;
                                    vector.z -= 1.5f;
                                }
                                else if (b % 2 == 0)
                                {
                                    vector.z -= 1f;
                                }
                                else
                                {
                                    vector.x -= 1f;
                                }
                                if (FallDamage.CheckUnsafePosition(vector))
                                {
                                    break;
                                }
                                if (b == 20)
                                {
                                    vector = Vector3.zero;
                                }
                            }
                            if (vector == Vector3.zero)
                            {
                                replySent = true;
                                sender.RaReply(programName + "#Can't find safe place to teleport to door " + xValue + ".", false, true, "DoorsManagement");
                                return;
                            }
                        }
                    }
                    else if (xValue != "-1")
                    {
                        userGroup = ServerStatic.PermissionsHandler.GetGroup(xValue);
                        if (userGroup == null)
                        {
                            replySent = true;
                            sender.RaReply(programName + "#Requested group doesn't exist! Use group \"-1\" to remove user group.", false, true, "");
                            return;
                        }
                    }
                    bool isVerified = ServerStatic.PermissionsHandler.IsVerified;
                    string nickname = sender.Nickname;
                    foreach (int num2 in list)
                    {
                        try
                        {
                            foreach (GameObject gameObject in PlayerManager.players)
                            {
                                if (num2 == gameObject.GetComponent<QueryProcessor>().PlayerId)
                                {
                                    uint num3 = PrivateImplementationDetails.ComputeStringHash(programName);
                                    if (num3 <= 2184885908U)
                                    {
                                        if (num3 <= 1159084506U)
                                        {
                                            if (num3 <= 858808885U)
                                            {
                                                if (num3 != 329200923U)
                                                {
                                                    if (num3 != 858808885U)
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (!(programName == "DOORTP"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    gameObject.GetComponent<PlyMovementSync>().OverridePosition(vector, 0f, false);
                                                    goto IL_1025;
                                                }
                                                else
                                                {
                                                    if (!(programName == "UNMUTE"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    MuteHandler.RevokePersistantMute(gameObject.GetComponent<CharacterClassManager>().UserId);
                                                    gameObject.GetComponent<CharacterClassManager>().SetMuted(false);
                                                    goto IL_1025;
                                                }
                                            }
                                            else if (num3 != 945458267U)
                                            {
                                                if (num3 != 1094345139U)
                                                {
                                                    if (num3 != 1159084506U)
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (!(programName == "SETGROUP"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    goto IL_A40;
                                                }
                                                else if (!(programName == "SETHP"))
                                                {
                                                    goto IL_1025;
                                                }
                                            }
                                            else
                                            {
                                                if (!(programName == "HEAL"))
                                                {
                                                    goto IL_1025;
                                                }
                                                PlayerStats component = gameObject.GetComponent<PlayerStats>();
                                                if (xValue != null && num > 0)
                                                {
                                                    component.HealHPAmount((float)num);
                                                    goto IL_1025;
                                                }
                                                component.SetHPAmount(component.ccm.Classes.SafeGet(component.ccm.CurClass).maxHP);
                                                goto IL_1025;
                                            }
                                        }
                                        else if (num3 <= 1630279262U)
                                        {
                                            if (num3 != 1297180441U)
                                            {
                                                if (num3 != 1630279262U)
                                                {
                                                    goto IL_1025;
                                                }
                                                if (!(programName == "IUNMUTE"))
                                                {
                                                    goto IL_1025;
                                                }
                                                MuteHandler.RevokePersistantMute("ICOM-" + gameObject.GetComponent<CharacterClassManager>().UserId);
                                                gameObject.GetComponent<CharacterClassManager>().NetworkIntercomMuted = false;
                                                goto IL_1025;
                                            }
                                            else
                                            {
                                                if (!(programName == "ROLE"))
                                                {
                                                    goto IL_1025;
                                                }
                                                QueryProcessor.LocalCCM.SetPlayersClass((RoleType)num, gameObject, true, false);
                                                goto IL_1025;
                                            }
                                        }
                                        else if (num3 != 1894470373U)
                                        {
                                            if (num3 != 2163566540U)
                                            {
                                                if (num3 != 2184885908U)
                                                {
                                                    goto IL_1025;
                                                }
                                                if (!(programName == "MUTE"))
                                                {
                                                    goto IL_1025;
                                                }
                                                MuteHandler.IssuePersistantMute(gameObject.GetComponent<CharacterClassManager>().UserId);
                                                gameObject.GetComponent<CharacterClassManager>().SetMuted(true);
                                                goto IL_1025;
                                            }
                                            else
                                            {
                                                if (!(programName == "NOCLIP"))
                                                {
                                                    goto IL_1025;
                                                }
                                                ServerRoles component2 = gameObject.GetComponent<ServerRoles>();
                                                if (string.IsNullOrEmpty(xValue))
                                                {
                                                    component2.NoclipReady = !component2.NoclipReady;
                                                    goto IL_1025;
                                                }
                                                if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    component2.NoclipReady = true;
                                                    goto IL_1025;
                                                }
                                                if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    component2.NoclipReady = false;
                                                    goto IL_1025;
                                                }
                                                replySent = true;
                                                sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                                                return;
                                            }
                                        }
                                        else if (!(programName == "HP"))
                                        {
                                            goto IL_1025;
                                        }
                                        gameObject.GetComponent<PlayerStats>().SetHPAmount(num);
                                    }
                                    else
                                    {
                                        if (num3 <= 3234565675U)
                                        {
                                            if (num3 <= 2331358749U)
                                            {
                                                if (num3 != 2245226254U)
                                                {
                                                    if (num3 != 2331358749U)
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (!(programName == "GOD"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (string.IsNullOrEmpty(xValue))
                                                    {
                                                        gameObject.GetComponent<CharacterClassManager>().GodMode = !gameObject.GetComponent<CharacterClassManager>().GodMode;
                                                        goto IL_1025;
                                                    }
                                                    if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        gameObject.GetComponent<CharacterClassManager>().GodMode = true;
                                                        goto IL_1025;
                                                    }
                                                    if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        gameObject.GetComponent<CharacterClassManager>().GodMode = false;
                                                        goto IL_1025;
                                                    }
                                                    replySent = true;
                                                    sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                                                    return;
                                                }
                                                else if (!(programName == "FC"))
                                                {
                                                    goto IL_1025;
                                                }
                                            }
                                            else if (num3 != 2674786406U)
                                            {
                                                if (num3 != 3182344701U)
                                                {
                                                    if (num3 != 3234565675U)
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (!(programName == "BRING"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    if (!(sender is PlayerCommandSender))
                                                    {
                                                        return;
                                                    }
                                                    if (gameObject.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || gameObject.GetComponent<CharacterClassManager>().CurClass == RoleType.None)
                                                    {
                                                        failures++;
                                                        continue;
                                                    }
                                                    Vector3 realModelPosition = ((PlayerCommandSender)sender).Processor.GetComponent<PlyMovementSync>().RealModelPosition;
                                                    gameObject.GetComponent<PlyMovementSync>().OverridePosition(realModelPosition, 0f, false);
                                                    goto IL_1025;
                                                }
                                                else
                                                {
                                                    if (!(programName == "IMUTE"))
                                                    {
                                                        goto IL_1025;
                                                    }
                                                    MuteHandler.IssuePersistantMute("ICOM-" + gameObject.GetComponent<CharacterClassManager>().UserId);
                                                    gameObject.GetComponent<CharacterClassManager>().NetworkIntercomMuted = true;
                                                    goto IL_1025;
                                                }
                                            }
                                            else
                                            {
                                                if (!(programName == "BAN"))
                                                {
                                                    goto IL_1025;
                                                }
                                                string myNick = gameObject.GetComponent<NicknameSync>().MyNick;
                                                if (!sender.FullPermissions)
                                                {
                                                    UserGroup group = gameObject.GetComponent<ServerRoles>().Group;
                                                    int b2 = (group != null) ? group.RequiredKickPower : 0;
                                                    if (b2 > sender.KickPower)
                                                    {
                                                        failures++;
                                                        string text = string.Format("You can't kick/ban {0}. Required kick power: {1}, your kick power: {2}.", myNick, b2, sender.KickPower);
                                                        error = text;
                                                        sender.RaReply(text, false, true, string.Empty);
                                                        continue;
                                                    }
                                                }
                                                if (isVerified && gameObject.GetComponent<ServerRoles>().BypassStaff)
                                                {
                                                    QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(gameObject, 0, arg1, nickname);
                                                    goto IL_1025;
                                                }
                                                if (num == 0 && ConfigFile.ServerConfig.GetBool("broadcast_kicks", false))
                                                {
                                                    QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_kick_text", "%nick% has been kicked from this server.").Replace("%nick%", myNick), (uint)ConfigFile.ServerConfig.GetInt("broadcast_kick_duration", 5), false);
                                                }
                                                else if (num != 0 && ConfigFile.ServerConfig.GetBool("broadcast_bans", true))
                                                {
                                                    QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", myNick), (uint)ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), false);
                                                }
                                                QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(gameObject, num, arg1, nickname);
                                                goto IL_1025;
                                            }
                                        }
                                        else if (num3 <= 3554601228U)
                                        {
                                            if (num3 != 3510393926U)
                                            {
                                                if (num3 != 3554601228U)
                                                {
                                                    goto IL_1025;
                                                }
                                                if (!(programName == "FORCECLASS"))
                                                {
                                                    goto IL_1025;
                                                }
                                            }
                                            else
                                            {
                                                if (!(programName == "OVERWATCH"))
                                                {
                                                    goto IL_1025;
                                                }
                                                if (string.IsNullOrEmpty(xValue))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().CmdSetOverwatchStatus(2);
                                                    goto IL_1025;
                                                }
                                                if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().CmdSetOverwatchStatus(1);
                                                    goto IL_1025;
                                                }
                                                if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().CmdSetOverwatchStatus(0);
                                                    goto IL_1025;
                                                }
                                                replySent = true;
                                                sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                                                return;
                                            }
                                        }
                                        else if (num3 != 3611046620U)
                                        {
                                            if (num3 != 3709327981U)
                                            {
                                                if (num3 != 3730152828U)
                                                {
                                                    goto IL_1025;
                                                }
                                                if (!(programName == "GBAN-KICK"))
                                                {
                                                    goto IL_1025;
                                                }
                                                QueryProcessor.Localplayer.GetComponent<BanPlayer>().KickUser(gameObject, "Globally Banned", nickname, true);
                                                goto IL_1025;
                                            }
                                            else
                                            {
                                                if (!(programName == "BYPASS"))
                                                {
                                                    goto IL_1025;
                                                }
                                                if (string.IsNullOrEmpty(xValue))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().BypassMode = !gameObject.GetComponent<ServerRoles>().BypassMode;
                                                    goto IL_1025;
                                                }
                                                if (xValue == "1" || string.Equals(xValue, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "enable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "on", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().BypassMode = true;
                                                    goto IL_1025;
                                                }
                                                if (xValue == "0" || string.Equals(xValue, "false", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "disable", StringComparison.OrdinalIgnoreCase) || string.Equals(xValue, "off", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    gameObject.GetComponent<ServerRoles>().BypassMode = false;
                                                    goto IL_1025;
                                                }
                                                replySent = true;
                                                sender.RaReply(programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            if (!(programName == "GIVE"))
                                            {
                                                goto IL_1025;
                                            }
                                            try
                                            {
                                                gameObject.GetComponent<Inventory>().AddNewItem((ItemType)num, -4.65664672E+11f, 0, 0, 0);
                                                goto IL_1025;
                                            }
                                            catch (Exception ex)
                                            {
                                                failures++;
                                                error = ex.Message;
                                                continue;
                                            }
                                            goto IL_A40;
                                        }
                                        QueryProcessor.LocalCCM.SetPlayersClass((RoleType)num, gameObject, false, false);
                                    }
                                IL_1025:
                                    successes++;
                                    continue;
                                IL_A40:
                                    ServerRoles component3 = gameObject.GetComponent<ServerRoles>();
                                    if (component3.PublicKeyAccepted)
                                    {
                                        component3.SetGroup(userGroup, false, true, false);
                                        goto IL_1025;
                                    }
                                    failures++;
                                    goto IL_1025;
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            failures++;
                            error = ex2.Message + "\nStackTrace:\n" + ex2.StackTrace;
                        }
                    }
                    return;
                }
                catch (Exception ex3)
                {
                    replySent = true;
                    sender.RaReply(string.Concat(new string[]
                    {
                        programName,
                        "#An unexpected problem has occurred!\nMessage: ",
                        ex3.Message,
                        "\nStackTrace: ",
                        ex3.StackTrace,
                        "\nAt: ",
                        ex3.Source,
                        "\nMost likely the PlayerId array was not in the correct format."
                    }), false, true, "");
                    throw;
                }
            }
            replySent = true;
            sender.RaReply(programName + "#The third parameter has to be an integer!", false, true, "");
        }

        // Token: 0x060018A9 RID: 6313 RVA: 0x00091D94 File Offset: 0x0008FF94
        private static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions[] perm, string replyScreen = "", bool reply = true)
        {
            if (ServerStatic.IsDedicated && sender.FullPermissions)
            {
                return true;
            }
            if (ServerStatic.PermissionsHandler.IsPermitted(sender.Permissions, perm))
            {
                return true;
            }
            if (!reply)
            {
                return false;
            }
            string text = perm.Aggregate("", (string current, PlayerPermissions p) => current + "\n- " + p);
            text.Remove(text.Length - 3);
            sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nYou need at least one of following permissions: " + text, false, true, replyScreen);
            return false;
        }

        // Token: 0x060018AA RID: 6314 RVA: 0x00091E20 File Offset: 0x00090020
        private static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm, string replyScreen = "", bool reply = true)
        {
            if (ServerStatic.IsDedicated && sender.FullPermissions)
            {
                return true;
            }
            if (PermissionsHandler.IsPermitted(sender.Permissions, perm))
            {
                return true;
            }
            if (reply)
            {
                sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm, false, true, replyScreen);
            }
            return false;
        }

        // Token: 0x060018AB RID: 6315 RVA: 0x00019548 File Offset: 0x00017748
        private static bool IsPlayer(CommandSender sender, string queryZero, string replyScreen = "")
        {
            if (sender is PlayerCommandSender)
            {
                return true;
            }
            sender.RaReply(queryZero + "#This command can be executed only from the game level.", false, true, replyScreen);
            return false;
        }
    }
}
