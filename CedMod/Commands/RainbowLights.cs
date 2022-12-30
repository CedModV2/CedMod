using System;
using CommandSystem;
#if !EXILED
using NWAPIPermissionSystem;
#else
using Exiled.Permissions.Extensions;
#endif
using Object = UnityEngine.Object;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RainbowLightsCommand : ICommand
    {
        public string Command { get; } = "rainbowlights";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "yes";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("cedmod.rainbowlights"))
            {
                response = "no permission";
                return false;
            }
            
            if (arguments.Count < 1)
            {
                response = "missing argument";
                return false;
            }

            bool state;
            if (!bool.TryParse(arguments.At(0), out state))
            {
                response = "Please specify the state";
                return false;
            }
            

            foreach(var light in FlickerableLightController.Instances)
            {
                if (state)
                {
                    if (light.TryGetComponent(out RainbowLight _))
                    {
                        response = "Please remove rainbowlights before trying to re enable it";
                        return false;
                    }
                    light.gameObject.AddComponent<RainbowLight>();
                }
                else
                {
                    if (light.TryGetComponent(out RainbowLight rainbowLight))
                    {
                        light.Network_warheadLightOverride = false;
                        Object.Destroy(rainbowLight);
                    }
                }
            }
            
            response = "Done";
            return true;
        }
    }
}