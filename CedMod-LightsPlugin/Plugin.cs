using System;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using MEC;

namespace CedMod.LightsPlugin
{
    public class CedModLightsPlugin : Plugin<Config>
    {
        public override PluginPriority Priority { get; } = PluginPriority.Default;
        

        public override string Author { get; } = "ced777ric#8321";

        public override string Name { get; } = "CedMod-LightsPlugin";

        public override string Prefix { get; } = "cm_lights";

        public static CedModLightsPlugin Singleton;
        
        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(3, 0, 0);
        
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.RoundStarted -= _server.OnRoundStart;
            Exiled.Events.Handlers.Player.ChangingRole -= _server.Onchangerole;
            Exiled.Events.Handlers.Server.RoundEnded += _server.OnRoundEnd;
            
            _server = null;
            ServerEventHandler.BlackoutOn = true;
            Timing.KillCoroutines("CMLightsPluginCoroutines");
            Singleton = null;
            base.OnDisabled();
        }
        
        public static Harmony Harmony;
        private ServerEventHandler _server;
        
        public override void OnEnabled()
        {
            Singleton = this;
            Harmony = new Harmony("com.cedmodLightsPlugin.patch");
            Harmony.PatchAll();
            
            _server = new ServerEventHandler();
            Exiled.Events.Handlers.Server.RestartingRound += _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.RoundStarted += _server.OnRoundStart;
            Exiled.Events.Handlers.Player.ChangingRole += _server.Onchangerole;
            Exiled.Events.Handlers.Server.RoundEnded -= _server.OnRoundEnd;
            base.OnEnabled();
        }
    }
}