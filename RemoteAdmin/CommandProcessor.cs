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
		// Token: 0x06001810 RID: 6160 RVA: 0x00078914 File Offset: 0x00076B14
		internal static void ProcessQuery(string q, CommandSender sender)
	{
		string[] query = q.Split(' ');
		string text = sender.Nickname;
		PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
		QueryProcessor queryProcessor = playerCommandSender?.Processor;
		if (playerCommandSender != null)
		{
			text = text + " (" + playerCommandSender.CCM.UserId + ")";
		}
		int failures;
		int successes;
		string error;
		bool replySent;
		switch (query[0].ToUpper())
		{
		case "HELLO":
			sender.RaReply(query[0].ToUpper() + "#Hello World!", success: true, logToConsole: true, "");
			break;
		case "HELP":
			sender.RaReply(query[0].ToUpper() + "#This should be useful!", success: true, logToConsole: true, "");
			break;
		case "CASSIE":
			if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
			{
				if (query.Length > 1)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), makeHold: false, makeNoise: true);
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
				}
			}
			break;
		case "CASSIE_SILENTNOISE":
		case "CASSIE_SN":
		case "CASSIE_SILENT":
		case "CASSIE_SL":
			if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
			{
				if (query.Length > 1)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement(q.Remove(0, 7), makeHold: false, makeNoise: false);
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
				}
			}
			break;
		case "BROADCAST":
		case "BC":
		case "ALERT":
		case "BROADCASTMONO":
		case "BCMONO":
		case "ALERTMONO":
			if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
			{
				if (query.Length < 2)
				{
					sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
				}
				uint result9 = 0u;
				if (!uint.TryParse(query[1], out result9) || result9 < 1)
				{
					sender.RaReply(query[0].ToUpper() + "#First argument must be a positive integer.", success: false, logToConsole: true, "");
				}
				string text7 = q.Substring(query[0].Length + query[1].Length + 2);
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the broadcast command (duration: " + query[1] + " seconds) with text \"" + text7 + "\" players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(text7, result9, query[0].Contains("mono", StringComparison.OrdinalIgnoreCase));
				sender.RaReply(query[0].ToUpper() + "#Broadcast sent.", success: false, logToConsole: true, "");
			}
			break;
		case "CLEARBC":
		case "BCCLEAR":
		case "CLEARALERT":
		case "ALERTCLEAR":
			if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcClearElements();
				sender.RaReply(query[0].ToUpper() + "#All broadcasts cleared.", success: false, logToConsole: true, "");
			}
			break;
		case "BAN":
		case "OFFLINEBAN":
		case "OBAN":
			if (query.Length >= 3)
			{
				string text8 = string.Empty;
				if (query.Length > 3)
				{
					text8 = query.Skip(3).Aggregate((string current, string n) => current + " " + n);
				}
				int num = 0;
				try
				{
					num = Misc.RelativeTimeToSeconds(query[2], 60);
				}
				catch
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid time: " + query[2], success: false, logToConsole: true, "");
					return;
				}
				if (num < 0)
				{
					num = 0;
					query[2] = "0";
				}
				if ((num == 0 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
				{
					PlayerPermissions.KickingAndShortTermBanning,
					PlayerPermissions.BanningUpToDay,
					PlayerPermissions.LongTermBanning
				})) || (num > 0 && num <= 3600 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.KickingAndShortTermBanning)) || (num > 3600 && num <= 86400 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.BanningUpToDay)) || (num > 86400 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning)))
				{
					break;
				}
				if (query[0].Equals("BAN", StringComparison.OrdinalIgnoreCase))
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the ban command (duration: " + query[2] + ") on " + query[1] + " players. Reason: " + ((text8 == string.Empty) ? "(none)" : text8) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					List<int> list2 = Misc.ProcessRaPlayersList(query[1]);
					if (list2 == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Invalid players list.", success: false, logToConsole: true, "");
						break;
					}
					ushort num2 = 0;
					ushort num3 = 0;
					string text9 = string.Empty;
					foreach (int item in list2)
					{
						try
						{
							ReferenceHub hub = ReferenceHub.GetHub(item);
							string myNick2;
							if (hub == null)
							{
								num3 = (ushort)(num3 + 1);
							}
							else
							{
								myNick2 = hub.nicknameSync.MyNick;
								if (sender.FullPermissions)
								{
									goto IL_152a;
								}
								byte b = hub.serverRoles.Group?.RequiredKickPower ?? 0;
								if (b <= sender.KickPower)
								{
									goto IL_152a;
								}
								num3 = (ushort)(num3 + 1);
								text9 = $"You can't kick/ban {myNick2}. Required kick power: {b}, your kick power: {sender.KickPower}.";
								sender.RaReply(text9, success: false, logToConsole: true, string.Empty);
							}
							goto end_IL_1496;
							IL_152a:
							if (ServerStatic.PermissionsHandler.IsVerified && hub.serverRoles.BypassStaff)
							{
								QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(hub.gameObject, 0, text8, sender.Nickname);
							}
							else
							{
								if (num == 0 && ConfigFile.ServerConfig.GetBool("broadcast_kicks"))
								{
									QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_kick_text", "%nick% has been kicked from this server.").Replace("%nick%", myNick2), (uint)ConfigFile.ServerConfig.GetInt("broadcast_kick_duration", 5), monospaced: false);
								}
								else if (num != 0 && ConfigFile.ServerConfig.GetBool("broadcast_bans", def: true))
								{
									QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", myNick2), (uint)ConfigFile.ServerConfig.GetInt("broadcast_ban_duration", 5), monospaced: false);
								}
								QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(hub.gameObject, num, text8, sender.Nickname);
							}
							num2 = (ushort)(num2 + 1);
							end_IL_1496:;
						}
						catch (Exception ex5)
						{
							num3 = (ushort)(num3 + 1);
							Debug.Log(ex5);
							text9 = "Error occured during banning: " + ex5.Message + ".\n" + ex5.StackTrace;
						}
					}
					if (num3 == 0)
					{
						string text10 = "Banned";
						if (int.TryParse(query[2], out int result10))
						{
							text10 = ((result10 > 0) ? "Banned" : "Kicked");
						}
						sender.RaReply(query[0] + "#Done! " + text10 + " " + num2 + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The process has occured an issue! Failures: " + num3 + "\nLast error log:\n" + text9, success: false, logToConsole: true, "");
					}
				}
				else
				{
					bool flag7 = Misc.IsIpAddress(query[1]);
					if (!flag7 && !query[1].Contains("@"))
					{
						sender.RaReply(query[0].ToUpper() + "#Target must be a valid UserID or IP (v4 or v6) address.", success: false, logToConsole: true, "");
						break;
					}
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the offline ban command (duration: " + query[2] + ") on " + (flag7 ? "IP address" : "UserID") + query[1] + ". Reason: " + ((text8 == string.Empty) ? "(none)" : text8) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					BanHandler.IssueBan(new BanDetails
					{
						OriginalName = "Unknown - offline ban",
						Id = query[1],
						IssuanceTime = TimeBehaviour.CurrentTimestamp(),
						Expires = TimeBehaviour.GetBanExpieryTime((uint)num),
						Reason = text8,
						Issuer = sender.Nickname
					}, flag7 ? BanHandler.BanType.IP : BanHandler.BanType.UserId);
					sender.RaReply(query[0].ToUpper() + "#" + (flag7 ? "IP address " : "UserID ") + query[1] + " has been banned from this server.", success: true, logToConsole: true, string.Empty);
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "GBAN-KICK":
			if (playerCommandSender == null || (!playerCommandSender.SR.RaEverywhere && !playerCommandSender.SR.Staff))
			{
				sender.RaReply(query[0].ToUpper() + "#You don't have permissions to run this command!", success: false, logToConsole: true, "");
			}
			if (query.Length != 2)
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 1 argument! (some parameters are missing)", success: false, logToConsole: true, "");
				break;
			}
			ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " globally banned and kicked " + query[1] + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
			StandardizedQueryModel1(sender, query[0], query[1], "0", out failures, out successes, out error, out replySent);
			break;
		case "SUDO":
		case "RCON":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands))
			{
				if (query.Length < 2)
				{
					sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 1 argument! (some parameters are missing)", success: false, logToConsole: true, "");
					break;
				}
				if (query[1].StartsWith("!") && !ServerStatic.RolesConfig.GetBool("allow_central_server_commands_as_ServerConsoleCommands"))
				{
					sender.RaReply(query[0] + "#Running central server commands in Remote Admin is disabled in RA config file!", success: false, logToConsole: true, "");
					break;
				}
				string text6 = query.Skip(1).Aggregate("", (string current, string arg) => current + arg + " ");
				text6 = text6.Substring(0, text6.Length - 1);
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " executed command as server console: " + text6 + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				ServerConsole.EnterCommand(text6, sender);
				sender.RaReply(query[0] + "#Command \"" + text6 + "\" executed in server console!", success: true, logToConsole: true, "");
			}
			break;
		case "SNR":
		case "STOPNEXTROUND":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands))
			{
				ServerStatic.StopNextRound = !ServerStatic.StopNextRound;
				sender.RaReply(query[0] + "#Server " + (ServerStatic.StopNextRound ? "WILL" : "WON'T") + " stop after next round.", success: true, logToConsole: true, "");
			}
			break;
		case "SETGROUP":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.SetGroup))
			{
				break;
			}
			if (!ConfigFile.ServerConfig.GetBool("online_mode", def: true))
			{
				sender.RaReply(query[0] + "#This command requires the server to operate in online mode!", success: false, logToConsole: true, "");
			}
			else if (query.Length >= 3)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Permissions, text + " ran the setgroup command (new group: " + query[2] + " min) on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "PM":
		{
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement))
			{
				break;
			}
			Dictionary<string, UserGroup> allGroups2 = ServerStatic.PermissionsHandler.GetAllGroups();
			List<string> allPermissions = ServerStatic.PermissionsHandler.GetAllPermissions();
			if (query.Length == 1)
			{
				sender.RaReply(query[0].ToUpper() + "#Permissions manager help:\nSyntax: " + query[0] + " action\n\nAvailable actions:\ngroups - lists all groups\ngroup info <group name> - prints group info\ngroup grant <group name> <permission name> - grants permission to a group\ngroup revoke <group name> <permission name> - revokes permission from a group\ngroup setcolor <group name> <color name> - sets group color\ngroup settag <group name> <tag> - sets group tag\ngroup enablecover <group name> - enables badge cover for group\ngroup disablecover <group name> - disables badge cover for group\n\nusers - lists all privileged users\nsetgroup <UserID> <group name> - sets membership of user (use group name \"-1\" to remove user from group)\nreload - reloads permission file\n\n\"< >\" are only used to indicate the arguments, don't put them\nMore commands will be added in the next versions of the game", success: true, logToConsole: true, "");
			}
			else if (string.Equals(query[1], "groups", StringComparison.OrdinalIgnoreCase))
			{
				int num4 = 0;
				string text14 = "\n";
				string text15 = "";
				string[] source = new string[23]
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
				int num5 = (int)Math.Ceiling((double)allPermissions.Count / 12.0);
				for (int k = 0; k < num5; k++)
				{
					num4 = 0;
					text14 = text14 + "\n-----" + source.Skip(k * 12).Take(12).Aggregate((string current, string adding) => current + " " + adding);
					foreach (KeyValuePair<string, UserGroup> item2 in allGroups2)
					{
						if (k == 0)
						{
							text15 = text15 + "\n" + num4 + " - " + item2.Key;
						}
						string text16 = num4.ToString();
						for (int l = text16.Length; l < 5; l++)
						{
							text16 += " ";
						}
						foreach (string item3 in allPermissions.Skip(k * 12).Take(12))
						{
							text16 = text16 + "  " + (ServerStatic.PermissionsHandler.IsPermitted(item2.Value.Permissions, item3) ? "X" : " ") + " ";
						}
						num4++;
						text14 = text14 + "\n" + text16;
					}
				}
				sender.RaReply(query[0].ToUpper() + "#All defined groups: " + text14 + "\n" + text15, success: true, logToConsole: true, "");
			}
			else if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length == 2)
			{
				sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
			}
			else if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length > 2)
			{
				if (string.Equals(query[2], "info", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
				{
					KeyValuePair<string, UserGroup> keyValuePair = allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
					if (keyValuePair.Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#Details of group " + keyValuePair.Key + "\nTag text: " + keyValuePair.Value.BadgeText + "\nTag color: " + keyValuePair.Value.BadgeColor + "\nPermissions: " + keyValuePair.Value.Permissions + "\nCover: " + (keyValuePair.Value.Cover ? "YES" : "NO") + "\nHidden by default: " + (keyValuePair.Value.HiddenByDefault ? "YES" : "NO") + "\nKick power: " + keyValuePair.Value.KickPower + "\nRequired kick power: " + keyValuePair.Value.RequiredKickPower, success: true, logToConsole: true, "");
					}
				}
				else if ((string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase) || string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase)) && query.Length == 5)
				{
					if (allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					if (!allPermissions.Contains(query[4]))
					{
						sender.RaReply(query[0].ToUpper() + "#Permission can't be found.", success: false, logToConsole: true, "");
						break;
					}
					Dictionary<string, string> stringDictionary = ServerStatic.RolesConfig.GetStringDictionary("Permissions");
					List<string> list3 = null;
					foreach (string key in stringDictionary.Keys)
					{
						if (!(key != query[4]))
						{
							list3 = YamlConfig.ParseCommaSeparatedString(stringDictionary[key]).ToList();
						}
					}
					if (list3 == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Permission can't be found in the config.", success: false, logToConsole: true, "");
						break;
					}
					if (list3.Contains(query[3]) && string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
					{
						sender.RaReply(query[0].ToUpper() + "#Group already has that permission.", success: false, logToConsole: true, "");
						break;
					}
					if (!list3.Contains(query[3]) && string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase))
					{
						sender.RaReply(query[0].ToUpper() + "#Group already doesn't have that permission.", success: false, logToConsole: true, "");
						break;
					}
					if (string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
					{
						list3.Add(query[3]);
					}
					else
					{
						list3.Remove(query[3]);
					}
					list3.Sort();
					string text17 = "[";
					foreach (string item4 in list3)
					{
						if (text17 != "[")
						{
							text17 += ", ";
						}
						text17 += item4;
					}
					text17 += "]";
					ServerStatic.RolesConfig.SetStringDictionaryItem("Permissions", query[4], text17);
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
					sender.RaReply(query[0].ToUpper() + "#Permissions updated.", success: true, logToConsole: true, "");
				}
				else if (string.Equals(query[2], "setcolor", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
				{
					if (allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					ServerStatic.RolesConfig.SetString(query[3] + "_color", query[4]);
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
					sender.RaReply(query[0].ToUpper() + "#Group color updated.", success: true, logToConsole: true, "");
				}
				else if (string.Equals(query[2], "settag", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
				{
					if (allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					ServerStatic.RolesConfig.SetString(query[3] + "_badge", query[4]);
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
					sender.RaReply(query[0].ToUpper() + "#Group tag updated.", success: true, logToConsole: true, "");
				}
				else if (string.Equals(query[2], "enablecover", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
				{
					if (allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					ServerStatic.RolesConfig.SetString(query[3] + "_cover", "true");
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
					sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", success: true, logToConsole: true, "");
				}
				else if (query[2].ToLower() == "disablecover" && query.Length == 4)
				{
					if (allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					ServerStatic.RolesConfig.SetString(query[3] + "_cover", "false");
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
					sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", success: true, logToConsole: true, "");
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
				}
			}
			else if (string.Equals(query[1], "users", StringComparison.OrdinalIgnoreCase))
			{
				Dictionary<string, string> stringDictionary2 = ServerStatic.RolesConfig.GetStringDictionary("Members");
				Dictionary<string, string> dictionary = ServerStatic.SharedGroupsMembersConfig?.GetStringDictionary("SharedMembers");
				string text18 = "Players with assigned groups:";
				foreach (KeyValuePair<string, string> item5 in stringDictionary2)
				{
					text18 = text18 + "\n" + item5.Key + " - " + item5.Value;
				}
				if (dictionary != null)
				{
					foreach (KeyValuePair<string, string> item6 in dictionary)
					{
						text18 = text18 + "\n" + item6.Key + " - " + item6.Value + " <color=#FFD700>[SHARED MEMBERSHIP]</color>";
					}
				}
				sender.RaReply(query[0].ToUpper() + "#" + text18, success: true, logToConsole: true, "");
			}
			else if (string.Equals(query[1], "setgroup", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
			{
				string text19 = "";
				if (query[3] == "-1")
				{
					text19 = null;
				}
				else
				{
					KeyValuePair<string, UserGroup> keyValuePair2 = allGroups2.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
					if (keyValuePair2.Key == null)
					{
						sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
						break;
					}
					text19 = keyValuePair2.Key;
				}
				ServerStatic.RolesConfig.SetStringDictionaryItem("Members", query[2], text19);
				ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
				sender.RaReply(query[0].ToUpper() + "#User permissions updated. If user is online, please use \"setgroup\" command to change it now (without this command, new role will be applied during next round).", success: true, logToConsole: true, "");
			}
			else if (string.Equals(query[1], "reload", StringComparison.OrdinalIgnoreCase))
			{
				ConfigFile.ReloadGameConfigs();
				ServerStatic.RolesConfig.Reload();
				ServerStatic.SharedGroupsConfig = ((ConfigSharing.Paths[4] == null) ? null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt"));
				ServerStatic.SharedGroupsMembersConfig = ((ConfigSharing.Paths[5] == null) ? null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt"));
				ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
				sender.RaReply(query[0].ToUpper() + "#Permission file reloaded.", success: true, logToConsole: true, "");
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
			}
			break;
		}
		case "SLML_STYLE":
		case "SLML_TAG":
			if (query.Length >= 3)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Logger, text + " Requested a download of " + query[2] + " on " + query[1] + " players' computers.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! " + successes + " player(s) affected!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
                case "UNBAN":
                    if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning))
                    {
                        break;
                    }
                    if (query.Length >= 4)
                    {
                        ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the unban " + query[1] + " command on " + query[2] + ".", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                        switch (query[1].ToLower())
                        {
                            case "id":
                            case "playerid":
                            case "player":
                                string str2 = string.Empty;
                                if (query.Length > 3)
                                {
                                    str2 = ((IEnumerable<string>)query).Skip<string>(3).Aggregate<string>((Func<string, string, string>)((current, n) => current + " " + n));
                                }
                                using (WebClient webClient = new WebClient())
                                {
                                    webClient.Credentials = (ICredentials)new NetworkCredential(ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"), ConfigFile.ServerConfig.GetString("bansystem_apikey", "none"));
                                    webClient.DownloadString("http://83.82.126.185//scpserverbans/scpplugin/unban.php?id=" + query[2] + "&reason=" + str2 + "&aname=" + sender.Nickname + ("&webhook=" + ConfigFile.ServerConfig.GetString("bansystem_webhook", "none") + "&alias=" + ConfigFile.ServerConfig.GetString("bansystem_alias", "none")));
                                }
                                ServerConsole.AddLog("User " + query[2] + " Unbanned by RA user " + sender.Nickname);
                                sender.RaReply(query[0] + "#Done!", success: true, logToConsole: true, "");
                                break;
                            case "ip":
                            case "address":
                                BanHandler.RemoveBan(query[2], BanHandler.BanType.IP);
                                sender.RaReply(query[0] + "#Done!", success: true, logToConsole: true, "");
                                break;
                            default:
                                sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere ReasonHere OR unban ip IpAddressHere ReasonHere", false, true, "");
                                break;
                        }
                    }
                    else
                    {
                        sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere ReasonHere OR unban ip IpAddressHere ReasonHere", true, true, "");
                    }
                    break;
		case "GROUPS":
		{
			string text11 = "Groups defined on this server:";
			Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
			ServerRoles.NamedColor[] namedColors = QueryProcessor.Localplayer.GetComponent<ServerRoles>().NamedColors;
			foreach (KeyValuePair<string, UserGroup> permentry in allGroups)
			{
				try
				{
					text11 = text11 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - <color=#" + namedColors.FirstOrDefault((ServerRoles.NamedColor x) => x.Name == permentry.Value.BadgeColor).ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
				}
				catch
				{
					text11 = text11 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
				{
					text11 += " BN1";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
				{
					text11 += " BN2";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
				{
					text11 += " BN3";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
				{
					text11 += " FSE";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
				{
					text11 += " FSP";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
				{
					text11 += " FWR";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
				{
					text11 += " GIV";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
				{
					text11 += " EWA";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
				{
					text11 += " ERE";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
				{
					text11 += " ERO";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
				{
					text11 += " SGR";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData))
				{
					text11 += " GMD";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch))
				{
					text11 += " OVR";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
				{
					text11 += " FCM";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement))
				{
					text11 += " PLM";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement))
				{
					text11 += " PRM";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands))
				{
					text11 += " SCC";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges))
				{
					text11 += " VHB";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs))
				{
					text11 += " CFG";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Broadcasting))
				{
					text11 += " BRC";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayerSensitiveDataAccess))
				{
					text11 += " CDA";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Noclip))
				{
					text11 += " NCP";
				}
				if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AFKImmunity))
				{
					text11 += " AFK";
				}
			}
			sender.RaReply(query[0].ToUpper() + "#" + text11, success: true, logToConsole: true, "");
			break;
		}
		case "FS":
		case "FORCESTART":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
			{
				bool flag8 = CharacterClassManager.ForceRoundStart();
				if (flag8)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " forced round start.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				}
				sender.RaReply(query[0] + "#" + (flag8 ? "Done! Forced round start." : "Failed to force start."), flag8, logToConsole: true, "ServerEvents");
			}
			break;
		case "SC":
		case "CONFIG":
		case "SETCONFIG":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs, "ServerConfigs"))
			{
				break;
			}
			if (query.Length >= 3)
			{
				if (query.Length > 3)
				{
					string text4 = query[2];
					for (int i = 3; i < query.Length; i++)
					{
						text4 = text4 + " " + query[i];
					}
					query = new string[3]
					{
						query[0],
						query[1],
						text4
					};
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the setconfig command (" + query[1] + ": " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				switch (query[1].ToUpper())
				{
				case "FRIENDLY_FIRE":
				{
					if (bool.TryParse(query[2], out bool result5))
					{
						ServerConsole.FriendlyFire = result5;
						WeaponManager[] array2 = UnityEngine.Object.FindObjectsOfType<WeaponManager>();
						for (int j = 0; j < array2.Length; j++)
						{
							array2[j].NetworkfriendlyFire = result5;
						}
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result5.ToString() + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				case "PLAYER_LIST_TITLE":
				{
					string text5 = query[2] ?? string.Empty;
					PlayerList.Title.Value = text5;
					try
					{
						PlayerList.singleton.RefreshTitle();
					}
					catch (Exception ex4)
					{
						if (!(ex4 is CommandInputException) && !(ex4 is InvalidOperationException))
						{
							throw;
						}
						sender.RaReply(query[0].ToUpper() + "#Could not set player list title [" + text5 + "]:\n" + ex4.Message, success: false, logToConsole: true, "ServerConfigs");
						return;
					}
					sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + PlayerList.singleton.syncServerName + "]!", success: true, logToConsole: true, "ServerConfigs");
					break;
				}
				case "PD_REFRESH_EXIT":
				{
					if (bool.TryParse(query[2], out bool result6))
					{
						UnityEngine.Object.FindObjectOfType<PocketDimensionTeleport>().RefreshExit = result6;
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result6.ToString() + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				case "SPAWN_PROTECT_DISABLE":
				{
					if (bool.TryParse(query[2], out bool result3))
					{
						CharacterClassManager[] array = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
						for (int j = 0; j < array.Length; j++)
						{
							array[j].EnableSP = !result3;
						}
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result3.ToString() + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				case "SPAWN_PROTECT_TIME":
				{
					if (int.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int result7))
					{
						CharacterClassManager[] array = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
						for (int j = 0; j < array.Length; j++)
						{
							array[j].SProtectedDuration = result7;
						}
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result7 + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid integer!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				case "HUMAN_GRENADE_MULTIPLIER":
				{
					if (float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float result4))
					{
						ConfigFile.ServerConfig.SetString("human_grenade_multiplier", result4.ToString());
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result4 + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				case "SCP_GRENADE_MULTIPLIER":
				{
					if (float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float result2))
					{
						ConfigFile.ServerConfig.SetString("scp_grenade_multiplier", result2.ToString());
						sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result2 + "]!", success: true, logToConsole: true, "ServerConfigs");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", success: false, logToConsole: true, "ServerConfigs");
					}
					break;
				}
				default:
					sender.RaReply(query[0].ToUpper() + "#Invalid config " + query[1], success: false, logToConsole: true, "ServerConfigs");
					break;
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "ServerConfigs");
			}
			break;
		case "FC":
		case "FORCECLASS":
			if (query.Length >= 3)
			{
				int result8 = 0;
				if (!int.TryParse(query[2], out result8) || result8 < 0 || result8 >= QueryProcessor.LocalCCM.Classes.Length)
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid class ID.", success: false, logToConsole: true, "");
					break;
				}
				string fullName = QueryProcessor.LocalCCM.Classes.SafeGet(result8).fullName;
				GameObject gameObject8 = GameObject.Find("Host");
				if (gameObject8 == null)
				{
					sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", success: false, logToConsole: true, "");
					break;
				}
				CharacterClassManager component8 = gameObject8.GetComponent<CharacterClassManager>();
				if (component8 == null || !component8.isLocalPlayer || !component8.isServer || !component8.RoundStarted)
				{
					sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", success: false, logToConsole: true, "");
					break;
				}
				PlayerCommandSender playerCommandSender4;
				bool flag5 = (playerCommandSender4 = (sender as PlayerCommandSender)) != null && (query[1] == playerCommandSender4.PlayerId.ToString() || query[1] == playerCommandSender4.PlayerId + ".");
				bool flag6 = result8 == 2;
				if ((flag5 && flag6 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
				{
					PlayerPermissions.ForceclassWithoutRestrictions,
					PlayerPermissions.ForceclassToSpectator,
					PlayerPermissions.ForceclassSelf
				})) || (flag5 && !flag6 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
				{
					PlayerPermissions.ForceclassWithoutRestrictions,
					PlayerPermissions.ForceclassSelf
				})) || (!flag5 && flag6 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
				{
					PlayerPermissions.ForceclassWithoutRestrictions,
					PlayerPermissions.ForceclassToSpectator
				})) || (!flag5 && !flag6 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[1]
				{
					PlayerPermissions.ForceclassWithoutRestrictions
				})))
				{
					break;
				}
				if (string.Equals(query[0], "role", StringComparison.OrdinalIgnoreCase))
				{
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, text + " ran the role command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				}
				else
				{
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, text + " ran the forceclass command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				}
				StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "WARHEAD":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents))
			{
				break;
			}
			if (query.Length == 1)
			{
				sender.RaReply("Syntax: warhead (status|detonate|instant|cancel|enable|disable)", success: false, logToConsole: true, string.Empty);
				break;
			}
			switch (query[1].ToLower())
			{
			case "status":
				if (AlphaWarheadController.Host.detonated || Math.Abs(AlphaWarheadController.Host.timeToDetonation) < 0.001f)
				{
					sender.RaReply("Warhead has been detonated.", success: true, logToConsole: true, string.Empty);
				}
				else if (AlphaWarheadController.Host.inProgress)
				{
					sender.RaReply("Detonation is in progress.", success: true, logToConsole: true, string.Empty);
				}
				else if (!AlphaWarheadOutsitePanel.nukeside.enabled)
				{
					sender.RaReply("Warhead is disabled.", success: true, logToConsole: true, string.Empty);
				}
				else if (AlphaWarheadController.Host.timeToDetonation > AlphaWarheadController.Host.RealDetonationTime())
				{
					sender.RaReply("Warhead is restarting.", success: true, logToConsole: true, string.Empty);
				}
				else
				{
					sender.RaReply("Warhead is ready to detonation.", success: true, logToConsole: true, string.Empty);
				}
				break;
			case "detonate":
				AlphaWarheadController.Host.StartDetonation();
				sender.RaReply("Detonation sequence started.", success: true, logToConsole: true, string.Empty);
				break;
			case "instant":
				AlphaWarheadController.Host.InstantPrepare();
				AlphaWarheadController.Host.StartDetonation();
				AlphaWarheadController.Host.NetworktimeToDetonation = 5f;
				sender.RaReply("Instant detonation started.", success: true, logToConsole: true, string.Empty);
				break;
			case "cancel":
				AlphaWarheadController.Host.CancelDetonation(null);
				sender.RaReply("Detonation has been canceled.", success: true, logToConsole: true, string.Empty);
				break;
			case "enable":
				AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
				sender.RaReply("Warhead has been enabled.", success: true, logToConsole: true, string.Empty);
				break;
			case "disable":
				AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
				sender.RaReply("Warhead has been disabled.", success: true, logToConsole: true, string.Empty);
				break;
			default:
				sender.RaReply("WARHEAD: Unknown subcommand.", success: false, logToConsole: true, string.Empty);
				break;
			}
			break;
		case "HEAL":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools"))
			{
				break;
			}
			if (query.Length >= 2)
			{
				int result = (query.Length >= 3 && int.TryParse(query[2], out result)) ? result : 0;
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the heal command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, query[0], query[1], result.ToString(), out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "N":
		case "NC":
		case "NOCLIP":
		{
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Noclip, "AdminTools"))
			{
				break;
			}
			PlayerCommandSender playerCommandSender5;
			if (query.Length >= 2)
			{
				if (query.Length == 2)
				{
					query = new string[3]
					{
						query[0],
						query[1],
						""
					};
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the noclip command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, "NOCLIP", query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply("NOCLIP#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
					}
					else
					{
						sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else if ((playerCommandSender5 = (sender as PlayerCommandSender)) != null)
			{
				StandardizedQueryModel1(sender, "NOCLIP", playerCommandSender5.PlayerId.ToString(), "", out failures, out successes, out error, out replySent);
				if (failures == 0)
				{
					sender.RaReply("NOCLIP#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
				}
				else
				{
					sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
			}
			break;
		}
		case "HP":
		case "SETHP":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement))
			{
				break;
			}
			if (query.Length >= 3)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the sethp command on " + query[1] + " players (HP: " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "OVR":
		case "OVERWATCH":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Overwatch, "AdminTools"))
			{
				break;
			}
			if (query.Length >= 2)
			{
				if (query.Length == 2)
				{
					query = new string[3]
					{
						query[0],
						query[1],
						""
					};
				}
				ServerLogs.AddLog(ServerLogs.Modules.ClassChange, text + " ran the overwatch command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, "OVERWATCH", query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply("OVERWATCH#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
					}
					else
					{
						sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
			}
			break;
		case "GOD":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools"))
			{
				break;
			}
			if (query.Length >= 2)
			{
				if (query.Length == 2)
				{
					query = new string[3]
					{
						query[0],
						query[1],
						""
					};
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the god command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, "GOD", query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply("OVERWATCH#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
					}
					else
					{
						sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
			}
			break;
		case "MUTE":
		case "UNMUTE":
		case "IMUTE":
		case "IUNMUTE":
			if (!CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
			{
				PlayerPermissions.BanningUpToDay,
				PlayerPermissions.LongTermBanning,
				PlayerPermissions.PlayersManagement
			}, "PlayersManagement"))
			{
				break;
			}
			if (query.Length == 2)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the " + query[0].ToLower() + " command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				StandardizedQueryModel1(sender, query[0].ToUpper(), query[1], null, out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0].ToUpper() + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "PlayersManagement");
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "PlayersManagement");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "PlayersManagement");
			}
			break;
		case "INTERCOM-TIMEOUT":
			if (CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[6]
			{
				PlayerPermissions.KickingAndShortTermBanning,
				PlayerPermissions.BanningUpToDay,
				PlayerPermissions.LongTermBanning,
				PlayerPermissions.RoundEvents,
				PlayerPermissions.FacilityManagement,
				PlayerPermissions.PlayersManagement
			}, "ServerEvents"))
			{
				if (!Intercom.host.speaking)
				{
					sender.RaReply(query[0].ToUpper() + "#Intercom is not being used.", success: false, logToConsole: true, "ServerEvents");
					break;
				}
				if (Intercom.host.speechRemainingTime == -77f)
				{
					sender.RaReply(query[0].ToUpper() + "#Intercom is being used by player with bypass mode enabled.", success: false, logToConsole: true, "ServerEvents");
					break;
				}
				Intercom.host.speechRemainingTime = -1f;
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " timeouted the intercom speaker.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				sender.RaReply(query[0].ToUpper() + "#Done! Intercom speaker timeouted.", success: true, logToConsole: true, "ServerEvents");
			}
			break;
		case "INTERCOM-RESET":
			if (CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
			{
				PlayerPermissions.RoundEvents,
				PlayerPermissions.FacilityManagement,
				PlayerPermissions.PlayersManagement
			}, "ServerEvents"))
			{
				if (Intercom.host.remainingCooldown <= 0f)
				{
					sender.RaReply(query[0].ToUpper() + "#Intercom is already ready to use.", success: false, logToConsole: true, "ServerEvents");
					break;
				}
				Intercom.host.remainingCooldown = -1f;
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " reset the intercom cooldown.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				sender.RaReply(query[0].ToUpper() + "#Done! Intercom cooldown reset.", success: true, logToConsole: true, "ServerEvents");
			}
			break;
		case "SPEAK":
		case "ICOM":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Broadcasting, "ServerEvents") || !IsPlayer(sender, query[0], "ServerEvents"))
			{
				break;
			}
			if (!Intercom.AdminSpeaking)
			{
				if (Intercom.host.speaking)
				{
					sender.RaReply(query[0].ToUpper() + "#Intercom is being used by someone else.", success: false, logToConsole: true, "ServerEvents");
					break;
				}
				Intercom.AdminSpeaking = true;
				Intercom.host.RequestTransmission(queryProcessor.GetComponent<Intercom>().gameObject);
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " requested global voice over the intercom.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom granted.", success: true, logToConsole: true, "ServerEvents");
			}
			else
			{
				Intercom.AdminSpeaking = false;
				Intercom.host.RequestTransmission(null);
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ended global intercom transmission.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
				sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom revoked.", success: true, logToConsole: true, "ServerEvents");
			}
			break;
		case "BM":
		case "BYPASS":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
			{
				break;
			}
			if (query.Length >= 2)
			{
				if (query.Length == 2)
				{
					query = new string[3]
					{
						query[0],
						query[1],
						""
					};
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the bypass mode command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, "BYPASS", query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply("BYPASS#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
					}
					else
					{
						sender.RaReply("BYPASS#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
			}
			break;
		case "BRING":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools") || !IsPlayer(sender, query[0], "AdminTools"))
			{
				break;
			}
			if (query.Length == 2)
			{
				if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.GetComponent<CharacterClassManager>().CurClass < RoleType.Scp173)
				{
					sender.RaReply("BRING#Command disabled when you are spectator!", success: false, logToConsole: true, "AdminTools");
					break;
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the bring command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, "BRING", query[1], "", out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply("BRING#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
					}
					else
					{
						sender.RaReply("BRING#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "AdminTools");
			}
			break;
		case "GOTO":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools") || !IsPlayer(sender, query[0], "AdminTools"))
			{
				break;
			}
			if (query.Length == 2)
			{
				if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.CurClass < RoleType.Scp173)
				{
					sender.RaReply("GOTO#Command disabled when you are spectator!", success: false, logToConsole: true, "AdminTools");
					break;
				}
				if (!int.TryParse(query[1], out int id))
				{
					sender.RaReply("GOTO#Player ID must be an integer.", success: false, logToConsole: true, "AdminTools");
					break;
				}
				if (query[1].Contains("."))
				{
					sender.RaReply("GOTO#Goto command requires exact one selected player.", success: false, logToConsole: true, "AdminTools");
					break;
				}
				GameObject gameObject9 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id);
				if (gameObject9 == null)
				{
					sender.RaReply("GOTO#Can't find requested player.", success: false, logToConsole: true, "AdminTools");
					break;
				}
				if (gameObject9.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || gameObject9.GetComponent<CharacterClassManager>().CurClass < RoleType.None)
				{
					sender.RaReply("GOTO#Requested player is a spectator!", success: false, logToConsole: true, "AdminTools");
					break;
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the goto command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				queryProcessor.GetComponent<PlyMovementSync>().OverridePosition(gameObject9.GetComponent<PlyMovementSync>().RealModelPosition, 0f);
				sender.RaReply("GOTO#Done!", success: true, logToConsole: true, "AdminTools");
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "AdminTools");
			}
			break;
		case "LD":
		case "LOCKDOWN":
		{
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
			{
				break;
			}
			Door[] array3;
			if (!QueryProcessor.Lockdown)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " enabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				array3 = UnityEngine.Object.FindObjectsOfType<Door>();
				foreach (Door door in array3)
				{
					if (!door.locked)
					{
						door.lockdown = true;
						door.UpdateLock();
					}
				}
				QueryProcessor.Lockdown = true;
				sender.RaReply(query[0] + "#Lockdown enabled!", success: true, logToConsole: true, "AdminTools");
				break;
			}
			ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " disabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
			array3 = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door2 in array3)
			{
				if (door2.lockdown)
				{
					door2.lockdown = false;
					door2.UpdateLock();
				}
			}
			QueryProcessor.Lockdown = false;
			sender.RaReply(query[0] + "#Lockdown disabled!", success: true, logToConsole: true, "AdminTools");
			break;
		}
		case "O":
		case "OPEN":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
			{
				if (query.Length != 2)
				{
					sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
				}
				else
				{
					ProcessDoorQuery(sender, "OPEN", query[1]);
				}
			}
			break;
		case "C":
		case "CLOSE":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
			{
				if (query.Length != 2)
				{
					sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
				}
				else
				{
					ProcessDoorQuery(sender, "CLOSE", query[1]);
				}
			}
			break;
		case "L":
		case "LOCK":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
			{
				if (query.Length != 2)
				{
					sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
				}
				else
				{
					ProcessDoorQuery(sender, "LOCK", query[1]);
				}
			}
			break;
		case "UL":
		case "UNLOCK":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
			{
				if (query.Length != 2)
				{
					sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
				}
				else
				{
					ProcessDoorQuery(sender, "UNLOCK", query[1]);
				}
			}
			break;
		case "DESTROY":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
			{
				if (query.Length != 2)
				{
					sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
				}
				else
				{
					ProcessDoorQuery(sender, "DESTROY", query[1]);
				}
			}
			break;
		case "DOORTP":
		case "DTP":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "DoorsManagement"))
			{
				break;
			}
			if (query.Length != 3)
			{
				sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " PlayerIDs DoorName", success: false, logToConsole: true, "");
				break;
			}
			ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the DoorTp command (Door: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
			StandardizedQueryModel1(sender, "DOORTP", query[1], query[2], out failures, out successes, out error, out replySent);
			if (!replySent)
			{
				if (failures == 0)
				{
					sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "DoorsManagement");
				}
				else
				{
					sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "DoorsManagement");
				}
			}
			break;
		case "DL":
		case "DOORS":
		case "DOORLIST":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
			{
				string str2 = "List of named doors in the facility:\n";
				List<string> list = (from item in UnityEngine.Object.FindObjectsOfType<Door>()
					where !string.IsNullOrEmpty(item.DoorName)
					select item.DoorName + " - " + (item.isOpen ? "<color=green>OPENED</color>" : "<color=orange>CLOSED</color>") + (item.locked ? " <color=red>[LOCKED]</color>" : "") + (string.IsNullOrEmpty(item.permissionLevel) ? "" : " <color=blue>[CARD REQUIRED]</color>")).ToList();
				list.Sort();
				str2 += list.Aggregate((string current, string adding) => current + "\n" + adding);
				sender.RaReply(query[0] + "#" + str2, success: true, logToConsole: true, "");
			}
			break;
		case "GIVE":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GivingItems))
			{
				break;
			}
			if (query.Length >= 3)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " ran the give command (ID: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
					}
					else
					{
						sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
					}
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "REQUEST_DATA":
			if (query.Length >= 2)
			{
				switch (query[1].ToUpper())
				{
				case "PLAYER_LIST":
					try
					{
						string text2 = "\n";
						bool gameplayData = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, string.Empty, reply: false);
						PlayerCommandSender playerCommandSender2;
						if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
						{
							playerCommandSender2.Processor.GameplayData = gameplayData;
						}
						bool flag2 = q.Contains("STAFF", StringComparison.OrdinalIgnoreCase);
						foreach (GameObject player in PlayerManager.players)
						{
							QueryProcessor component4 = player.GetComponent<QueryProcessor>();
							if (!flag2)
							{
								string text3 = string.Empty;
								bool flag3 = false;
								ServerRoles component5 = component4.GetComponent<ServerRoles>();
								try
								{
									text3 = (component5.RaEverywhere ? "[~] " : (component5.Staff ? "[@] " : (component5.RemoteAdmin ? "[RA] " : string.Empty)));
									flag3 = component5.OverwatchEnabled;
								}
								catch
								{
								}
								text2 = text2 + text3 + "(" + component4.PlayerId + ") " + component4.GetComponent<NicknameSync>().MyNick.Replace("\n", "").Replace("<", "").Replace(">", "") + (flag3 ? "<OVRM>" : "");
							}
							else
							{
								text2 = text2 + component4.PlayerId + ";" + component4.GetComponent<NicknameSync>().MyNick;
							}
							text2 += "\n";
						}
						if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
						{
							sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + text2, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
						}
						else
						{
							sender.RaReply("StaffPlayerListReply#" + text2, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
						}
					}
					catch (Exception ex2)
					{
						sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source, success: false, logToConsole: true, "");
						throw;
					}
					break;
				case "PLAYER":
				case "SHORT-PLAYER":
					if (query.Length >= 3)
					{
						if (!string.Equals(query[1], "PLAYER", StringComparison.OrdinalIgnoreCase) || (playerCommandSender != null && playerCommandSender.SR.Staff) || CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
						{
							try
							{
								GameObject gameObject5 = null;
								NetworkConnection networkConnection = null;
								foreach (NetworkConnection value in NetworkServer.connections.Values)
								{
									GameObject gameObject6 = GameCore.Console.FindConnectedRoot(value);
									if (query[2].Contains("."))
									{
										query[2] = query[2].Split('.')[0];
									}
									if (!(gameObject6 == null) && !(gameObject6.GetComponent<QueryProcessor>().PlayerId.ToString() != query[2]))
									{
										gameObject5 = gameObject6;
										networkConnection = value;
									}
								}
								if (gameObject5 == null)
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
								}
								else
								{
									bool flag4 = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, "", reply: false);
									PlayerCommandSender playerCommandSender3;
									if ((playerCommandSender3 = (sender as PlayerCommandSender)) != null)
									{
										playerCommandSender3.Processor.GameplayData = flag4;
									}
									CharacterClassManager component6 = gameObject5.GetComponent<CharacterClassManager>();
									ServerRoles component7 = gameObject5.GetComponent<ServerRoles>();
									if (query[1].ToUpper() == "PLAYER")
									{
										ServerLogs.AddLog(ServerLogs.Modules.DataAccess, text + " accessed IP address of player " + gameObject5.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject5.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
									}
									StringBuilder stringBuilder = new StringBuilder("<color=white>");
									stringBuilder.Append("Nickname: " + gameObject5.GetComponent<NicknameSync>().MyNick);
									stringBuilder.Append("\nPlayer ID: " + gameObject5.GetComponent<QueryProcessor>().PlayerId);
									stringBuilder.Append("\nIP: " + ((networkConnection == null) ? "null" : ((query[1].ToUpper() == "PLAYER") ? networkConnection.address : "[REDACTED]")));
									stringBuilder.Append("\nUser ID: " + (string.IsNullOrEmpty(component6.UserId) ? "(none)" : component6.UserId));
									if (component6.SaltedUserId != null && component6.SaltedUserId.Contains("$"))
									{
										stringBuilder.Append("\nSalted User ID: " + component6.SaltedUserId);
									}
									if (!string.IsNullOrEmpty(component6.UserId2))
									{
										stringBuilder.Append("\nUser ID 2: " + component6.UserId2);
									}
									stringBuilder.Append("\nServer role: " + component7.GetColoredRoleString());
									if (!string.IsNullOrEmpty(component7.HiddenBadge))
									{
										stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + component7.HiddenBadge);
									}
									if (component7.RaEverywhere)
									{
										stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
									}
									else if (component7.Staff)
									{
										stringBuilder.Append("\nActive flag: Studio Staff");
									}
									if (component6.Muted)
									{
										stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
									}
									else if (component6.IntercomMuted)
									{
										stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
									}
									if (component6.GodMode)
									{
										stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
									}
									if (component6.NoclipEnabled)
									{
										stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
									}
									else if (component7.NoclipReady)
									{
										stringBuilder.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
									}
									if (component7.DoNotTrack)
									{
										stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
									}
									if (component7.BypassMode)
									{
										stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
									}
									if (component7.RemoteAdmin)
									{
										stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
									}
									if (component7.OverwatchEnabled)
									{
										stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
									}
									else
									{
										stringBuilder.Append("\nClass: " + ((!flag4) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (component6.Classes.CheckBounds(component6.CurClass) ? component6.Classes.SafeGet(component6.CurClass).fullName : "None")));
										stringBuilder.Append("\nHP: " + (flag4 ? gameObject5.GetComponent<PlayerStats>().HealthToString() : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
										if (!flag4)
										{
											stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
										}
									}
									stringBuilder.Append("</color>");
									sender.RaReply(query[0].ToUpper() + ":PLAYER#" + stringBuilder, success: true, logToConsole: true, "PlayerInfo");
									sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(component6.UserId) ? "(no User ID)" : component6.UserId), success: true, logToConsole: false, "PlayerInfo");
								}
							}
							catch (Exception ex3)
							{
								sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex3.Message + "\nStackTrace: " + ex3.StackTrace + "\nAt: " + ex3.Source, success: false, logToConsole: true, "PlayerInfo");
								throw;
							}
						}
					}
					else
					{
						sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", success: false, logToConsole: true, "");
					}
					break;
				case "AUTH":
					if ((playerCommandSender != null && playerCommandSender.SR.Staff) || CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
					{
						if (query.Length >= 3)
						{
							try
							{
								GameObject gameObject3 = null;
								foreach (NetworkConnection value2 in NetworkServer.connections.Values)
								{
									GameObject gameObject4 = GameCore.Console.FindConnectedRoot(value2);
									if (query[2].Contains("."))
									{
										query[2] = query[2].Split('.')[0];
									}
									if (gameObject4 != null && gameObject4.GetComponent<QueryProcessor>().PlayerId.ToString() == query[2])
									{
										gameObject3 = gameObject4;
									}
								}
								if (gameObject3 == null)
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
								}
								else if (string.IsNullOrEmpty(gameObject3.GetComponent<CharacterClassManager>().AuthToken))
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Can't obtain auth token. Is server using offline mode or you selected the host?", success: false, logToConsole: true, "PlayerInfo");
								}
								else
								{
									ServerLogs.AddLog(ServerLogs.Modules.DataAccess, text + " accessed authentication token of player " + gameObject3.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject3.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
									if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
									{
										string myNick = gameObject3.GetComponent<NicknameSync>().MyNick;
										string str = "<color=white>Authentication token of player " + myNick + "(" + gameObject3.GetComponent<QueryProcessor>().PlayerId + "):\n" + gameObject3.GetComponent<CharacterClassManager>().AuthToken + "</color>";
										sender.RaReply(query[0].ToUpper() + ":PLAYER#" + str, success: true, logToConsole: true, "null");
										sender.RaReply("BigQR#" + gameObject3.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
									}
									else
									{
										sender.RaReply("StaffTokenReply#" + gameObject3.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
									}
								}
							}
							catch (Exception ex)
							{
								sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, success: false, logToConsole: true, "PlayerInfo");
								throw;
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", success: false, logToConsole: true, "");
						}
					}
					break;
								case "PBC":
                    if (query.Length < 4)
                    {
                        sender.RaReply(query[0].ToUpper() + "#Usage: PBC <PLAYER> <TIME> <MESSAGE>", false, true, "");
                        return;
                    }
                    if (!CommandProcessor.CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting, "", true))
                    {
                        sender.RaReply(query[0].ToUpper() + "#Missing permission: Broadcasting", false, true, "");
                        return;
                    }
                    uint num13;
                    if (!uint.TryParse(query[2], out num13) || num13 < 1U)
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
                        if (gameObject10.GetComponent<NicknameSync>().MyNick.Contains(query[1], StringComparison.OrdinalIgnoreCase))
                        {
                            NetworkConnection connectionToClient = gameObject10.GetComponent<NetworkIdentity>().connectionToClient;
                            if (connectionToClient != null)
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
                            " by: ",
                            sender.Nickname,
                                }), ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                            }
                        }
                    }
                    sender.RaReply(query[0].ToUpper() + "#PBC command sent.", true, true, "");
                    break;
				default:
					sender.RaReply(query[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", success: false, logToConsole: true, "PlayerInfo");
					break;
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "PlayerInfo");
			}
			break;
		case "CONTACT":
			sender.RaReply(query[0].ToUpper() + "#Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email"), success: false, logToConsole: true, "");
			break;
		case "SERVER_EVENT":
			if (query.Length >= 2)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, text + " forced a server event: " + query[1].ToUpper(), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				GameObject gameObject2 = GameObject.Find("Host");
				MTFRespawn component = gameObject2.GetComponent<MTFRespawn>();
				AlphaWarheadController component2 = gameObject2.GetComponent<AlphaWarheadController>();
				bool flag = true;
				switch (query[1].ToUpper())
				{
				case "FORCE_CI_RESPAWN":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents"))
					{
						return;
					}
					component.nextWaveIsCI = true;
					component.timeToNextRespawn = 0.1f;
					break;
				case "FORCE_MTF_RESPAWN":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents"))
					{
						return;
					}
					component.nextWaveIsCI = false;
					component.timeToNextRespawn = 0.1f;
					break;
				case "DETONATION_START":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
					{
						return;
					}
					component2.InstantPrepare();
					component2.StartDetonation();
					break;
				case "DETONATION_CANCEL":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
					{
						return;
					}
					component2.CancelDetonation();
					break;
				case "DETONATION_INSTANT":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
					{
						return;
					}
					component2.InstantPrepare();
					component2.StartDetonation();
					component2.NetworktimeToDetonation = 5f;
					break;
				case "TERMINATE_UNCONN":
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
					{
						return;
					}
					foreach (NetworkConnection value3 in NetworkServer.connections.Values)
					{
						if (GameCore.Console.FindConnectedRoot(value3) == null)
						{
							value3.Disconnect();
							value3.Dispose();
						}
					}
					break;
				case "ROUND_RESTART":
				case "ROUNDRESTART":
				case "RR":
				case "RESTART":
				{
					if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
					{
						return;
					}
					PlayerStats component3 = PlayerManager.localPlayer.GetComponent<PlayerStats>();
					if (component3.isServer)
					{
						component3.Roundrestart();
					}
					break;
				}
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					sender.RaReply(query[0].ToUpper() + "#Started event: " + query[1].ToUpper(), success: true, logToConsole: true, "ServerEvents");
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#Incorrect event! (Doesn't exist)", success: false, logToConsole: true, "ServerEvents");
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
			}
			break;
		case "HIDETAG":
			if (IsPlayer(sender, query[0]))
			{
				queryProcessor.GetComponent<ServerRoles>().HiddenBadge = queryProcessor.GetComponent<ServerRoles>().MyText;
				queryProcessor.GetComponent<ServerRoles>().NetworkGlobalBadge = null;
				queryProcessor.GetComponent<ServerRoles>().SetText(null);
				queryProcessor.GetComponent<ServerRoles>().SetColor(null);
				queryProcessor.GetComponent<ServerRoles>().GlobalSet = false;
				queryProcessor.GetComponent<ServerRoles>().RefreshHiddenTag();
				sender.RaReply(query[0].ToUpper() + "#Tag hidden!", success: true, logToConsole: true, "");
			}
			break;
		case "SHOWTAG":
			if (IsPlayer(sender, query[0]) && !(queryProcessor == null))
			{
				queryProcessor.Roles.HiddenBadge = null;
				queryProcessor.Roles.RpcResetFixed();
				queryProcessor.Roles.RefreshPermissions(disp: true);
				sender.RaReply(query[0].ToUpper() + "#Local tag refreshed!", success: true, logToConsole: true, "");
			}
			break;
		case "GTAG":
		case "GLOBALTAG":
			if (IsPlayer(sender, query[0], "ServerEvents") && !(queryProcessor == null))
			{
				if (string.IsNullOrEmpty(queryProcessor.Roles.PrevBadge))
				{
					sender.RaReply(query[0].ToUpper() + "#You don't have global tag.", success: false, logToConsole: true, "");
					break;
				}
				queryProcessor.Roles.HiddenBadge = null;
				queryProcessor.Roles.RpcResetFixed();
				queryProcessor.Roles.NetworkGlobalBadge = queryProcessor.Roles.PrevBadge;
				queryProcessor.Roles.GlobalSet = true;
				sender.RaReply(query[0].ToUpper() + "#Global tag refreshed!", success: true, logToConsole: true, "");
			}
			break;
		case "PERM":
			if (IsPlayer(sender, query[0]) && !(queryProcessor == null))
			{
				ulong permissions = queryProcessor.Roles.Permissions;
				string text12 = "Your permissions:";
				foreach (string allPermission in ServerStatic.PermissionsHandler.GetAllPermissions())
				{
					string text13 = ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler.GetPermissionValue(allPermission)) ? "*" : "";
					text12 = text12 + "\n" + allPermission + text13 + " (" + ServerStatic.PermissionsHandler.GetPermissionValue(allPermission) + "): " + (ServerStatic.PermissionsHandler.IsPermitted(permissions, allPermission) ? "YES" : "NO");
				}
				sender.RaReply(query[0].ToUpper() + "#" + text12, success: true, logToConsole: true, "");
			}
			break;
		case "RL":
		case "RLOCK":
		case "ROUNDLOCK":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents))
			{
				RoundSummary.RoundLock = !RoundSummary.RoundLock;
				sender.RaReply(query[0].ToUpper() + "#Round lock " + (RoundSummary.RoundLock ? "enabled!" : "disabled!"), success: true, logToConsole: true, "ServerEvents");
			}
			break;
		case "LL":
		case "LLOCK":
		case "LOBBYLOCK":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents))
			{
				RoundStart.LobbyLock = !RoundStart.LobbyLock;
				sender.RaReply(query[0].ToUpper() + "#Lobby lock " + (RoundStart.LobbyLock ? "enabled!" : "disabled!"), success: true, logToConsole: true, "ServerEvents");
			}
			break;
		case "RT":
		case "RTIME":
		case "ROUNDTIME":
			if (RoundStart.RoundLenght.Ticks == 0L)
			{
				sender.RaReply(query[0].ToUpper() + "#The round has not yet started!", success: false, logToConsole: true, "");
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#Round time: " + RoundStart.RoundLenght.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture), success: true, logToConsole: true, "");
			}
			break;
		case "PING":
			if (query.Length == 1)
			{
				if (queryProcessor == null)
				{
					sender.RaReply(query[0].ToUpper() + "#This command is only available for players!", success: false, logToConsole: true, "");
					break;
				}
				int connectionId = queryProcessor.connectionToClient.connectionId;
				if (connectionId == 0)
				{
					sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", success: false, logToConsole: true, "");
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#Your ping: " + LiteNetLib4MirrorServer.Peers[connectionId].Ping + "ms", success: true, logToConsole: true, "");
				}
			}
			else if (query.Length == 2)
			{
				if (!int.TryParse(query[1], out int id2))
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
					break;
				}
				GameObject gameObject7 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id2);
				if (gameObject7 == null)
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
					break;
				}
				int connectionId2 = gameObject7.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
				if (connectionId2 == 0)
				{
					sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", success: false, logToConsole: true, "");
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#Ping: " + LiteNetLib4MirrorServer.Peers[connectionId2].Ping + "ms", success: true, logToConsole: true, "");
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#Too many arguments! (expected 0 or 1)", success: false, logToConsole: true, "");
			}
			break;
		case "VERSION":
			sender.RaReply(query[0].ToUpper() + "#Server Version: " + CustomNetworkManager.CompatibleVersions[0] + " " + Application.buildGUID, success: true, logToConsole: true, "");
			break;
		case "RELOADCONFIG":
			if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs))
			{
				try
				{
					ConfigFile.ReloadGameConfigs();
					sender.RaReply(query[0].ToUpper() + "#Reloaded all configs!", success: true, logToConsole: true, "");
				}
				catch (Exception arg2)
				{
					sender.RaReply(query[0].ToUpper() + "#Reloading configs failed: " + arg2, success: false, logToConsole: true, "");
				}
			}
			break;
		case "KILL":
			if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement))
			{
				break;
			}
			if (query.Length <= 2)
			{
				if (!int.TryParse(query[1], out int id3))
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
					break;
				}
				GameObject gameObject = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id3);
				if (gameObject == null)
				{
					sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
				}
				else if (ReferenceHub.GetHub(gameObject).playerStats.HurtPlayer(new PlayerStats.HitInfo(-1f, "ADMIN", DamageTypes.Wall, 0), gameObject))
				{
					sender.RaReply(query[0].ToUpper() + "#Player has been killed!", success: true, logToConsole: true, "");
				}
				else
				{
					sender.RaReply(query[0].ToUpper() + "#Kill failed!", success: false, logToConsole: true, "");
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#Please specify the PlayerId!", success: false, logToConsole: true, "");
			}
			break;
		default:
			sender.RaReply("SYSTEM#Unknown command!", success: false, logToConsole: true, "");
			break;
		}
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
