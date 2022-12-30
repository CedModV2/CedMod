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

        public void Awake()
        {
            player = Player.Get(this.GetComponent<ReferenceHub>());
            Log.Info($"Initialized HintManager for {player.Nickname}");
            currentHint = "<size=25><color=yellow>" +
                          "This server is currently running an event:" +
                          $"\n{EventManager.currentEvent.EventName} By {EventManager.currentEvent.EvenAuthor}" +
                          $"\n" +
                          $"\n{EventManager.currentEvent.EventDescription}" +
                          $"</color></size>";
        }

        public void Update()
        {
            if (hintTimer <= 0)
            {
                hintTimer = hintRefreshRate - 0.2f;
                if (HintProcessed != null)
                {
                    string hintAddition = "";
                    foreach (var callback in HintProcessed)
                    {
                        hintAddition += callback.Invoke(player);
                    }

                    if (!string.IsNullOrEmpty(hintAddition))
                        currentHint = hintAddition;
                }
                
                if (!string.IsNullOrEmpty(currentHint))
                    player.ReceiveHint(currentHint, hintRefreshRate);
            }
        }
    }
}