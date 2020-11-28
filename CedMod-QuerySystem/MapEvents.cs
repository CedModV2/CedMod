using System;
using CedMod.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Events.EventArgs;

namespace CedMod.QuerySystem
{
    public class MapEvents
    {
        public void OnWarheadDetonation()
        {
            foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
            {
                if (webSocketSystemBehavior.authed)
                {
                    webSocketSystemBehavior.Context.WebSocket.Send("Warhead has been detonated");
                }
            }
        }
        
        public void OnDecon(DecontaminatingEventArgs ev)
        {
            foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
            {
                if (webSocketSystemBehavior.authed)
                {
                    webSocketSystemBehavior.Context.WebSocket.Send("Light containment zone has been decontaminated.");
                }
            }
        }
        
        public void OnWarheadStart(StartingEventArgs ev)
        {
            foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
            {
                if (webSocketSystemBehavior.authed)
                {
                    webSocketSystemBehavior.Context.WebSocket.Send(string.Format("warhead has been started: {0} seconds", Warhead.Controller.NetworktimeToDetonation));
                }
            }
        }
        
        public void OnWarheadCancelled(StoppingEventArgs ev)
        {
            foreach (WebSocketSystemBehavior webSocketSystemBehavior in WebSocketSystem.Clients)
            {
                if (webSocketSystemBehavior.authed)
                {
                    webSocketSystemBehavior.Context.WebSocket.Send(ev.Player.Nickname + " - " + ev.Player.UserId + " has stopped the detonation.");
                }
            }
        }
    }
}