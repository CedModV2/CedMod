﻿using System.Collections.Generic;
using CedMod.Addons.Events.Commands;

namespace CedMod.ApiModals
{
    public class HeartbeatRequest
    {
        public List<PlayerObject> Players { get; set; } //is only sent if playerstats are enabled
        public List<EventModal> Events { get; set; } //is only sent if events are enabled
        public string PluginVersion { get; set; }
        public string PluginCommitHash { get; set; }
        public bool UpdateStats { get; set; }
    }
}