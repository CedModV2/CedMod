using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
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

        public void OnJoin(JoinedEventArgs ev)
        {
            if (RunOnStart)
                ev.Player.Broadcast(5, "Survival of the fittest is starting", Broadcast.BroadcastFlags.Normal);
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
                hub.Broadcast(30, "<color=orange>You are a Dboi, you need to find a hiding place and pray you are the last person alive, the nuts wil be release at their chamber in 2 minutes.</color>", Broadcast.BroadcastFlags.Normal);
                yield return Timing.WaitForOneFrame * 30;
				
                hub.ReferenceHub.inventory.Clear();
                hub.ReferenceHub.inventory.AddNewItem(ItemType.Flashlight);
                hub.ReferenceHub.inventory.AddNewItem(ItemType.KeycardO5);
                hub.ReferenceHub.playerMovementSync.OverridePosition(Map.GetRandomSpawnPoint(RoleType.Scp096), 0f);
            }
        }

        public IEnumerator<float> SpawnNuts(List<Player> hubs)
        {
            foreach (Player hub in hubs)
            {
                hub.ReferenceHub.characterClassManager.NetworkCurClass = RoleType.Scp173;
                hub.Broadcast(120,
                    "<color=red>You are a Nut, once this notice disapears (2 minutes), you will be set loose to kill the Dbois!</color>", Broadcast.BroadcastFlags.Normal);
                hub.ReferenceHub.serverRoles.BypassMode = true;
                yield return Timing.WaitForOneFrame * 30;
                hub.ReferenceHub.playerMovementSync.OverridePosition(Map.GetRandomSpawnPoint(RoleType.Scp173), 0f);
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
                door.CheckpointDoor = false;
                if (door.DoorName == "173" || door.DoorName == "GATE_A" || door.DoorName == "GATE_B" ||
                    door.DoorName == "NUKE_SURFACE")
                {
                    door.Networklocked = true;
                    door.NetworkisOpen = false;
                    door.enabled = false;
                }
                else
                {
                    door.Networklocked = false;
                    door.NetworkisOpen = false;
                    door.enabled = true;
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
                if (door.DoorName == "173")
                {
                    door.Networklocked = true;
                    door.NetworkisOpen = true;
                    door.enabled = true;
                    door.DestroyDoor(true);
                    door.destroyed = true;
                }
            }

            Timing.RunCoroutine(Blackout());
        }

        public IEnumerator<float> Blackout()
        {
            Generator079.Generators[0].ServerOvercharge(59.5f, false);
            yield return Timing.WaitForSeconds(60f);
            Timing.RunCoroutine(Blackout(), "SurvivalOfTheFittest");
        }
    }
}