using System;
using EXILED;
using MEC;

namespace CedMod
{
    public class CommandsOld
    {
        public Plugin Plugin;
        public CommandsOld(Plugin plugin) => Plugin = plugin;
        public void OnRoundEnd()
        {
            IsEnabled = false;
            Timing.KillCoroutines("LightsOut");
        }
        public void OnCommand(ref RACommandEvent ev)
        {
            string[] command = ev.Command.Split(' ');
            switch (command[0].ToUpper())
            {
                case "DISABLEFFA":
                    ev.Allow = false;
                    if (!CheckPermissions(ev.Sender, command[0], PlayerPermissions.FacilityManagement))
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#No perms to DisableFFA bro.", false, true, "");
                        break;
                    }
                    FriendlyFireAutoBan.AdminDisabled = !FriendlyFireAutoBan.AdminDisabled;
                    if (FriendlyFireAutoBan.AdminDisabled)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#FFA is now Disabled FFA wil reset at round end unless FFA is disabled", true, true, "");
                    }
                    else
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#FFA is now Enabled FFA wil reset at round end unless FFA is disabled", true, true, "");
                    }
                    break;
                case "AIRSTRIKE":
                    ev.Allow = false;
                    if (!CheckPermissions(ev.Sender, command[0], PlayerPermissions.FacilityManagement))
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#No perms to airbomb bro.", false, true, "");
                        break;
                    }
                    if (command.Length < 3)
                    {
                        ev.Sender.RaReply(command[0].ToUpper() + "#Usage: AIRSTRIKE <delay> <duration>", false, true, "");
                        break;
                    }
                    Timing.RunCoroutine(Functions.Coroutines.AirSupportBomb(Convert.ToInt32(command[1]), Convert.ToInt32(command[2])), "airstrike");
                    ev.Sender.RaReply(command[0].ToUpper() + "#Done", true, true, "");
                    break;
            }
        }
        public bool IsEnabled;
        private static bool CheckPermissions(CommandSender sender, string queryZero, PlayerPermissions perm, string replyScreen = "", bool reply = true)
        {
            if (ServerStatic.IsDedicated && sender.FullPermissions)
            {
                return true;
            }
            if (PermissionsHandler.IsPermitted(sender.Permissions, perm))
            {
                return true;
            }
            if (reply)
            {
                sender.RaReply(queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm, false, true, replyScreen);
            }
            return false;
        }
    }
}
