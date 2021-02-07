using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using UnityEngine;

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
            foreach (GameObject player in PlayerManager.players)
            {
                CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                if (component.CurClass == RoleType.Spectator)
                {
                    component.SetClassID(RoleType.Tutorial);
                    component.GetComponent<PlayerStats>().Health = 100;
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