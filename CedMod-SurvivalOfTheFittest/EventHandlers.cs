using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Permissions.Extensions;
using Interactables.Interobjects.DoorUtils;
using MEC;
using UnityEngine;
using Random = System.Random;

namespace CedMod.SurvivalOfTheFittest
{
    public class EventHandlers
    {
        public static bool RunOnStart = false;
        public static bool GameModeRunning = false;
        public void Respawn(RespawningTeamEventArgs ev)
        {
            if (GameModeRunning)
            {
                ev.MaximumRespawnAmount = 0;
                ev.Players.Clear();
            }
        }

        public void OnDamage(HurtingEventArgs ev)
        {
            if (GameModeRunning)
                if (ev.Attacker.Role != RoleType.Scp173)
                    ev.Amount = 0;
        }
        
        public void OnJoin(VerifiedEventArgs ev)
        {
            if (RunOnStart)
                ev.Player.Broadcast(5, "Survival of the fittest is starting", Broadcast.BroadcastFlags.Normal, false);
        }
        public void TriggerTesla(TriggeringTeslaEventArgs ev)
        {
            if (GameModeRunning)
                ev.IsTriggerable = false;
        }
        public void OnEndingRound(RoundEndedEventArgs ev)
        {
            GameModeRunning = false;
            RunOnStart = false;
            Timing.KillCoroutines("SurvivalOfTheFittest");
        }

        public void OnRoundRestart()
        {
            GameModeRunning = false;
            RunOnStart = false;
            Timing.KillCoroutines("SurvivalOfTheFittest");
        }
        public IEnumerator<float> SpawnDbois(List<Player> hubs)
        {
            GameModeRunning = true;
            RunOnStart = false;
            foreach (Player hub in hubs)
            {
                hub.ReferenceHub.characterClassManager.NetworkCurClass = RoleType.ClassD;
                hub.Broadcast(30, "<color=orange>You are a Dboi, you need to find a hiding place and pray you are the last person alive, the nuts wil be release at their chamber in 2 minutes.</color>", Broadcast.BroadcastFlags.Normal, false);
                yield return Timing.WaitForOneFrame * 30;
				
                hub.ClearInventory();
                hub.AddItem(ItemType.Flashlight);
                hub.AddItem(ItemType.KeycardO5);
                hub.ReferenceHub.playerMovementSync.OverridePosition(Exiled.API.Extensions.RoleExtensions.GetRandomSpawnProperties(RoleType.Scp096).Item1, 0f);
            }
        }

        public IEnumerator<float> SpawnNuts(List<Player> hubs)
        {
            foreach (Player hub in hubs)
            {
                hub.ReferenceHub.characterClassManager.NetworkCurClass = RoleType.Scp173;
                hub.Broadcast(120,
                    "<color=red>You are a Nut, once this notice disapears (2 minutes), you will be set loose to kill the Dbois!</color>", Broadcast.BroadcastFlags.Normal, false);
                hub.ReferenceHub.serverRoles.BypassMode = true;
                yield return Timing.WaitForOneFrame * 30;
                hub.ReferenceHub.playerMovementSync.OverridePosition(Exiled.API.Extensions.RoleExtensions.GetRandomSpawnProperties(RoleType.Scp173).Item1, 0f);
            }
        }
        public Random Gen = new Random();
        public void OnRoundStart()
        {
            if (RunOnStart)
                Timing.RunCoroutine(Run(), "SurvivalOfTheFittest");
        }
        public static void PlayAmbientSound(int id)
        {
            PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 31));
        }

        public IEnumerator<float> Run()
        {
            Map.ClearBroadcasts();
            yield return Timing.WaitForSeconds(2f);
            Timing.KillCoroutines("CMLightsPluginCoroutines");
            List<Player> hubs = Player.List.ToList();
            List<Player> nuts = new List<Player>();

            for (int i = 0; i < 5 && hubs.Count > 2; i++)
            {
                int r = Gen.Next(hubs.Count);
                nuts.Add(hubs[r]);
                hubs.Remove(hubs[r]);
            }
            Timing.RunCoroutine(SpawnDbois(hubs), "SurvivalOfTheFittest");
            Timing.RunCoroutine(SpawnNuts(nuts), "SurvivalOfTheFittest");
            foreach (Door door in Map.Doors)
            {
                if (door.Nametag == "173" || door.Nametag == "GATE_A" || door.Nametag == "GATE_B" || door.Nametag == "NUKE_SURFACE")
                {
                    door.DoorLockType = DoorLockType.SpecialDoorFeature;
                    door.Open = false;
                }
                else
                {
                    door.DoorLockType = DoorLockType.None;
                    door.Open = false;
                }
            }
            Cassie.Message("90 seconds", false, false);
            yield return Timing.WaitForSeconds(30f);
            Cassie.Message("60 seconds", false, false);
            yield return Timing.WaitForSeconds(30f);
            Cassie.Message("30 seconds", false, false);
            yield return Timing.WaitForSeconds(18);
            Cassie.Message("10 . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . 0", false, false);
            int waitforready = 12;
            while (waitforready > 0)
            {
                PlayAmbientSound(7);
                waitforready--;
                yield return Timing.WaitForSeconds(1f);
            }
            Cassie.Message("ready or not here come the snap snap", false, false);
            foreach (Door door in Map.Doors)
            {
                if (door.Nametag == "173")
                {
                    door.DoorLockType = DoorLockType.SpecialDoorFeature;
                    door.Open = true;
                }
            }

            Timing.RunCoroutine(Blackout());
        }

        public IEnumerator<float> Blackout()
        {
            Generator079.ServerOvercharge(59.5f, false);
            yield return Timing.WaitForSeconds(60f);
            Timing.RunCoroutine(Blackout(), "SurvivalOfTheFittest");
        }
    }
}