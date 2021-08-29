using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Patches;
using CedMod.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json;

namespace CedMod.QuerySystem
{
	public class ServerEvents
	{
		public IEnumerator<float> SyncStart()
		{
			yield return Timing.WaitForSeconds(3);
			if (QuerySystem.config.SecurityKey != "None")
			{
				Log.Debug("Checking configs", CedModMain.config.ShowDebug);
				if (QuerySystem.config.EnableExternalLookup)
				{
					Log.Debug("Setting lookup mode", CedModMain.config.ShowDebug);
					ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupMode = "fullauth";
					ServerConfigSynchronizer.Singleton.NetworkRemoteAdminExternalPlayerLookupURL = $"https://{QuerySystem.PanelUrl}/Api/Lookup/";
					ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupToken = QuerySystem.config.SecurityKey;
				}

				Task.Factory.StartNew(() =>
				{
					Log.Debug("Checking configs", CedModMain.config.ShowDebug);
					if (QuerySystem.config.EnableBanreasonSync)
					{
						Log.Debug("Enabling ban reasons", CedModMain.config.ShowDebug);
						ServerConfigSynchronizer.Singleton.NetworkEnableRemoteAdminPredefinedBanTemplates = true;
						Log.Debug("Clearing ban reasons", CedModMain.config.ShowDebug);
						ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Clear();
						HttpClient client = new HttpClient();
						Log.Debug("Downloading ban reasons", CedModMain.config.ShowDebug);
						var response =
							client.GetAsync($"https://{QuerySystem.PanelUrl}/Api/BanReasons/{QuerySystem.config.SecurityKey}");
						Log.Debug("Addding ban reasons", CedModMain.config.ShowDebug);
						foreach (var dict in JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(response.Result
							.Content.ReadAsStringAsync().Result))
						{
							Log.Debug($"Addding ban reason {JsonConvert.SerializeObject(dict)}",
								CedModMain.config.ShowDebug);
							var DurationNice = "";
							TimeSpan timeSpan = TimeSpan.FromMinutes(double.Parse(dict["Dur"]));
							int num2 = timeSpan.Days / 365;
							if (num2 > 0)
							{
								DurationNice = string.Format("{0}y", num2);
							}
							else if (timeSpan.Days > 0)
							{
								DurationNice = string.Format("{0}d", timeSpan.Days);
							}
							else if (timeSpan.Hours > 0)
							{
								DurationNice = string.Format("{0}h", timeSpan.Hours);
							}
							else if (timeSpan.Minutes > 0)
							{
								DurationNice = string.Format("{0}m", timeSpan.Minutes);
							}
							else
							{
								DurationNice = string.Format("{0}s", timeSpan.Seconds);
							}

							ServerConfigSynchronizer.Singleton.RemoteAdminPredefinedBanTemplates.Add(
								new ServerConfigSynchronizer.PredefinedBanTemplate()
								{
									Duration = Convert.ToInt32(dict["Dur"]),
									DurationNice = DurationNice,
									Reason = dict["Reason"]
								});
						}
					}
					
					HttpClient client1 = new HttpClient();
					Log.Debug("Downloading syncs", CedModMain.config.ShowDebug);
					var response1 = client1.GetAsync($"https://{QuerySystem.PanelUrl}/Api/ReservedSlotUsers/{QuerySystem.config.SecurityKey}");
					Log.Debug($"Downloaded Reserved slots: {response1.Result.Content.ReadAsStringAsync().Result}", CedModMain.config.ShowDebug);
					QuerySystem.ReservedSlotUserids = JsonConvert.DeserializeObject<List<string>>(response1.Result.Content.ReadAsStringAsync().Result);
				});
			}
		}
		
