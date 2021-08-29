using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod
{

    public sealed class Config : IConfig
    {
        [Description("Indicates whether the plugin is enabled or not")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("kick a player if they have a name someone else already has")]
        public bool KickSameName { get; set; } = true;
        
        [Description("API key for the plugin to use, find yours here https://admin.cedmod.nl/Stats")]
        public string CedModApiKey { get; set; } = GameCore.ConfigFile.ServerConfig.GetString("bansystem_apikey", "None");
        
        [Description("If true the CedMod FF Autoban will be used")]
        public bool AutobanEnabled { get; set; } = GameCore.ConfigFile.ServerConfig.GetBool("ffa_enable", false);
        
        [Description("The amount of people a user has to kill before being autobanned")]
        public int AutobanThreshold { get; set; } = GameCore.ConfigFile.ServerConfig.GetInt("ffa_ammountoftkbeforeban", 3);
        
        [Description("The duration of the autoban ban a user will get if the autoban is triggered in MINUTES")]
        public int AutobanDuration { get; set; } = GameCore.ConfigFile.ServerConfig.GetInt("ffa_banduration", 4320);
        
        [Description("The ban reason of the ban a user gets if the autoban is triggered")]
        public string AutobanReason { get; set; } = GameCore.ConfigFile.ServerConfig.GetString("ffa_banreason", "You have teamkilled too many people");
        
        [Description("If the autoban will count killing disarmed class D as teamkill")]
        public bool AutobanDisarmedClassDTk { get; set; } = !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedclassdsallowed", false);
        
        [Description("If the autoban will count killing disarmed scientists as teamkill")]
        public bool AutobanDisarmedScientistDTk { get; set; } = !GameCore.ConfigFile.ServerConfig.GetBool("ffa_killingdisarmedscientistallowed", false);
        
        [Description("If the autoban will count class D vs class D as teamkill")]
        public bool AutobanClassDvsClassD { get; set; } = !GameCore.ConfigFile.ServerConfig.GetBool("ffa_dclassvsdclasstk", false);
        
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
    }
}