using System;
using System.Collections.Generic;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerStatsSystem;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.Sentinal.Patches.Utilities
{
    public class RoomCache
    {
        public static Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, int>> Rooms = new Dictionary<RoomIdentifier, Dictionary<RoomIdentifier, int>>();
        public static Dictionary<RoomIdentifier, List<Vector3>> GeneralPositions = new Dictionary<RoomIdentifier, List<Vector3>>(); 
        public static float spacing = 2f;

        public static void MapGenerated(MapGeneratedEventArgs ev)
        {
            if (CedModMain.Singleton.Config.CedMod.DisableFakeSyncing)
                return;
            
            Rooms.Clear();
            GeneralPositions.Clear();
            Timing.RunCoroutine(GenerateMap());
        }

        public static IEnumerator<float> GenerateMap()
        {
            yield return Timing.WaitForSeconds(1);
            
            try
            {
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

                    //rest is disabled for now.
                    continue;

                    var bounds = room.WorldspaceBounds;
                    List<Vector3> waypoints = new List<Vector3>();

                    Vector3 min = bounds.min;
                    Vector3 max = bounds.max;
                    float raycastDistance = bounds.size.y + 2.5f;
                    if (raycastDistance >= 30) //high room, ignore
                        continue;

                    for (float x = min.x; x <= max.x; x += spacing)
                    {
                        for (float z = min.z; z <= max.z; z += spacing)
                        {
                            Vector3 origin = new Vector3(x, max.y + 2.5f, z);
                            Timing.CallDelayed(3, () =>
                            {
                                try
                                {
                                    var toy = PrimitiveObjectToy.Create(origin, Quaternion.identity, null, false);
                                    toy.Type = PrimitiveType.Cube;
                                    toy.Color = Color.red;
                                    toy.Spawn();

                                    toy = PrimitiveObjectToy.Create(origin, Quaternion.identity, null, false);
                                    toy.Type = PrimitiveType.Cube;
                                    toy.Color = Color.green;
                                    toy.Spawn();
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Logger.Error(e.ToString());
                                }
                            });

                            if (Physics.Linecast(origin, origin + Vector3.down * raycastDistance, out RaycastHit hit, VisionInformation.VisionLayerMask, QueryTriggerInteraction.Ignore))
                            {
                                waypoints.Add(hit.point);
                            }
                        }
                    }

                    if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    {
                        Logger.Info($"Calculated {waypoints.Count} waypoints for {room.Name} {room.WorldspaceBounds.size.ToString()}");
                    }

                    GeneralPositions[room] = waypoints;
                }

                yield break;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public static bool TryGetNearestWaypoint(RoomIdentifier room, Vector3 receiverPos, Vector3 position, out Vector3 waypoint)
        {
            if (!GeneralPositions.TryGetValue(room, out var positions))
            {
                waypoint = Vector3.zero;
                return false;
            }

            foreach (var point in positions)
            {
                if (Vector3.Distance(point, position) < spacing + 0.1f)
                {
                    waypoint = point;
                    return true;
                }
            }

            waypoint = Vector3.zero;
            return false;
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