using System;
using InventorySystem.Items.Firearms.Modules;
using PlayerStatsSystem;
using UnityEngine;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IFriendlyFireAutoBanBehaviour
    {
        string VictimMessage(CedModPlayer player, CedModPlayer attacker, DamageHandlerBase damageHandler);
        string KillerMessage(CedModPlayer player, CedModPlayer attacker, DamageHandlerBase damageHandler);
        bool IsTeamkill(CedModPlayer player, CedModPlayer attacker, DamageHandlerBase damageHandler);
        bool ShouldBan(CedModPlayer player, CedModPlayer attacker, DamageHandlerBase damageHandler, int teamkillAmount, out int cedModAutobanThreshold, out bool ignoreImmunity, out TimeSpan banDuration, out string banReason);
    }
}