﻿namespace CedMod.LightsPlugin
{
    using System.ComponentModel;
    using Exiled.API.Interfaces;

    /// <inheritdoc cref="IConfig"/>
    public sealed class Config : IConfig
    {
        /// <inheritdoc/>
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;

        [Description("The maximum time that has to pass before a blackout can happen")]
        public float SpawnChance { get; set; } = 45f;
        
        [Description("The maximum time that has to pass before a blackout can happen")]
        public float BlackoutWaitMax { get; set; } = 360f;
        
        [Description("The minimum time that has to pass before a blackout can happen")]
        public float BlackoutWaitMin { get; set; } = 180f;
        
        [Description("The maximum time a blackout can last")]
        public float BlackoutDurationMax { get; set; } = 120f;
        
        [Description("The minimum time a blackout has to last")]
        public float BlackoutDurationMin { get; set; } = 40f;
        
        [Description("If blackouts happen when 173 is ingame (default value recommended)")]
        public bool BlackoutWhen173Ingame { get; set; } = false;

        [Description("The announcement cassie plays when a blackout happens")]
        public string CassieAnnouncementBlackoutStart { get; set; } = "warning JAM_4_10 f pitch_0.1 .g1 .g3 .g4";
        
        [Description("The announcement cassie plays when a blackout stops")]
        public string CassieAnnouncementBlackoutStop { get; set; } = "pitch_0.1 .g1 pitch_0.5 .g2 pitch_0.7 .g3 facility pitch_0.8 .g4 systems pitch_0.9 .g5 now pitch_1 operational";

        [Description("If the cassie announcement uses cassies bells")]
        public bool CassieBells { get; set; } = true;
        
        [Description("If cassie will have issues announcing stuff during a blackout (lower pitch and random noises)")]
        public bool CassieMalfunction { get; set; } = true;
        
        [Description("If people will be given flash lights during a blackout (as it is really dark)")]
        public bool GiveFlashlights { get; set; } = true;
        
        [Description("If people will get a notification that they have been given a flashlight")]
        public bool GiveFlashlightsNotification { get; set; } = true;

    }
}