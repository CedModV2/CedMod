using System.Collections.Generic;
using CedMod.FFA;
using Exiled.Events;

namespace CedMod
{
    using Exiled.API.Enums;
    using Exiled.API.Features;

    /// <summary>
    /// The example plugin.
    /// </summary>
    public class CedModMain : Plugin<Config_Main>
    {
        public static List<ItemType> items = new List<ItemType>();
        private Handlers.Server server;
        private Handlers.Player player;
        private BanSystem.BanSystem bansystem;
        private FFA.FriendlyFireAutoBan ffa;

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            items.Add(ItemType.GunLogicer);
            items.Add(ItemType.GunProject90);
            items.Add(ItemType.GunMP7);
            items.Add(ItemType.GunCOM15);
            items.Add(ItemType.GunE11SR);
            items.Add(ItemType.GunUSP);
            base.OnEnabled();

            RegisterEvents();
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
            player = new Handlers.Player();
            bansystem = new BanSystem.BanSystem();
            ffa = new FriendlyFireAutoBan();
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += server.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.EndingRound += server.OnEndingRound;

            Exiled.Events.Handlers.Player.Died += player.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole += player.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem += player.OnChangingItem;
            
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

            Exiled.Events.Handlers.Player.Died -= player.OnDied;
            Exiled.Events.Handlers.Player.ChangingRole -= player.OnChangingRole;
            Exiled.Events.Handlers.Player.ChangingItem -= player.OnChangingItem;
            
            Exiled.Events.Handlers.Player.Joined -= bansystem.OnPlayerJoin;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= bansystem.OnCommand;
            
            Exiled.Events.Handlers.Player.Hurting -= ffa.OnHurt;
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= ffa.ConsoleCommand;

            server = null;
            player = null;
            bansystem = null;
            ffa = null;
        }
    }
}