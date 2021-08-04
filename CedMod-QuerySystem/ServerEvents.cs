using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MapGeneration;
using Newtonsoft.Json;
using UnityEngine;

namespace CedMod.QuerySystem
{
	public class ServerEvents
	{
		public static List<ImageGenerator.MinimapElement> map = new List<ImageGenerator.MinimapElement>();
		public static List<MiniMapElement> Minimap = new List<MiniMapElement>();
		public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
		{
			if (ev.Name.Contains("playerlistcolored"))
			{
				return;
			}
			Task.Factory.StartNew(delegate()
			{
				string text = string.Join(" ", ev.Arguments);
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnCommand)},
						{"Message", string.Concat(new string[]
						{
							ev.Sender.Nickname,
							"(",
							ev.Sender.UserId,
							") Used command: ",
							ev.Name,
							" ",
							text,
							"."
						})}
					}
				}));
			});
		}
		
		public void OnWaitingForPlayers()
		{
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
					Log.Debug($"Downloaded Reserved slots: {response1.Result.Content.ReadAsStringAsync().Result}");
					QuerySystem.ReservedSlotUserids = JsonConvert.DeserializeObject<List<string>>(response1.Result.Content.ReadAsStringAsync().Result);
				});
			}
			
			Minimap.Clear();
			foreach (ImageGenerator gen in ImageGenerator.ZoneGenerators)
			{
				foreach (ImageGenerator.MinimapElement elem in gen.minimap)
				{
					MiniMapElement elem1 = new MiniMapElement();
					elem1.Name = elem.roomSource.GetComponentsInChildren<RoomIdentifier>().FirstOrDefault().Name.ToString();
					elem1.Position = elem.roomSource.transform.position.ToString();
					elem1.Rotation = elem.rotation.ToString();
					elem1.ZoneType = elem.roomSource.GetComponentsInChildren<RoomIdentifier>().FirstOrDefault().Zone.ToString().Replace("HCZ", "HeavyContainment").Replace("LCZ", "LightContainment").Replace("ENTRANCE", "Entrance");
					Minimap.Add(elem1);
				}
			}
			
			Task.Factory.StartNew(delegate()
			{
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
		
		public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
		{
			Task.Factory.StartNew(delegate()
			{
				string text = string.Join(" ", ev.Arguments);
				WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
				{
					Recipient = "ALL",
					Data = new Dictionary<string, string>()
					{
						{"Type", nameof(OnConsoleCommand)},
						{"Message", string.Format("{0} - {1} ({2}) sent client console command: {3} {4}.", new object[]
						{
							ev.Player.Nickname,
							ev.Player.UserId,
							ev.Player.Role,
							ev.Name,
							text
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
