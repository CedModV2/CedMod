using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.INIT;
using Exiled.Events.EventArgs;
using GameCore;
using Grenades;
using HarmonyLib;
using MEC;
using Mirror;
using UnityEngine;
using Console = System.Console;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CedMod.Handlers
{
    public static class MiniGameHandler
    {
        public static int Balls = 0;

        public static IEnumerator<float> Playerjoinhandle(JoinedEventArgs ev)
        {
            ReferenceHub Player = ev.Player.ReferenceHub;
            yield return Timing.WaitForSeconds(0.5f);
            if (!RoundSummary.RoundInProgress())
            {
                Player.characterClassManager.SetPlayersClass(RoleType.Tutorial, Player.gameObject);
                ev.Player.IsGodModeEnabled = false;
                yield return Timing.WaitForSeconds(0.2f);
                ev.Player.Position = (new Vector3(176.2169f, 987.3649f,112.187f));
                yield return Timing.WaitForSeconds(0.2f);
                if (Balls <= 5 && SelectedGameType == PreRoundGameType.BouncyCage)
                {
                    Vector3 spawnrand = new Vector3(Random.Range(0f, 2f), Random.Range(0f, 2f),
                        Random.Range(0f, 2f));
                    GrenadeManager gm = ev.Player.GrenadeManager;
                    GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
                    if (ball == null)
                    {
                        yield return 0f;
                    }

                    Vector3 olds = ball.grenadeInstance.transform.localScale;
                    ball.grenadeInstance.transform.localScale = new Vector3(3f, 3f, 3f);
                    Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
                    component.InitData(gm, spawnrand, Vector3.zero);
                    NetworkServer.Spawn(component.gameObject);
                    ball.grenadeInstance.transform.localScale = olds;
                    component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
                    component.InitData(gm, spawnrand, Vector3.zero);
                    Balls++;
                }
            }
            yield return 1f;
        }
        public enum PreRoundGameType
        {
            BouncyCage = 0,
            WorkStationParcour = 1
        }
        public static PreRoundGameType SelectedGameType;
        public class PreGameProps
        {
            public PreGameProps(Vector3 pos, Vector3 rot, Vector3 size, string objectname, PreRoundGameType gametype, ItemType item = ItemType.None)
            {
                Pos = pos;
                Rot = rot;
                Size = size;
                ObjectName = objectname;
                GameType = gametype;
                Item = item;
            }
            public Vector3 Pos;
            public Vector3 Rot;
            public Vector3 Size;
            public string ObjectName;
            public PreRoundGameType GameType;
            public ItemType Item;

        }
        public static List<GameObject> PreGamePropSpawned = new List<GameObject>();
        public static List<PreGameProps> WorkBenchposses()
        {
            return new List<PreGameProps>
            {
                //bouncy cage
                new PreGameProps(new Vector3(174.7735f, 986.9232f, 126.773f), new Vector3(0,0,0), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(174.7735f, 986.9232f, 126.773f), new Vector3(0,0,180), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(191.3532f, 986.9232f, 112.6005f), new Vector3(0,90,0), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(191.3532f, 986.9232f, 112.6005f), new Vector3(0,90,180), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(163.9842f, 986.9232f, 112.6005f), new Vector3(0,90,0), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(163.9842f, 986.9232f, 112.6005f), new Vector3(0,90,180), new Vector3(18,18,1), "Work Station",PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(174.7735f, 986.9232f, 98.24794f), new Vector3(0,0,0), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(174.7735f, 986.9232f, 98.24794f), new Vector3(0,0,180), new Vector3(18,18,1), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(178.7831f, 999.4953f, 113.6765f), new Vector3(0,0,0), new Vector3(10,1,20), "Work Station", PreRoundGameType.BouncyCage, ItemType.None),
                new PreGameProps(new Vector3(178.7831f, 999.4953f, 113.6765f), new Vector3(0,0,90), new Vector3(20,20,20), "Item", PreRoundGameType.BouncyCage, ItemType.Flashlight),
                
                //parcour
                new PreGameProps(new Vector3(168.8969f, 978.03f, 114.0997f), new Vector3(0,0,0), new Vector3(2,0.2f,5), "Work Station", PreRoundGameType.WorkStationParcour, ItemType.None),
                new PreGameProps(new Vector3(161.8969f, 979.03f, 114.0997f), new Vector3(0,0,0), new Vector3(2,0.2f,5), "Work Station", PreRoundGameType.WorkStationParcour, ItemType.None),
                new PreGameProps(new Vector3(153.8969f, 980.03f, 114.0997f), new Vector3(0,0,0), new Vector3(2,0.2f,5), "Work Station", PreRoundGameType.WorkStationParcour, ItemType.None),
                new PreGameProps(new Vector3(144.8969f, 981.03f, 114.0997f), new Vector3(0,0,0), new Vector3(2,0.2f,5), "Work Station", PreRoundGameType.WorkStationParcour, ItemType.None),
                new PreGameProps(new Vector3(134.8969f, 982.03f, 118.0997f), new Vector3(0,0,0), new Vector3(2,0.2f,5), "Work Station", PreRoundGameType.WorkStationParcour, ItemType.None),
            };
        }
        public static IEnumerator<float> WaitingForPlayers()
        {
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                try
                {
                    GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    var rnd = new System.Random();
                    PreRoundGameType GameType =
                        (PreRoundGameType) rnd.Next(Enum.GetNames(typeof(PreRoundGameType)).Length);
                    Initializer.Logger.Debug("PreRoundMiniGame", "Minigame chosen: " + GameType);
                    SelectedGameType = GameType;
                    foreach (PreGameProps wk in WorkBenchposses())
                    {
                        if (wk.GameType == GameType && wk.ObjectName == "Work Station")
                        {
                            GameObject bench =
                                Object.Instantiate(
                                    NetworkManager.singleton.spawnPrefabs.Find(p =>
                                        p.gameObject.name == wk.ObjectName));
                            Offset offset = new Offset();
                            offset.position = wk.Pos;
                            offset.rotation = wk.Rot;
                            offset.scale = Vector3.one;
                            bench.gameObject.transform.localScale = wk.Size;
                            Initializer.Logger.Debug("PreRoundMiniGame",
                                $"Spawning workstation at {wk.Pos.ToString()} with a rotation of {wk.Rot} and a size of {wk.Size}");
                            NetworkServer.Spawn(bench);
                            bench.GetComponent<WorkStation>().Networkposition = offset;
                            bench.AddComponent<WorkStationUpgrader>();
                            PreGamePropSpawned.Add(bench);
                        }

                        if (wk.ObjectName == "Item" && wk.Item != ItemType.None && wk.GameType == GameType)
                        {
                            Pickup yesnt =
                                Exiled.API.Extensions.Item.Spawn(wk.Item, 0, wk.Pos, Quaternion.Euler(wk.Rot));

                            yesnt.gameObject.transform.localScale = wk.Size;
                            yesnt.Locked = true;
                            NetworkServer.UnSpawn(yesnt.gameObject);
                            NetworkServer.Spawn(yesnt.gameObject);
                            PreGamePropSpawned.Add(yesnt.gameObject);
                            Initializer.Logger.Debug("PreRoundMiniGame",
                                $"Spawning {wk.Item} at {wk.Pos.ToString()} with a rotation of {wk.Rot} and a size of {wk.Size}");
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Initializer.Logger.Error("PreRoundMinigame", e.StackTrace);
                }
            }

            yield return 0f;
        }

        public static void RoundStart()
        {
            List<GameObject> spawned = PreGamePropSpawned.ToList();
            foreach (GameObject gb in PreGamePropSpawned)
            {
                Initializer.Logger.Debug("PreRoundMiniGame", "Cleaning up spawned prop: " + gb.name);
                NetworkServer.UnSpawn(gb);
                spawned.Remove(gb);
            }
            PreGamePropSpawned = spawned;
            Balls = 0;
        }

        public static void Pickup(PickingUpItemEventArgs ev)
        {
            if (PreGamePropSpawned.Contains(ev.Pickup.gameObject))
                ev.IsAllowed = false;
        }

        [HarmonyPatch(typeof(Pickup), nameof(global::Pickup.LateUpdate))]
        public static class FloatingItemsPatch
        {
            public static void Postfix(Pickup __instance)
            {
                if (!MiniGameHandler.PreGamePropSpawned.Contains(__instance.gameObject))
                    return;
                __instance.Rb.useGravity = false;
                Vector3 v3 = __instance.gameObject.transform.position;
                __instance.position = v3;
            }
        }
    }
}