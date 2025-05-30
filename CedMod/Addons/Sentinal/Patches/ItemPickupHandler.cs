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
using LabApi.Features.Wrappers;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using UnityEngine;
using Locker = MapGeneration.Distributors.Locker;
using LockerChamber = MapGeneration.Distributors.LockerChamber;
using Logger = LabApi.Features.Console.Logger;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace CedMod.Addons.Sentinal.Patches
{
    public class ItemPickupHandler: CustomEventsHandler
    {
        static RaycastHit[] raycastHits = new RaycastHit[50];
        public override void OnPlayerPickingUpItem(PlayerPickingUpItemEventArgs ev)
        {
            if (!RaycastPickup(ev.Player, ev.Pickup))
            {
                ev.IsAllowed = false;
                var info = ev.Pickup.Base.NetworkInfo;
                info.InUse = false;
                ev.Pickup.Base.NetworkInfo = info;
            }
        }
        
        public override void OnPlayerPickingUpAmmo(PlayerPickingUpAmmoEventArgs ev)
        {
            if (!RaycastPickup(ev.Player, ev.AmmoPickup))
            {
                ev.IsAllowed = false;
                var info = ev.AmmoPickup.Base.NetworkInfo;
                info.InUse = false;
                ev.AmmoPickup.Base.NetworkInfo = info;
            }
        }
        
        public override void OnPlayerPickingUpArmor(PlayerPickingUpArmorEventArgs ev)
        {
            if (!RaycastPickup(ev.Player, ev.BodyArmorPickup))
            {
                ev.IsAllowed = false;
                var info = ev.BodyArmorPickup.Base.NetworkInfo;
                info.InUse = false;
                ev.BodyArmorPickup.Base.NetworkInfo = info;
            }
        }
        

        public bool RaycastPickup(Player plr, Pickup pickup)
        {
            //Logger.Info("Testing pickup event");
            if (plr.IsNoclipEnabled || pickup.Base.transform.localScale != Vector3.one)
                return true;
            
            //Logger.Info("Tested pickup event");

            var colls = pickup.GameObject.GetComponentsInChildren<Renderer>();
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
            var plrRoom = plr.Room;

            List<object> metaData = new List<object>();
            foreach (var vector in raycastVectors)
            {
                var dir = (vector - plr.ReferenceHub.PlayerCameraReference.position).normalized;
                var origin = plr.ReferenceHub.PlayerCameraReference.position - dir * 0.2f;
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
                    
                    metaData.Add(new
                    {
                        HitDist = hitDist,
                        EstDist = estPos,
                        HitObject = hit.collider.name,
                        HitPost = hit.point.ToString(),
                        TargetPost = vector.ToString(),
                        LocalHit = plrRoom != null ? plrRoom.Transform.InverseTransformPoint(hit.point).ToString() : "",
                        LocalVector =  plrRoom != null ? plrRoom.Transform.InverseTransformPoint(vector).ToString() : ""
                    });
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
                        { "UserId", plr.UserId },
                        { "ItemType", pickup.Base.ItemId.ToString()},
                        { "Room", plrRoom != null ? plrRoom.Base.name : "" },
                        { "ItemPos", pickup.Transform.position.ToString() },
                        { "LocalItemPos", plrRoom != null ? plrRoom.Transform.InverseTransformPoint(pickup.Transform.position).ToString() : "" },
                        { "CamPos", plr.ReferenceHub.PlayerCameraReference.position.ToString() },
                        { "LocalCamPos", plrRoom != null ? plrRoom.Transform.InverseTransformPoint(plr.ReferenceHub.PlayerCameraReference.position).ToString() : "" },
                        { "RaysMissed", raysMissed.ToString() },
                        { "TotalRays", totalRays.ToString() },
                        { "Ping", (LiteNetLib4MirrorServer.Peers[plr.ReferenceHub.connectionToClient.connectionId].Ping * 2).ToString() },
                        { "Meta", JsonConvert.SerializeObject(metaData) }
                    }
                    
                });
                return false;
            }

            return true;
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