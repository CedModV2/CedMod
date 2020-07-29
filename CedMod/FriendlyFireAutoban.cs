using System;
using System.Collections.Generic;
using Exiled.Events.EventArgs;
using GameCore;
using Hints;

namespace CedMod
{
    public static class FriendlyFireAutoban
    {
        public static Dictionary<string, int> Teamkillers = new Dictionary<string, int>();
        public static bool AdminDisabled = false;
        public static void HandleKill(DyingEventArgs ev)
        {
            if (!RoundSummary.RoundInProgress() && !ConfigFile.ServerConfig.GetBool("ffa_enable") && AdminDisabled)
                return;
            if (ev.Killer == ev.Target)
                return;
            if (!IsTeakill(ev))
                return;
            //more to be done later:tm:
        }

        public static bool IsTeakill(DyingEventArgs ev)
        {
            if (ev.Killer == ev.Target)
                return false;
            if (ev.Killer.Team == ev.Target.Team)
                return true;
            switch (ev.Killer.Team)
            {
                case Team.CDP when ev.Target.Team == Team.CHI:
                case Team.CHI when ev.Target.Team == Team.CDP:
                case Team.RSC when ev.Target.Team == Team.MTF:
                case Team.MTF when ev.Target.Team == Team.RSC:
                case Team.MTF when ev.Target.Team == Team.CDP && ev.Target.IsCuffed && !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed"):
                case Team.CHI when ev.Target.Team == Team.RSC && ev.Target.IsCuffed && !ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed"):
                    return true;
                case Team.CDP when ev.Target.Team == Team.CDP && !ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", true):
                default:
                    return false;
            }
        }
    }
}