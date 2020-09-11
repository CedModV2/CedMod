using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace CedMod.QuerySystem
{
    public class MapEvents
    {
        public void OnWarheadDetonation()
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Warhead has been detonated");
            }
        }
        
        public void OnDecon(DecontaminatingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"Light containment zone has been decontaminated.");
            }
        }
        
        public void OnWarheadStart(StartingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"warhead has been started: {Warhead.Controller.NetworktimeToDetonation} seconds");
            }
        }

        public void OnWarheadCancelled(StoppingEventArgs ev)
        {
            foreach (WS.WebSocketSession wss in WS.WebSocketServer.ws.Clients)
            {
                if (wss.IsAuthenticated)
                    wss.SendMessage($"{ev.Player.Nickname} - {ev.Player.UserId} has stopped the detonation.");
            }
        }

    }
}