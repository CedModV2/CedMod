using System;
using CommandSystem;
using PluginAPI.Core;

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class CmSyncCommandCommand : ICommand
    {
        public string Command { get; } = "cmsync";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "syncs roles for cedmod panel";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (sender.IsPanelUser())
            {
                if (!sender.CheckPermission(PlayerPermissions.SetGroup))
                {
                    response = "No permission";
                    return false;
                }
                if (ServerStatic.PermissionsHandler._members.ContainsKey(CedModPlayer.Get(int.Parse(arguments.At(0))).UserId))
                {
                    response = "User already has a role";
                    return false;
                }

                if (CedModPlayer.Get(int.Parse(arguments.At(0))).UserId != arguments.At(2))
                {
                    response = "UserId mismatch";
                    return false;
                }
                ServerStatic.GetPermissionsHandler()._members[CedModPlayer.Get(int.Parse(arguments.At(0))).UserId] = arguments.At(1);
                CedModPlayer.Get(int.Parse(arguments.At(0))).ReferenceHub.serverRoles.SetGroup(ServerStatic.GetPermissionsHandler()._groups[arguments.At(1)], false);
                CommandHandler.Synced.Add(CedModPlayer.Get(int.Parse(arguments.At(0))).UserId);
                response = "Done.";
                return true;
            }

            response = "This command may only be run by the panel";
            return true;
        }
    }
}