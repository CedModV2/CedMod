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
			Minimap.Clear();
			foreach (ImageGenerator gen in ImageGenerator.ZoneGenerators)
			{
				foreach (ImageGenerator.MinimapElement elem in gen.minimap)
				{
					MiniMapElement elem1 = new MiniMapElement();
					elem1.Name = elem.roomSource.GetComponentsInChildren<RoomInformation>().FirstOrDefault().CurrentRoomType.ToString();
					elem1.Position = elem.roomSource.transform.position.ToString();
					elem1.Rotation = elem.rotation.ToString();
					elem1.ZoneType = elem.roomSource.GetComponentsInChildren<RoomInformation>().FirstOrDefault().CurrentZoneType.ToString().Replace("HCZ", "HeavyContainment").Replace("LCZ", "LightContainment").Replace("ENTRANCE", "Entrance");
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
						.PostAsync($"https://frikanweb.cedmod.nl/Api/Reports/{QuerySystem.config.SecurityKey}",
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
						.PostAsync($"https://frikanweb.cedmod.nl/Api/Reports/{QuerySystem.config.SecurityKey}",
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
