using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Commands;
using Exiled.API.Features;
using MEC;
using Exiled.Events.EventArgs;
using Newtonsoft.Json;

namespace CedMod.Handlers
{
    /// <summary>
    /// Used to handle server events.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// A dictionary of people and who they reported.
        /// </summary>
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

        /// <summary>
        /// Called when the round starts.
        /// </summary>
        public void OnRoundRestart()
        {
            LightsoutCommand.isEnabled = false;
            FriendlyFireAutoban.Teamkillers.Clear();
            Timing.KillCoroutines(LightsoutCommand.CoroutineHandle);
        }
    }
}