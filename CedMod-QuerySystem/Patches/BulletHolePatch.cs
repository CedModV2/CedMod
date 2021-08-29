using System.Collections.Generic;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.QuerySystem.Patches
{
    [HarmonyPatch(typeof(StandardHitregBase), nameof(StandardHitregBase.PlaceBullethole))]
    public static class BulletHolePatch
    {
        public static Dictionary<Player, BulletHoleCreator> HoleCreators = new Dictionary<Player, BulletHoleCreator>();
        
        public static void Prefix(StandardHitregBase __instance, Ray ray, RaycastHit hit)
        {
            Player player = Player.Get(__instance.Hub);
           if (!HoleCreators.ContainsKey(player))
               HoleCreators.Add(player, new BulletHoleCreator());
           if (!HoleCreators[player].Rooms.ContainsKey(player.CurrentRoom))
               HoleCreators[player].Rooms.Add(player.CurrentRoom, new RoomHoles());
           
           HoleCreators[player].Rooms[player.CurrentRoom].Holes.Add(hit.point);
        }
    }

    public class BulletHoleCreator
    {
        public Dictionary<Room, RoomHoles> Rooms = new Dictionary<Room, RoomHoles>();
    }

    public class RoomHoles
    {
        public List<Vector3> Holes = new List<Vector3>();
    }
}