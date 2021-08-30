using System;
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
        private Handlers.Server _server;
        private Handlers.Player _player;
        private Harmony _harmony;
        public static CedModMain Singleton;
        
        public override PluginPriority Priority { get; } = PluginPriority.First;

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod";

        public override string Prefix { get; } = "cm";

        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(3, 0, 0);

        public override void OnEnabled()
        {
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

            Singleton = this;
            
            RegisterEvents();
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            _harmony.UnpatchAll();
            Singleton = null;
            UnregisterEvents();
            base.OnDisabled();
        }
        
        private void RegisterEvents()
        {
            _server = new Handlers.Server();
            _player = new Handlers.Player();
            Exiled.Events.Handlers.Server.RestartingRound += _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting += _server.OnReport;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified += _player.OnJoin;
            Exiled.Events.Handlers.Player.Dying += _player.OnDying;
        }
        
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RestartingRound -= _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting -= _server.OnReport;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified -= _player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= _player.OnDying;

            _server = null;
            _player = null;
        }
    }
}