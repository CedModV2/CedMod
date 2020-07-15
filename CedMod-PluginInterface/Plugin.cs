using System;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace CedMod.PluginInterface
{
    public class CedModPluginInterface : Plugin<Config_PluginInterface>
    {

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Default;

        /// <inheritdoc/>

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-PluginInterface";

        public override string Prefix { get; } = "cm_pi";

        
        public void OnDisable()
        {
            // Unload the event handlers.

            // Close the HTTP server.
            WebService.StopWebServer();
        }

        public static string SecurityKey =
            GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "none");
        public void OnEnable()
        {
            // Load the event handlers.

            if (SecurityKey != "none")
            {
                WebService.StartWebServer();
            }
            else
               Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is set to none plugin will nog load due to security risks");
        }

        public void OnReload()
        {
        }
    }
}