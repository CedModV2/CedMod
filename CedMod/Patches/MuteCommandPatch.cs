using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using GameCore;
using HarmonyLib;
using PluginAPI.Core;
using RemoteAdmin;
using UnityEngine;
using Utils;
using VoiceChat;

namespace CedMod.Patches
{
	[HarmonyPatch(typeof(MuteCommand), nameof(MuteCommand.Execute))]
    public static class MuteCommandPatch
    {
	    public static string Command { get; } = "mute";

	    public static string[] Aliases { get; } = new string[]
	    {
	    };
	    
	    public static string Description { get; } = "Prevents the specified player(s) from being able to speak.";
	    
	    public static string[] Usage { get; } = new string[]
	    {
		    "%player%",
		    "Duration",
		    "Reason"
	    };
	    
	    
        public static bool Prefix(BanCommand __instance, bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
	        if (!sender.CheckPermission(new global::PlayerPermissions[]
	            {
		            global::PlayerPermissions.BanningUpToDay,
		            global::PlayerPermissions.LongTermBanning,
		            global::PlayerPermissions.PlayersManagement
	            }, out response))
	        {
		        response = "No permissions.";
		        return false;
	        }

	        if (CedModMain.Singleton.Config.CedMod.OnlyAllowPanelMutes)
	        {
		        response = "Mutes can only be issued from the CedMod CommunityManagement panel, use External Panel with a player selected to issue mutes";
		        return false;
	        }
            if (arguments.Count < 1)
			{
				response = "To execute this command provide at least 1 arguments!\nUsage: " + arguments.Array[0] + " " + __instance.DisplayCommandUsage();
				return false;
			}

            if (arguments.Count < 3 && CedModMain.Singleton.Config.CedMod.UseMuteDurationAndReason)
            {
	            response = "To execute this command provide at least 3 arguments!\nUsage: mute [PlayerIDs/PlayerNames] [Duration] [Reason]";
	            return false;
            }
            
			string[] array;
			List<ReferenceHub> list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out array);
			if (list == null)
			{
				response = "An unexpected problem has occurred during PlayerId/Name array processing.";
				return false;
			}
			if (array == null && CedModMain.Singleton.Config.CedMod.UseMuteDurationAndReason)
			{
				response = "An error occured while processing this command.\nUsage: [PlayerIDs/PlayerNames] [Duration] [Reason]";
				return false;
			}
			string text = "None Specified";
			if (CedModMain.Singleton.Config.CedMod.UseMuteDurationAndReason)
			{
				if (array.Length > 1)
				{
					text = array.Skip(1).Aggregate((current, n) => current + " " + n);
				}
			}
			
			long num = (long)TimeSpan.FromMinutes(143998560).TotalSeconds;
			if (CedModMain.Singleton.Config.CedMod.UseMuteDurationAndReason)
			{
				try
				{
					num = Misc.RelativeTimeToSeconds(array[0], 60);
				}
				catch
				{
					response = "Invalid time: " + array[0];
					return false;
				}
				if (num < 0L)
				{
					num = 0L;
					array[0] = "0";
				}
			}
			
			ushort num2 = 0;
			ushort num3 = 0;
			string text2 = string.Empty;
			foreach (ReferenceHub referenceHub in list)
			{
				try
				{
					if (referenceHub == null)
					{
						num3 += 1;
					}
					else
					{
						var plr = CedModPlayer.Get(referenceHub);
						plr.SendConsoleMessage(CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", MuteType.Global.ToString()).Replace("{duration}", num.ToString()).Replace("{reason}", text), "red");
						Broadcast.Singleton.TargetAddElement(plr.Connection, CedModMain.Singleton.Config.CedMod.MuteMessage.Replace("{type}", MuteType.Global.ToString()).Replace("{duration}", num.ToString()).Replace("{reason}", text), 5, Broadcast.BroadcastFlags.Normal);
						//plr.Mute(true);
						VoiceChatMutes.SetFlags(plr.ReferenceHub, VcMuteFlags.LocalRegular);
						
						plr.CustomInfo = CedModMain.Singleton.Config.CedMod.MuteCustomInfo.Replace("{type}", MuteType.Global.ToString());
						Task.Factory.StartNew(async () =>
						{
							await API.Mute(plr, sender.LogName, num, text, MuteType.Global);
						});
						global::ServerLogs.AddLog(global::ServerLogs.Modules.Administrative, sender.LogName + " muted player " + referenceHub.LoggedNameFromRefHub() + ".", global::ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
						num2++;
					}
				}
				catch (Exception ex)
				{
					num3 += 1;
					Debug.Log(ex);
					text2 = "Error occured during banning: " + ex.Message + ".\n" + ex.StackTrace;
				}
			}
			if (num3 == 0)
			{
				string arg = "Muted";
				response = string.Format("Done! {0} {1} player{2}", arg, num2, (num2 == 1) ? "!" : "s!");
				__result = true;
				return false;
			}
			response = string.Format("Failed to execute the command! Failures: {0}\nLast error log:\n{1}", num3, text2);
			return false;
        }
    }
}