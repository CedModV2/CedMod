using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class AutoSlPermsSlRequest
    {
        public List<SLPermissionEntry> PermissionEntries { get; set; } = new List<SLPermissionEntry>();
        public List<AutoSlPermissionMembers> MembersList { get; set; } = new List<AutoSlPermissionMembers>();
        public List<string> DefaultPermissions { get; set; } = new List<string>();
    }

    public class AutoSlPermissionMembers
    {
        public string UserId { get; set; }
        public string Group { get; set; }
        public bool ReservedSlot { get; set; }
    }
    
    public class SLPermissionEntry
    {
        public string Name { get; set; }
        public int KickPower { get; set; }
        public int RequiredKickPower { get; set; }
        public bool Hidden { get; set; }
        public bool Cover { get; set; }
        public bool ReservedSlot { get; set; }
        public string BadgeText { get; set; }
        public string BadgeColor { get; set; }
        public ulong RoleId { get; set; }
        public PlayerPermissions Permissions { get; set; }
        public List<string> ExiledPermissions { get; set; }
    }
}