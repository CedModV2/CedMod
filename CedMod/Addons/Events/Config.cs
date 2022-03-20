using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod.Addons.Events
{
    public sealed class EventsConfig : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("If debug messages are shown")]
        public bool Debug { get; set; } = false;
    }
}