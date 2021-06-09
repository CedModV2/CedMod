using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;

namespace CedMod.QuerySystem
{
    public class MapEvents
    {
        public void OnWarheadDetonation()
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", "Warhead has been detonated"}
                    }
                }));
            });
        }
        
        public void OnDecon(DecontaminatingEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", "Light containment zone has been decontaminated."}
                    }
                }));
            });
        }
        
        public void OnWarheadStart(StartingEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", string.Format("warhead has been started: {0} seconds", Warhead.Controller.NetworktimeToDetonation)}
                    }
                }));
            });
        }
        
        public void OnWarheadCancelled(StoppingEventArgs ev)
        {
            Task.Factory.StartNew(delegate()
            {
                WebSocketSystem.socket.Send(JsonConvert.SerializeObject(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has stopped the detonation."}
                    }
                }));
            });
        }
    }
}