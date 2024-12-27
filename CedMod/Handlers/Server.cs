using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CedMod.Addons.Audio;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Commands;
using CedMod.Components;
using MEC;
using Mirror;
using Newtonsoft.Json;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace CedMod.Handlers
{
    public class Server
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

        [PluginEvent(ServerEventType.RoundRestart)]
        public void OnRoundRestart(RoundRestartEvent ev)
        {
            FpcSyncDataPatch.AuthorizedLunge.Clear();
            FriendlyFireAutoban.Teamkillers.Clear();
            FriendlyFireAutoban.AdminDisabled = false;
            foreach (var fake in AudioCommand.FakeConnections)
            {
                NetworkServer.Destroy(fake.Value.gameObject);
            }
            AudioCommand.FakeConnections.Clear();
            AudioCommand.FakeConnectionsIds.Clear();
            RemoteAdminModificationHandler.IngameUserPreferencesMap.Clear();
            RemoteAdminModificationHandler.Singleton.Requesting.Clear();
            Task.Run(() =>
            {
                lock (BanSystem.CachedStates)
                {
                    BanSystem.CachedStates.Clear();
                }
            });
            BanSystem.Authenticating.Clear();
            VoicePacketPacket.Floats.Clear();
            VoicePacketPacket.OpusDecoders.Clear();
        }
    }
}