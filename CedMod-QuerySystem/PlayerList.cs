using System;
using System.Collections.Generic;
using CedMod.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using NorthwoodLib;
using RemoteAdmin;

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
            
            }
        }
    }
}