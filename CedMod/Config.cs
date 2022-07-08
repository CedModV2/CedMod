using System.Collections.Generic;
using System.ComponentModel;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem;
using Exiled.API.Interfaces;

namespace CedMod
{
    /// <summary>
    /// A plugin config.
    /// </summary>
    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        public CedModConfig CedMod { get; set; } = new CedModConfig();
        public QueryConfig QuerySystem { get; set; } = new QueryConfig();
        public EventsConfig EventManager { get; set; } = new EventsConfig();
    }
}