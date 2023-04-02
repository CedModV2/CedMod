using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using GameCore;
using HarmonyLib;
using RemoteAdmin;
using UnityEngine;
using Utils;

namespace CedMod.Patches
{
	[HarmonyPatch(typeof(OfflineBanCommand), nameof(OfflineBanCommand.Execute))]
    public static class OBanCommandPatch
    {
	    public static bool Prefix(OfflineBanCommand __instance, bool __result, ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
			{
				response = "To execute this command provide at least 3 arguments!\nUsage: oban " + __instance.DisplayCommandUsage();
				return false;
			}
			string text = string.Empty;
			if (arguments.Count > 2)
			{
				text = arguments.Skip(2).Aggregate((string current, string n) => current + " " + n);
			}
			long num = 0L;
			try
			{
				num = Misc.RelativeTimeToSeconds(arguments.At(1), 60);
			}
			catch
			{
				response = "Invalid time: " + arguments.At(1);
				return false;
			}
			if (num < 0L)
			{
				num = 0L;
				arguments.At(1).Replace(arguments.At(1), "0");
			}
			if (num == 0L && !sender.CheckPermission(new PlayerPermissions[]
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
			Misc.IPAddressType ipaddressType;
			bool flag = Misc.ValidateIpOrHostname(arguments.At(0), out ipaddressType, false, false);
			if (flag)
			{
				response = "IP Bans are automatically issued by CedMod once a UserId ban is issued.";
				return false;
			}
			if (!arguments.At(0).Contains("@"))
			{
				response = "Target must be a valid UserID.";
				return false;
			}
			ServerLogs.AddLog(ServerLogs.Modules.Administrative, string.Concat(new string[]
			{
				sender.LogName,
				" banned an offline player with ",
				"UserID",
				arguments.At(0),
				". Ban duration: ",
				arguments.At(1),
				". Reason: ",
				(text == string.Empty) ? "(none)" : text,
				"."
			}), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
			new Thread(() =>
			{
				lock (BanSystem.Banlock)
				{
					API.BanId(arguments.At(0), num, sender.LogName, text, true).Wait();
				}
			}).Start();
			response = "UserID " + arguments.At(0) + " has been banned from this server.";
			return false;
        }
    }
}