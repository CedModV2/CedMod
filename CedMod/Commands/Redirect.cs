using System;
using CommandSystem;
using Exiled.Permissions.Extensions;

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

            ReferenceHub._hostHub.playerStats.RpcRoundrestartRedirect(1f, Convert.ToUInt16(arguments.At(0)));
            response = "redirecting";
            return true;
        }
    }
}