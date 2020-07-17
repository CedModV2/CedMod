using System.Collections.Generic;
using CedMod.FFA;
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
        private BanSystem.BanSystem bansystem;
        private FFA.FriendlyFireAutoBan ffa;

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.First;

        /// <inheritdoc/>

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod";

        public override string Prefix { get; } = "cm";

        public override void OnEnabled()
        {
            if (!Config.IsEnabled)
                return;
            items.Add(ItemType.GunLogicer);
            items.Add(ItemType.GunProject90);
            items.Add(ItemType.GunMP7);
            items.Add(ItemType.GunCOM15);
            items.Add(ItemType.GunE11SR);
            items.Add(ItemType.GunUSP);
            base.OnEnabled();

            RegisterEvents();
            Initializer.Setup();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            base.OnDisabled();

            UnregisterEvents();
        }

        /// <summary>
        /// Registers the plugin events.
        /// </summary>
        private void RegisterEvents()
        {
            server = new Handlers.Server();
            bansystem = new BanSystem.BanSystem();
            ffa = new FriendlyFireAutoBan();
            Exiled.Events.Handlers.Server.WaitingForPlayers += server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.EndingRound += server.OnEndingRound;

            Exiled.Events.Handlers.Player.Joined += bansystem.OnPlayerJoin;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += bansystem.OnCommand;

            Exiled.Events.Handlers.Player.Hurting += ffa.OnHurt;
            Exiled.Events.Handlers.Server.SendingConsoleCommand += ffa.ConsoleCommand;
            
        }

        /// <summary>
        /// Unregisters the plugin events.
        /// </summary>
        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.WaitingForPlayers -= server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.EndingRound -= server.OnEndingRound;

            Exiled.Events.Handlers.Player.Joined -= bansystem.OnPlayerJoin;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= bansystem.OnCommand;
            
            Exiled.Events.Handlers.Player.Hurting -= ffa.OnHurt;
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= ffa.ConsoleCommand;

            server = null;
            bansystem = null;
            ffa = null;
        }
    }
}