using System.ComponentModel;
using CedMod.Addons.Events;

namespace CedMod.SampleEvent
{
    public sealed class Config: IEventConfig
    {
        [Description("Indicates whether the event is enabled or not")]
        public bool IsEnabled { get; set; } = true;
    }
}