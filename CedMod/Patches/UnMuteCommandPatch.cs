﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using Exiled.API.Features;
using GameCore;
using HarmonyLib;
using RemoteAdmin;
using UnityEngine;
using Utils;

namespace CedMod.Patches
{
	/// <summary>
	/// Patches <see cref="UnmuteCommand"/>
	/// </summary>
	[HarmonyPatch(typeof(UnmuteCommand), nameof(UnmuteCommand.Execute))]
    public static class UnMuteCommandPatch
    {
	    public static string Command { get; } = "unmute";

	    public static string[] Aliases { get; } = new string[]
	    {
	    };
	    
	    public static string Description { get; } = "Allows the specified player(s) to speak again.";
	    
	    public static string[] Usage { get; } = new string[]
	    {
		    "%player%",
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

            string[] array;
			List<ReferenceHub> list = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out array);
			if (list == null)
			{
				response = "An unexpected problem has occurred during PlayerId/Name array processing.";
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
						var plr = Player.Get(referenceHub);
						plr.SendConsoleMessage("You have been unmuted", "green");
						plr.Broadcast(5, "You have been unmuted", Broadcast.BroadcastFlags.Normal);
						plr.IsMuted = false;
						plr.CustomInfo = "";
						Task.Factory.StartNew(() =>
						{
							API.UnMute(plr);
						});
						global::ServerLogs.AddLog(global::ServerLogs.Modules.Administrative, sender.LogName + " unmuted player " + referenceHub.LoggedNameFromRefHub() + ".", global::ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
						num2++;
					}
				}
				catch (Exception ex)
				{
					num3 += 1;
					Debug.Log(ex);
					text2 = "Error occured during unmuting: " + ex.Message + ".\n" + ex.StackTrace;
				}
			}
			if (num3 == 0)
			{
				string arg = "Unmuted";
				response = string.Format("Done! {0} {1} player{2}", arg, num2, (num2 == 1) ? "!" : "s!");
				__result = true;
				return false;
			}
			response = string.Format("Failed to execute the command! Failures: {0}\nLast error log:\n{1}", num3, text2);
			return false;
        }
    }
}