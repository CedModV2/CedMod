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

        public override void OnEnable()
        {
            // Load the event handlers.

            // Start the http server.
            WebService.StartWebServer();
        }

        public override void OnReload()
        {
            throw new NotImplementedException();
        }
    }
}