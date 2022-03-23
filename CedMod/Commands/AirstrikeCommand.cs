using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class AirstrikeCommand: ICommand
    {
        public string Command { get; } = "airstrike";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Calls an airstrike";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (arguments.Count <= 1)
            {
                response = "Usage: AIRSTRIKE <delay> <duration>";
                return false;
            }
            Timing.RunCoroutine(AirSupportBomb(Convert.ToInt32(arguments.At(0)), Convert.ToInt32(arguments.At(1))), "airstrike");
            response = "Bombs away";
            return true;
        }
        
        public static void PlayAmbientSound(int id)
        {
            PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 31));
        }
        
        public static bool IsAirBombGoing;

        public static IEnumerator<float> AirSupportBomb(int waitforready = 5, int duration = 30)
        {
            if (IsAirBombGoing)
            {
                yield break;
            }

            IsAirBombGoing = true;
            Cassie.Message("danger . outside zone emergency termination sequence activated .",
                false, true);
            yield return Timing.WaitForSeconds(5f);
            
            while (waitforready > 0)
            {
                PlayAmbientSound(7);
                waitforready--;
                yield return Timing.WaitForSeconds(1f);
            }

            Timing.RunCoroutine(AirSupportstop(duration), "airstrike");
            var throwcount = 0;
            while (IsAirBombGoing)
            {
                var randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
                foreach (var pos in randampos)
                {
                    ExplosiveGrenade grenade = (ExplosiveGrenade)Item.Create(ItemType.GrenadeHE);
                    grenade.FuseTime = 0.1f;
                    grenade.BurnDuration = 10;
                    grenade.DeafenDuration = 10;
                    grenade.ConcussDuration = 10;
                    grenade.SpawnActive(pos, null);
                    yield return Timing.WaitForSeconds(0.1f);
                }

                throwcount++;
                yield return Timing.WaitForSeconds(0.25f);
            }

            Cassie.Message("outside zone termination completed .", false, true);
        }

        public static IEnumerator<float> AirSupportstop(int duration = 30)
        {
            yield return Timing.WaitForSeconds(duration);
            IsAirBombGoing = false;
        }
    }
}