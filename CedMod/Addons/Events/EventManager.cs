using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using HarmonyLib;
using PluginAPI.Core;

namespace CedMod.Addons.Events
{
    public class EventManager
    {
        public static IEvent currentEvent = null;
        public static List<IEvent> nextEvent = new List<IEvent>();
        public static List<IEvent> AvailableEvents = new List<IEvent>();
        public static EventManagerServerEvents EventManagerServerEvents;
        public static EventManagerPlayerEvents EventManagerPlayerEvents;
        public static List<PluginHandler> AvailableEventPlugins = new List<PluginHandler>();
#if EXILED
        public static List<IPlugin<IConfig>> AvailableEventPluginsExiled = new List<IPlugin<IConfig>>();
#endif
    }
}