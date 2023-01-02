using System.Collections.Generic;
using System.ComponentModel;

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
        
        [Description("Experimental RemoteCommands Feature, if the executing player is not present, the server will spawn a dummy player, run the command, and despawn the dummy")]
        public bool DummyExperimental { get; set; } = false;
        
        [Description("The message show to staff when a new report gets made")]
        public string StaffReportNotification { get; set; } = "<size=30><color=yellow>{reporterName} has reported {reportedName} check {checkType} for more info.</color></size>";
        
        [Description("The message show to staff if they do not have the ingame report in RA enabled")]
        public string StaffReportNotificationIngameDisabled { get; set; } = "<size=25>To be able to view ingame reports in RemoteAdmin, open the CedMod website (External Lookup or navigating directly)\nClick on your user icon, and click Instance Preferences.\nenable RemoteAdminReports</size>";
        
        [Description("The message show to a player when a the state of their report updates")]
        public string PlayerReportUpdateNotification { get; set; } = "<size=30><color=yellow>Your report regarding {reportedName} is now {reportState} by {handlerName}</size>";
        
        [Description("The message show to staff if they do not have the ingame report in RA enabled")]
        public string StaffReportWatchlistIngameDisabled { get; set; } = "<size=25>To be able to view Watchlisted players RemoteAdmin, open the CedMod website (External Lookup or navigating directly)\nClick on your user icon, and click Instance Preferences.\nenable ShowWatchListUsersInRemoteAdmin</size>";
        
        [Description("The message show to staff when a player on the watchlist joins")]
        public string PlayerWatchlistJoin { get; set; } = "<size=25><color=yellow>{playerId} - {playerName} ({userId}) is on the watchlist for: {reason}</size></color>";
        
        [Description("The message show to staff when a player on the Group Watchlist joins")]
        public string PlayerGroupWatchlistJoin { get; set; } = "<size=25><color=yellow>{playerId} - {playerName} ({userId}) is on the Group watchlist:\nGroups: {groups}\nReason: {reason}</size></color>";
    }
}