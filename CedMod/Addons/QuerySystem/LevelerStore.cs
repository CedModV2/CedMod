using System.Collections.Generic;
using Exiled.API.Features;

namespace CedMod.Addons.QuerySystem
{
    public class LevelerStore
    {
        public static bool TrackingEnabled;
        public static Dictionary<Player, RoleType> InitialPlayerRoles = new Dictionary<Player, RoleType>();
    }
}