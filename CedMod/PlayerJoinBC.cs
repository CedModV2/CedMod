using System;
using CedMod.CedMod.INIT;
using EXILED;
using EXILED.Extensions;

namespace CedMod
{
    public class PlayerJoinBc
    {
        public Plugin Plugin;
        public PlayerJoinBc(Plugin plugin) => Plugin = plugin;
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!ev.Player.characterClassManager.isLocalPlayer)
            {
                Initializer.Logger.Info("PlayerJoin", string.Concat("Player joined: ", ev.Player.nicknameSync.MyNick, " Userid: ", ev.Player.characterClassManager.UserId, " PlayerID: ", ev.Player.GetPlayerId().ToString(), Environment.NewLine, "from IP: ", ev.Player.characterClassManager.RequestIp));
            }
        }
    }
}
