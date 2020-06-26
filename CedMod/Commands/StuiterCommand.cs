using System;
using CommandSystem;
using EXILED.Extensions;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class StuiterCommand : ICommand
    {
        public string Command { get; } = "stuiter";

        public string[] Aliases { get; } = new string[]
        {
            "idkwhattonamethis"
        };

        public string Description { get; } = "Does fun stuff";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            switch (arguments.At(0))
            {
                case "all":
                    Cassie.CassieMessage("xmas_bouncyballs", false, false);
                    foreach (GameObject player in PlayerManager.players)
                    {
                        CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                        component.SetClassID(RoleType.Tutorial);
                        component.GetComponent<PlayerStats>().Health = 100;
                        component.GetComponent<Inventory>().items.Clear();
                        component.GetComponent<Inventory>().AddNewItem(ItemType.SCP018);
                        component.GodMode = false;
                    }

                    break;
                default:
                    Cassie.CassieMessage("xmas_bouncyballs", false, false);
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

                    break;
            }

            response = "Stuiter time";
            return true;
        }
    }
}