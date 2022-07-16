using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace CedMod.Addons.QuerySystem
{
    public sealed class QueryConfig
    {
        [Description("Commands in this list will not be allowed to run via the web API (must be uppercase)")]
        public List<string> DisallowedWebCommands { get; set; } = new List<string> { "REQUEST_DATA AUTH", "GBAN_KICK" };

        [Description("If true, the plugin will sync predefined reasons with the panel")]
        public bool EnableBanreasonSync { get; set; } = true;
        
        [Description("If true, the plugin will automatically enable and setup the External lookup function in remote admin")]
        public bool EnableExternalLookup { get; set; } = true;
        
        [Description("Server full text.")]
        public string ServerFullBase { get; set; } = "Server is full.";
        
        [Description("If true, the plugin will show a custom message when the server is full, promote your patreon reserved slots here :)")]
        public string CustomServerFullMessage { get; set; } = "";
        
        [Description("Users that are not allowed to create reports.")]
        public List<string> ReportBlacklist { get; set; } = new List<string>();
        
        [Description("If players are allowed to report staff members")]
        public bool StaffReportAllowed { get; set; } = false;
        
        [Description("The message that players recieve if they try to report a staff member")]
        public string StaffReportMessage { get; set; } = "You can not report a staff member";
        
        [Description("The message that players recieve when their report has been sent")]
        public string ReportSuccessMessage { get; set; } = "Report has been sent, Server staff will assist you as soon as possible";
        
        [Description("If debug messages are shown")]
        public bool Debug { get; set; } = false;
    }
}