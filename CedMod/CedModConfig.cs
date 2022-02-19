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
        
        public List<string> ReportBlacklist { get; set; } = new List<string>();
        
        [Description("If players are allowed to report staff members")]
        public bool StaffReportAllowed { get; set; } = false;
        
        [Description("The message that players recieve if they try to report a staff member")]
        public string StaffReportMessage { get; set; } = "You can not report a staff member";
        
        [Description("The message sent trough the ingame reports webhook.")]
        public string ReportMessage { get; set; } = "";
        
        [Description("Additional message when a banned member tries to join or gets banned.")]
        public string AdditionalBanMessage { get; set; } = "";
        
        [Description("If debug logs are shown")]
        public bool ShowDebug { get; set; } = false;
        
        [Description("If dependencies should be automatically downloaded")]
        public bool AutoDownloadDependency { get; set; } = true;
    }
}