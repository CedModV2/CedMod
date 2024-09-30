using System;
using System.Linq;
using CommandSystem;
using LabApi.Features.Permissions;
using MEC;

namespace CedMod.Addons.AdminSitSystem.Commands.Jail
{
    public class Delete : ICommand
    {
        public string Command { get; } = "delete";

        public string[] Aliases { get; } = {
            "d"
        };

        public string Description { get; } = "Removes all players currently in a jail and deletes the jail.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.HasPermissions("cedmod.jail"))
            {
                response = "no permission";
                return false;
            }
            
            var invoker = CedModPlayer.Get((sender as CommandSender).SenderId);
            if (!AdminSitHandler.Singleton.Sits.Any(s => s.Players.Any(s => s.UserId == invoker.UserId)))
            {
                response = "You are currently not part of any jail.";
                return false;
            }
            
            if (invoker == null)
            {
                response = $"Invoker could not be found.";
                return false;
            }
            
            var sit = AdminSitHandler.Singleton.Sits.FirstOrDefault(s => s.Players.Any(s => s.UserId == invoker.UserId));

            foreach (var plr in sit.Players)
            {
                var cmPlr = CedModPlayer.Get(plr.UserId);
                if (cmPlr == null)
                    continue;
                
                JailParentCommand.RemovePlr(cmPlr, plr, sit);
            }
            
            Timing.CallDelayed(0.5f, () =>
            {
                JailParentCommand.RemoveJail(sit);
            });

            response = "Jail Removed";
            return false;
        }
    }
}