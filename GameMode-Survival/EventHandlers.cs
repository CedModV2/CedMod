using EXILED;
using EXILED.Extensions;
using MEC;
using Mirror;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CedMod.GameMode.Survival
{
    public class EventHandlers
    {
        private readonly Player plugin;
        public EventHandlers(Player plugin) => this.plugin = plugin;

        public void OnWaitingForPlayers()
        {
            plugin.RoundStarted = false;
            Timing.KillCoroutines("blackout");
        }

        public void OnRoundStart()
        {
            Timing.RunCoroutine(Start(), "blackout");

        }
        private IEnumerator<float> Start()
        {
            if (plugin.GamemodeEnabled)
            {
                foreach (GameObject player in PlayerManager.players)
                {
                    CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                    component.GetComponent<ServerRoles>().TargetSetOverwatch(component.GetComponent<NetworkIdentity>().connectionToClient, false);
                    component.GetComponent<ServerRoles>().CmdSetOverwatchStatus(0);
                }
                RoundSummary.RoundLock = true;
                plugin.RoundStarted = true;
                plugin.Functions.DoSetup();
                List<ReferenceHub> hubs = EXILED.Extensions.Player.GetHubs().ToList();
                List<ReferenceHub> nuts = new List<ReferenceHub>();

                for (int i = 0; i < 5 && hubs.Count > 2; i++)
                {
                    int r = plugin.Gen.Next(hubs.Count);
                    nuts.Add(hubs[r]);
                    hubs.Remove(hubs[r]);
                }
                yield return Timing.WaitForSeconds(2f);
                Timing.RunCoroutine(plugin.Functions.SpawnDbois(hubs));
                Timing.RunCoroutine(plugin.Functions.SpawnNuts(nuts), "blackout");
                RoundSummary.RoundLock = false;
            }
        }
        public void OnRoundEnd()
        {
            plugin.Functions.DisableGamemode();
            plugin.RoundStarted = false;
            Timing.KillCoroutines("blackout");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!plugin.GamemodeEnabled)
                return;
            //kek
            if (plugin.RoundStarted)
                ev.Player.Broadcast(10, "<color=yellow>Now playing: Survival of the Fittest gamemode</color", false);
            else
            {
                Broadcast broadcast = PlayerManager.localPlayer.GetComponent<Broadcast>();
                broadcast.CallRpcClearElements();
                broadcast.RpcAddElement("<color=yellow>Survival of the Fittest gamemode is starting..</color>", 10, Broadcast.BroadcastFlags.Normal);
            }
        }
        public void OnTeamRespawn(ref TeamRespawnEvent ev)
        {
            if (!plugin.RoundStarted)
                return;
            foreach (GameObject player in PlayerManager.players)
            {
                CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                if (component.CurClass != RoleType.ClassD)
                {
                    if (component.CurClass != RoleType.Scp173)
                    {
                        component.SetPlayersClass(RoleType.Spectator, player);
                    }
                }
            }
            ev.ToRespawn = new List<ReferenceHub>();
            ev.MaxRespawnAmt = 0;
        }
        public void OnTesla(ref TriggerTeslaEvent ev)
        {
            if (plugin.GamemodeEnabled)
            {
                ev.Triggerable = false;
            }
        }
    }
}