using System.Collections.Generic;
using CedMod.Addons.QuerySystem.WS;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.Addons.QuerySystem
{
    public class QueryMapEvents
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

        [PluginEvent(ServerEventType.LczDecontaminationAnnouncement)]
        public void OnDecon(int i)
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

        [PluginEvent(ServerEventType.WarheadStart)]
        public void OnWarheadStart(bool b, CedModPlayer player, bool isResumed)
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
                            Warhead.DetonationTime)
                    }
                }
            });
        }

        [PluginEvent(ServerEventType.WarheadStop)]
        public void OnWarheadCancelled(CedModPlayer player)
        {
            WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
            {
                Recipient = "ALL",
                Data = new Dictionary<string, string>()
                {
                    {"Type", nameof(OnWarheadCancelled)},
                    {"Message", player.Nickname + " - " + player.UserId + " has stopped the detonation."}
                }
            });
        }
    }
}