using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using HarmonyLib;
using MEC;
using Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
using RemoteAdmin;
using RemoteAdmin.Communication;
using UnityEngine;
using UnityEngine.Networking;

namespace CedMod.Patches
{
	/// <summary>
	/// Patches <see cref="RaPlayer"/>.
	/// </summary>
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), new Type[] {typeof(CommandSender), typeof(string)})]
    public static class RaPlayerPatch
    {
        public static bool Prefix(RaPlayer __instance, CommandSender sender, string data)
        {
            Timing.RunCoroutine(RaPlayerCoRoutine(__instance, sender, data));
            return false;
        }

        public static IEnumerator<float> RaPlayerCoRoutine(RaPlayer __instance, CommandSender sender, string data)
        {
	        string[] array = data.Split(new char[]
			{
				' '
			});
			if (array.Length != 2)
			{
				yield break;
			}
			int num;
			if (!int.TryParse(array[0], out num))
			{
				yield break;
			}
			bool flag = num == 1;
			PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
			if (!flag && playerCommandSender != null && !playerCommandSender.ServerRoles.Staff && !CommandProcessor.CheckPermissions(sender, global::PlayerPermissions.PlayerSensitiveDataAccess))
			{
				yield break;
			}
			string[] array2;
			List<global::ReferenceHub> list = Utils.RAUtils.ProcessPlayerIdOrNamesList(new ArraySegment<string>(array.Skip(1).ToArray<string>()), 0, out array2, false);
			if (list.Count == 0)
			{
				yield break;
			}
			bool flag2 = global::PermissionsHandler.IsPermitted(sender.Permissions, 18007046UL);
			if (playerCommandSender != null && (playerCommandSender.ServerRoles.Staff || playerCommandSender.ServerRoles.RaEverywhere))
			{
				flag2 = true;
			}
			if (list.Count > 1)
			{
				StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
				stringBuilder.Append("Selecting multiple players:");
				stringBuilder.Append("\nPlayer ID: <color=green><link=CP_ID></link></color>");
				stringBuilder.Append("\nIP Address: " + ((!flag) ? "<color=green><link=CP_IP></link></color>" : "[REDACTED]"));
				stringBuilder.Append("\nUser ID: " + (flag2 ? "<color=green><link=CP_USERID></link></color>" : "[REDACTED]"));
				stringBuilder.Append("</color>");
				string text = string.Empty;
				string text2 = string.Empty;
				string text3 = string.Empty;
				foreach (global::ReferenceHub referenceHub in list)
				{
					text = text + referenceHub.playerId + ".";
					if (!flag)
					{
						text2 = text2 + ((referenceHub.networkIdentity.connectionToClient.IpOverride != null) ? referenceHub.networkIdentity.connectionToClient.OriginalIpAddress : referenceHub.networkIdentity.connectionToClient.address) + ",";
					}
					if (flag2)
					{
						text3 = text3 + referenceHub.characterClassManager.UserId + ".";
					}
				}
				if (text.Length > 0)
				{
					RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, text);
				}
				if (text2.Length > 0)
				{
					RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, text2);
				}
				if (text3.Length > 0)
				{
					RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, text3);
				}
				sender.RaReply(string.Format("${0} {1}", __instance.DataId, stringBuilder), true, true, string.Empty);
				StringBuilderPool.Shared.Return(stringBuilder);
				yield break;
			}
			global::ServerLogs.AddLog(global::ServerLogs.Modules.DataAccess, string.Format("{0} accessed IP address of player {1} ({2}).", sender.LogName, list[0].playerId, list[0].nicknameSync.MyNick), global::ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging, false);
			bool flag3 = global::PermissionsHandler.IsPermitted(sender.Permissions, global::PlayerPermissions.GameplayData);
			global::CharacterClassManager characterClassManager = list[0].characterClassManager;
			global::NicknameSync nicknameSync = list[0].nicknameSync;
			NetworkConnectionToClient connectionToClient = list[0].networkIdentity.connectionToClient;
			global::ServerRoles serverRoles = list[0].serverRoles;
			PlayerCommandSender playerCommandSender2;
			if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
			{
				playerCommandSender2.Processor.GameplayData = flag3;
			}
			StringBuilder stringBuilder2 = StringBuilderPool.Shared.Rent("<color=white>");
			stringBuilder2.Append("Nickname: " + nicknameSync.CombinedName);
			stringBuilder2.Append(string.Format("\nPlayer ID: {0} <color=green><link=CP_ID></link></color>", list[0].playerId));
			RaClipboard.Send(sender, RaClipboard.RaClipBoardType.PlayerId, string.Format("{0}", list[0].playerId));
			if (connectionToClient == null)
			{
				stringBuilder2.Append("\nIP Address: null");
			}
			else if (!flag)
			{
				stringBuilder2.Append("\nIP Address: " + connectionToClient.address + " ");
				if (connectionToClient.IpOverride != null)
				{
					RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connectionToClient.OriginalIpAddress ?? "");
					stringBuilder2.Append(" [routed via " + connectionToClient.OriginalIpAddress + "]");
				}
				else
				{
					RaClipboard.Send(sender, RaClipboard.RaClipBoardType.Ip, connectionToClient.address ?? "");
				}
				stringBuilder2.Append(" <color=green><link=CP_IP></link></color>");
			}
			else
			{
				stringBuilder2.Append("\nIP Address: [REDACTED]");
			}
			stringBuilder2.Append("\nUser ID: " + (flag2 ? (string.IsNullOrEmpty(characterClassManager.UserId) ? "(none)" : (characterClassManager.UserId + " <color=green><link=CP_USERID></link></color>")) : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
			if (flag2)
			{
				RaClipboard.Send(sender, RaClipboard.RaClipBoardType.UserId, characterClassManager.UserId ?? "");
				if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
				{
					stringBuilder2.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
				}
				if (!string.IsNullOrEmpty(characterClassManager.UserId2))
				{
					stringBuilder2.Append("\nUser ID 2: " + characterClassManager.UserId2);
				}
			}
			stringBuilder2.Append("\nServer role: " + serverRoles.GetColoredRoleString(false));
			bool flag4 = CommandProcessor.CheckPermissions(sender, global::PlayerPermissions.ViewHiddenBadges);
			bool flag5 = CommandProcessor.CheckPermissions(sender, global::PlayerPermissions.ViewHiddenGlobalBadges);
			if (playerCommandSender != null && playerCommandSender.ServerRoles.Staff)
			{
				flag4 = true;
				flag5 = true;
			}
			bool flag6 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
			bool flag7 = !flag6 || (serverRoles.GlobalHidden && flag5) || (!serverRoles.GlobalHidden && flag4);
			if (flag7)
			{
				if (flag6)
				{
					stringBuilder2.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
					stringBuilder2.Append("\n<color=#DC143C>Hidden role type: </color>" + (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
				}
				if (serverRoles.RaEverywhere)
				{
					stringBuilder2.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
				}
				else if (serverRoles.Staff)
				{
					stringBuilder2.Append("\nActive flag: Studio Staff");
				}
			}
			if (list[0].dissonanceUserSetup.AdministrativelyMuted)
			{
				stringBuilder2.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
			}
			else if (characterClassManager.IntercomMuted)
			{
				stringBuilder2.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
			}
			if (characterClassManager.GodMode)
			{
				stringBuilder2.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
			}
			if (characterClassManager.NoclipEnabled)
			{
				stringBuilder2.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
			}
			else if (serverRoles.NoclipReady)
			{
				stringBuilder2.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
			}
			if (serverRoles.DoNotTrack)
			{
				stringBuilder2.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
			}
			if (serverRoles.BypassMode)
			{
				stringBuilder2.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
			}
			if (flag7 && serverRoles.RemoteAdmin)
			{
				stringBuilder2.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
			}
			if (serverRoles.OverwatchEnabled)
			{
				stringBuilder2.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
			}
			else if (flag3)
			{
				stringBuilder2.Append("\nClass: ");
				stringBuilder2.Append(characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) ? characterClassManager.CurRole.fullName : "None");
				stringBuilder2.Append("\nHP: ");
				stringBuilder2.Append(CommandProcessor.GetRoundedStat<PlayerStatsSystem.HealthStat>(list[0]));
				stringBuilder2.Append("\nAHP: ");
				stringBuilder2.Append(CommandProcessor.GetRoundedStat<PlayerStatsSystem.AhpStat>(list[0]));
				stringBuilder2.Append("\nPosition: ");
				stringBuilder2.Append(list[0].playerMovementSync.RealModelPosition);
			}
			else
			{
				stringBuilder2.Append("\n<color=#D4AF37>Some fields were hidden. GameplayData permission required.</color>");
			}

			if (sender.CheckPermission("cedmod.requestdata"))
			{
				sender.RaReply(string.Format("${0} {1}", __instance.DataId, "Loading from CedMod API, please wait..."), true, true, string.Empty);
				UnityWebRequest www = new UnityWebRequest(API.APIUrl + $"/Auth/{characterClassManager.UserId}&{connectionToClient.address}", "OPTIONS");
				DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
				www.downloadHandler = dH;
				
				www.SetRequestHeader("ApiKey", CedModMain.Singleton.Config.CedMod.CedModApiKey);
				yield return Timing.WaitUntilDone(www.SendWebRequest());
				try
				{
					if (www.responseCode != 200) 
					{
						Log.Error($"Failed to RequestData CMAPI: {www.responseCode} | {www.downloadHandler.text}");
					}
					else
					{
						Dictionary<string, object> cmData = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
						stringBuilder2.Append("\n<color=#D4AF37>CedMod Added Fields:</color>");
						if (cmData["antiVpnWhitelist"] is bool ? (bool)cmData["antiVpnWhitelist"] : false)
							stringBuilder2.Append("\nActive flag: <color=#008080>AntiVpn Whitelisted</color>");
					
						if (cmData["triggeredAvpnPast"] is bool ? (bool)cmData["triggeredAvpnPast"] : false)
							stringBuilder2.Append("\nActive flag: <color=#red>AntiVpn Triggered</color>");
					
						stringBuilder2.Append($"\nModeration: {cmData["warns"]} Warnings, {cmData["banLogs"]} Banlogs");
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
				
				sender.RaReply(string.Format("${0} {1}", __instance.DataId, "Loading from Panel API, please wait..."), true, true, string.Empty);
				www = new UnityWebRequest(QuerySystem.PanelUrl + $"/Api/RequestData/{CedModMain.Singleton.Config.QuerySystem.SecurityKey}/{characterClassManager.UserId}", "OPTIONS");
				DownloadHandlerBuffer dH1 = new DownloadHandlerBuffer();
				www.downloadHandler = dH1;
				yield return Timing.WaitUntilDone(www.SendWebRequest());
				try
				{
					if (www.responseCode != 200) 
					{
						Log.Error($"Failed to RequestData PanelAPI: {www.responseCode} | {www.downloadHandler.text}");
					}
					else
					{
						Dictionary<string, object> cmData = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);

						if (!serverRoles.DoNotTrack)
						{
							stringBuilder2.Append($"\nActivity in the last 14 days: {TimeSpan.FromMinutes(cmData["activityLast14"] is double ? (double)cmData["activityLast14"] : 0).TotalHours}. Level {cmData["level"]} ({cmData["experience"]} Exp)");
						}
						if (cmData["panelUser"].ToString() != "")
							stringBuilder2.Append($"\nPanelUser: {cmData["panelUser"]}");

						stringBuilder2.Append($"\nPossible Alt Accounts: {cmData["usersFound"]}");
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
			
			stringBuilder2.Append("</color>");
			sender.RaReply(string.Format("${0} {1}", __instance.DataId, stringBuilder2), true, true, string.Empty);
			StringBuilderPool.Shared.Return(stringBuilder2);
			RaPlayerQR.Send(sender, false, string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId);
        }
    }
}