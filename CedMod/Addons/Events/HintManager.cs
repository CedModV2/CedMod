using System;
using System.Collections.Generic;
using LabApi.Features.Wrappers;
using UnityEngine;

namespace CedMod.Addons.Events
{
    public class HintManager: MonoBehaviour
    {
        public float hintTimer = 0;
        public float hintRefreshRate = 1;
        public static List<Func<Player, string>> HintProcessed = new List<Func<Player, string>>();
        public Player player;
        public string currentHint = "";
        private bool wipe = false;
        
        public void Awake()
        {
            player = Player.Get(this.GetComponent<ReferenceHub>());
            LabApi.Features.Console.Logger.Info($"Initialized HintManager for {player.Nickname}");
            currentHint = "\n\n<size=25><color=yellow>" +
                          "This server is currently running an event:" +
                          $"\n{EventManager.CurrentEvent.EventName} By {EventManager.CurrentEvent.EvenAuthor}" +
                          $"\n" +
                          $"\n{EventManager.CurrentEvent.EventDescription}" +
                          $"</color></size>";
        }

        public void Update()
        {
            if (!wipe && RoundSummary.RoundInProgress())
            {
                wipe = true;
                currentHint = "";
            }
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0)
            {
                hintTimer = hintRefreshRate - 0.2f;
                string hintAddition = "";
                foreach (var callback in HintProcessed)
                {
                    hintAddition += callback.Invoke(player);
                }

                if (HintProcessed.Count >= 1)
                {
                    if (!string.IsNullOrEmpty(hintAddition))
                        currentHint = hintAddition;

                    if (string.IsNullOrEmpty(hintAddition))
                        currentHint = "";
                }
                
                if (CedModMain.Singleton.Config.EventManager.Debug)
                    LabApi.Features.Console.Logger.Debug($"Hint display: {currentHint}");
                
                if (!string.IsNullOrEmpty(currentHint))
                    player.ReceiveHint(currentHint, hintRefreshRate);
            }
        }
    }
}