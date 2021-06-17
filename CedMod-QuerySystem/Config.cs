using GameCore;

namespace CedMod.QuerySystem
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <inheritdoc cref="IConfig"/>
    public sealed class Config : IConfig
    {
        /// <inheritdoc/>
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("Commands in this list will not be allowed to run via the web API (must be uppercase)")]
        public List<string> DisallowedWebCommands { get; set; } = new List<string> { "REQUEST_DATA AUTH", "GBAN_KICK" };

        [Description("Secret passphrase used to authenticate request KEEP THIS SECRET")]
        public string SecurityKey { get; set; } = "None";

        [Description("The name that this server will show up as on the panel")]
        public string Identifier { get; set; } = "Server 1";
    }
}