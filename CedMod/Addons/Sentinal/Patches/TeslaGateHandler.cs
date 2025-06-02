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
using Logger = LabApi.Features.Console.Logger;

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
        public static Dictionary<string, (TeslaGate gate, DateTime time, int amount)> TeslaKills = new Dictionary<string, (TeslaGate gate, DateTime time, int amount)>();
        public static List<Player> CastList = new List<Player>();
        
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
            Dictionary<Player, (int amount, List<string> positions)> players = new Dictionary<Player, (int amount, List<string> positions)>();
            //Logger.Info("Tesla burdt");

            //TeslaCast(teslaGate, players);
            //yield return Timing.WaitForSeconds(0.25f);
            //TeslaCast(teslaGate, players);
            //yield return Timing.WaitForSeconds(0.25f);
            //TeslaCast(teslaGate, players);

            List<CoroutineHandle> coroutineHandles = new List<CoroutineHandle>();
            foreach (var plr in Player.ReadyList)
            {
                if ((teslaGate.sizeOfTrigger * 2) >= Vector3.Distance(plr.Position, teslaGate.Position))
                {
                    coroutineHandles.Add(Timing.RunCoroutine(DedicateCheck(plr, teslaGate, players)));
                }
            }

            yield return Timing.WaitUntilTrue(() => coroutineHandles.All(s => !s.IsRunning));
            foreach (var plr in players)
            {
                Timing.RunCoroutine(EvaluatePlayer(teslaGate, plr));
            }
        }

        public static void TeslaCast(TeslaGate teslaGate, Dictionary<Player, (int amount, List<string> positions)> players, Player plr1 = null)
        {
            CastList.Clear();
            foreach (var killer in teslaGate.killers)
            {
                if (killer == null)
                    continue;

                var size = Physics.OverlapBoxNonAlloc(killer.transform.position + Vector3.up * (teslaGate.sizeOfKiller.y / 2), teslaGate.sizeOfKiller / 2, hitColliders, new Quaternion(), teslaGate.killerMask);

                for (int i = 0; i < size; i++)
                {
                    var plr = Player.Get(hitColliders[i].gameObject);
                    if (plr == null || (plr1 != null && plr1 != plr) || CastList.Contains(plr))
                        continue;
                    
                    if (!players.ContainsKey(plr))
                        players.Add(plr, (0, new List<string>()));
                    
                    CastList.Add(plr);
                    players[plr].positions.Add(plr.Position.ToString());
                    players[plr] = (players[plr].amount + 1, players[plr].positions);
                }
            }
        }
        
        public static IEnumerator<float> DedicateCheck(Player plr1, TeslaGate teslaGate, Dictionary<Player, (int amount, List<string> positions)> players)
        {
            yield return Timing.WaitForSeconds(Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[plr1.ReferenceHub.connectionToClient.connectionId].Ping * 2f) / 1000));
            players.Remove(plr1);
            
            TeslaCast(teslaGate, players, plr1);
            yield return Timing.WaitForSeconds(0.20f);
            TeslaCast(teslaGate, players, plr1);
            yield return Timing.WaitForSeconds(0.20f);
            TeslaCast(teslaGate, players, plr1);
        }

        private static IEnumerator<float> EvaluatePlayer(TeslaGate teslaGate, KeyValuePair<Player, (int amount, List<string> positions)> plr)
        {
            yield return Timing.WaitForSeconds(0.10f + (LiteNetLib4MirrorServer.Peers[plr.Key.ReferenceHub.connectionToClient.connectionId].Ping * 2 / 100f));
            if (TeslaHits.ContainsKey(plr.Key.NetworkId) && TeslaHits[plr.Key.NetworkId].Elapsed.TotalMilliseconds <= 1500 + Mathf.Min(0.5f, (LiteNetLib4MirrorServer.Peers[plr.Key.ReferenceHub.connectionToClient.connectionId].Ping * 1.75f) / 1000))
            {
                yield return Timing.WaitForSeconds(teslaGate.cooldownTime);
                TeslaHits.Remove(plr.Key.NetworkId);
                yield break;
            }
            
            WebSocketSystem.Enqueue(new QueryCommand()
            {
                Recipient = "PANEL",
                Data = new Dictionary<string, string>()
                {
                    { "SentinalType", "HarmlessTesla" }, 
                    { "UserId", plr.Key.UserId },
                    { "Tesla", teslaGate.netId.ToString()},
                    { "Ping", (LiteNetLib4MirrorServer.Peers[plr.Key.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() },
                    { "GatePos", teslaGate.Position.ToString() },
                    { "Positions", JsonConvert.SerializeObject(plr.Value.positions) }
                }
            });
            TeslaKills[plr.Key.UserId] = (teslaGate, DateTime.UtcNow, plr.Value.amount);
            //TeslaGateController.ServerReceiveMessage(plr.Connection, new TeslaHitMsg(teslaGate));
        }
    }
}