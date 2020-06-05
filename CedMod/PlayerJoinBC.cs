using System;
using System.Collections.Generic;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using Grenades;
using System.Net;
using MEC;
using RemoteAdmin;
using UnityEngine;
using CedMod.INIT;
using Mirror;

namespace CedMod
{
    public class PlayerJoinBC
    {
        public Plugin plugin;
        public PlayerJoinBC(Plugin plugin) => this.plugin = plugin;
        public void OnPlayerJoin(PlayerJoinEvent ev)
        {
            if (!ev.Player.characterClassManager.isLocalPlayer)
            {
                Initializer.logger.Info("PlayerJoin", string.Concat(new string[]
                {
                        "Player joined: ",
                        ev.Player.nicknameSync.MyNick,
                        " Userid: ",
                        ev.Player.characterClassManager.UserId,
                        " PlayerID: ",
                        ev.Player.GetPlayerId().ToString(),
                        Environment.NewLine,
                        "from IP: ",
                        ev.Player.characterClassManager.RequestIp
                   }));
                bool enabled = false;
                //enabled = GameCore.ConfigFile.ServerConfig.GetBool("cm_playerjoinbcenable", true); currently borked
                if (enabled)
                {
                    
                }
            }
        }
        private IEnumerable<string> EnclosedStrings(string s, string begin, string end)
        {
            int stop;
            for (int i = s.IndexOf(begin, 0); i >= 0; i = s.IndexOf(begin, stop + end.Length))
            {
                int num = i + begin.Length;
                stop = s.IndexOf(end, num);
                if (stop < 0)
                {
                    yield break;
                }
                yield return s.Substring(num, stop - num);
            }
            yield break;
        }
    }
}
