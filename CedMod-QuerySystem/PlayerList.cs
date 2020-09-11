using System;
using CedMod.INIT;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.Events.EventArgs;
using GameCore;
using NorthwoodLib;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.QuerySystem
{
    public class CommandHandler
    {
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
	            case "RESTARTQUERYSERVER":
		            ev.IsAllowed = false;
		            if (!QuerySystem.config.NewWebSocketSystem)
		            {
			            ev.ReplyMessage = "Query server is not enabled";
			            return;
		            }
		            WS.WebSocketServer.Stop();
		            WS.WebSocketServer.Start(ConfigFile.ServerConfig.GetInt("cm_port", 8000));
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
									flag4 = component2.OverwatchEnabled;
								}
								catch
								{
								}
								text = text + text2 + "(" + component.PlayerId + ") " + component.GetComponent<NicknameSync>().CombinedName.Replace("\n", string.Empty) + (flag4 ? "<OVRM>" : string.Empty);
								CharacterClassManager ccm = component.GetComponent<CharacterClassManager>();
								text = $"<color={ccm.CurRole.classColor.ToHex()}>" +
								       text + "</color>";
							}
							else
							{
								text = text + component.PlayerId + ";" + component.GetComponent<NicknameSync>().CombinedName;
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
            }
        }
    }
}