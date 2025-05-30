﻿using System;
using LabApi.Features.Wrappers;
using PlayerStatsSystem;

namespace CedMod.Addons.Events.Interfaces
{
    public interface IFriendlyFireAutoBanBehaviour
    {
        string VictimMessage(Player player, Player attacker, DamageHandlerBase damageHandler);
        string KillerMessage(Player player, Player attacker, DamageHandlerBase damageHandler);
        bool IsTeamkill(Player player, Player attacker, DamageHandlerBase damageHandler);
        bool ShouldBan(Player player, Player attacker, DamageHandlerBase damageHandler, int teamkillAmount, out int cedModAutobanThreshold, out bool ignoreImmunity, out TimeSpan banDuration, out string banReason);
    }
}