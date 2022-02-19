using CedMod.Addons.QuerySystem.WS;
using CommandSystem;

namespace CedMod.Addons.QuerySystem
{
    public static class Extensions
    {
        public static bool IsPanelUser(this ICommandSender sender)
        {
            return sender is CmSender;
        }
    }
}