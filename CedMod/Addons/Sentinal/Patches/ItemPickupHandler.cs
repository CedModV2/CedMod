using System;
using System.Collections.Generic;
using System.Linq;
using AdminToys;
using CedMod.Addons.QuerySystem.WS;
using DrawableLine;
using Interactables;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Searching;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using MapGeneration.Distributors;
using Mirror.LiteNetLib4Mirror;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Addons.Sentinal.Patches
{
    public class ItemPickupHandler: CustomEventsHandler
    {
        static RaycastHit[] raycastHits = new RaycastHit[50];
        
        public override void OnPlayerSearchingPickup(PlayerSearchingPickupEventArgs ev)
        {
            //Logger.Info("Testing pickup event");
            if (ev.Player.IsNoclipEnabled || ev.Pickup.Base.transform.localScale != Vector3.one)
                return;
            
            //Logger.Info("Tested pickup event");

            var colls = ev.Pickup.GameObject.GetComponentsInChildren<Renderer>();
            List<Vector3> raycastVectors = new List<Vector3>();
            foreach (var coll in colls)
            {
                var v = GetBoundPoints(coll.bounds);
                raycastVectors.AddRange(v);
            }
            bool canSee = false;
            bool nothingFound = true;
            int totalRays = raycastVectors.Count;
            int raysMissed = 0;

            foreach (var vector in raycastVectors)
            {
                var dir = (vector - ev.Player.ReferenceHub.PlayerCameraReference.position).normalized;
                var origin = ev.Player.ReferenceHub.PlayerCameraReference.position - dir * 0.2f;
                //DrawableLines.IsDebugModeEnabled = true;
                //Logger.Info("Casting");
                var estPos = Vector3.Distance(origin, vector);
                if (estPos >= 10)
                    continue;
                
                //DrawableLines.GenerateLine(10f, Color.blue, new []{ origin, vector });
                int size = Physics.RaycastNonAlloc(new Ray(origin, dir), raycastHits, estPos, LayerMask.GetMask("Door", "Glass", "Default"));
                if (nothingFound)
                    nothingFound = size == 0;
                Array.Sort(raycastHits, 0, size, new HitDistanceComparer());
                for (int i = 0; i < size; i++)
                {
                    var hit = raycastHits[i];
                    //DrawableLines.GenerateSphere(hit.point, 0.5f, 10f, Color.red);
                    var prim = hit.collider.GetComponentInParent<PrimitiveObjectToy>();
                    if (prim != null && !prim.NetworkPrimitiveFlags.HasFlag(PrimitiveFlags.Collidable))
                        continue;
                    
                    //we ignore lockers as they cant pickup items through locked lockers anyway unless someone was kind enough to open a locker and close it back up again
                    var chamber = hit.collider.GetComponentInParent<LockerChamber>();
                    if (chamber != null && chamber.WasEverOpened)
                        canSee = true;
                    
                    var locker = hit.collider.GetComponentInParent<Locker>();
                    if (locker != null && locker.Chambers.Any(s => s.WasEverOpened))
                        canSee = true;

                    var hitDist = Vector3.Distance(origin, hit.point);
                    if (hitDist + 0.2f >= estPos)
                    {
                        canSee = true;
                    }
                    else
                    {
                        raysMissed++;
                    }
                    //Logger.Info($"Hit: {hit.collider.name} {hit.point.ToString()} dist {hitDist} est was {estPos} {canSee} {nothingFound}");
                    break;
                }
            }

            if (!canSee && !nothingFound)
            {
                WebSocketSystem.Enqueue(new QueryCommand()
                {
                    Recipient = "PANEL",
                    Data = new Dictionary<string, string>()
                    {
                        { "SentinalType", "PickupViolation" }, 
                        { "UserId", ev.Player.UserId },
                        { "ItemType", ev.Pickup.Base.ItemId.ToString()},
                        { "ItemPos", ev.Pickup.Transform.position.ToString() },
                        { "CamPos", ev.Player.ReferenceHub.PlayerCameraReference.position.ToString() },
                        { "RaysMissed", raysMissed.ToString() },
                        { "TotalRays", totalRays.ToString() },
                        { "Ping", (LiteNetLib4MirrorServer.Peers[ev.Player.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() }
                    }
                });
                ev.IsAllowed = false;
            }
        }

        public IEnumerable<Vector3> GetBoundPoints(Bounds bounds)
        {
            yield return bounds.center;
            
            yield return new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
            yield return new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
            yield return new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            yield return new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            yield return new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
            yield return new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
            yield return new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            yield return new Vector3(bounds.max.x, bounds.max.y, bounds.max.z);
        }
        
        class HitDistanceComparer : IComparer<RaycastHit>
        {
            public int Compare(RaycastHit a, RaycastHit b)
            {
                return a.distance.CompareTo(b.distance);
            }
        }
    }
}