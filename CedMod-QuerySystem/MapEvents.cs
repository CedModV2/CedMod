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
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadDetonation)},
                    {"Message", "Warhead has been detonated"}
                }
            });
        }

        public void OnDecon(DecontaminatingEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnDecon)},
                    {"Message", "Light containment zone has been decontaminated."}
                }
            });
        }

        public void OnWarheadStart(StartingEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadStart)},
                    {
                        "Message",
                        string.Format("warhead has been started: {0} seconds",
                            Warhead.Controller.NetworktimeToDetonation)
                    }
                }
            });
        }

        public void OnWarheadCancelled(StoppingEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadCancelled)},
                    {"Message", ev.Player.Nickname + " - " + ev.Player.UserId + " has stopped the detonation."}
                }
            });
        }
    }
}