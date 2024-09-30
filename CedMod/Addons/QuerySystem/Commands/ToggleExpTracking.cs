using System;
using CedMod.Addons.QuerySystem.WS;
using CommandSystem;
using LabApi.Features.Permissions;
#if !EXILED
#else
using Exiled.Permissions.Extensions;
#endif

namespace CedMod.Addons.QuerySystem.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TogglExptracking : ICommand
    {
        public string Command { get; } = "toggleexptracking";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Toggles tracking of EXP for this specific server";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.HasPermissions("cedmod.exptoggle"))
            {
                response = "No permission";
                return false;
            }

            if (!WebSocketSystem.HelloMessage.ExpEnabled)
            {
                response = "EXP Tracking is disabled on panel level and cannot be enabled locally";
                return false;
            }
            LevelerStore.TrackingEnabled = !LevelerStore.TrackingEnabled;
            response = $"EXP tracking {(LevelerStore.TrackingEnabled ? "Enabled" : "Disabled")}";
            return true;
        }
    }
}