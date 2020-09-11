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

        [Description("If we override the query system")]
        public bool QueryOverride { get; set; } = false;

        [Description("If true it will start a websocket server instead of a webserver")]
        public bool NewWebSocketSystem { get; set; } = true;

        [Description("The port that the WebServer or WebSocket server wil run on")]
        public int Port { get; set; } = ConfigFile.ServerConfig.GetInt("cm_port", 8000);
    }
}