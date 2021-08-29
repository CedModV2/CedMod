using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs;
using Hints;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CedMod.LightsPlugin
{
    public class ServerEventHandler
    {
        public void OnRoundRestart()
        {
            BlackoutOn = false;
            Timing.KillCoroutines("CMLightsPluginCoroutines");
        }

        public void OnRoundEnd(RoundEndedEventArgs ev)
        {
            BlackoutOn = false;
        }

        public void OnRoundStart()
        {
            Timing.RunCoroutine(Run(), "CMLightsPluginCoroutines");
            BlackoutOn = false;
        }

        public void Onchangerole(ChangingRoleEventArgs ev)
        {
            if (BlackoutOn)
            {
                Timing.CallDelayed(1f, () =>
                {
                    bool hasflashlight = false;
                    if (CedModLightsPlugin.Singleton.Config.GiveFlashlightsNotification)
                    {
                        foreach (var item in ev.Player.Inventory.UserInventory.Items)
                        {
                            if (item.Value.ItemTypeId == ItemType.Flashlight)
                            {
                                hasflashlight = true;
                            }
                        }

                        if (!hasflashlight)
                        {
                            Item item = new Item(ItemType.Flashlight);
                            if (ev.Player.Inventory.UserInventory.Items.Count <= 7)
                            {
                                ev.Player.HintDisplay.Show(new TextHint("<color=red>You have been given a flashlight.</color>", new HintParameter[] {new StringHintParameter("")}, null, 10f));
                                ev.Player.AddItem(ItemType.Flashlight);
                            }
                            else
                            {
                                ev.Player.HintDisplay.Show(new TextHint("<color=red>A flashlight has been dropped near you.</color>", new HintParameter[] {new StringHintParameter("")}, null, 10f));
                                item.Spawn(ev.Player.Position, Quaternion.identity);
                            }
                        }
                    }
                });
            }
        }

        public static bool BlackoutOn;

        public IEnumerator<float> Run()
        {
            Log.Debug("CMlights-init", CedModMain.Singleton.Config.ShowDebug);
            float chance = Random.Range(1, 100);
            if (chance <= CedModLightsPlugin.Singleton.Config.SpawnChance)
            {
                Log.Debug($"chance too low {chance} <= {CedModLightsPlugin.Singleton.Config.SpawnChance}", CedModMain.Singleton.Config.ShowDebug);
                yield break;
            }

            yield return Timing.WaitForSeconds(5f);
            foreach (Player ply in Player.List)
            {
                if (ply.Role == RoleType.Scp173 && !CedModLightsPlugin.Singleton.Config.BlackoutWhen173Ingame)
                {
                    Timing.KillCoroutines("CMLightsPluginCoroutines");
                    Log.Debug("173 stop", CedModMain.Singleton.Config.ShowDebug);
                    yield return 0f;
                }
            }

            float runin = Random.Range(CedModLightsPlugin.Singleton.Config.BlackoutWaitMin, CedModLightsPlugin.Singleton.Config.BlackoutWaitMax);
            yield return Timing.WaitForSeconds(runin);
            Cassie.Message(CedModLightsPlugin.Singleton.Config.CassieAnnouncementBlackoutStart, true, CedModLightsPlugin.Singleton.Config.CassieBells);
            BlackoutOn = true;
            float dur = Random.Range(CedModLightsPlugin.Singleton.Config.BlackoutDurationMin, CedModLightsPlugin.Singleton.Config.BlackoutDurationMax);
            Log.Debug("Running", CedModMain.Singleton.Config.ShowDebug);
            List<Room> rooms = Map.Rooms.ToList();
            foreach (Room r in rooms)
            {
                r.TurnOffLights(dur);
            }
            try
            {
                foreach (Player ply in Player.List)
                {
                    if (ply.Team != Team.SCP && ply.Team != Team.RIP && CedModLightsPlugin.Singleton.Config.GiveFlashlights)
                    {
                        bool hasflashlight = false;
                        if (CedModLightsPlugin.Singleton.Config.GiveFlashlightsNotification)
                        {
                            foreach (var item in ply.Inventory.UserInventory.Items)
                            {
                                if (item.Value.ItemTypeId == ItemType.Flashlight)
                                {
                                    hasflashlight = true;
                                }
                            }

                            if (!hasflashlight)
                            {
                                Item item = new Item(ItemType.Flashlight);
                                if (ply.Inventory.UserInventory.Items.Count <= 7)
                                {
                                    ply.HintDisplay.Show(new TextHint("<color=red>You have been given a flashlight.</color>", new HintParameter[] {new StringHintParameter("")}, null, 10f));
                                    ply.AddItem(ItemType.Flashlight);
                                }
                                else
                                {
                                    ply.HintDisplay.Show(new TextHint("<color=red>A flashlight has been dropped near you.</color>", new HintParameter[] {new StringHintParameter("")}, null, 10f));
                                    item.Spawn(ply.Position, Quaternion.identity);
                                    while (true)
                                    {
                                        Item.Get(ply.Inventory.UserInventory.Items.Values.FirstOrDefault()).Spawn(ply.Position, Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }

            yield return Timing.WaitForSeconds(dur);
            BlackoutOn = false;
            Cassie.Message(CedModLightsPlugin.Singleton.Config.CassieAnnouncementBlackoutStop, true, CedModLightsPlugin.Singleton.Config.CassieBells);
        }
    }
}