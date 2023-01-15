using System;
using System.Collections.Generic;
using PluginAPI.Core;
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
            Log.Info($"Initialized HintManager for {player.Nickname}");
            currentHint = "\n\n<size=25><color=yellow>" +
                          "This server is currently running an event:" +
                          $"\n{EventManager.currentEvent.EventName} By {EventManager.currentEvent.EvenAuthor}" +
                          $"\n" +
                          $"\n{EventManager.currentEvent.EventDescription}" +
                          $"</color></size>";
        }

        public void Update()
        {
            if (!wipe)
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

                if (!string.IsNullOrEmpty(hintAddition))
                    currentHint = hintAddition;
                
                if (CedModMain.Singleton.Config.EventManager.Debug)
                    Log.Debug($"Hint display: {currentHint}");
                
                if (!string.IsNullOrEmpty(currentHint))
                    player.ReceiveHint(currentHint, hintRefreshRate);
            }
        }
    }
}