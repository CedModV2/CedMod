using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;

namespace CedMod.Commands.Stuiter
{
    public class StuiterSpecCommand : ICommand
    {
        public string Command { get; } = "spec";

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
                CharacterClassManager component = player.ReferenceHub.characterClassManager;
                if (component.CurClass == RoleType.Spectator)
                {
                    component.SetClassID(RoleType.Tutorial, CharacterClassManager.SpawnReason.ForceClass);
                    player.Health = 100;
                    player.ClearInventory();
                    player.AddItem(ItemType.Flashlight);
                    component.GodMode = false;
                }
            }

            response = "Stuiter time";
            return true;
        }
    }
}