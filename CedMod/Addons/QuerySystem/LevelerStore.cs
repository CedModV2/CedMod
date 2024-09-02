using System.Collections.Generic;
using PlayerRoles;
using PluginAPI.Core;

namespace CedMod.Addons.QuerySystem
{
    public class LevelerStore
    {
        public static bool TrackingEnabled;
        public static Dictionary<Player, RoleTypeId> InitialPlayerRoles = new Dictionary<Player, RoleTypeId>();
    }
}