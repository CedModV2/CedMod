using System;
using System.Threading;
using HarmonyLib;
using LabApi.Features.Console;
#if EXILED
using Exiled.Events.EventArgs.Player;
using Player = Exiled.API.Features.Player;
#endif

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new Type[] {typeof(ReferenceHub), typeof(ReferenceHub), typeof(string), typeof(long)})]
    public static class BanPatch
    {
        public static bool Prefix(ReferenceHub target, ReferenceHub issuer, string reason, long duration)
        {
            try
            {
                Logger.Info($"MainGame ban patch: banning user. {duration} {reason} {issuer}");
#if EXILED
                var exiledEvent = new BannedEventArgs(Player.Get(target), Player.Get(issuer), new BanDetails()
                {
                    Expires = DateTime.Now.AddSeconds(duration).Ticks,
                    IssuanceTime = DateTime.Now.Ticks,
                    Id = target.authManager.UserId,
                    Issuer = issuer.LoggedNameFromRefHub(),
                    OriginalName = target.nicknameSync.Network_myNickSync,
                    Reason = reason
                }, BanHandler.BanType.UserId, true);
                
                Exiled.Events.Handlers.Player.OnBanned(exiledEvent);
#endif
                new Thread(() =>
                {
                    try
                    {
                        lock (BanSystem.Banlock)
                        {
                            API.Ban(CedModPlayer.Get(target), duration, issuer.LoggedNameFromRefHub(), reason).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                       Logger.Error($"MainGame ban patch failed {ex}");
                    }
                }).Start();
            }
            catch (Exception ex)
            {
                Logger.Error($"MainGame ban patch failed {ex}");
            }

            return false;
        }
    }
}