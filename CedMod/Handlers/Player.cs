using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal.Patches;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MEC;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;

namespace CedMod.Handlers
{
    public class Player: CustomEventsHandler
    {
        public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
        {
            Task.Run(async () => { await BanSystem.HandleJoin(ev.Player); });
            Timing.RunCoroutine(Name(ev.Player));
        }

        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            VoicePacketPacket.Floats.Remove(ev.Player.ReferenceHub.netId);
            VoicePacketPacket.OpusDecoders.Remove(ev.Player.ReferenceHub.netId);
        }
        
        public IEnumerator<float> Name(LabApi.Features.Wrappers.Player player)
        {
            foreach (var pp in LabApi.Features.Wrappers.Player.ReadyList)
            {
                if (pp.UserId == player.UserId) 
                    yield break;
                if (CedModMain.Singleton.Config.CedMod.KickSameName)
                {
                    if (pp.Nickname == player.Nickname)
                    {
                        if (player.ReferenceHub.serverRoles.RemoteAdmin && !pp.ReferenceHub.serverRoles.RemoteAdmin)
                        {
                            pp.Kick("You have been kicked by a plugin: \n Please change your name to something unique (A staff member joined with your name)");
                            yield break;
                        }
                        else if (pp.UserId != player.UserId)
                        {
                            player.Kick("You have been kicked by a plugin: \n Please change your name to something unique (there is already someone with your name)");
                        }
                    }
                }
            }
        }

        public override void OnPlayerDying(PlayerDyingEventArgs ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;
            FriendlyFireAutoban.HandleKill(ev.Player, ev.Attacker, ev.DamageHandler);
        }
    }
}
