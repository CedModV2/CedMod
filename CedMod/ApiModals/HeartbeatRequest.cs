using System.Collections.Generic;

namespace CedMod.ApiModals
{
    public class HeartbeatRequest
    {
        public List<PlayerObject> Players { get; set; } //is only sent if playerstats are enabled
        public List<EventModal> Events { get; set; } //is only sent if events are enabled
        public string PluginVersion { get; set; }
        public string PluginCommitHash { get; set; }
        public bool UpdateStats { get; set; }
        public bool TrackingEnabled { get; set; }
        public string ExiledVersion { get; set; }
        public string ScpSlVersion { get; set; }
        public string FileHash { get; set; }
        public string CedModVersionIdentifier { get; set; }
        public string KeyHash { get; set; }
    }
}