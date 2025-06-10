using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.Audio;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Components;
using LabApi.Events.CustomHandlers;
using Mirror;

namespace CedMod.Handlers
{
    public class Server: CustomEventsHandler
    {
        public static Dictionary<ReferenceHub, ReferenceHub> reported = new Dictionary<ReferenceHub, ReferenceHub>();

        public override void OnServerRoundRestarted()
        {
            FpcServerPositionDistributorPatch.VisiblePlayers.Clear();
            SentinalBehaviour.GunshotSource.Clear();
            SentinalBehaviour.Stalking106.Clear();
            Scp939LungePatch.LungeTime.Clear();
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