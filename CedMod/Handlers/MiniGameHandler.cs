using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.INIT;
using Exiled.Events.EventArgs;
using GameCore;
using Grenades;
using HarmonyLib;
using MEC;
using Mirror;
using UnityEngine;
using Console = System.Console;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace CedMod.Handlers
{
    public static class MiniGameHandler
    {
        public static IEnumerator<float> Playerjoinhandle(JoinedEventArgs ev)
        {
            yield return Timing.WaitForSeconds(0.5f);
            if (RoundSummary.roundTime == 0)
            {
                if (RemainingSecondSlots != 0)
                {
                    ev.Player.SetRole(SelectedGame.SecondRoleType);
                    yield return Timing.WaitForSeconds(0.3f);
                    ev.Player.Position = CharacterClassManager._spawnpointManager.GetRandomPosition(RoleType.Tutorial).transform.position;
                    RemainingSecondSlots--;
                }
                else
                {
                    ev.Player.SetRole(SelectedGame.PlayerRole);
                    yield return Timing.WaitForSeconds(0.3f);
                    ev.Player.Position = CharacterClassManager._spawnpointManager.GetRandomPosition(RoleType.Tutorial).transform.position;
                }
            }
            yield return 1f;
        }
        public enum PreRoundGameType
        {
            Peanut = 0,
            Hounds = 1
        }
        public static PreGame SelectedGame;
        public static int RemainingSecondSlots;
        public class PreGame
        {
            public PreGame (RoleType playerroles, RoleType secondroletype, int ammount, PreRoundGameType gametype)
            {
                PlayerRole = playerroles;
                SecondRoleType = secondroletype;
                Ammount = ammount;
                GameType = gametype;
            }
            public RoleType PlayerRole;
            public RoleType SecondRoleType;
            public int Ammount;
            public PreRoundGameType GameType;

        }
        public static List<PreGame> Games()
        {
            return new List<PreGame>
            {
                //peanut
                new PreGame(RoleType.Tutorial, RoleType.Scp173, 3, PreRoundGameType.Peanut),
                
                //hounds
                new PreGame(RoleType.Tutorial, RoleType.Scp93953, 3, PreRoundGameType.Hounds),
            };
        }
        public static IEnumerator<float> WaitingForPlayers()
        {
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                try
                {
                    GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                    var rnd = new System.Random();
                    PreRoundGameType GameType =
                        (PreRoundGameType) rnd.Next(Enum.GetNames(typeof(PreRoundGameType)).Length);
                    Initializer.Logger.Debug("PreRoundMiniGame", "Minigame chosen: " + GameType);
                    foreach (PreGame wk in Games())
                    {
                        if (wk.GameType == GameType)
                        {
                            RemainingSecondSlots = wk.Ammount;
                            SelectedGame = wk;
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Initializer.Logger.LogException(e, "CedMod", "PreRoundMinigameHandler.WaitingForPlayers");
                    Initializer.Logger.Error("PreRoundMinigame", e.StackTrace);
                }
            }

            yield return 0f;
        }

        public static void RoundStart()
        {
            
        }

        public static void Pickup(PickingUpItemEventArgs ev)
        {
            
        }

        [HarmonyPatch(typeof(Pickup), nameof(global::Pickup.LateUpdate))]
        public static class FloatingItemsPatch
        {
            
        }
    }
}