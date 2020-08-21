using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;
using UnityEngine;

namespace CedMod.SurvivalOfTheFittest
{
    /// <summary>
    /// The example plugin.
    /// </summary>
    public class CedModMain : Plugin<Config>
    {
        public static List<ItemType> items = new List<ItemType>();
        private EventHandlers handler;
        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.First;

        /// <inheritdoc/>
        public static Config config;
        
        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-SurvivalOfTheFittest";

        public override string Prefix { get; } = "cm-soft";

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            RegisterEvents();
            Initializer.Logger.Info("CedMod-SurvivalOfTheFittest", "Plugin has loaded");
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// Registers the plugin events.
        /// </summary>
        private void RegisterEvents()
        {
            handler = new EventHandlers();
            Exiled.Events.Handlers.Player.Joined += handler.OnJoin;
            Exiled.Events.Handlers.Server.RoundStarted += handler.OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound += handler.OnRoundRestart;
            Exiled.Events.Handlers.Server.RestartingRound += handler.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += handler.OnEndingRound;
            Exiled.Events.Handlers.Player.TriggeringTesla += handler.TriggerTesla;
            Exiled.Events.Handlers.Server.RespawningTeam += handler.Respawn;
            Exiled.Events.Handlers.Player.Hurting += handler.OnDamage;
        }

        /// <summary>
        /// Unregisters the plugin events.
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Player.Joined -= handler.OnJoin;
            Exiled.Events.Handlers.Server.RoundStarted -= handler.OnRoundStart;
            Exiled.Events.Handlers.Server.RestartingRound -= handler.OnRoundRestart;
            Exiled.Events.Handlers.Server.RestartingRound -= handler.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= handler.OnEndingRound;
            Exiled.Events.Handlers.Player.TriggeringTesla -= handler.TriggerTesla;
            Exiled.Events.Handlers.Server.RespawningTeam -= handler.Respawn;
            Exiled.Events.Handlers.Player.Hurting -= handler.OnDamage;
            handler = null;
        }
    }
}