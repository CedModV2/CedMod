using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using GameCore;
using Mirror;
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

        public string Description { get; } = "redirects ALL to the specified port";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
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
            NetworkServer.SendToAll<RoundRestartMessage>(new RoundRestartMessage(RoundRestartType.FullRestart, 1, ushort.Parse(arguments.At(0)), true), 0, false);
            response = "redirecting";
            return true;
        }
    }
}