using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace CedMod.Commands.Stuiter
{
    public class StuiterAllCommand : ICommand
    {
        public string Command { get; } = "all";

        public string[] Aliases { get; } = new string[]
        {
            
        };

        public string Description { get; } = "Does fun stuff";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.stuiter"))
            {
                response = "no permission";
                return false;
            }
            Cassie.Message("xmas_bouncyballs", false, false);
            foreach (Player player in Player.List)
            {
                player.SetRole(RoleType.Tutorial, SpawnReason.ForceClass, false);
                player.Health = 100;
                player.ClearInventory();
                player.AddItem(ItemType.Flashlight);
                player.IsGodModeEnabled = false;
            }

            response = "Stuiter time";
            return true;
        }
    }
}