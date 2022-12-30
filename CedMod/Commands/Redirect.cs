using System;
using CommandSystem;
using GameCore;
using Mirror;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using RoundRestarting;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RedirectCommand : ICommand
    {
        public string Command { get; } = "redirect";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "redirects ALL players to the specified port";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.redirect"))
            {
                response = "no permission";
                return false;
            }
            if (arguments.IsEmpty())
            {
                response = "you must specify a port";
                return false;
            }
            NetworkServer.SendToAll<RoundRestarting.RoundRestartMessage>(new RoundRestartMessage(RoundRestartType.RedirectRestart, 1f, ushort.Parse(arguments.At(0)), true, false));
            response = "redirecting";
            return true;
        }
    }
}