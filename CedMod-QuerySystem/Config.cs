using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod.QuerySystem
{
    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("Commands in this list will not be allowed to run via the web API (must be uppercase)")]
        public List<string> DisallowedWebCommands { get; set; } = new List<string> { "REQUEST_DATA AUTH", "GBAN_KICK" };

        [Description("Secret passphrase used to authenticate request KEEP THIS SECRET")]
        public string SecurityKey { get; set; } = "None";

        [Description("The name that this server will show up as on the panel")]
        public string Identifier { get; set; } = "Server 1";
        
        [Description("If true, the plugin will sync predefined reasons with the panel")]
        public bool EnableBanreasonSync { get; set; } = true;
        
        [Description("If true, the plugin will automatically enable and setup the External lookup function in remote admin")]
        public bool EnableExternalLookup { get; set; } = true;
        
        [Description("If true, the plugin will show a custom message when the server is full, promote your patreon reserved slots here :)")]
        public string CustomServerFullMessage { get; set; } = "";
    }
}