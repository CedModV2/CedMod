using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CedMod.EventManager
{
    public class EventManager : Plugin<Config>
    {
        public IEvent currentEvent = null;
        public List<IEvent> nextEvent = new List<IEvent>();
        public List<IEvent> AvailableEvents = new List<IEvent>();
        public MapEvents MapEvents;
        public ServerEvents ServerEvents;
        public PlayerEvents PlayerEvents;
        public static Harmony Harmony;
        /// <inheritdoc/>
        public override string Author { get; } = "ced777ric#8321";

        public override string Name { get; } = "CedMod-Events";

        public override string Prefix { get; } = "cm_events";

        public static EventManager Singleton;

        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(0, 0, 1);

        public override void OnDisabled()
        {
            Harmony.UnpatchAll();
            Exiled.Events.Handlers.Server.EndingRound -= ServerEvents.EndingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= ServerEvents.WaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerEvents.EndRound;
            Exiled.Events.Handlers.Server.RestartingRound -= ServerEvents.RestartingRound;
            Exiled.Events.Handlers.Player.Joined -= PlayerEvents.OnPlayerJoin;
            MapEvents = null;
            ServerEvents = null;
            PlayerEvents = null;
            Singleton = null;
            base.OnDisabled();
        }

        public override void OnEnabled()
        {
            Singleton = this;
            Harmony = new Harmony("com.cedmod.eventmanager");
            Harmony.PatchAll();
            
            MapEvents = new MapEvents();
            ServerEvents = new ServerEvents();
            PlayerEvents = new PlayerEvents();
            Exiled.Events.Handlers.Server.EndingRound += ServerEvents.EndingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += ServerEvents.WaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded += ServerEvents.EndRound;
            Exiled.Events.Handlers.Server.RestartingRound += ServerEvents.RestartingRound;
            Exiled.Events.Handlers.Player.Joined += PlayerEvents.OnPlayerJoin;
            
            foreach (var plugin in Exiled.Loader.Loader.Plugins)
            {
                if (plugin.Name.StartsWith("Exiled.")) //dont check exiled itself
                    continue;
                Log.Debug($"Checking {plugin.Name} for CedMod-Events functionality", CedModMain.Singleton.Config.ShowDebug);
                foreach (var type in plugin.Assembly.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    
                    Log.Debug($"Checked {plugin.Name} for CedMod-Events functionality, IEvent inherited", CedModMain.Singleton.Config.ShowDebug);
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        Log.Debug($"Checked {plugin.Name} Constructor is null, cannot continue", CedModMain.Singleton.Config.ShowDebug);
                        continue;
                    }
                    else
                    {
                        IEvent @event = constructor.Invoke(null) as IEvent;
                        if (@event == null) 
                            continue;
                        AvailableEvents.Add(@event);
                        Log.Info($"Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
                    }
                    
                }
            }
            base.OnEnabled();
        }
    }
}