using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.Events.Interfaces;
using Hints;
using InventorySystem.Disarming;
using PlayerRoles;
using PlayerStatsSystem;
using PluginAPI.Core;
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

            string resultOfTK = attackerHasFFImmunity ? "<color=#49E1E9><b> You have Friendly Fire Ban Immunity.</b></color>" : "<color=yellow><b> If you continue teamkilling it will result in a ban</b></color>";
            string ffaTextKiller = $"<size=25><b><color=yellow>You teamkilled: </color></b><color=red> {player.Nickname} </color>\n{resultOfTK}</size>";
            if (behaviour != null)
            {
                var msg = behaviour.KillerMessage(player, attacker, damageHandler);
                if (!string.IsNullOrEmpty(msg))
                    ffaTextKiller = msg;
            }
                
            attacker.ReferenceHub.hints.Show(new TextHint(ffaTextKiller, new HintParameter[] {new StringHintParameter("")}, null, 20f));
            attacker.SendConsoleMessage(ffaTextKiller, "white");
            string ffaTextVictim = $"<size=25><b><color=yellow>You have been teamkilled by: </color></b></size><color=red><size=25> {attacker.Nickname} ({attacker.UserId} {attacker.Role} You were a {player.Role}</size></color>\n<size=25><b><color=yellow> Use this as a screenshot as evidence for a report</color></b>\n{CedModMain.Singleton.Config.CedMod.AutobanExtraMessage}\n</size><size=25><i><color=yellow> Note: if they continues to teamkill the server will ban them</color></i></size>";
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
                            Log.Info( $"Player: {attacker.Nickname} {attacker.PlayerId.ToString()} {attacker.UserId} exceeded teamkill limit");
                            Task.Run(async () =>
                            {
                                await API.Ban(attacker, (long) banDuration.TotalSeconds, "Server.Module.FriendlyFireAutoban", banReason, false);
                            });
                            Server.SendBroadcast($"<size=25><b><color=yellow>user: </color></b><color=red> {attacker.Nickname} </color><color=yellow><b> has been automatically banned for teamkilling</b></color></size>", 20);
                        }
                    }
                }
            }
        }

        public static bool IsTeamKill(Player player, Player attacker, DamageHandlerBase damageHandler)
        {
            if (!RoundSummary.RoundInProgress())
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

            if (attacker.CurrentItem is not null && attacker.CurrentItem.name == "Scp330(Clone)" && !CedModMain.Singleton.Config.CedMod.AutobanPinkCandies)
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