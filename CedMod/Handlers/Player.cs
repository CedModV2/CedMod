using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal.Patches;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
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
            lock (BanSystem.CachedStates)
            {
                BanSystem.CachedStates.Remove(ev.Player.UserId);
            }
        }

        public override void OnPlayerBanning(PlayerBanningEventArgs ev)
        {
            ev.IsAllowed = false;
            Logger.Info($"Issuing ban on cedmod for {ev.Player.UserId}...");
            Task.Run(async () =>
            {
                try
                {
                    if (ev.Issuer != null)
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => ev.Issuer.ReferenceHub.queryProcessor.SendToClient("Issuing ban, please wait", true, true, "BAN#Issuing ban, please wait"));
                    await API.Ban(ev.Player, ev.Duration, ev.Issuer.LogName, ev.Reason, false);
                    if (ev.Issuer != null)
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => ev.Issuer.ReferenceHub.queryProcessor.SendToClient("Ban issued", true, true, "BAN#Ban issued."));
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
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
