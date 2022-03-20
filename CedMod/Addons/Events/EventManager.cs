using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using HarmonyLib;

namespace CedMod.Addons.Events
{
    public class EventManager
    {
        public static IEvent currentEvent = null;
        public static List<IEvent> nextEvent = new List<IEvent>();
        public static List<IEvent> AvailableEvents = new List<IEvent>();
        public static EventManagerServerEvents EventManagerServerEvents;
        public static EventManagerPlayerEvents EventManagerPlayerEvents;
        public static List<IPlugin<IConfig>> AvailableEventPlugins = new List<IPlugin<IConfig>>();
    }
}