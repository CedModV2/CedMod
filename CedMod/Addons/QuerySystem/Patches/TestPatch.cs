namespace CedMod.Addons.QuerySystem.Patches
{
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
#if !EXILED
	[HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static class TestPatch
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
          while ((UnityEngine.Object)roundSummary != (UnityEngine.Object)null)
          {
            yield return Timing.WaitForSeconds(2.5f);
            if (!RoundSummary.RoundLock && (!roundSummary.KeepRoundOnOne || ReferenceHub.AllHubs.Count<ReferenceHub>((Func<ReferenceHub, bool>)(x => x.characterClassManager.InstanceMode != ClientInstanceMode.DedicatedServer)) >= 2) && RoundSummary.RoundInProgress() && (double)Time.unscaledTime - (double)time >= 15.0)
            {
              RoundSummary.SumInfo_ClassList newList = new RoundSummary.SumInfo_ClassList();
              foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
              {
                switch (allHub.GetTeam())
                {
                  case Team.SCPs:
                    if (allHub.GetRoleId() == RoleTypeId.Scp0492)
                    {
                      ++newList.zombies;
                      continue;
                    }

                    ++newList.scps_except_zombies;
                    continue;
                  case Team.FoundationForces:
                    ++newList.mtf_and_guards;
                    continue;
                  case Team.ChaosInsurgency:
                    ++newList.chaos_insurgents;
                    continue;
                  case Team.Scientists:
                    ++newList.scientists;
                    continue;
                  case Team.ClassD:
                    ++newList.class_ds;
                    continue;
                  default:
                    continue;
                }
              }

              yield return float.NegativeInfinity;
              newList.warhead_kills = AlphaWarheadController.Detonated ? AlphaWarheadController.Singleton.WarheadKills : -1;
              yield return float.NegativeInfinity;
              int facilityForces = newList.mtf_and_guards + newList.scientists;
              int chaosInsurgency = newList.chaos_insurgents + newList.class_ds;
              int anomalies = newList.scps_except_zombies + newList.zombies;
              int num1 = newList.class_ds + RoundSummary.EscapedClassD;
              int num2 = newList.scientists + RoundSummary.EscapedScientists;
              RoundSummary.SurvivingSCPs = newList.scps_except_zombies;
              float dEscapePercentage = roundSummary.classlistStart.class_ds == 0 ? 0.0f : (float)(num1 / roundSummary.classlistStart.class_ds);
              float sEscapePercentage = roundSummary.classlistStart.scientists == 0 ? 1f : (float)(num2 / roundSummary.classlistStart.scientists);
              bool flag1;
              if (newList.class_ds <= 0 && facilityForces <= 0)
              {
                flag1 = true;
              }
              else
              {
                int num3 = 0;
                if (facilityForces > 0)
                  ++num3;
                if (chaosInsurgency > 0)
                  ++num3;
                if (anomalies > 0)
                  ++num3;
                flag1 = num3 <= 1;
              }

              if (!roundSummary._roundEnded)
              {
                switch (EventManager.ExecuteEvent<RoundEndConditionsCheckCancellationData>(ServerEventType.RoundEndConditionsCheck, (object)flag1).Cancellation)
                {
                  case RoundEndConditionsCheckCancellationData.RoundEndConditionsCheckCancellation.ConditionsSatisfied:
                    roundSummary._roundEnded = true;
                    break;
                  case RoundEndConditionsCheckCancellationData.RoundEndConditionsCheckCancellation.ConditionsNotSatisfied:
                    if (!roundSummary._roundEnded)
                      continue;
                    break;
                }

                if (flag1)
                  roundSummary._roundEnded = true;
              }
              
              if (roundSummary._roundEnded)
              {
                int num4 = facilityForces > 0 ? 1 : 0;
                bool flag2 = chaosInsurgency > 0;
                bool flag3 = anomalies > 0;
                RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;
                if (num4 != 0) 
                  leadingTeam = RoundSummary.EscapedScientists >= RoundSummary.EscapedClassD ? RoundSummary.LeadingTeam.FacilityForces : RoundSummary.LeadingTeam.Draw;
                else if (flag3 || flag3 & flag2)
                  leadingTeam = RoundSummary.EscapedClassD > RoundSummary.SurvivingSCPs ? RoundSummary.LeadingTeam.ChaosInsurgency : (RoundSummary.SurvivingSCPs > RoundSummary.EscapedScientists ? RoundSummary.LeadingTeam.Anomalies : RoundSummary.LeadingTeam.Draw);
                else if (flag2)
                  leadingTeam = RoundSummary.EscapedClassD >= RoundSummary.EscapedScientists ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Draw;
                object[] objArray1 = new object[1]
                {
                  (object)leadingTeam
                };
                object[] objArray2;
                for (RoundEndCancellationData cancellationData = EventManager.ExecuteEvent<RoundEndCancellationData>(ServerEventType.RoundEnd, objArray1); cancellationData.IsCancelled; cancellationData = EventManager.ExecuteEvent<RoundEndCancellationData>(ServerEventType.RoundEnd, objArray2))
                {
                  if ((double)cancellationData.Delay <= 0.0)
                  {
                    break;
                  }
                  else
                  {
                    yield return Timing.WaitForSeconds(cancellationData.Delay);
                    objArray2 = new object[1]
                    {
                      (object)leadingTeam
                    };
                  }
                }

                if (Statistics.FastestEndedRound.Duration > RoundStart.RoundLength)
                  Statistics.FastestEndedRound =
                    new Statistics.FastestRound(leadingTeam, RoundStart.RoundLength, DateTime.Now);
                Statistics.CurrentRound.ClassDAlive = newList.class_ds;
                Statistics.CurrentRound.ScientistsAlive = newList.scientists;
                Statistics.CurrentRound.MtfAndGuardsAlive = newList.mtf_and_guards;
                Statistics.CurrentRound.ChaosInsurgencyAlive = newList.chaos_insurgents;
                Statistics.CurrentRound.ZombiesAlive = newList.zombies;
                Statistics.CurrentRound.ScpsAlive = newList.scps_except_zombies;
                Statistics.CurrentRound.WarheadKills = newList.warhead_kills;
                FriendlyFireConfig.PauseDetector = true;
                string str = "Round finished! Anomalies: " + (object)anomalies + " | Chaos: " + 
                             (object)chaosInsurgency + " | Facility Forces: " + (object)facilityForces +
                             " | D escaped percentage: " + (object)dEscapePercentage + " | S escaped percentage: : " +
                             (object)sEscapePercentage;
                GameCore.Console.AddLog(str, Color.gray);
                ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
                yield return Timing.WaitForSeconds(1.5f);
                int roundCd = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
                if ((UnityEngine.Object)roundSummary != (UnityEngine.Object)null)
                  roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, newList, leadingTeam,
                    RoundSummary.EscapedClassD, RoundSummary.EscapedScientists, RoundSummary.KilledBySCPs, roundCd,
                    (int)RoundStart.RoundLength.TotalSeconds);
                yield return Timing.WaitForSeconds((float)(roundCd - 1));
                roundSummary.RpcDimScreen();
                yield return Timing.WaitForSeconds(1f);
                RoundRestart.InitiateRoundRestart();
              }
            }
          }
        }
    }
#endif
}
}