using System.ComponentModel;

namespace CedMod.SampleEvent
{
    public sealed class Config
    {
        [Description("Indicates whether the event is enabled or not")]
        public bool IsEnabled { get; set; } = true;
    }
}