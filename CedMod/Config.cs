﻿namespace CedMod
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <inheritdoc cref="IConfig"/>
    public sealed class Config : IConfig
    {
        /// <inheritdoc/>
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("If CM Badges are enabled ,these badges do not provide RA access of any sort, badgehandles located at: CedMod.BadgeController")]
        public bool CmBadge { get; set; } = true;
        [Description("Message that will be printed above the embed,for things like role pings")]
        public string ReportContent { get; set; } = "A new report has been made";
        [Description("channel where the message will be sent")]
        public ulong ReportChannel { get; set; } = 0;
        
        [Description("kick a player if they have a name someone else already has")]
        public bool KickSameName { get; set; } = true;

        public List<string> ReportBlacklist { get; set; } = new List<string> { "masonic@northwood", "sirmeep@northwood" };
    }
}