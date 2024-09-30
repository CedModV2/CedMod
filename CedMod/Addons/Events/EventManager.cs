using System.Collections.Generic;
using CedMod.Addons.Events.Interfaces;
using Exiled.API.Interfaces;
using LabApi.Loader.Features.Plugins;

namespace CedMod.Addons.Events
{
    public class EventManager
    {
        public static IEvent CurrentEvent = null;
        public static List<IEvent> EventQueue = new List<IEvent>();
        public static List<IEvent> AvailableEvents = new List<IEvent>();
        public static List<Plugin> AvailableEventPlugins = new List<Plugin>();
#if EXILED
        public static List<IPlugin<IConfig>> AvailableEventPluginsExiled = new List<IPlugin<IConfig>>();
#endif
    }
}