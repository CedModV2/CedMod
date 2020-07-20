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
    }
}