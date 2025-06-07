using System.Collections.Generic;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MapGeneration;

namespace CedMod.Addons.Sentinal.Patches.Utilities
{
    public class RoomCache
    {
        public static Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, int>> Rooms = new Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, int>>();

        public static void MapGenerated(MapGeneratedEventArgs ev)
        {
            Rooms.Clear();
            List<RoomIdentifier> toCheck = new List<RoomIdentifier>();
            List<RoomIdentifier> toCheck2 = new List<RoomIdentifier>();
            foreach (var room in RoomIdentifier.AllRoomIdentifiers)
            {
                toCheck.Clear();
                toCheck2.Clear();
                Rooms[room] = new Dictionary<RoomIdentifier, int>();
                toCheck.AddRange(room.ConnectedRooms);

                int depth = 1;
                while (depth < 3)
                {
                    toCheck2.Clear();
                    foreach (var check in toCheck)
                    {
                        if (Rooms[room].ContainsKey(check))
                            continue;
                        
                        Rooms[room][check] = depth;
                        toCheck2.AddRange(check.ConnectedRooms);
                    }
                    toCheck.Clear();
                    toCheck.AddRange(toCheck2);
                    depth++;
                }
            }
        }

        public static bool InRange(ReferenceHub plr, ReferenceHub target, int range)
        {
            var room = Player.Get(plr).Room;
            var targetRoom = Player.Get(target).Room;
            if (room == null || targetRoom == null || room == targetRoom || !Rooms.TryGetValue(room.Base, out var cache))
                return true;

            if (targetRoom != room && (room.Name == RoomName.HczCheckpointToEntranceZone || targetRoom.Name == RoomName.HczCheckpointToEntranceZone)) //these are not officially connected apparently
                return true;
            
            if (cache.TryGetValue(targetRoom.Base, out var roomRange) && roomRange <= range)
                return true;
            
            return false;
        }
    }
}