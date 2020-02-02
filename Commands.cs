using EXILED;
using Mirror;
using RemoteAdmin;
using System.Collections.Generic;
using System;
using UnityEngine;
using MEC;

namespace CedMod
{
    public class Commands
    {
        public Plugin plugin;
        public Commands(Plugin plugin) => this.plugin = plugin;
        public void OnRoundEnd()
        {
            IsEnabled = false;
            Timing.KillCoroutines("LightsOut");
        }
        public void OnCommand(ref RACommandEvent ev)
        {
            string[] Command = ev.Command.Split(new char[]
            {
                ' '
            });
            switch (Command[0].ToUpper())
            {
                case "PBC":
                    ev.Allow = false;
                    if (Command.Length < 4)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#Usage: PBC <PLAYER> <TIME> <MESSAGE>", false, true, "");
                        return;
                    }
                    uint durationPbc;
                    if (!CheckPermissions(ev.Sender, Command[0], PlayerPermissions.Broadcasting, "", true))
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#No perms to PBC bro.", false, true, "");
                        return;
                    }
                    if (!uint.TryParse(Command[2], out durationPbc) || durationPbc < 1u)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#Argument after the name must be a positive integer.", false, true, "");
                        return;
                    }
                    string pbcText = Command[3];
                    for (int i = 4; i < Command.Length; i++)
                    {
                        pbcText = pbcText + " " + Command[i];
                    }
                    foreach (GameObject player in PlayerManager.players)
                    {
                        if (!player.GetComponent<NicknameSync>().MyNick.Contains(Command[1], StringComparison.OrdinalIgnoreCase)) continue;
                        NetworkConnection myConn = player.GetComponent<NetworkIdentity>().connectionToClient;
                        if (myConn == null)
                        {
                            continue;
                        }
                        QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(myConn, pbcText, durationPbc, false);
                        ev.Sender.RaReply(Command[0].ToUpper() + "#Sent: " + pbcText + " to: " + player.GetComponent<NicknameSync>().MyNick, true, true, "");
                        ServerLogs.AddLog(ServerLogs.Modules.DataAccess, "Broadcasted: " + pbcText + " to: " + player.GetComponent<NicknameSync>().MyNick + " by " + ev.Sender.Nickname,
                            ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
                    }
                    ev.Sender.RaReply(Command[0].ToUpper() + "#PBC command sent.", true, true, "");
                    return;
                case "LIGHTSOUT":
                    ev.Allow = false;
                    if (Command.Length < 2)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#Usage: LightsOut <OnlyHeavy>", false, true, "");
                        return;
                    }
                    if (!CheckPermissions(ev.Sender, Command[0], PlayerPermissions.FacilityManagement, "", true))
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#No perms to LightsOut bro.", false, true, "");
                        return;
                    }
                    if (IsEnabled == false)
                    {
                        IsEnabled = true;
                        Timing.RunCoroutine(LightsOut(Convert.ToBoolean(Command[1])), "LightsOut");
                        ev.Sender.RaReply(Command[0].ToUpper() + "#Lights have been turned off", true, true, "");
                        return;
                    }
                    else
                    {
                        if (IsEnabled == true)
                        {
                            IsEnabled = false;
                            Timing.KillCoroutines("LightsOut");
                            ev.Sender.RaReply(Command[0].ToUpper() + "#Lights have been turned on", true, true, "");
                            return;
                        }
                    }
                    ev.Sender.RaReply(Command[0].ToUpper() + "#Something went wrong", false, true, "");
                    return;
                case "DISABLEFFA":
                    ev.Allow = false;
                    if (!CheckPermissions(ev.Sender, Command[0], PlayerPermissions.FacilityManagement, "", true))
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#No perms to DisableFFA bro.", false, true, "");
                        return;
                    }
                    FriendlyFireAutoBan.AdminDisabled = !FriendlyFireAutoBan.AdminDisabled;
                    if (FriendlyFireAutoBan.AdminDisabled)
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#FFA is now Disabled FFA wil reset at round end unless FFA is disabled", true, true, "");
                    }
                    else
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#FFA is now Enabled FFA wil reset at round end unless FFA is disabled", true, true, "");
                    }
                    return;
                case "STUITER":
                    ev.Allow = false;
                    if (!CheckPermissions(ev.Sender, Command[0], PlayerPermissions.FacilityManagement, "", true))
                    {
                        ev.Sender.RaReply(Command[0].ToUpper() + "#No perms to Stuiter bro.", false, true, "");
                        return;
                    }
                    foreach (GameObject player in PlayerManager.players)
                    {
                        CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                        component.SetClassID(RoleType.Tutorial);
                        component.GetComponent<PlayerStats>().health = 100;
                        component.GetComponent<Inventory>().items.Clear();
                        component.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                        component.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                        component.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                    }
                    return;
            }
        }
        public bool IsEnabled = false;
        private IEnumerator<float> LightsOut(bool HeavyOnly)
        {
            Generator079.generators[0].RpcCustomOverchargeForOurBeautifulModCreators(9.5f, HeavyOnly);
            yield return Timing.WaitForSeconds(10f);
            Timing.RunCoroutine(LightsOut(Convert.ToBoolean(HeavyOnly)), "LightsOut");
        }
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
