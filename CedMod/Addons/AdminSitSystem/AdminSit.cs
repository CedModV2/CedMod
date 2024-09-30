using System;
using System.Collections.Generic;
using AdminToys;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using PlayerRoles;
using UnityEngine;

namespace CedMod.Addons.AdminSitSystem
{
    public class AdminSit
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public AdminSitLocation Location { get; set; }
        public AdminSitType Type { get; set; }
        public string InitialReason { get; set; }
        public long InitialDuration { get; set; }
        public int AssociatedReportId { get; set; }
        public List<AdminSitPlayer> Players { get; set; }
        public List<AdminToyBase> SpawnedObjects { get; set; }
    }
    
    public class AdminSitPlayer
    {
        public string UserId { get; set; }
        public Player Player { get; set; }
        public AdminSitPlayerType PlayerType { get; set; }
        public Dictionary<ushort, ItemBase> Items { get; set; }
        public RoleTypeId Role { get; set; }
        public Vector3 Position { get; set; }
        public float Health { get; set; }
        public Dictionary<ItemType, ushort> Ammo { get; set; }
        public Dictionary<string, Tuple<bool, byte, float>> Effects = new Dictionary<string, Tuple<bool, byte, float>>();
    }
    
    public enum AdminSitPlayerType
    {
        Staff,
        Handler,
        Offender,
        Victim,
        User
    }

    public enum AdminSitType
    {
        Generic,
        Jail,
        BanOffenderOnLeave,
    }
}