using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;

namespace CedMod.Addons.QuerySystem
{
    public class QueryMapEvents: CustomEventsHandler
    {
        public override void OnWarheadDetonated(WarheadDetonatedEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadDetonated)},
                    {"Message", "Warhead has been detonated"}
                }
            });
        }

        public override void OnServerLczDecontaminationAnnounced(LczDecontaminationAnnouncedEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnServerLczDecontaminationAnnounced)},
                    {"Message", $"Light containment decontamination stage {ev.Phase}."}
                }
            });
        }

        public override void OnWarheadStarted(WarheadStartedEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadStarted)},
                    {
                        "Message",
                        string.Format("warhead has been started: {0} seconds", 0) //todo implement
                    }
                }
            });
        }

        public override void OnWarheadStopped(WarheadStoppedEventArgs ev)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadStopped)},
                    {"Message", (ev.Player != null ? ev.Player.Nickname + " - " + ev.Player.UserId : "Server") + " has stopped the detonation."}
                }
            });
        }
    }
}