using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cryptography;
using GameCore;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using PluginAPI.Core;
using PluginAPI.Enums;
using PluginAPI.Events;
using SlProxy;
using Log = PluginAPI.Core.Log;

namespace CedMod.Addons.QuerySystem.Patches
{
	[HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
		nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
	public static class ReservedSlotsPatch
	{
		public static bool Prefix(ConnectionRequest request)
		{
			ProcessConnectionRequest(request);
			return false;
		}

		public static void ProcessConnectionRequest(ConnectionRequest request)
		{
			try
			{
				if (!request.Data.TryGetByte(out var clientType) || clientType >= 2)
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.InvalidToken);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				if (clientType == (byte)CustomLiteNetLib4MirrorTransport.ClientType.VerificationService)
				{
					if (CustomLiteNetLib4MirrorTransport.VerificationChallenge != null &&
					    request.Data.TryGetString(out var verChallenge) &&
					    verChallenge == CustomLiteNetLib4MirrorTransport.VerificationChallenge)
					{
						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.VerificationAccepted);
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport
							.VerificationResponse);
						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);

						CustomLiteNetLib4MirrorTransport.VerificationChallenge = null;
						CustomLiteNetLib4MirrorTransport.VerificationResponse = null;
						ServerConsole.AddLog("Verification challenge and response have been sent.", ConsoleColor.Green);
						return;
					}

					CustomLiteNetLib4MirrorTransport.Rejected++;
					if (CustomLiteNetLib4MirrorTransport.Rejected >
					    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
						CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

					if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
					    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
						ServerConsole.AddLog(
							$"Invalid verification challenge has been received from endpoint {request.RemoteEndPoint}.");

					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.VerificationRejected);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				byte compatibilityRevision = 0;
				if (!request.Data.TryGetByte(out var major) || !request.Data.TryGetByte(out var minor) ||
				    !request.Data.TryGetByte(out var revision) ||
				    !request.Data.TryGetBool(out var backwardCompatibility) || backwardCompatibility &&
				    !request.Data.TryGetByte(out compatibilityRevision))
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.VersionMismatch);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				if (!GameCore.Version.CompatibilityCheck(GameCore.Version.Major, GameCore.Version.Minor,
					    GameCore.Version.Revision, major, minor, revision, backwardCompatibility,
					    compatibilityRevision))
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.VersionMismatch);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				var read = request.Data.TryGetInt(out var responseKey);
				if (!request.Data.TryGetBytesWithLength(out var response)) read = false;

				if (!read)
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.InvalidChallenge);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				if (CustomLiteNetLib4MirrorTransport.DelayConnections)
				{
					CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Delay);
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.DelayTime);
					if (CustomLiteNetLib4MirrorTransport.DelayVolume < 255)
						CustomLiteNetLib4MirrorTransport.DelayVolume++;

					if (CustomLiteNetLib4MirrorTransport.DelayVolume <
					    CustomLiteNetLib4MirrorTransport.DelayVolumeThreshold)
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Delayed connection incoming from endpoint {request.RemoteEndPoint} by {CustomLiteNetLib4MirrorTransport.DelayTime} seconds.");
						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
					}
					else
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Force delayed connection incoming from endpoint {request.RemoteEndPoint} by {CustomLiteNetLib4MirrorTransport.DelayTime} seconds.");
						request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					}

					return;
				}

				if (CustomLiteNetLib4MirrorTransport.UseChallenge)
				{
					if (responseKey == 0 || response == null || response.Length == 0)
					{
						string s = Encoding.Default.GetString(response);
						var reader = new NetDataReader(request.Data.RawData);
						reader._position = 30;
						var preauthdata = PreAuthModel.ReadPreAuth(reader);
						if (preauthdata == null)
						{
							if (CedModMain.Singleton.Config.CedMod.ShowDebug)
								Log.Debug($"Rejected preauth due to null data");
							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Custom);
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(
								$"[CedModAntiPreAuthSpam]\nYour connection has been rejected as the 'PreAuth' data sent from your client appears to be invalid, please restart your game or run 'ar' in your client console, You can usually open the client console by pressing ` or ~");
							return;
						}

						if (!ECDSA.VerifyBytes($"{s};{preauthdata.Flags};{preauthdata.Region};{preauthdata.Expiration}",
							    preauthdata.Signature, ServerConsole.PublicKey))
						{
							if (CedModMain.Singleton.Config.CedMod.ShowDebug)
								Log.Debug($"Rejected preauth due to invalidity\n{preauthdata}");
							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Custom);
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(
								$"[CedModAntiPreAuthSpam]\nYour connection has been rejected as the 'PreAuth' data sent from your client appears to be invalid, please restart your game or run 'ar' in your client console, You can usually open the client console by pressing ` or ~");
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
							return;
						}
						
						if (!CustomLiteNetLib4MirrorTransport.CheckIpRateLimit(request)) return;
						int id = 0;
						var key = string.Empty;

						for (byte i = 0; i < 3; i++)
						{
							id = RandomGenerator.GetInt32(false);
							if (id == 0) id = 1;
							key = request.RemoteEndPoint.Address + "-" + id;

							if (!CustomLiteNetLib4MirrorTransport.Challenges.ContainsKey(key)) break;
							if (i != 2) continue;
							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Error);
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

							if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
								ServerConsole.AddLog(
									$"Failed to generate ID for challenge for incoming connection from endpoint {request.RemoteEndPoint}.");
							return;
						}

						var data = RandomGenerator.GetBytes(CustomLiteNetLib4MirrorTransport.ChallengeInitLen + CustomLiteNetLib4MirrorTransport.ChallengeSecretLen, true);

						CustomLiteNetLib4MirrorTransport.ChallengeIssued++;
						if (CustomLiteNetLib4MirrorTransport.ChallengeIssued >
						    CustomLiteNetLib4MirrorTransport.IssuedThreshold)
							CustomLiteNetLib4MirrorTransport.SuppressIssued = true;

						if (!CustomLiteNetLib4MirrorTransport.SuppressIssued && CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Requested challenge for incoming connection from endpoint {request.RemoteEndPoint}.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Challenge);
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)CustomLiteNetLib4MirrorTransport
							.ChallengeMode);
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put(id);

						switch (CustomLiteNetLib4MirrorTransport.ChallengeMode)
						{
							case ChallengeType.MD5:
								CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(data, 0,
									CustomLiteNetLib4MirrorTransport.ChallengeInitLen);
								CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.ChallengeSecretLen);
								CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(Md.Md5(data));
								CustomLiteNetLib4MirrorTransport.Challenges.Add(key,
									new PreauthChallengeItem(new ArraySegment<byte>(data,
										CustomLiteNetLib4MirrorTransport.ChallengeInitLen,
										CustomLiteNetLib4MirrorTransport.ChallengeSecretLen)));
								break;

							case ChallengeType.SHA1:
								CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(data, 0,
									CustomLiteNetLib4MirrorTransport.ChallengeInitLen);
								CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CustomLiteNetLib4MirrorTransport.ChallengeSecretLen);
								CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(Sha.Sha1(data));
								CustomLiteNetLib4MirrorTransport.Challenges.Add(key,
									new PreauthChallengeItem(new ArraySegment<byte>(data,
										CustomLiteNetLib4MirrorTransport.ChallengeInitLen,
										CustomLiteNetLib4MirrorTransport.ChallengeSecretLen)));
								break;

							default:
								CustomLiteNetLib4MirrorTransport.RequestWriter.PutBytesWithLength(data);
								CustomLiteNetLib4MirrorTransport.Challenges.Add(key,
									new PreauthChallengeItem(new ArraySegment<byte>(data)));
								break;
						}

						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
						return;
					}

					{
						var key = request.RemoteEndPoint.Address + "-" + responseKey;
						if (!CustomLiteNetLib4MirrorTransport.Challenges.ContainsKey(key))
						{
							CustomLiteNetLib4MirrorTransport.Rejected++;
							if (CustomLiteNetLib4MirrorTransport.Rejected >
							    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
								CustomLiteNetLib4MirrorTransport.SuppressRejections =
									true;

							if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
							    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
								ServerConsole.AddLog(
									$"Security challenge response of incoming connection from endpoint {request.RemoteEndPoint} has been CustomLiteNetLib4MirrorTransport.Rejected (invalid Challenge ID).");

							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(
								(byte)RejectionReason.InvalidChallengeKey);
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
							return;
						}

						var exp = CustomLiteNetLib4MirrorTransport.Challenges[key]
							.ValidResponse;
						if (!response.SequenceEqual(exp))
						{
							CustomLiteNetLib4MirrorTransport.Rejected++;
							if (CustomLiteNetLib4MirrorTransport.Rejected >
							    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
								CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

							if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
							    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
								ServerConsole.AddLog(
									$"Security challenge response of incoming connection from endpoint {request.RemoteEndPoint} has been CustomLiteNetLib4MirrorTransport.Rejected (invalid response).");

							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.InvalidChallenge);
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
							return;
						}

						CustomLiteNetLib4MirrorTransport.Challenges.Remove(key);
						CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();

						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Security challenge response of incoming connection from endpoint {request.RemoteEndPoint} has been accepted.");
					}
				}
				else if (!CustomLiteNetLib4MirrorTransport.CheckIpRateLimit(request)) return;

				int position = request.Data.Position;

				if (!CharacterClassManager.OnlineMode)
				{
					var ban = BanHandler.QueryBan(null, request.RemoteEndPoint.Address.ToString());
					if (ban.Value != null)
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Player tried to connect from banned endpoint {request.RemoteEndPoint}.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Banned);
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put(ban.Value.Expires);
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put(ban.Value?.Reason ?? string.Empty);
						request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (!CustomLiteNetLib4MirrorTransport.ProcessCancellationData(request, EventManager.ExecuteEvent<PreauthCancellationData>(ServerEventType.PlayerPreauth, null, request.RemoteEndPoint.Address.ToString(), 0, CentralAuthPreauthFlags.None, null, null, request, position)))
						return;

					request.Accept();
					CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
					return;
				}

				if (!request.Data.TryGetString(out var userId) || userId == string.Empty)
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.AuthenticationRequired);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				if (!request.Data.TryGetLong(out var expiration) ||
				    !request.Data.TryGetByte(out var flagsRaw) || !request.Data.TryGetString(out var country) ||
				    !request.Data.TryGetBytesWithLength(out var signature))
				{
					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Error);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					return;
				}

				string ep;
				string realIp = null;
				if (!CustomLiteNetLib4MirrorTransport.IpPassthroughEnabled ||
				    !CustomLiteNetLib4MirrorTransport.TrustedProxies.Contains(request.RemoteEndPoint.Address) ||
				    !request.Data.TryGetString(out realIp))
					ep = request.RemoteEndPoint.ToString();
				else
					ep = $"{realIp} [routed via {request.RemoteEndPoint}]";

				CentralAuthPreauthFlags flags = (CentralAuthPreauthFlags)flagsRaw;

				try
				{
					if (!ECDSA.VerifyBytes($"{userId};{flagsRaw};{country};{expiration}", signature,
						    ServerConsole.PublicKey))
					{
						CustomLiteNetLib4MirrorTransport.Rejected++;
						if (CustomLiteNetLib4MirrorTransport.Rejected >
						    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
							CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

						if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
						    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Player from endpoint {ep} sent preauthentication token with invalid digital signature.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.InvalidToken);
						request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (TimeBehaviour.CurrentUnixTimestamp > expiration)
					{
						CustomLiteNetLib4MirrorTransport.Rejected++;
						if (CustomLiteNetLib4MirrorTransport.Rejected >
						    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
							CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

						if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
						    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
						{
							ServerConsole.AddLog($"Player from endpoint {ep} sent expired preauthentication token.");
							ServerConsole.AddLog(
								"Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
						}

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.ExpiredAuth);
						request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
					{
						if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(userId))
						{
							CustomLiteNetLib4MirrorTransport.Rejected++;
							if (CustomLiteNetLib4MirrorTransport.Rejected >
							    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
								CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

							if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
							    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
								ServerConsole.AddLog(
									$"Incoming connection from {userId} ({ep}) CustomLiteNetLib4MirrorTransport.Rejected due to exceeding the rate limit.");

							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.RateLimit);
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
							CustomLiteNetLib4MirrorTransport.ResetIdleMode();
							return;
						}

						CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(userId);
					}

					if (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreBans) ||
					    !ServerStatic.GetPermissionsHandler().IsVerified)
					{
						var ban = BanHandler.QueryBan(userId, realIp ?? request.RemoteEndPoint.Address.ToString());
						if (ban.Key != null || ban.Value != null)
						{
							CustomLiteNetLib4MirrorTransport.Rejected++;
							if (CustomLiteNetLib4MirrorTransport.Rejected >
							    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
								CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

							if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
							    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							{
								ServerConsole.AddLog(
									$"{(ban.Key == null ? "Player" : "Banned player")} {userId} tried to connect from {(ban.Value == null ? "" : "banned ")} endpoint {ep}.");
								ServerLogs.AddLog(ServerLogs.Modules.Networking,
									$"{(ban.Key == null ? "Player" : "Banned player")} {userId} tried to connect from {(ban.Value == null ? "" : "banned ")} endpoint {ep}.",
									ServerLogs.ServerLogType.ConnectionUpdate);
							}

							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Banned);
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(ban.Key?.Expires ?? ban.Value.Expires);
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(ban.Key?.Reason ??
							                                                   ban.Value?.Reason ?? string.Empty);
							request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
							CustomLiteNetLib4MirrorTransport.ResetIdleMode();
							return;
						}
					}

					if (flags.HasFlagFast(CentralAuthPreauthFlags.AuthRejected))
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Player {userId} ({ep}) kicked due to auth rejection by central server.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason
							.CentralServerAuthRejected);
						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (flags.HasFlagFast(CentralAuthPreauthFlags.GloballyBanned) &&
					    (ServerStatic.PermissionsHandler.IsVerified || CustomLiteNetLib4MirrorTransport.UseGlobalBans))
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog($"Player {userId} ({ep}) kicked due to an active global ban.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.GloballyBanned);
						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (!(flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist) &&
					      ServerStatic.GetPermissionsHandler().IsVerified) && !WhiteList.IsWhitelisted(userId))
					{
						if (CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
							ServerConsole.AddLog(
								$"Player {userId} tried joined from endpoint {ep}, but is not whitelisted.");

						CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
						CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.NotWhitelisted);
						request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
						CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						return;
					}

					if (CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None &&
					    !(flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreGeoblock) &&
					      ServerStatic.PermissionsHandler.BanTeamBypassGeo) &&
					    !(CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted && WhiteList.IsOnWhitelist(userId)))
					{
						if ((CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Whitelist &&
						     !CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(country.ToUpper())) ||
						    (CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Blacklist &&
						     CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(country.ToUpper())))
						{
							CustomLiteNetLib4MirrorTransport.Rejected++;
							if (CustomLiteNetLib4MirrorTransport.Rejected >
							    CustomLiteNetLib4MirrorTransport.RejectionThreshold)
								CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

							if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
							    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
								ServerConsole.AddLog(
									$"Player {userId} ({ep}) tried joined from blocked country {country.ToUpper()}.");

							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Geoblocked);
							request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
							CustomLiteNetLib4MirrorTransport.ResetIdleMode();
							return;
						}
					}

					if (CustomLiteNetLib4MirrorTransport.UserIdFastReload.Contains(userId))
						CustomLiteNetLib4MirrorTransport.UserIdFastReload.Remove(userId);

					bool shouldLet = LiteNetLib4MirrorCore.Host.ConnectedPeersCount < CustomNetworkManager.slots;

					if (!shouldLet && QuerySystem.ReservedSlotUserids.Contains(userId)) //todo. feature to allow users to join if the server is full due to a user with a reserved slot
						shouldLet = true;

					if (!shouldLet && flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) && ServerStatic.PermissionsHandler.BanTeamSlots)
						shouldLet = true;

					if (!shouldLet && ReservedSlot.HasReservedSlot(userId, out bool bypass) && (bypass || LiteNetLib4MirrorCore.Host.ConnectedPeersCount < CustomNetworkManager.slots + CustomNetworkManager.reservedSlots))
						shouldLet = true;
					

					if (shouldLet)
					{
						if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
							CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(userId);
						else
							CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint,
								new PreauthItem(userId));

						if (!CustomLiteNetLib4MirrorTransport.ProcessCancellationData(request, EventManager.ExecuteEvent<PreauthCancellationData>(ServerEventType.PlayerPreauth, userId, request.RemoteEndPoint.Address.ToString(), expiration, flags, country, signature, request, position)))
							return;

						Task.Factory.StartNew(async () =>
						{
							string id = userId;
							string ip = realIp ?? request.RemoteEndPoint.Address.ToString();
							Dictionary<string, string> info = (Dictionary<string, string>) await API.APIRequest("Auth/", $"{id}&{ip}");
							lock (BanSystem.CachedStates)
							{
								if (BanSystem.CachedStates.ContainsKey(id))
									BanSystem.CachedStates.Remove(id);
								BanSystem.CachedStates.Add(id, info);
							}
							
						});
						NetPeer netPeer = request.Accept();

						if (realIp != null)
						{
							if (CustomLiteNetLib4MirrorTransport.RealIpAddresses.ContainsKey(netPeer.Id))
								CustomLiteNetLib4MirrorTransport.RealIpAddresses[netPeer.Id] = realIp;
							else
								CustomLiteNetLib4MirrorTransport.RealIpAddresses.Add(netPeer.Id, realIp);
						}

						if (Statistics.PeakPlayers.Total < LiteNetLib4MirrorCore.Host.ConnectedPeersCount)
						{
							Statistics.PeakPlayers =
								new Statistics.Peak(LiteNetLib4MirrorCore.Host.ConnectedPeersCount, DateTime.Now);
						}

						ServerConsole.AddLog($"Player {userId} preauthenticated from endpoint {ep}.");
						ServerLogs.AddLog(ServerLogs.Modules.Networking,
							$"{userId} preauthenticated from endpoint {ep}.",
							ServerLogs.ServerLogType.ConnectionUpdate);
						CustomLiteNetLib4MirrorTransport.PreauthDisableIdleMode();
					}
					else
					{
						if (string.IsNullOrEmpty(CedModMain.Singleton.Config.QuerySystem.CustomServerFullMessage))
						{
							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.ServerFull);
							request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
							CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						}
						else
						{
							CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Custom);
							CustomLiteNetLib4MirrorTransport.RequestWriter.Put(CedModMain.Singleton.Config.QuerySystem.ServerFullBase + "\n" + CedModMain.Singleton.Config.QuerySystem.CustomServerFullMessage);
							request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
							CustomLiteNetLib4MirrorTransport.ResetIdleMode();
						}
					}
				}
				catch (Exception e)
				{
					CustomLiteNetLib4MirrorTransport.Rejected++;
					if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
						CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

					if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
					    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
						ServerConsole.AddLog(
							$"Player from endpoint {ep} sent an invalid preauthentication token. {e.Message}");

					CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
					CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.InvalidToken);
					request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
					CustomLiteNetLib4MirrorTransport.ResetIdleMode();
				}
			}
			catch (Exception e)
			{
				CustomLiteNetLib4MirrorTransport.Rejected++;
				if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
					CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

				if (!CustomLiteNetLib4MirrorTransport.SuppressRejections)
					ServerConsole.AddLog(
						$"Player from endpoint {request.RemoteEndPoint} failed to preauthenticate: {e.Message}");

				CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
				CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Error);
				request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
			}
		}
	}
}