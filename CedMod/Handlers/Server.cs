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
    public class Server
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

        public void OnRoundRestart()
        {
            LightsoutCommand.isEnabled = false;
            FriendlyFireAutoban.Teamkillers.Clear();
            Timing.KillCoroutines(LightsoutCommand.CoroutineHandle);
        }
    }
}