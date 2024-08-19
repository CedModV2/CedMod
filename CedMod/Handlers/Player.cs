using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.Sentinal.Patches;
using MEC;
using PlayerStatsSystem;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;

namespace CedMod.Handlers
{
    public class Player
    {
        [PluginEvent(ServerEventType.PlayerJoined)]
        public void OnJoin(PlayerJoinedEvent ev)
        {
            var plr = CedModPlayer.Get(ev.Player.ReferenceHub);
            Task.Run(async () => { await BanSystem.HandleJoin(plr); });
            Timing.RunCoroutine(Name(CedModPlayer.Get(ev.Player.ReferenceHub)));
        }
        
        [PluginEvent(ServerEventType.PlayerLeft)]
        public void OnJoin(PlayerLeftEvent ev)
        {
            VoicePacketPacket.Floats.Remove(ev.Player.ReferenceHub.netId);
            VoicePacketPacket.OpusDecoders.Remove(ev.Player.ReferenceHub.netId);
        }
        
        public IEnumerator<float> Name(CedModPlayer player)
        {
            foreach (var pp in PluginAPI.Core.Player.GetPlayers<CedModPlayer>())
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
        
        [PluginEvent(ServerEventType.PlayerDying)]
        public void OnDying(PlayerDyingEvent ev)
        {
            if (ev.Player == null || ev.Attacker == null)
                return;
            FriendlyFireAutoban.HandleKill(CedModPlayer.Get(ev.Player.ReferenceHub), CedModPlayer.Get(ev.Attacker.ReferenceHub), ev.DamageHandler);
        }
    }
}