		public void OnWaitingForPlayers()
		{
			Timing.RunCoroutine(SyncStart());
			
			Task.Factory.StartNew(delegate()
			{
				if (!WebSocketSystem.socket.IsAlive)
				{
					CedMod.QuerySystem.WS.WebSocketSystem.Stop();
					CedMod.QuerySystem.WS.WebSocketSystem.Start();
				}
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnWaitingForPlayers)},
						{"Message", "Server is waiting for players."}
					}
				}));
			});
		}
		
		public void OnRoundStart()
		{
			Task.Factory.StartNew(delegate()
			{
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnRoundStart)},
						{"Message", "Round is restarting."}
					}
				}));
			});
		}
		
		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			BulletHolePatch.HoleCreators.Clear();
			Task.Factory.StartNew(delegate()
			{
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnRoundEnd)},
						{"Message", "Round is restarting."}
					}
				}));
			});
		}
		
		public void OnCheaterReport(ReportingCheaterEventArgs ev)
		{
			Log.Debug("sending report WR", CedModMain.config.ShowDebug);
			Task.Factory.StartNew(() =>
			{
				Log.Debug("Thread report send", CedModMain.config.ShowDebug);
				if (QuerySystem.config.SecurityKey == "None")
					return;
				Log.Debug("sending report WR", CedModMain.config.ShowDebug);
				HttpClient client = new HttpClient();
				try
				{
					var response = client
						.PostAsync($"https://{QuerySystem.PanelUrl}/Api/Reports/{QuerySystem.config.SecurityKey}",
							new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
								{
									{"reporter", ev.Reporter.UserId},
									{"reported", ev.Reported.UserId},
									{"reason", ev.Reason},
								}), Encoding.Default,
								"application/json")).Result;
					Log.Debug(response.Content.ReadAsStringAsync().Result, CedModMain.config.ShowDebug);
				}
				catch (Exception ex)
				{
					Log.Error(ex);
				}
			});
			Task.Factory.StartNew(delegate()
			{
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnCheaterReport)},
						{"UserId", ev.Reported.UserId},
						{"UserName", ev.Reported.Nickname},
						{"Message", string.Concat(new string[]
						{
							"ingame: ",
							ev.Reporter.UserId,
							" report ",
							ev.Reported.UserId,
							" for ",
							ev.Reason,
							"."
						})}
					}
				}));
			});
		}

		public void OnRespawn(RespawningTeamEventArgs ev)
		{
			Task.Factory.StartNew(delegate()
			{
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnRespawn)},
						{"Message", string.Format("Respawn: {0} as {1}.", ev.Players.Count, ev.NextKnownTeam)}
					}
				}));
			});
		}

		public void OnReport(LocalReportingEventArgs ev)
		{
			Log.Debug("sending report WR", CedModMain.config.ShowDebug);
			Task.Factory.StartNew(() =>
			{
				Log.Debug("Thread report send", CedModMain.config.ShowDebug);
				if (QuerySystem.config.SecurityKey == "None")
					return;
				Log.Debug("sending report WR", CedModMain.config.ShowDebug);
				HttpClient client = new HttpClient();
				try
				{
					var response = client
						.PostAsync($"https://{QuerySystem.PanelUrl}/Api/Reports/{QuerySystem.config.SecurityKey}",
							new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string>()
								{
									{"reporter", ev.Issuer.UserId},
									{"reported", ev.Target.UserId},
									{"reason", ev.Reason},
								}), Encoding.Default,
								"application/json")).Result;
					Log.Debug(response.Content.ReadAsStringAsync().Result, CedModMain.config.ShowDebug);
				}
				catch (Exception ex)
				{
					Log.Error(ex);
				}
			});Task.Factory.StartNew(delegate()
			{
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnReport)},
						{"UserId", ev.Target.UserId},
						{"UserName", ev.Target.Nickname},
						{"Message", string.Concat(new string[]
						{
							"ingame: ",
							ev.Issuer.UserId,
							" report ",
							ev.Target.UserId,
							" for ",
							ev.Reason,
							"."
						})}
					}
				}));
			});
		}
	}

	public class MiniMapClass
	{
		public List<MiniMapElement> MapElements = new List<MiniMapElement>();
		public List<MiniMapPlayerElement> PlayerElements = new List<MiniMapPlayerElement>();
	}

	public class MiniMapElement
	{
		public string Position;
		public string Name;
		public string ZoneType;
		public string Rotation;
	}
	
	public class MiniMapPlayerElement
	{
		public string Position;
		public string Name;
		public string Zone;
		public string TeamColor;
	}
}
