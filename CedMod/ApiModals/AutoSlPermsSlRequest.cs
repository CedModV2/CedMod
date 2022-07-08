using System.Collections.Generic;

namespace CedMod.ApiModals
{
    /// <summary>
    /// Represents an AutoSlPerms config.
    /// </summary>
    public class AutoSlPermsSlRequest
    {
        /// <summary>
        /// The list of <see cref="SLPermissionEntry"/>.
        /// </summary>
        public List<SLPermissionEntry> PermissionEntries { get; set; } = new List<SLPermissionEntry>();
        /// <summary>
        /// The list of <see cref="AutoSlPermissionMembers"/>.
        /// </summary>
        public List<AutoSlPermissionMembers> MembersList { get; set; } = new List<AutoSlPermissionMembers>();
    }

    /// <summary>
    /// Represents an AutoSLPerms player.
    /// </summary>
    public struct AutoSlPermissionMembers
    {
        /// <summary>
        /// The players UserID.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The players group.
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// Whether or not the player has a reserved slot.
        /// </summary>
        public bool ReservedSlot { get; set; }
    }
    
    /// <summary>
    /// Represents an AutoSLPerms permission entry.
    /// </summary>
    public struct SLPermissionEntry
    {
        /// <summary>
        /// The entries name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The entries kick power.
        /// </summary>
        public int KickPower { get; set; }
        /// <summary>
        /// The kick power required to kick a member of this entry.
        /// </summary>
        public int RequiredKickPower { get; set; }
        /// <summary>
        /// Whether or not this entries badge is hidden.
        /// </summary>
        public bool Hidden { get; set; }
        /// <summary>
        /// Whether or not this entries badge will cover other badges.
        /// </summary>
        public bool Cover { get; set; }
        /// <summary>
        /// Whether or not this entry gives an reserved slot.
        /// </summary>
        public bool ReservedSlot { get; set; }
        /// <summary>
        /// The entries badge text.
        /// </summary>
        public string BadgeText { get; set; }
        /// <summary>
        /// The entries badge color.
        /// </summary>
        public string BadgeColor { get; set; }
        /// <summary>
        /// The entries roleID.
        /// </summary>
        public ulong RoleId { get; set; }
        /// <summary>
        /// The entries permissions.
        /// </summary>
        public PlayerPermissions Permissions { get; set; }
        /// <summary>
        /// The entries Exiled permissions.
        /// </summary>
        public List<string> ExiledPermissions { get; set; }
    }
}