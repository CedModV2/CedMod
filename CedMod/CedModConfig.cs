using System.Collections.Generic;
using System.ComponentModel;

namespace CedMod
{
    public class CedModConfig
    {
        [Description("kick a player if they have a name someone else already has")]
        public bool KickSameName { get; set; } = true;
        
        [Description("API key for the plugin to use, find yours here https://admin.cedmod.nl/ On the Statistics page of your server (Create one if you didnt already)")]
        public string CedModApiKey { get; set; } = "None";
        
        [Description("If true the CedMod FF Autoban will be used")]
        public bool AutobanEnabled { get; set; } = false;
        
        [Description("The amount of people a user has to kill before being autobanned")]
        public int AutobanThreshold { get; set; } = 3;
        
        [Description("The duration of the autoban ban a user will get if the autoban is triggered in MINUTES")]
        public int AutobanDuration { get; set; } = 4320;
        
        [Description("The ban reason of the ban a user gets if the autoban is triggered")]
        public string AutobanReason { get; set; } = "You have been automatically banned for teamkilling";
        
        [Description("If the autoban will count killing disarmed class D as teamkill")]
        public bool AutobanDisarmedClassDTk { get; set; } = true;
        
        [Description("If the autoban will count killing disarmed scientists as teamkill")]
        public bool AutobanDisarmedScientistDTk { get; set; } = true;
        
        [Description("If the autoban will count class D vs class D as teamkill")]
        public bool AutobanClassDvsClassD { get; set; } = true;
        
        [Description("If set the FF Autoban will add this onto the message sent to the victim.")]
        public string AutobanExtraMessage { get; set; } = "";

        [Description("Additional message when a banned member tries to join or gets banned.")]
        public string AdditionalBanMessage { get; set; } = "";
        
        [Description("Message sent to the user when they get muted or join while muted")]
        public string MuteMessage { get; set; } = "You have been {type} muted on this server by an Admin.\nDuration: {duration}\nReason: {reason}";
        
        [Description("Message shown on the players 'CustomInfo' to indicate that they are muted")]
        public string MuteCustomInfo { get; set; } = "{type} muted by an admin.";
        
        [Description("If mutes are required to set and use a Duration and Reason")]
        public bool UseMuteDurationAndReason { get; set; } = false;
        [Description("If mutes will only be possible using the panel")]
        public bool OnlyAllowPanelMutes { get; set; } = false;
        
        [Description("If debug logs are shown")]
        public bool ShowDebug { get; set; } = false;
        
        [Description("If set to false, the plugin will not automatically check for updates (All updates are tested before pushed live)")]
        public bool AutoUpdate { get; set; } = true;
        
        [Description("If set to true, the plugin will softrestart the server at the end of the round if an update is pending (All updates are tested before pushed live)")]
        public bool AutoUpdateRoundEnd { get; set; } = true;
        
        [Description("If an update is pending and the server has been empty for the past X time, the plugin will start the autoupdate process (All updates are tested before pushed live)")]
        public int AutoUpdateWait { get; set; } = 5;
    }
}