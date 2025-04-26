using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.Events.Interfaces;
using Hints;
using InventorySystem.Disarming;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerStatsSystem;
using RemoteAdmin;

namespace CedMod
{
    public static class FriendlyFireAutoban
    {
        public static Dictionary<string, int> Teamkillers = new Dictionary<string, int>();
        public static bool AdminDisabled = false;
        public static void HandleKill(Player player, Player attacker, DamageHandlerBase damageHandler)
        {
            if (!RoundSummary.RoundInProgress() || !CedModMain.Singleton.Config.CedMod.AutobanEnabled || AdminDisabled || !IsTeamKill(player, attacker, damageHandler))
                return;
            IFriendlyFireAutoBanBehaviour behaviour = null;
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IFriendlyFireAutoBanBehaviour fireAutoBanBehaviour)
            {
                behaviour = fireAutoBanBehaviour;
            }
            bool attackerHasFFImmunity = new PlayerCommandSender(attacker.ReferenceHub).CheckPermission(new PlayerPermissions[] 
            {
                PlayerPermissions.FriendlyFireDetectorImmunity
            });

            string resultOfTK = attackerHasFFImmunity ? CedModMain.Singleton.Config.CedMod.AutobanPerpetratorHintImmunity : CedModMain.Singleton.Config.CedMod.AutobanPerpetratorHint;
            string ffaTextKiller = $"<size=25>{CedModMain.Singleton.Config.CedMod.AutobanPerpetratorHintUser.Replace("{playerName}", player.Nickname)}\n{resultOfTK}</size>";
            if (behaviour != null)
            {
                var msg = behaviour.KillerMessage(player, attacker, damageHandler);
                if (!string.IsNullOrEmpty(msg))
                    ffaTextKiller = msg;
            }
                
            attacker.ReferenceHub.hints.Show(new TextHint(ffaTextKiller, new HintParameter[] {new StringHintParameter("")}, null, 20f));
            attacker.SendConsoleMessage(ffaTextKiller, "white");
            Dictionary<string, string> autobanReplaceMap = new()
            {
                { "{attackerName}", attacker.Nickname },
                { "{attackerID}", attacker.UserId },
                { "{attackerRole}", attacker.Role.ToString() },
                { "{playerRole}", player.Role.ToString() },
                { "{AutobanExtraMessage}", CedModMain.Singleton.Config.CedMod.AutobanExtraMessage },
            };

            string ffaTextVictim = CedModMain.Singleton.Config.CedMod.AutobanVictimHint;
            foreach (var key in autobanReplaceMap.Keys)
            {
                ffaTextVictim = ffaTextVictim.Replace(key, autobanReplaceMap[key]);
            }

            if (attacker.DoNotTrack)
                ffaTextVictim = ffaTextVictim.Replace(attacker.UserId, "DNT");
            
            if (behaviour != null)
            {
                var msg = behaviour.VictimMessage(player, attacker, damageHandler);
                if (!string.IsNullOrEmpty(msg))
                    ffaTextVictim = msg;
            }
            
            player.ReferenceHub.hints.Show(new TextHint(ffaTextVictim, new HintParameter[] {new StringHintParameter("")}, null, 20f));
            player.SendConsoleMessage(ffaTextVictim, "white");
    
            if (Teamkillers.ContainsKey(attacker.UserId))
                Teamkillers[attacker.UserId]++;
            else
                Teamkillers.Add(attacker.UserId, 1);
            
            foreach (KeyValuePair<string, int> s in Teamkillers)
            {
                if (s.Key == attacker.UserId)
                {
                    var banDuration = TimeSpan.FromMinutes(CedModMain.Singleton.Config.CedMod.AutobanDuration);
                    var banReason = CedModMain.Singleton.Config.CedMod.AutobanReason;
                    int threshold = CedModMain.Singleton.Config.CedMod.AutobanThreshold;
                    bool ignoreImmunity = false;
                    if (behaviour == null || behaviour.ShouldBan(player, attacker, damageHandler, s.Value, out threshold, out ignoreImmunity, out banDuration, out banReason))
                    {
                        if ((!attackerHasFFImmunity || ignoreImmunity) && s.Value >= threshold)
                        {
                            Logger.Info( $"Player: {attacker.Nickname} {attacker.PlayerId.ToString()} {attacker.UserId} exceeded teamkill limit");
                            Task.Run(async () =>
                            {
                                await API.Ban(attacker, (long) banDuration.TotalSeconds, "Server.Module.FriendlyFireAutoban", banReason, false);
                            });
                            Broadcast.Singleton.RpcAddElement(CedModMain.Singleton.Config.CedMod.AutobanBroadcastMesage.Replace("{attackerName}", attacker.Nickname), 20, Broadcast.BroadcastFlags.Normal);
                        }
                    }
                }
            }
        }

        public static bool IsTeamKill(Player player, Player attacker, DamageHandlerBase damageHandler)
        {
            if (!RoundSummary.RoundInProgress() || RoundSummary.singleton.IsRoundEnded)
                return false;
            
            if (EventManager.CurrentEvent != null && EventManager.CurrentEvent is IFriendlyFireAutoBanBehaviour fireAutoBanBehaviour)
            {
                if (!fireAutoBanBehaviour.IsTeamkill(player, attacker, damageHandler))
                    return false;
                return true;
            }
            
            bool result = false;
            if (player == null || attacker == null)
                return false;
            if (attacker == player)
                return false;

            var attackerRole = attacker.Role.GetTeam();

            if (damageHandler is AttackerDamageHandler attackerDamageHandler)
                attackerRole = attackerDamageHandler.Attacker.Role.GetTeam();

            if (attacker.CurrentItem is not null && attacker.CurrentItem.Type == ItemType.SCP330 && !CedModMain.Singleton.Config.CedMod.AutobanPinkCandies)
            {
                return false;
            }
            
            switch (attackerRole)
            {
                case Team.ClassD when player.Role.GetTeam() == Team.ChaosInsurgency:
                case Team.ChaosInsurgency when player.Role.GetTeam() == Team.ClassD:
                case Team.Scientists when player.Role.GetTeam() == Team.FoundationForces:
                case Team.FoundationForces when player.Role.GetTeam() == Team.Scientists:
                case Team.FoundationForces when player.Role.GetTeam() == Team.ClassD && player.ReferenceHub.inventory.IsDisarmed() && CedModMain.Singleton.Config.CedMod.AutobanDisarmedClassDTk:
                case Team.ChaosInsurgency when player.Role.GetTeam() == Team.Scientists && player.ReferenceHub.inventory.IsDisarmed() && CedModMain.Singleton.Config.CedMod.AutobanDisarmedScientistDTk:
                case Team.ClassD when player.Role.GetTeam() == Team.ClassD && CedModMain.Singleton.Config.CedMod.AutobanClassDvsClassD:
                    result = true;
                    break;
                case Team.ClassD when player.Role.GetTeam() == Team.ClassD && !CedModMain.Singleton.Config.CedMod.AutobanClassDvsClassD:
                default:
                    result = false;
                    break;
            }
            if (attacker.Role.GetTeam() == player.Role.GetTeam() && (attacker.Role.GetTeam() != Team.ClassD && player.Role.GetTeam() != Team.ClassD))
                result = true;
            if (attacker.Role == RoleTypeId.Tutorial)
                result = false;

            return result;
        }
    }
}