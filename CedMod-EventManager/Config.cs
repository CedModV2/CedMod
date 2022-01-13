using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod.EventManager
{
    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("If debug messages are shown")]
        public bool Debug { get; set; } = false;
    }
}