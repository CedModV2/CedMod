using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using CedMod.INIT;
using UnityEngine;

namespace CedMod
{
    using Exiled.API.Enums;
    using Exiled.API.Features;

    /// <summary>
    /// The example plugin.
    /// </summary>
    public class CedModMain : Plugin<Config>
    {
        public static List<ItemType> items = new List<ItemType>();
        private Handlers.Server server;
        private Handlers.Player player;
        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.First;

        /// <inheritdoc/>
        public static Config config;
        
        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod";

        public override string Prefix { get; } = "cm";

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CedMod.System.Runtime.InteropServices.RuntimeInformation.dll"))
            {
                if (stream == null)
                    throw new InvalidOperationException("Cannot find resource.0");
                using (var reader = new BinaryReader(stream))
                {
                    var rawAssembly = new byte[(int)stream.Length];

                    reader.Read(rawAssembly, 0, (int)stream.Length);
                    var assembly = Assembly.Load(rawAssembly);
                }
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CedMod.Sentry.dll"))
            {
                if (stream == null)
                    throw new InvalidOperationException("Cannot find resource.1");
                using (var reader = new BinaryReader(stream))
                {
                    var rawAssembly = new byte[(int)stream.Length];

                    reader.Read(rawAssembly, 0, (int)stream.Length);
                    var assembly = Assembly.Load(rawAssembly);
                }
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CedMod.Sentry.PlatformAbstractions.dll"))
            {
                if (stream == null)
                    throw new InvalidOperationException("Cannot find resource.2");
                using (var reader = new BinaryReader(stream))
                {
                    var rawAssembly = new byte[(int)stream.Length];

                    reader.Read(rawAssembly, 0, (int)stream.Length);
                    var assembly = Assembly.Load(rawAssembly);
                }
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CedMod.Sentry.Protocol.dll"))
            {
                if (stream == null)
                    throw new InvalidOperationException("Cannot find resource.3");
                using (var reader = new BinaryReader(stream))
                {
                    var rawAssembly = new byte[(int)stream.Length];

                    reader.Read(rawAssembly, 0, (int)stream.Length);
                    var assembly = Assembly.Load(rawAssembly);
                }
            }

            if (!File.Exists(Application.dataPath + "/Managed/Newtonsoft.Json.dll"))
            {
                WebClient wc = new WebClient();
                Initializer.Logger.Error("CEDMOD-INIT", "Dependency missing, downloading...");
                ServicePointManager.ServerCertificateValidationCallback += API.ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                wc.DownloadFile("https://cdn.cedmod.nl/files/Newtonsoft.Json.dll", Application.dataPath + "/Managed/Newtonsoft.Json.dll");
                Application.Quit();
            }
            
            if (!File.Exists(Exiled.API.Features.Paths.Dependencies + "/websocket-sharp.dll"))
            {
                WebClient wc = new WebClient();
                Initializer.Logger.Error("CEDMOD-INIT", "Dependency missing, downloading...");
                ServicePointManager.ServerCertificateValidationCallback += API.ValidateRemoteCertificate;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                wc.DownloadFile("https://cdn.cedmod.nl/files/websocket-sharp.dll", Paths.Dependencies + "/websocket-sharp.dll");
                Application.Quit();
            }
            config = Config;
            items.Add(ItemType.GunProject90);
            items.Add(ItemType.GunMP7);
            items.Add(ItemType.GunCOM15);
            items.Add(ItemType.GunE11SR);
            items.Add(ItemType.GunUSP);

            RegisterEvents();
            Initializer.Setup();
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
            server = new Handlers.Server();
            player = new Handlers.Player();
            Exiled.Events.Handlers.Server.RoundStarted += server.onRoundStart;
            Exiled.Events.Handlers.Server.WaitingForPlayers += server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound += server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting += server.OnReport;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Left += player.OnLeave;
            Exiled.Events.Handlers.Player.Joined += player.OnJoin;
            Exiled.Events.Handlers.Player.Dying += player.OnDying;
            Exiled.Events.Handlers.Player.PickingUpItem += player.OnPickup;
        }

        /// <summary>
        /// Unregisters the plugin events.
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= server.onRoundStart;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting -= server.OnReport;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Left -= player.OnLeave;
            Exiled.Events.Handlers.Player.Joined -= player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= player.OnDying;
            Exiled.Events.Handlers.Player.PickingUpItem += player.OnPickup;

            server = null;
            player = null;
        }
    }
}