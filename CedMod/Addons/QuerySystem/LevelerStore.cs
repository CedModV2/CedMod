using System.Collections.Generic;
using LabApi.Features.Wrappers;
using PlayerRoles;

namespace CedMod.Addons.QuerySystem
{
    public class LevelerStore
    {
        public static bool TrackingEnabled;
        public static Dictionary<Player, RoleTypeId> InitialPlayerRoles = new Dictionary<Player, RoleTypeId>();
    }
}