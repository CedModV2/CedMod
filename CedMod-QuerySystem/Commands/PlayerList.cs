using System;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class PlayersCommand : ICommand
    {
        public string Command { get; } = "playerlistcolored";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the list of players for the cedmod panel";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            try
            {
                response = "";
                PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
                string text = "\n";
                bool gameplayData = sender.CheckPermission(PlayerPermissions.GameplayData);
                PlayerCommandSender playerCommandSender2;
                if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
                {
                    playerCommandSender2.Processor.GameplayData = gameplayData;
                }
                
                bool flag2 = sender.CheckPermission(PlayerPermissions.ViewHiddenBadges);
                bool flag3 = sender.CheckPermission(PlayerPermissions.ViewHiddenGlobalBadges);
                if (playerCommandSender != null && playerCommandSender.ServerRoles.Staff)
                {
                    flag2 = true;
                    flag3 = true;
                }

                foreach (ReferenceHub player in ReferenceHub.Hubs.Values)
                {
                    if (player.isLocalPlayer)
                        continue;
                    
                    QueryProcessor component = player.queryProcessor;
                    string text2 = string.Empty;
                    bool flag4 = false;
                    ServerRoles component2 = player.serverRoles;
                    try
                    {
                        if (string.IsNullOrEmpty(component2.HiddenBadge) || (component2.GlobalHidden && flag3) ||
                            (!component2.GlobalHidden && flag2))
                        {
                            text2 = (component2.RaEverywhere
                                ? "[~] "
                                : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty)));
                        }

                        flag4 = component2.OverwatchEnabled;
                    }
                    catch
                    {
                    }

                    text = text + text2 + "(" + component.PlayerId + ") " + player.nicknameSync.CombinedName.Replace("\n", string.Empty) + (flag4 ? "<OVRM>" : string.Empty);
                    CharacterClassManager ccm = player.characterClassManager;
                    text = $"<color={ccm.CurRole.classColor.ToHex()}>" + text + "</color>";


                    text += "\n";
                }
                response = text;
                
            }
            catch (Exception ex2)
            {
                Log.Error(ex2);
                throw;
            }

            return true;
        }
    }
}