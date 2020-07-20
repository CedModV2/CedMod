
using System;
using System.Linq;
using System.Text;
using CedMod.BanSystem;
using CedMod.INIT;
using HarmonyLib;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.BadgeController
{
	public class Badge
    {
        public static void HandleBadge(string badgeinfo, ReferenceHub player)
        {
	        CmUser user = new CmUser();
            string text;
            if (badgeinfo == "None")
            {
	            user.hasbadge = false;
	            user.Badge = new[] {"None"};
            }
            else
            {
	            byte[] data = System.Convert.FromBase64String(badgeinfo);
	            text = System.Text.ASCIIEncoding.ASCII.GetString(data);
	            string[] badge = text.Split('~');
	            user.Badge = badge;
	            user.hasbadge = true;
	            if (CedModMain.config.CmBadge)
	            {
		            if (player.serverRoles.RemoteAdmin || player.serverRoles.RaEverywhere)
		            {
			            if (!player.serverRoles.Group.Cover)
			            {
				            player.serverRoles.NetworkMyText = badge[0];
				            player.serverRoles.NetworkMyColor = badge[2];
			            }
		            }
		            else
		            {
			            player.serverRoles.NetworkMyText = badge[0];
			            player.serverRoles.NetworkMyColor = badge[2];
		            }

		            if (badge[4] == "true")
		            {
			            player.serverRoles.SetText(null);
			            player.serverRoles.SetColor(null);
			            player.serverRoles.HiddenBadge = badge[0];
			            player.serverRoles.RefreshHiddenTag();
		            }
	            }
            }

            if (!BanSystem.BanSystem.Users.ContainsKey(player))
            {
	            Initializer.Logger.Debug("BadgeHandler","Adding player to playerlist");
	            BanSystem.BanSystem.Users.Add(player, user);
            }
        }
    }
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery), typeof(string), typeof(CommandSender))]
    internal static class SendingRemoteAdminCommand
    {
	    private static bool Prefix(ref string q, ref CommandSender sender)
        {
	        RemoteAdmin.PlayerCommandSender playerCommandSender = sender as RemoteAdmin.PlayerCommandSender;
	        QueryProcessor queryProcessor = playerCommandSender?.Processor;
	        string logName = sender.LogName;
	        string[] query = q.Split(' ');
            switch (query[0].ToUpper())
            {
                case "REQUEST_DATA":
			if (query.Length >= 2)
			{
				switch (query[1].ToUpper())
				{
				case "PLAYER_LIST":
					try
					{
						string text = "\n";
						bool gameplayData = Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, string.Empty, reply: false);
						RemoteAdmin.PlayerCommandSender playerCommandSender2;
						if ((playerCommandSender2 = (sender as RemoteAdmin.PlayerCommandSender)) != null)
						{
							playerCommandSender2.Processor.GameplayData = gameplayData;
						}
						bool flag = Misc.Contains(q, "STAFF", StringComparison.OrdinalIgnoreCase);
						bool flag2 = Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenBadges, string.Empty, reply: false);
						bool flag3 = Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenGlobalBadges, string.Empty, reply: false);
						if (playerCommandSender != null && playerCommandSender.SR.Staff)
						{
							flag2 = true;
							flag3 = true;
						}
						foreach (GameObject player in PlayerManager.players)
						{
							QueryProcessor component = player.GetComponent<QueryProcessor>();
							if (!flag)
							{
								string text2 = string.Empty;
								bool flag4 = false;
								ServerRoles component2 = component.GetComponent<ServerRoles>();
								try
								{
									if (string.IsNullOrEmpty(component2.HiddenBadge) || (component2.GlobalHidden && flag3) || (!component2.GlobalHidden && flag2))
									{
										text2 = (component2.RaEverywhere ? "[~] " : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty)));
									}

									CmUser user;
									if (BanSystem.BanSystem.Users.TryGetValue(player.GetComponent<ReferenceHub>(),
										out user))
									{
										if (user.hasbadge)
											text2 = $"[{user.Badge[1]}]";
									}
									flag4 = component2.OverwatchEnabled;
								}
								catch
								{
								}
								text = text + text2 + "(" + component.PlayerId + ") " + component.GetComponent<NicknameSync>().CombinedName.Replace("\n", string.Empty) + (flag4 ? "<OVRM>" : string.Empty);
							}
							else
							{
								text = text + component.PlayerId + ";" + component.GetComponent<NicknameSync>().CombinedName;
							}
							text += "\n";
						}
						if (!Misc.Contains(q, "STAFF", StringComparison.OrdinalIgnoreCase))
						{
							sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + text, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
						}
						else
						{
							sender.RaReply("StaffPlayerListReply#" + text, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
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
						if (!string.Equals(query[1], "PLAYER", StringComparison.OrdinalIgnoreCase) || (playerCommandSender != null && playerCommandSender.SR.Staff) || Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
						{
							try
							{
								GameObject gameObject4 = null;
								NetworkConnection networkConnection = null;
								foreach (NetworkConnection value in NetworkServer.connections.Values)
								{
									GameObject gameObject5 = GameCore.Console.FindConnectedRoot(value);
									if (query[2].Contains("."))
									{
										query[2] = query[2].Split('.')[0];
									}
									if (!(gameObject5 == null) && !(gameObject5.GetComponent<QueryProcessor>().PlayerId.ToString() != query[2]))
									{
										gameObject4 = gameObject5;
										networkConnection = value;
									}
								}
								if (gameObject4 == null)
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
								}
								else
								{
									bool flag5 = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
									bool flag6 = PermissionsHandler.IsPermitted(sender.Permissions, 18007046uL);
									RemoteAdmin.PlayerCommandSender playerCommandSender3;
									if ((playerCommandSender3 = (sender as RemoteAdmin.PlayerCommandSender)) != null)
									{
										playerCommandSender3.Processor.GameplayData = flag5;
									}
									if (playerCommandSender != null && (playerCommandSender.SR.Staff || playerCommandSender.SR.RaEverywhere))
									{
										flag6 = true;
									}
									ReferenceHub hub = ReferenceHub.GetHub(gameObject4.gameObject);
									CharacterClassManager characterClassManager = hub.characterClassManager;
									ServerRoles serverRoles = hub.serverRoles;
									if (query[1].ToUpper() == "PLAYER")
									{
										ServerLogs.AddLog(ServerLogs.Modules.DataAccess, logName + " accessed IP address of player " + gameObject4.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject4.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
									}
									StringBuilder stringBuilder = StringBuilderPool.Rent("<color=white>");
									stringBuilder.Append("Nickname: " + hub.nicknameSync.CombinedName);
									stringBuilder.Append("\nPlayer ID: " + hub.queryProcessor.PlayerId);
									stringBuilder.Append("\nIP: " + ((networkConnection == null) ? "null" : ((query[1].ToUpper() == "PLAYER") ? networkConnection.address : "[REDACTED]")));
									stringBuilder.Append("\nUser ID: " + ((!flag6) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (string.IsNullOrEmpty(characterClassManager.UserId) ? "(none)" : characterClassManager.UserId)));
									if (flag6)
									{
										if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
										{
											stringBuilder.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
										}
										if (!string.IsNullOrEmpty(characterClassManager.UserId2))
										{
											stringBuilder.Append("\nUser ID 2: " + characterClassManager.UserId2);
										}
									}
									stringBuilder.Append("\nServer role: " + serverRoles.GetColoredRoleString());
									bool flag7 = Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenBadges, string.Empty, reply: false);
									bool flag8 = Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenGlobalBadges, string.Empty, reply: false);
									if (playerCommandSender != null && playerCommandSender.SR.Staff)
									{
										flag7 = true;
										flag8 = true;
									}
									bool flag9 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
									bool num = !flag9 || (serverRoles.GlobalHidden && flag8) || (!serverRoles.GlobalHidden && flag7);
									if (num)
									{
										if (flag9)
										{
											stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
											stringBuilder.Append("\n<color=#DC143C>Hidden role type: </color>" + (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
										}
										if (serverRoles.RaEverywhere)
										{
											stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
										}
										else if (serverRoles.Staff)
										{
											stringBuilder.Append("\nActive flag: Studio Staff");
										}
									}
									if (characterClassManager.Muted)
									{
										stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
									}
									else if (characterClassManager.IntercomMuted)
									{
										stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
									}
									if (characterClassManager.GodMode)
									{
										stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
									}
									if (characterClassManager.NoclipEnabled)
									{
										stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
									}
									else if (serverRoles.NoclipReady)
									{
										stringBuilder.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
									}
									if (serverRoles.DoNotTrack)
									{
										stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
									}
									if (serverRoles.BypassMode)
									{
										stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
									}
									if (num && serverRoles.RemoteAdmin)
									{
										stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
									}
									CmUser user;
									if (BanSystem.BanSystem.Users.TryGetValue(hub, out user))
									{
										if (user.hasbadge)
											stringBuilder.Append($"\nActive flag: <color=#FFFF00>CedMod Staff: {user.Badge[0]}</color>");
									}
									if (serverRoles.OverwatchEnabled)
									{
										stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
									}
									else
									{
										stringBuilder.Append("\nClass: " + ((!flag5) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (RoleExtensionMethods.CheckBounds(characterClassManager.Classes, characterClassManager.CurClass) ? characterClassManager.CurRole.fullName : "None")));
										stringBuilder.Append("\nHP: " + (flag5 ? hub.playerStats.HealthToString() : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
										stringBuilder.Append("\nPosition: " + (flag5 ? $"[{hub.playerMovementSync.RealModelPosition.x}; {hub.playerMovementSync.RealModelPosition.y}; {hub.playerMovementSync.RealModelPosition.z}]" : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
										if (!flag5)
										{
											stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
										}
									}
									stringBuilder.Append("</color>");
									sender.RaReply(query[0].ToUpper() + ":PLAYER#" + stringBuilder, success: true, logToConsole: true, "PlayerInfo");
									StringBuilderPool.Return(stringBuilder);
									sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId), success: true, logToConsole: false, "PlayerInfo");
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
					if ((playerCommandSender != null && playerCommandSender.SR.Staff) || Functions.CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
					{
						if (query.Length >= 3)
						{
							try
							{
								GameObject gameObject2 = null;
								foreach (NetworkConnection value2 in NetworkServer.connections.Values)
								{
									GameObject gameObject3 = GameCore.Console.FindConnectedRoot(value2);
									if (query[2].Contains("."))
									{
										query[2] = query[2].Split('.')[0];
									}
									if (gameObject3 != null && gameObject3.GetComponent<QueryProcessor>().PlayerId.ToString() == query[2])
									{
										gameObject2 = gameObject3;
									}
								}
								if (gameObject2 == null)
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
								}
								else if (string.IsNullOrEmpty(gameObject2.GetComponent<CharacterClassManager>().AuthToken))
								{
									sender.RaReply(query[0].ToUpper() + ":PLAYER#Can't obtain auth token. Is server using offline mode or you selected the host?", success: false, logToConsole: true, "PlayerInfo");
								}
								else
								{
									ServerLogs.AddLog(ServerLogs.Modules.DataAccess, logName + " accessed authentication token of player " + gameObject2.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject2.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
									if (!Misc.Contains(q, "STAFF", StringComparison.OrdinalIgnoreCase))
									{
										string myNick = gameObject2.GetComponent<NicknameSync>().MyNick;
										string str = "<color=white>Authentication token of player " + myNick + "(" + gameObject2.GetComponent<QueryProcessor>().PlayerId + "):\n" + gameObject2.GetComponent<CharacterClassManager>().AuthToken + "</color>";
										sender.RaReply(query[0].ToUpper() + ":PLAYER#" + str, success: true, logToConsole: true, "null");
										sender.RaReply("BigQR#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
									}
									else
									{
										sender.RaReply("StaffTokenReply#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
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
				default:
					sender.RaReply(query[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", success: false, logToConsole: true, "PlayerInfo");
					break;
				}
			}
			else
			{
				sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "PlayerInfo");
			}
			return false;
                case "CMTAG":
	                if (Functions.IsPlayer(sender, query[0]) && CedModMain.config.CmBadge)
	                {
		                
		                foreach (NetworkConnection value in NetworkServer.connections.Values)
		                {
			                GameObject gameObject5 = GameCore.Console.FindConnectedRoot(value);
			                if (gameObject5.GetComponent<NicknameSync>().MyNick == sender.Nickname)
			                {
				                ReferenceHub player = gameObject5.GetComponent<ReferenceHub>();
				                CmUser user;
				                if (BanSystem.BanSystem.Users.TryGetValue(player, out user))
				                {
					                if (user.hasbadge)
						                if (player.serverRoles.RemoteAdmin || player.serverRoles.RaEverywhere)
						                {
							                player.serverRoles.HiddenBadge = null;
							                player.serverRoles.GlobalHidden = false;
							                player.serverRoles.RpcResetFixed();
							                player.serverRoles.RefreshPermissions(true);
							                player.serverRoles.NetworkMyText = user.Badge[0];
							                player.serverRoles.NetworkMyColor = user.Badge[2];
						                }
						                else
						                {
							                player.serverRoles.HiddenBadge = null;
							                player.serverRoles.GlobalHidden = false;
							                player.serverRoles.RpcResetFixed();
							                player.serverRoles.RefreshPermissions(true);
							                player.serverRoles.NetworkMyText = user.Badge[0];
							                player.serverRoles.NetworkMyColor = user.Badge[2];
						                }
				                }
			                }
		                }
		                sender.RaReply(query[0].ToUpper() + "#CmTag tag refreshed!", success: true, logToConsole: true, "");
	                }
	                return false;
            }

            return true;
        }
    }
}