using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace CedMod.Addons.QuerySystem
{
    public class QueryMapEvents
    {
        public void OnWarheadDetonation(WarheadDetonationEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadDetonation)},
                    {"Message", "Warhead has been detonated"}
                }
            });
        }

        [PluginEvent(ServerEventType.LczDecontaminationAnnouncement)]
        public void OnDecon(LczDecontaminationAnnouncementEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnDecon)},
                    {"Message", $"Light containment decontamination stage {ev.Id}."}
                }
            });
        }

        [PluginEvent(ServerEventType.WarheadStart)]
        public void OnWarheadStart(WarheadStartEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadStart)},
                    {
                        "Message",
                        string.Format("warhead has been started: {0} seconds", Warhead.DetonationTime)
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.WarheadStop)]
        public void OnWarheadCancelled(WarheadStopEvent ev)
        {
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadCancelled)},
                    {"Message", (ev.Player != null ? ev.Player.Nickname + " - " + ev.Player.UserId : "Server") + " has stopped the detonation."}
                }
            });
        }
    }
}