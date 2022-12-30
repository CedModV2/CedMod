using System.Collections.Generic;
using PlayerRoles;

namespace CedMod.Addons.QuerySystem
{
    public class LevelerStore
    {
        public static bool TrackingEnabled;
        public static Dictionary<CedModPlayer, RoleTypeId> InitialPlayerRoles = new Dictionary<CedModPlayer, RoleTypeId>();
    }
}