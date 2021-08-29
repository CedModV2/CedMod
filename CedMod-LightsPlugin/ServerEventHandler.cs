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
                    bool _hasflashlight = false;
                    if (CedModLightsPlugin.config.GiveFlashlightsNotification)
                    {
                        foreach (var Item in ev.Player.Inventory.UserInventory.Items)
                        {
                            if (Item.Value.ItemTypeId == ItemType.Flashlight)
                            {
                                _hasflashlight = true;
                            }
                        }

                        if (!_hasflashlight)
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
            Log.Debug("CMlights-init", CedModMain.config.ShowDebug);
            float chance = Random.Range(1, 100);
            if (chance <= CedModLightsPlugin.config.SpawnChance)
            {
                Log.Debug($"chance too low {chance} <= {CedModLightsPlugin.config.SpawnChance}", CedModMain.config.ShowDebug);
                yield break;
            }

            yield return Timing.WaitForSeconds(5f);
            foreach (Player ply in Player.List)
            {
                if (ply.Role == RoleType.Scp173 && !CedModLightsPlugin.config.BlackoutWhen173Ingame)
                {
                    Timing.KillCoroutines("CMLightsPluginCoroutines");
                    Log.Debug("173 stop", CedModMain.config.ShowDebug);
                    yield return 0f;
                }
            }

            float runin = UnityEngine.Random.Range(CedModLightsPlugin.config.BlackoutWaitMin, CedModLightsPlugin.config.BlackoutWaitMax);
            yield return Timing.WaitForSeconds(runin);
            Cassie.Message(CedModLightsPlugin.config.CassieAnnouncementBlackoutStart, true, CedModLightsPlugin.config.CassieBells);
            BlackoutOn = true;
            float dur = Random.Range(CedModLightsPlugin.config.BlackoutDurationMin, CedModLightsPlugin.config.BlackoutDurationMax);
            Log.Debug("Running", CedModMain.config.ShowDebug);
            List<Room> rooms = Map.Rooms.ToList();
            foreach (Room r in rooms)
            {
                r.TurnOffLights(dur);
            }
            try
            {
                foreach (Player ply in Player.List)
                {
                    if (ply.Team != Team.SCP && ply.Team != Team.RIP && CedModLightsPlugin.config.GiveFlashlights)
                    {
                        bool _hasflashlight = false;
                        if (CedModLightsPlugin.config.GiveFlashlightsNotification)
                        {
                            foreach (var Item in ply.Inventory.UserInventory.Items)
                            {
                                if (Item.Value.ItemTypeId == ItemType.Flashlight)
                                {
                                    _hasflashlight = true;
                                }
                            }

                            if (!_hasflashlight)
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
            Cassie.Message(CedModLightsPlugin.config.CassieAnnouncementBlackoutStop, true, CedModLightsPlugin.config.CassieBells);
        }
    }
}