namespace CedMod.ApiModals
{
    public class IngameUserPreferences
    {
        public bool ShowReportsInRemoteAdmin { get; set; }
        public bool ShowWatchListUsersInRemoteAdmin { get; set; } = true;
        public bool ShowModerationInfoSpectator { get; set; }
        public bool ShowModerationInfoOverwatch { get; set; }
        public bool ShowWatchlistInStaffInfo { get; set; }
        public bool StreamerMode { get; set; } = false;
    }
}