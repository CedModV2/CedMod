using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using GameCore;
using HarmonyLib;
using PluginAPI.Core;
using RemoteAdmin;
using UnityEngine;
using Utils;

namespace CedMod.Patches
{
	[HarmonyPatch(typeof(BanCommand), nameof(BanCommand.Execute))]
    public static class BanCommandPatch
    {
	    public static string Command { get; } = "ban";

	    public static string[] Aliases { get; } = new string[]
	    {
	    };
	    
	    public static string Description { get; } = "Bans a player.";
	    
	    public static string[] Usage { get; } = new string[]
	    {
		    "%player%",
		    "Duration",
		    "Reason"
	    };
	    
	    
        public static bool Prefix(BanCommand __instance, bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
			{
				response = "To execute this command provide at least 2 arguments!\nUsage: " + arguments.Array[0] + " " + __instance.DisplayCommandUsage();
				return false;
			}
			string[] array;
			List<ReferenceHub> list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out array);
			if (list == null)
			{
				response = "An unexpected problem has occurred during PlayerId/Name array processing.";
				return false;
			}
			if (array == null)
			{
				response = "An error occured while processing this command.\nUsage: " + __instance.DisplayCommandUsage();
				return false;
			}
			string text = string.Empty;
			if (array.Length > 1)
			{
				text = array.Skip(1).Aggregate((current, n) => current + " " + n);
			}
			long num = 0L;
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
			if (num == 0L && !sender.CheckPermission(new[]
			{
				PlayerPermissions.KickingAndShortTermBanning,
				PlayerPermissions.BanningUpToDay,
				PlayerPermissions.LongTermBanning
			}, out response))
			{
				return false;
			}
			if (num > 0L && num <= 3600L && !sender.CheckPermission(PlayerPermissions.KickingAndShortTermBanning, out response))
			{
				return false;
			}
			if (num > 3600L && num <= 86400L && !sender.CheckPermission(PlayerPermissions.BanningUpToDay, out response))
			{
				return false;
			}
			if (num > 86400L && !sender.CheckPermission(PlayerPermissions.LongTermBanning, out response))
			{
				return false;
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
						string combinedName = referenceHub.nicknameSync.CombinedName;
						CommandSender commandSender;
						if ((commandSender = (sender as CommandSender)) != null && !commandSender.FullPermissions)
						{
							UserGroup group = referenceHub.serverRoles.Group;
							int b = (group != null) ? group.RequiredKickPower : 0;
							if (b > commandSender.KickPower)
							{
								num3 += 1;
								text2 = string.Format("You can't kick/ban {0}. Required kick power: {1}, your kick power: {2}.", combinedName, b, commandSender.KickPower);
								sender.Respond(text2, false);
								continue;
							}
						}
						ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
						{
							sender.LogName,
							" banned player ",
							referenceHub.LoggedNameFromRefHub(),
							". Ban duration: ",
							array[0],
							". Reason: ",
							(text == string.Empty) ? "(none)" : text,
							"."
						}), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
						if (ServerStatic.PermissionsHandler.IsVerified && referenceHub.serverRoles.BypassStaff)
						{
							BanPlayer.KickUser(referenceHub, text);
						}
						else
						{
							if (num == 0L && ConfigFile.ServerConfig.GetBool("broadcast_kicks"))
								Broadcast.Singleton.RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_kick_text", "%nick% has been kicked from this server.").Replace("%nick%", combinedName), ConfigFile.ServerConfig.GetUShort("broadcast_kick_duration", (ushort) 5), Broadcast.BroadcastFlags.Normal);
							else if (num != 0L && ConfigFile.ServerConfig.GetBool("broadcast_bans", true))
								Broadcast.Singleton.RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", combinedName), ConfigFile.ServerConfig.GetUShort("broadcast_ban_duration", (ushort) 5), Broadcast.BroadcastFlags.Normal);

							new Thread(() =>
							{
								lock (BanSystem.Banlock)
								{
									API.Ban(CedModPlayer.Get(referenceHub), num, sender.LogName, text, false).Wait();
								}
							}).Start();
						}
						num2 += 1;
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
				string arg = "Banned";
				int num4;
				if (int.TryParse(array[0], out num4))
				{
					arg = ((num4 > 0) ? "Banned" : "Kicked");
				}
				response = string.Format("Done! {0} {1} player{2}", arg, num2, (num2 == 1) ? "!" : "s!");
				__result = true;
				return false;
			}
			response = string.Format("Failed to execute the command! Failures: {0}\nLast error log:\n{1}", num3, text2);
			return false;
        }
    }
}