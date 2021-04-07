using System;
using System.Collections.Generic;
using CedMod.INIT;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Hints;
using MEC;
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
                    Initializer.Logger.Info("CedMod-LightsPlugin", "Giving flashlights");
                    if (CedModLightsPlugin.config.GiveFlashlightsNotification)
                    {
                        foreach (var Item in ev.Player.Inventory.items)
                        {
                            if (Item.id == ItemType.Flashlight)
                            {
                                _hasflashlight = true;
                            }
                        }

                        if (!_hasflashlight)
                        {
                            ev.Player.HintDisplay.Show(new TextHint(
                                "<color=red>You have been given a flashlight.</color>",
                                new HintParameter[] {new StringHintParameter("")}, null, 10f));
                            ev.Player.Inventory.AddNewItem(ItemType.Flashlight);
                        }
                    }
                });
            }
        }

        public static bool BlackoutOn;

        public IEnumerator<float> Run()
        {
            float chance = Random.Range(1, 100);

            Initializer.Logger.Info("CedMod-LightsPlugin", $"Chance: {chance}");
            if (chance <= CedModLightsPlugin.config.SpawnChance)
            {
                Initializer.Logger.Info("CedMod-LightsPlugin", $"Chance below {chance} skipping");
                yield break;
            }
            Initializer.Logger.Info("CedMod-LightsPlugin", $"Chance above {chance} continueing");
            
            yield return Timing.WaitForSeconds(5f);
            foreach (Player ply in Player.List)
            {
                if (ply.Role == RoleType.Scp173 && !CedModLightsPlugin.config.BlackoutWhen173Ingame)
                {
                    Initializer.Logger.Info("CedMod-LightsPlugin", "173 is ingame not running blackout");
                    Timing.KillCoroutines("CMLightsPluginCoroutines");
                    yield return 0f;
                }
            }

            float runin = UnityEngine.Random.Range(CedModLightsPlugin.config.BlackoutWaitMin,
                CedModLightsPlugin.config.BlackoutWaitMax);
            Initializer.Logger.Info("CedMod-LightsPlugin", "Running blackout in: " + runin);
            yield return Timing.WaitForSeconds(runin);
            Cassie.Message(CedModLightsPlugin.config.CassieAnnouncementBlackoutStart, true,
                CedModLightsPlugin.config.CassieBells);
            BlackoutOn = true;
            float dur = Random.Range(CedModLightsPlugin.config.BlackoutDurationMin,
                CedModLightsPlugin.config.BlackoutDurationMax);
            Initializer.Logger.Info("CedMod-LightsPlugin", "Running blackout, blackout will last: " + dur);
            Generator079.mainGenerator.ServerOvercharge(dur, false);
            foreach (Player ply in Player.List)
            {
                if (ply.Team != Team.SCP && ply.Team != Team.RIP && CedModLightsPlugin.config.GiveFlashlights)
                {
                    bool _hasflashlight = false;
                    Initializer.Logger.Info("CedMod-LightsPlugin", "Giving flashlights");
                    if (CedModLightsPlugin.config.GiveFlashlightsNotification)
                    {
                        foreach (var Item in ply.Inventory.items)
                        {
                            if (Item.id == ItemType.Flashlight)
                            {
                                _hasflashlight = true;
                            }
                        }

                        if (!_hasflashlight)
                        {
                            ply.HintDisplay.Show(new TextHint("<color=red>You have been given a flashlight.</color>",
                                new HintParameter[] {new StringHintParameter("")}, null, 10f));
                            ply.Inventory.AddNewItem(ItemType.Flashlight);
                        }
                    }
                }
            }

            yield return Timing.WaitForSeconds(dur);
            BlackoutOn = false;
            Cassie.Message(CedModLightsPlugin.config.CassieAnnouncementBlackoutStop, true,
                CedModLightsPlugin.config.CassieBells);
            Initializer.Logger.Info("CedMod-LightsPlugin", "Blackout has ended");
        }
    }
}