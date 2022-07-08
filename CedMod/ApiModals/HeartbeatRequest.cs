using System.Collections.Generic;
using CedMod.Addons.Events.Commands;

namespace CedMod.ApiModals
{
    /// <summary>
    /// Represents an heartbeat packet.
    /// </summary>
    public class HeartbeatRequest
    {
        /// <summary>
        /// The currently connected players. Only sent if playerstats is enabled.
        /// </summary>
        public List<PlayerObject> Players { get; set; }
        public List<EventModal> Events { get; set; } //is only sent if events are enabled
        /// <summary>
        /// The plugin version.
        /// </summary>
        public string PluginVersion { get; set; }
        public string PluginCommitHash { get; set; }
        /// <summary>
        /// Whether or not to update server stats.
        /// </summary>
        public bool UpdateStats { get; set; }
        /// <summary>
        /// Whether or not tracking is enabled.
        /// </summary>
        public bool TrackingEnabled { get; set; }
    }
}