using System.Collections.Generic;

namespace CedMod.ApiModals
{
    /// <summary>
    /// Represents an player.
    /// </summary>
    public class PlayerObject
    {
        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether or not a player is staff.
        /// </summary>
        public bool Staff { get; set; }
        /// <summary>
        /// Whether or not the player has DNT enabled.
        /// </summary>
        public bool DoNotTrack { get; set; }
        /// <summary>
        /// The players UserID.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The players id.
        /// </summary>
        public int PlayerId { get; set; }
        /// <summary>
        /// The players role.
        /// </summary>
        public RoleType RoleType { get; set; }
    }
}