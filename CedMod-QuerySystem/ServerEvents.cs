using Exiled.Events.EventArgs;

namespace CedMod.QuerySystem
{
    public class ServerEvents
    {
        public void OnCommand(SendingRemoteAdminCommandEventArgs ev)
        {
            if (ev.Name.Contains("playerlistcolored"))
                return;
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                string Args = string.Join(" ", ev.Arguments);
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Sender.Nickname}({ev.Sender.UserId}) Used command: {ev.Name} {Args}.");
            }
        }
        
        public void OnWaitingForPlayers()
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Server is waiting for players.");
            }
        }
        
        public void OnRoundStart()
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Round is restarting.");
            }
        }
        
        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Round is restarting.");
            }
        }
        
        public void OnCheaterReport(ReportingCheaterEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"ingame: {ev.Reporter.UserId} report {ev.Reported.UserId} for {ev.Reason}.");
            }
        }
        
        public void OnConsoleCommand(SendingConsoleCommandEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                string Argies = string.Join(" ", ev.Arguments);
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} ({ev.Player.Role}) sent client console command: {ev.Name} {Argies}.");
            }
        }
        
        public void OnRespawn(RespawningTeamEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Respawn: {ev.Players.Count} as {ev.NextKnownTeam}.");
            }
        }
    }
}