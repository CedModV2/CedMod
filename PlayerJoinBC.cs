using System;
using System.Collections.Generic;
using System.Linq;
using EXILED;
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
            CharacterClassManager component = ev.Player.characterClassManager;
            if (!component.isLocalPlayer)
            {
                Initializer.logger.Info("PlayerJoin", string.Concat(new string[]
                {
                        "Player joined: ",
                        component.GetComponent<NicknameSync>().MyNick,
                        " ",
                        component.GetComponent<CharacterClassManager>().UserId,
                        Environment.NewLine,
                        "from IP: ",
                        component.GetComponent<CharacterClassManager>().RequestIp
                   }));
                if (GameCore.ConfigFile.ServerConfig.GetBool("cm_playerjoinbcenable", true))
                {
                    foreach (string text in GameCore.ConfigFile.ServerConfig.GetStringList("cm_playerjoinbc"))
                    {
                        string[] array = text.Split(new char[]
                        {
                                ':'
                        });
                        string text2 = array[0];
                        uint time = (uint)Convert.ToUInt16(array[1]);
                        Initializer.logger.Debug("PlayerJoin", "Raw Broadcast message: " + text2);
                        foreach (string text3 in this.EnclosedStrings(text, "$S[", "]"))
                        {
                            text2 = text2.Replace("$S[" + text3 + "]", GameCore.ConfigFile.ServerConfig.GetString(text3, "-"));
                        }
                        foreach (string text4 in this.EnclosedStrings(text, "$I[", "]"))
                        {
                            text2 = text2.Replace("$I[" + text4 + "]", string.Concat(GameCore.ConfigFile.ServerConfig.GetInt(text4, 0)));
                        }
                        foreach (string text5 in this.EnclosedStrings(text, "$F[", "]"))
                        {
                            text2 = text2.Replace("$F[" + text5 + "]", string.Concat(GameCore.ConfigFile.ServerConfig.GetFloat(text5, 0f)));
                        }
                        foreach (string text6 in this.EnclosedStrings(text, "$B[", "]"))
                        {
                            text2 = text2.Replace("$B[" + text6 + "]", GameCore.ConfigFile.ServerConfig.GetBool(text6, false).ToString() ?? "");
                        }
                        text2 = text2.Replace("$curPlayerCount", (Convert.ToInt16(PlayerManager.players.Max())).ToString());
                        Initializer.logger.Debug("PlayerJoin", "Broadcasted message: " + text2);
                        Initializer.logger.Debug("PlayerJoin", "With Duration: " + time.ToString());
                        Initializer.logger.Debug("PlayerJoin", "To player: " + component.GetComponent<NicknameSync>().MyNick);
                        RemoteAdmin.QueryProcessor.Localplayer.GetComponent<Broadcast>().TargetAddElement(component.gameObject.GetComponent<NetworkIdentity>().connectionToClient, text2, time, GameCore.ConfigFile.ServerConfig.GetBool("cm_playerjoinbcmono", false));
                    }
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
