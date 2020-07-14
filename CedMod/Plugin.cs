using CedMod.FFA;
using Exiled.Events;

namespace CedMod
{
    using Exiled.API.Enums;
    using Exiled.API.Features;

    /// <summary>
    /// The example plugin.
    /// </summary>
    public class CedMod : Plugin<Config>
    {
        private Handlers.Server server;
        private Handlers.Player player;
        private BanSystem.BanSystem bansystem;
        private FFA.FriendlyFireAutoBan ffa;

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Medium;

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            base.OnEnabled();

            RegisterEvents();

            Log.Warn($"I correctly read the string config, its value is: {Config.String}");
            Log.Warn($"I correctly read the int config, its value is: {Config.Int}");
            Log.Warn($"I correctly read the float config, its value is: {Config.Float}");
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

            Exiled.Events.Handlers.Player.Died += ffa.Ondeath;
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
            
            Exiled.Events.Handlers.Player.Died -= ffa.Ondeath;
            Exiled.Events.Handlers.Server.SendingConsoleCommand -= ffa.ConsoleCommand;

            server = null;
            player = null;
            bansystem = null;
            ffa = null;
        }
    }
}