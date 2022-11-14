using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Commands;
using MEC;
using Newtonsoft.Json;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;

namespace CedMod.Handlers
{
    public class Server
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

        [PluginEvent(ServerEventType.RoundRestart)]
        public void OnRoundRestart()
        {
            FriendlyFireAutoban.Teamkillers.Clear();
        }
    }
}