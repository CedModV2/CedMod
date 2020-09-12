using System;
using System.Collections.Generic;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using HarmonyLib;
using MEC;

namespace CedMod.LightsPlugin
{
    public class CedModLightsPlugin : Plugin<Config>
    {
        public override PluginPriority Priority { get; } = PluginPriority.Default;
        

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-LightsPlugin";

        public override string Prefix { get; } = "cm_lights";

        public static Config config;
        
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= server.OnRoundRestart;
            Exiled.Events.Handlers.Server.RoundStarted -= server.OnRoundStart;
            Exiled.Events.Handlers.Player.ChangingRole -= server.Onchangerole;
            server = null;
            ServerEventHandler.BlackoutOn = true;
            Timing.KillCoroutines("CMLightsPluginCoroutines");
        }
        public static Harmony harmony;
        private ServerEventHandler server;
        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            config = Config;
            harmony = new Harmony("com.cedmodLightsPlugin.patch");
            harmony.PatchAll();
            Initializer.Logger.Info("CedMod-LightsPlugin", "Plugin has loaded");
            server = new ServerEventHandler();
            Exiled.Events.Handlers.Server.RestartingRound += server.OnRoundRestart;
            Exiled.Events.Handlers.Server.RoundStarted += server.OnRoundStart;
            Exiled.Events.Handlers.Player.ChangingRole += server.Onchangerole;
        }
        public override void OnReloaded()
        {}
    }
}