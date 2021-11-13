using CedMod.QuerySystem.WS;
using CommandSystem;

namespace CedMod.QuerySystem
{
    public static class Extensions
    {
        public static bool IsPanelUser(this ICommandSender sender)
        {
            return sender is CmSender;
        }
    }
}