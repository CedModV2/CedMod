using System;

namespace CedMod.PluginInterface
{
    public class PluginInterface : EXILED.Plugin
    {
        public override string getName => "CedModPluginInterface";

        // HTTP server


        public override void OnDisable()
        {
            // Unload the event handlers.

            // Close the HTTP server.
            WebService.StopWebServer();
        }

        public static string SecurityKey =
            GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "none");
        public override void OnEnable()
        {
            // Load the event handlers.

            if (SecurityKey != "none")
            {
                WebService.StartWebServer();
            }
            else
                CedMod.INIT.Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is set to none plugin will nog load due to security risks");
        }

        public override void OnReload()
        {
            throw new NotImplementedException();
        }
    }
}