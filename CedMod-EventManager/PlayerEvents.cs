using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;
using Newtonsoft.Json;
using UnityEngine;

namespace CedMod.EventManager
{
    public class PlayerEvents
    {
        public void OnPlayerJoin(JoinedEventArgs ev)
        {
            Log.Debug($"Join {EventManager.Singleton.currentEvent != null}", EventManager.Singleton.Config.Debug);
            if (EventManager.Singleton.currentEvent != null)
            {
                Timing.CallDelayed(1, () =>
                {
                    ev.Player.Broadcast(10, $"EventManager: This server is currently running an event: {EventManager.Singleton.currentEvent.EventName}\n{EventManager.Singleton.currentEvent.EventDescription}");
                });
            }
        }
    }
}