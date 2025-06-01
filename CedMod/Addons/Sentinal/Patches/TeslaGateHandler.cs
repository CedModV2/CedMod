using System;
using System.Collections.Generic;
using System.Diagnostics;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using PlayerStatsSystem;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace CedMod.Addons.Sentinal.Patches
{
    [HarmonyPatch(typeof(TeslaGateController), nameof(TeslaGateController.ServerReceiveMessage))]
    public static class TeslaPatch
    {
        public static void Prefix(NetworkConnection conn, TeslaHitMsg msg)
        {
            ReferenceHub hub;
            if (!ReferenceHub.TryGetHubNetID(conn.identity.netId, out hub) || msg.Gate == null || Vector3.Distance(msg.Gate.Position, hub.transform.position) > (msg.Gate.sizeOfTrigger * 2.200000047683716))
                return;
            
            TeslaGateHandler.TeslaHits[hub.netId] = Stopwatch.StartNew();
        }
    }
    
    public class TeslaGateHandler: CustomEventsHandler
    {
        public TeslaGateHandler()
        {
            TeslaGate.OnBursted += Burst;
        }
        
        readonly static Collider[] hitColliders = new Collider[70];
        public static Dictionary<uint, Stopwatch> TeslaHits = new Dictionary<uint, Stopwatch>();
        public static Dictionary<string, (TeslaGate gate, DateTime time)> TeslaKills = new Dictionary<string, (TeslaGate gate, DateTime time)>();
        
        public override void OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            TeslaHits.Remove(ev.Player.NetworkId);
        }

        private static void Burst(TeslaGate obj)
        {
            Timing.RunCoroutine(TeslaGateBurst(obj));
        }

        private static IEnumerator<float> TeslaGateBurst(TeslaGate teslaGate)
        {
            List<Player> players = new List<Player>();

            yield return Timing.WaitForSeconds(0.25f);
            foreach (var killer in teslaGate.killers)
            {
                if (killer == null)
                    continue;

                var size = Physics.OverlapBoxNonAlloc(killer.transform.position + Vector3.up * (teslaGate.sizeOfKiller.y / 2), teslaGate.sizeOfKiller / 2, hitColliders, new Quaternion(), teslaGate.killerMask);

                for (int i = 0; i < size; i++)
                {
                    var plr = Player.Get(hitColliders[i].gameObject);
                    if (plr == null)
                        continue;
                    
                    players.Add(plr);
                }
            }
            
            yield return Timing.WaitForSeconds(0.25f);
            foreach (var killer in teslaGate.killers)
            {
                if (killer == null)
                    continue;

                var size = Physics.OverlapBoxNonAlloc(killer.transform.position + Vector3.up * (teslaGate.sizeOfKiller.y / 2), teslaGate.sizeOfKiller / 2, hitColliders, new Quaternion(), teslaGate.killerMask);

                for (int i = 0; i < size; i++)
                {
                    var plr = Player.Get(hitColliders[i].gameObject);
                    if (plr == null || players.Contains(plr))
                        continue;
                    
                    players.Add(plr);
                }
            }

            List<CoroutineHandle> coroutineHandles = new List<CoroutineHandle>();
            foreach (var plr in players)
            {
                if (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2 >= 150)
                {
                    coroutineHandles.Add(Timing.RunCoroutine(DedicateCheck(plr, teslaGate, players)));
                }
            }

            yield return Timing.WaitUntilTrue(() => coroutineHandles.All(s => s.IsRunning));
            foreach (var plr in players)
            {
                Timing.RunCoroutine(EvaluatePlayer(teslaGate, plr));
            }
        }

        public static IEnumerator<float> DedicateCheck(Player plr1, TeslaGate teslaGate, List<Player> players)
        {
            yield return Timing.WaitForSeconds(Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[plr1.ReferenceHub.connectionToClient.connectionId].Ping * 1.75f) / 1000));
            yield return Timing.WaitForSeconds(0.25f);
            players.Remove(plr1);
            foreach (var killer in teslaGate.killers)
            {
                if (killer == null)
                    continue;

                var size = Physics.OverlapBoxNonAlloc(killer.transform.position + Vector3.up * (teslaGate.sizeOfKiller.y / 2), teslaGate.sizeOfKiller / 2, hitColliders, new Quaternion(), teslaGate.killerMask);

                for (int i = 0; i < size; i++)
                {
                    var plr = Player.Get(hitColliders[i].gameObject);
                    if (plr == null || plr != plr1)
                        continue;
                    
                    players.Add(plr);
                }
            }
            
            yield return Timing.WaitForSeconds(0.25f);
            foreach (var killer in teslaGate.killers)
            {
                if (killer == null)
                    continue;

                var size = Physics.OverlapBoxNonAlloc(killer.transform.position + Vector3.up * (teslaGate.sizeOfKiller.y / 2), teslaGate.sizeOfKiller / 2, hitColliders, new Quaternion(), teslaGate.killerMask);

                for (int i = 0; i < size; i++)
                {
                    var plr = Player.Get(hitColliders[i].gameObject);
                    if (plr == null || plr != plr1)
                        continue;
                    
                    players.Add(plr);
                }
            }
        }

        private static IEnumerator<float> EvaluatePlayer(TeslaGate teslaGate, Player plr)
        {
            yield return Timing.WaitForSeconds(0.10f + (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2 / 100f));
            if (TeslaHits.ContainsKey(plr.NetworkId) && TeslaHits[plr.NetworkId].Elapsed.TotalMilliseconds <= 1500 + Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 1.75f) / 1000))
            {
                TeslaHits.Remove(plr.NetworkId);
                yield break;
            }
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "HarmlessTesla" }, 
                    { "UserId", plr.UserId },
                    { "Tesla", teslaGate.netId.ToString()},
                    { "Ping", (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() }
                }
            });
            TeslaKills[plr.UserId] = (teslaGate, DateTime.UtcNow);
            //TeslaGateController.ServerReceiveMessage(plr.Connection, new TeslaHitMsg(teslaGate));
        }
    }
}