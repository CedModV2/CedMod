using System;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using LabApi.Features.Console;

namespace CedMod.Addons.QuerySystem.Patches
{
#if EXILED
    [HarmonyPatch(typeof(Exiled.Permissions.Extensions.Permissions), nameof(Exiled.Permissions.Extensions.Permissions.Reload))]
    public static class RefreshExiledPermissions
    {
        public static bool Prefix()
        {
            if (!WebSocketSystem.UseRa)
            {
                Task.Run(() =>
                {
                    try
                    {
                        WebSocketSystem.ApplyRa(true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Logger.Error(e.ToString());
                    }
                });
                return false;
            }

            return true;
        }
    }
#endif
}