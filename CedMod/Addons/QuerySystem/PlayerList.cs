using System.Collections.Generic;
using RemoteAdmin;

namespace CedMod.Addons.QuerySystem
{
    public class CommandHandler
    {
	    public static readonly List<string> Synced = new List<string>();
	    public static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm, string replyScreen = "", bool reply = true)
	    {
		    if ((ServerStatic.IsDedicated && sender.FullPermissions) || PermissionsHandler.IsPermitted(sender.Permissions, perm)) 
			    return true;
		    
		    if (reply)
			    sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm, false, true, replyScreen);
		    
		    return false;
	    }
	    public static bool IsPlayer(CommandSender sender, string queryZero, string replyScreen = "")
	    {
		    if (sender is PlayerCommandSender)
			    return true;
		    
		    sender.RaReply(queryZero + "#This command can be executed only from the game level.", success: false, logToConsole: true, replyScreen);
		    return false;
	    }
    }
}