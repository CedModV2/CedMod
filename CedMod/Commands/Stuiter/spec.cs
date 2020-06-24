using System;
using CommandSystem;
using EXILED.Extensions;
using UnityEngine;

namespace CedMod.Commands.Stuiter
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class SpecCommand : ICommand
    {
        public string Command { get; } = "spec";
        public string[] Aliases { get; } = new string[]
        {
            "spectator"
        };
        public string Description { get; } = "Does fun stuff";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Cassie.CassieMessage("xmas_bouncyballs", false, false);
            foreach (GameObject player in PlayerManager.players)
            {
                CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                if (component.CurClass == RoleType.Spectator)
                {
                    component.SetClassID(RoleType.Tutorial);
                    component.GetComponent<PlayerStats>()._health = 100;
                    component.GetComponent<Inventory>().items.Clear();
                    component.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                    component.GodMode = false;
                }
            }
            response = "Stuiter time";
            return true;
        }
    }
}