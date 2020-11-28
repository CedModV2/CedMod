using System;
using CedMod.QuerySystem.WS;
using Exiled.Events.EventArgs;

namespace CedMod.QuerySystem
{
	public class ServerEvents
	{
		public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
		{
			if (ev.Name.Contains("playerlistcolored"))
			{
				return;
			}
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				string text = string.Join(" ", ev.Arguments);
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send(string.Concat(new string[]
					{
						ev.Sender.Nickname,
						"(",
						ev.Sender.UserId,
						") Used command: ",
						ev.Name,
						" ",
						text,
						"."
					}));
				}
			}
		}
		
		public void OnWaitingForPlayers()
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send("Server is waiting for players.");
				}
			}
		}
		
		public void OnRoundStart()
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send("Round is restarting.");
				}
			}
		}
		
		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send("Round is restarting.");
				}
			}
		}
		
		public void OnCheaterReport(ReportingCheaterEventArgs ev)
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send(string.Concat(new string[]
					{
						"ingame: ",
						ev.Reporter.UserId,
						" report ",
						ev.Reported.UserId,
						" for ",
						ev.Reason,
						"."
					}));
				}
			}
		}
		
		public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				string text = string.Join(" ", ev.Arguments);
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send(string.Format("{0} - {1} ({2}) sent client console command: {3} {4}.", new object[]
					{
						ev.Player.Nickname,
						ev.Player.UserId,
						ev.Player.Role,
						ev.Name,
						text
					}));
				}
			}
		}
		
		public void OnRespawn(RespawningTeamEventArgs ev)
		{
			foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
			{
				if (webSocketSystemBehavior.authed)
				{
					webSocketSystemBehavior.Context.WebSocket.Send(string.Format("Respawn: {0} as {1}.", ev.Players.Count, ev.NextKnownTeam));
				}
			}
		}
	}
}
