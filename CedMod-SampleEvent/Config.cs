using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod.EventManager
{
    public sealed class Config : IConfig
    {
        [Description("Indicates whether the event is enabled or not")]
        public bool IsEnabled { get; set; } = true;
    }
}