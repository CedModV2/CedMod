using System.Collections.Generic;
using CedMod.INIT;

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
        }

        /// <summary>
        /// Unregisters the plugin events.
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RestartingRound -= server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting -= server.OnReport;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Left -= player.OnLeave;
            Exiled.Events.Handlers.Player.Joined -= player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= player.OnDying;

            server = null;
            player = null;
        }
    }
}