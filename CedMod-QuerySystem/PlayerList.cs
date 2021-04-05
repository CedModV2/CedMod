using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.INIT;
using CedMod.QuerySystem.WS;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using GameCore;
using Newtonsoft.Json;
using NorthwoodLib;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.QuerySystem
{
    public class CommandHandler
    {
	    public static List<string> synced = new List<string>();
	    public static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm,
		    string replyScreen = "", bool reply = true)
	    {
		    if (ServerStatic.IsDedicated && sender.FullPermissions) return true;
		    if (PermissionsHandler.IsPermitted(sender.Permissions, perm)) return true;
		    if (reply)
			    sender.RaReply(
				    queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm,
				    false, true, replyScreen);
		    return false;
	    }
	    public static bool IsPlayer(CommandSender sender, string queryZero, string replyScreen = "")
	    {
		    if (sender is PlayerCommandSender)
		    {
			    return true;
		    }
		    sender.RaReply(queryZero + "#This command can be executed only from the game level.", success: false, logToConsole: true, replyScreen);
		    return false;
	    }
        public static void HandleCommand(SendingRemoteAdminCommandEventArgs ev)
        {
	        CommandSender sender = ev.CommandSender;
            switch (ev.Name.ToUpper())
            {
	            case "CMMINIMAP":
		            return;
		            ev.IsAllowed = false;
		            MiniMapClass map = new MiniMapClass();
		            map.MapElements = ServerEvents.Minimap;
		            foreach (Player plr in Player.List)
		            {
			            if (plr.Team == Team.RIP)
				            continue;
			            MiniMapPlayerElement pele = new MiniMapPlayerElement();
			            pele.Name = plr.Nickname;
			            pele.Position = plr.Position.ToString();
			            pele.Zone = plr.CurrentRoom.Zone.ToString();
			            pele.TeamColor = plr.RoleColor.ToString();
			            map.PlayerElements.Add(pele);
		            }
		            ev.ReplyMessage = JsonConvert.SerializeObject(map);
					break;
	            case "CMSYNC":
		            if (sender is CmSender)
		            {
			            if (!CommandProcessor.CheckPermissions(sender, "CMSYNC", PlayerPermissions.SetGroup, "", false))
				            return;
			            ev.IsAllowed = false;
			            Initializer.Logger.Info("CedMod-RoleSync", $"Assigning role: {ev.Arguments[1]} to {ev.Arguments[0]}.");
			            Player.Get(int.Parse(ev.Arguments[0])).ReferenceHub.serverRoles.SetGroup(ServerStatic.PermissionsHandler._groups[ev.Arguments[1]], false);
			            ServerStatic.PermissionsHandler._members[Player.Get(int.Parse(ev.Arguments[0])).UserId] = ev.Arguments[1];
			            synced.Add(Player.Get(int.Parse(ev.Arguments[0])).UserId);
		            }
		            break;
	            case "RESTARTQUERYSERVER":
		            if (!sender.CheckPermission("cedmod.restartquery"))
		            {
			            return;
		            }
		            ev.IsAllowed = false;
		            if (!QuerySystem.config.NewWebSocketSystem)
		            {
			            ev.ReplyMessage = "Query server is not enabled";
			            return;
		            }
		            WS.WebSocketSystem.Stop();
		            WS.WebSocketSystem.Start();
		            ev.ReplyMessage = "Query server restarted";
		            break;
	            case "PLAYERLISTCOLORED":
		            ev.IsAllowed = false;
                    try
                    {
	                    string q = ev.Name + " " + string.Join(" ", ev.Arguments.ToArray());
	                    PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
						string text = "\n";
						bool gameplayData = CheckPermissions(sender, ev.Name, PlayerPermissions.GameplayData, string.Empty, reply: false);
						PlayerCommandSender playerCommandSender2;
						if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
						{
							playerCommandSender2.Processor.GameplayData = gameplayData;
						}
						bool flag = q.Contains("STAFF", StringComparison.OrdinalIgnoreCase);
						bool flag2 = CheckPermissions(sender, ev.Name, PlayerPermissions.ViewHiddenBadges, string.Empty, reply: false);
						bool flag3 = CheckPermissions(sender, ev.Name, PlayerPermissions.ViewHiddenGlobalBadges, string.Empty, reply: false);
						if (playerCommandSender != null && playerCommandSender.ServerRoles.Staff)
						{
							flag2 = true;
							flag3 = true;
						}
						foreach (Player player in Player.List)
						{
							QueryProcessor component = player.ReferenceHub.queryProcessor;
							if (!flag)
							{
								string text2 = string.Empty;
								bool flag4 = false;
								ServerRoles component2 = player.ReferenceHub.serverRoles;
								try
								{
									if (string.IsNullOrEmpty(component2.HiddenBadge) || (component2.GlobalHidden && flag3) || (!component2.GlobalHidden && flag2))
									{
										text2 = (component2.RaEverywhere ? "[~] " : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty)));
									}
									flag4 = component2.OverwatchEnabled;
								}
								catch
								{
								}
								text = text + text2 + "(" + component.PlayerId + ") " + player.ReferenceHub.nicknameSync.CombinedName.Replace("\n", string.Empty) + (flag4 ? "<OVRM>" : string.Empty);
								CharacterClassManager ccm = player.ReferenceHub.characterClassManager;
								text = $"<color={ccm.CurRole.classColor.ToHex()}>" +
								       text + "</color>";
							}
							else
							{
								text = text + component.PlayerId + ";" + player.ReferenceHub.nicknameSync.CombinedName;
							}
							text += "\n";
						}
						if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
						{
							sender.RaReply(ev.Name + ":PLAYER_LIST#" + text, success: true, ev.Arguments.Count < 2 || ev.Arguments[1].ToUpper() != "SILENT", "");
						}
						else
						{
							sender.RaReply("StaffPlayerListReply#" + text, success: true, ev.Arguments.Count <2 || ev.Arguments[1].ToUpper() != "SILENT", "");
						}
					}
					catch (Exception ex2)
					{
						Initializer.Logger.LogException(ex2, "CedMod.PluginInterface", "PlayerListCommand");
						sender.RaReply(ev.Name + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source, success: false, logToConsole: true, "");
						throw;
					}
					break;
	            
	            case "PLAYERLISTCOLOREDSTEAMID":
		            ev.IsAllowed = false;
                    try
                    {
	                    string text = "\n";
						foreach (Player player in Player.List)
						{
							QueryProcessor component = player.ReferenceHub.queryProcessor;
							text += $"{player.UserId}:{player.DoNotTrack}:{player.RemoteAdminAccess}\n";
						}
						sender.RaReply(ev.Name + ":PLAYER_LIST#" + text, success: true, ev.Arguments.Count < 2 || ev.Arguments[1].ToUpper() != "SILENT", "");
                    }
					catch (Exception ex2)
					{
						Initializer.Logger.LogException(ex2, "CedMod.PluginInterface", "PlayerListCommand");
						sender.RaReply(ev.Name + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source, success: false, logToConsole: true, "");
						throw;
					}
					break;
            }
        }
    }
}