using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Server;
using GameCore;
using HarmonyLib;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
using PluginAPI.Enums;
using PluginAPI.Events;
using RemoteAdmin;
using RoundRestarting;
using UnityEngine;
using Utils;
using VoiceChat;
using Console = GameCore.Console;
using Server = Exiled.Events.Handlers.Server;

namespace CedMod.Patches
{
#if EXILED
	[HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static class RoundEndPatchPatch
    {
        private static bool Prefix(RoundSummary __instance)
        {
            RoundSummary.singleton = __instance;
            RoundSummary._singletonSet = true;
            if (!NetworkServer.active)
                return false;
            RoundSummary.roundTime = 0;
            __instance.KeepRoundOnOne = !ConfigFile.ServerConfig.GetBool("end_round_on_one_player");
            Timing.RunCoroutine(Process(__instance), Segment.FixedUpdate);
            RoundSummary.KilledBySCPs = 0;
            RoundSummary.EscapedClassD = 0;
            RoundSummary.EscapedScientists = 0;
            RoundSummary.ChangedIntoZombies = 0;
            RoundSummary.Kills = 0;
            PlayerRoleManager.OnServerRoleSet += new PlayerRoleManager.ServerRoleSet(__instance.OnRoleChanged);
            PlayerStats.OnAnyPlayerDied += new Action<ReferenceHub, DamageHandlerBase>(__instance.OnAnyPlayerDied);
            return false;
        }
        
	    private static IEnumerator<float> Process(RoundSummary roundSummary)
        {
            float time = Time.unscaledTime;
            while (roundSummary != null)
            {
                yield return Timing.WaitForSeconds(2.5f);
                if (!RoundSummary.RoundLock)
                {
                    if (roundSummary.KeepRoundOnOne)
                    {
                        if (ReferenceHub.AllHubs.Count((ReferenceHub x) => x.characterClassManager.InstanceMode != ClientInstanceMode.DedicatedServer) < 2)
                        {
                            continue;
                        }
                    }

                    if (RoundSummary.RoundInProgress() && Time.unscaledTime - time >= 15f)
                    {
                        RoundSummary.SumInfo_ClassList newList = default(RoundSummary.SumInfo_ClassList);
                        foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                        {
                            switch (hub.GetTeam())
                            {
                                case Team.SCPs:
                                    if (hub.GetRoleId() == RoleTypeId.Scp0492)
                                    {
                                        newList.zombies++;
                                    }
                                    else
                                    {
                                        newList.scps_except_zombies++;
                                    }

                                    break;
                                case Team.FoundationForces:
                                    newList.mtf_and_guards++;
                                    break;
                                case Team.ChaosInsurgency:
                                    newList.chaos_insurgents++;
                                    break;
                                case Team.Scientists:
                                    newList.scientists++;
                                    break;
                                case Team.ClassD:
                                    newList.class_ds++;
                                    break;
                            }
                        }

                        yield return float.NegativeInfinity;

                        newList.warhead_kills = AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : -1;

                        yield return float.NegativeInfinity;

                        int num = newList.mtf_and_guards + newList.scientists;
                        int num2 = newList.chaos_insurgents + newList.class_ds;
                        int num3 = newList.scps_except_zombies + newList.zombies;
                        int num4 = newList.class_ds + RoundSummary.EscapedClassD;
                        int num5 = newList.scientists + RoundSummary.EscapedScientists;

                        RoundSummary.SurvivingSCPs = newList.scps_except_zombies;

                        float num6 = roundSummary.classlistStart.class_ds == 0 ? 0 : (num4 / roundSummary.classlistStart.class_ds);
                        float num7 = roundSummary.classlistStart.scientists == 0 ? 1 : (num5 / roundSummary.classlistStart.scientists);
                        if (newList.class_ds <= 0 && num <= 0)
                        {
                            roundSummary._roundEnded = true;
                        }
                        else
                        {
                            int num8 = 0;
                            if (num > 0)
                            {
                                num8++;
                            }

                            if (num2 > 0)
                            {
                                num8++;
                            }

                            if (num3 > 0)
                            {
                                num8++;
                            }

                            roundSummary._roundEnded = num8 <= 1;
                        }

                        EndingRoundEventArgs endingRoundEventArgs = new(LeadingTeam.Draw, newList, roundSummary._roundEnded);

                        Server.OnEndingRound(endingRoundEventArgs);

                        roundSummary._roundEnded = endingRoundEventArgs.IsRoundEnded && endingRoundEventArgs.IsAllowed;

                        if (roundSummary._roundEnded)
                        {
                            FriendlyFireConfig.PauseDetector = true;

                            bool flag = num > 0;
                            bool flag2 = num2 > 0;
                            bool flag3 = num3 > 0;

                            RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;

                            if (flag)
                            {
                                leadingTeam = (RoundSummary.EscapedScientists >= RoundSummary.EscapedClassD) ? RoundSummary.LeadingTeam.FacilityForces : RoundSummary.LeadingTeam.Draw;
                            }
                            else if (flag3 || (flag3 && flag2))
                            {
                                leadingTeam = (RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs) ? RoundSummary.LeadingTeam.ChaosInsurgency : ((RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists) ? RoundSummary.LeadingTeam.Anomalies : RoundSummary.LeadingTeam.Draw);
                            }
                            else if (flag2)
                            {
                                leadingTeam = (RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists) ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Draw;
                            }

                            string text = string.Concat(new object[]
                            {
                            "Round finished! Anomalies: ",
                            num3,
                            " | Chaos: ",
                            num2,
                            " | Facility Forces: ",
                            num,
                            " | D escaped percentage: ",
                            num6,
                            " | S escaped percentage: : ",
                            num7,
                            });
                            
                            if (!EventManager.ExecuteEvent(ServerEventType.RoundEnd, leadingTeam)) //make it so how sl code does it, until NW releases RC6
                                break;

                            Console.AddLog(text, Color.gray, false, Console.ConsoleLogType.Log);

                            ServerLogs.AddLog(ServerLogs.Modules.Logger, text, ServerLogs.ServerLogType.GameEvent, false);

                            yield return Timing.WaitForSeconds(1.5f);

                            int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);

                            if (roundSummary != null)
                            {
                                RoundEndedEventArgs roundEndedEventArgs = new(endingRoundEventArgs.LeadingTeam, newList, timeToRoundRestart);

                                Exiled.Events.Handlers.Server.OnRoundEnded(roundEndedEventArgs);

                                roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, newList, leadingTeam, RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs, timeToRoundRestart, (int)RoundStart.RoundLength.TotalSeconds);
                            }

                            yield return Timing.WaitForSeconds(timeToRoundRestart - 1);

                            roundSummary.RpcDimScreen();

                            yield return Timing.WaitForSeconds(1f);

                            RoundRestart.InitiateRoundRestart();
                        }
                    }
                }
            }
        }
    }
#endif
}