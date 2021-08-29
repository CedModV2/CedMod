using System.IO;
using System.Net;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace CedMod
{

    public class CedModMain : Plugin<Config>
    {
        private Handlers.Server server;
        private Handlers.Player player;
        private Harmony _harmony;
        
        public override PluginPriority Priority { get; } = PluginPriority.First;
        
        public static Config config;
        
        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod";

        public override string Prefix { get; } = "cm";

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;

            _harmony = new Harmony("com.cedmod.patch");
            _harmony.PatchAll();
            
            if (!File.Exists(Application.dataPath + "/Managed/Newtonsoft.Json.dll"))
            {
                WebClient wc = new WebClient();
                Log.Error("Dependency missing, downloading...");
                ServicePointManager.ServerCertificateValidationCallback += API.ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                wc.DownloadFile("https://cdn.cedmod.nl/files/Newtonsoft.Json.dll", Application.dataPath + "/Managed/Newtonsoft.Json.dll");
                Application.Quit();
            }
            
            if (!File.Exists(Exiled.API.Features.Paths.Dependencies + "/websocket-sharp.dll"))
            {
                WebClient wc = new WebClient();
                Log.Error("Dependency missing, downloading...");
                ServicePointManager.ServerCertificateValidationCallback += API.ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                wc.DownloadFile("https://cdn.cedmod.nl/files/websocket-sharp.dll", Paths.Dependencies + "/websocket-sharp.dll");
                Application.Quit();
            }
            config = Config;
            
            RegisterEvents();
        }
        
        public override void OnDisabled()
        {
            _harmony.UnpatchAll();
            UnregisterEvents();
        }
        
        private void RegisterEvents()
        {
            server = new Handlers.Server();
            player = new Handlers.Player();
            Exiled.Events.Handlers.Server.RestartingRound += server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting += server.OnReport;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified += player.OnJoin;
            Exiled.Events.Handlers.Player.Dying += player.OnDying;
        }
        
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting -= server.OnReport;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified -= player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= player.OnDying;

            server = null;
            player = null;
        }
    }
}