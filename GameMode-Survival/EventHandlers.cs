using System.Collections.Generic;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using MEC;
using Mirror;
using UnityEngine;

namespace CedMod.GameMode.Survival
{
    public class EventHandlers
    {
        private readonly Player _plugin;
        public EventHandlers(Player plugin) => _plugin = plugin;

        public void OnWaitingForPlayers()
        {
            _plugin.RoundStarted = false;
            Timing.KillCoroutines("blackout");
        }

        public void OnRoundStart()
        {
            Timing.RunCoroutine(Start(), "blackout");

        }
        private IEnumerator<float> Start()
        {
            if (_plugin.GamemodeEnabled)
            {
                foreach (GameObject player in PlayerManager.players)
                {
                    CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                    component.GetComponent<ServerRoles>().TargetSetOverwatch(component.GetComponent<NetworkIdentity>().connectionToClient, false);
                    component.GetComponent<ServerRoles>().CmdSetOverwatchStatus(0);
                }
                RoundSummary.RoundLock = true;
                _plugin.RoundStarted = true;
                _plugin.Functions.DoSetup();
                List<ReferenceHub> hubs = EXILED.Extensions.Player.GetHubs().ToList();
                List<ReferenceHub> nuts = new List<ReferenceHub>();

                for (int i = 0; i < 3 && hubs.Count > 2; i++)
                {
                    int r = _plugin.Gen.Next(hubs.Count);
                    nuts.Add(hubs[r]);
                    hubs.Remove(hubs[r]);
                }
                yield return Timing.WaitForSeconds(2f);
                Timing.RunCoroutine(_plugin.Functions.SpawnDbois(hubs));
                Timing.RunCoroutine(_plugin.Functions.SpawnNuts(nuts), "blackout");
                RoundSummary.RoundLock = false;
            }
        }
        public void OnRoundEnd()
        {
            _plugin.Functions.DisableGamemode();
            _plugin.RoundStarted = false;
            Timing.KillCoroutines("blackout");
        }

        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!_plugin.GamemodeEnabled)
                return;
            //kek
            if (_plugin.RoundStarted)
                ev.Player.Broadcast(10, "<color=yellow>Now playing: Survival of the Fittest gamemode</color");
            else
            {
                Broadcast broadcast = PlayerManager.localPlayer.GetComponent<Broadcast>();
                broadcast.CallRpcClearElements();
                broadcast.RpcAddElement("<color=yellow>Survival of the Fittest gamemode is starting..</color>", 10, Broadcast.BroadcastFlags.Normal);
            }
        }
        public void OnTeamRespawn(ref TeamRespawnEvent ev)
        {
            if (!_plugin.RoundStarted)
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
            if (_plugin.GamemodeEnabled)
            {
                ev.Triggerable = false;
            }
        }
    }
}