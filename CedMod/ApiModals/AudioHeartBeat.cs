using System;
using System.Collections.Generic;
using CedMod.Addons.Events.Commands;

namespace CedMod.ApiModals
{
    public class AudioHeartbeatRequest
    {
        public List<CedModAudioPlayer> Players { get; set; }
    }

    public class CedModAudioPlayer
    {
        public int PlayerId { get; set; }
        public List<string> Queue { get; set; }
        public float Volume { get; set; }
        public bool LoopList { get; set; }
        public bool Continue { get; set; }
        public bool Shuffle { get; set; }
        public TimeSpan CurrentSpan { get; set; }
        public TimeSpan TotalTime { get; set; }
        public bool IsCedModPlayer { get; set; }
    }
}