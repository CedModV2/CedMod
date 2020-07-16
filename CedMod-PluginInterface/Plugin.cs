using System;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events;

namespace CedMod.PluginInterface
{
    public class CedModPluginInterface : Plugin<Config>
    {

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Default;

        /// <inheritdoc/>

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-WebAPI";

        public override string Prefix { get; } = "cm_WAPI";

        public static Config config;
        public override void OnDisabled()
        {
            // Unload the event handlers.
            // Close the HTTP server.
            WebService.StopWebServer();
        }

        public static string SecurityKey;
        public override void OnEnabled()
        {
            config = Config;
            // Load the event handlers.
            if (!Config.IsEnabled)
                return;
            SecurityKey =
                GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "None");
            if (SecurityKey == "None")
            {
                SecurityKey = Config.SecurityKey;
            }
            else
            {
                Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is depricated, please use the new exiled config system, this opention will be removed in future release");
            }
            if (SecurityKey != "None")
            {
                // Start the HTTP server.
                WebService.StartWebServer();
            }
            else
               Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is set to none plugin will nog load due to security risks");
        }
        public override void OnReloaded()
        {}
    }
}