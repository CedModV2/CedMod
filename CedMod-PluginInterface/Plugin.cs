using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using EXILED;

namespace FrikanwebPlugin
{
    public class FrikanwebPlugin : EXILED.Plugin
    {
        public override string getName => "FrikanPlugin";

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