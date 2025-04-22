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

        [Description("The Hint (message) the teamkilling victim will receive")]
        public int AutobanVictimHint { get; set; } = "<size=25><b><color=yellow>You have been teamkilled by: </color></b></size><color=red><size=25> {attackerName} ({attackerID} {attackerRole} You were a {playerRole}</size></color>\n<size=25><b><color=yellow> Use this as a screenshot as evidence for a report</color></b>\n{AutobanExtraMessage}\n</size><size=25><i><color=yellow> Note: if they continues to teamkill the server will ban them</color></i></size>";
        
        [Description("The Hint (message) the teamkilling perpetrator will receive")]
        public int AutobanPerpetratorHint { get; set; } = "<color=yellow><b> If you continue teamkilling it will result in a ban</b></color>";
        
        [Description("The Hint (message) the teamkilling perpetrator will receive (displaying the user)")]
        public int AutobanPerpetratorHintUser { get; set; } = "<b><color=yellow>You teamkilled: </color></b><color=red> {playerName} </color>";


        [Description("The Hint (message) the teamkilling perpetrator will receive if he has FriendlyFire immunity ")]
        public int AutobanPerpetratorHintImmunity { get; set; } = "<color=#49E1E9><b> You have Friendly Fire Ban Immunity.</b></color>";

        [Description("The Broadcast that will be displayed upon the player getting autobanned")]
        public int AutobanBroadcastMesage { get; set; } = "<size=25><b><color=yellow>user: </color></b><color=red> {attackerName} </color><color=yellow><b> has been automatically banned for teamkilling</b></color></size>";
        
        [Description("The ban reason of the ban a user gets if the autoban is triggered")]
        public string AutobanReason { get; set; } = "You have been automatically banned for teamkilling";
        
        [Description("If the autoban will count pink candy teamkills")]
        public bool AutobanPinkCandies { get; set; } = true;
        
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
        [Description("If Enabled mutes that dont have a duration specified will have this duration, Set to a low value if you wish for ingame mutes to only apply for a small amount of time, Mutes with a duration smaller or equal to 3 minutes (value of 3) will not be synced to the panel.")]
        public long DefaultMuteDuration { get; set; } = 143998560;
        
        [Description("If debug logs are shown")]
        public bool ShowDebug { get; set; } = false;
        
        [Description("If set to false, the plugin will not automatically check for updates (All updates are tested before pushed live)")]
        public bool AutoUpdate { get; set; } = true;
        
        [Description("If set to true, the plugin will softrestart the server at the end of the round if an update is pending (All updates are tested before pushed live)")]
        public bool AutoUpdateRoundEnd { get; set; } = true;
        
        [Description("If an update is pending and the server has been empty for the past X time, the plugin will start the autoupdate process (All updates are tested before pushed live)")]
        public int AutoUpdateWait { get; set; } = 5;

        [Description("If bullet holes should be spawned for a player that is globally muted.")]
        public bool PreventBulletHolesWhenMuted { get; set; } = true;
        
        [Description("If true Ingame reports are enabled, if false the IngameReportDisabledMessage will be shown to the user.")]
        public bool EnableIngameReports { get; set; } = true;
        
        [Description("If EnableIngameReports is set to false this message will be shown to the user.")]
        public string IngameReportDisabledMessage { get; set; } = "Ingame reporting is disabled on this server.";

        [Description("Disables the fakesyncing used to confuse cheater's playerlist by showing players that are out of range as Filmmaker")]
        public bool DisableFakeSyncing { get; set; } = false;

    }
}