using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.QuerySystem.WS;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CedMod.QuerySystem
{
    public class QuerySystem : Plugin<Config>
    {
        public MapEvents MapEvents;
        public ServerEvents ServerEvents;
        public PlayerEvents PlayerEvents;
        public static Harmony Harmony;
        public static List<string> ReservedSlotUserids = new List<string>();

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Default;

        /// <inheritdoc/>
        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-WebAPI";

        public override string Prefix { get; } = "cm_WAPI";

        public static QuerySystem Singleton;

        public static string PanelUrl = "communitymanagementpanel.cedmod.nl";
        
        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(3, 0, 0);

        public override void OnDisabled()
        {
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher != null)
                Object.Destroy(dispatcher);
            Harmony.UnpatchAll();
            WebSocketSystem.Stop();

            Exiled.Events.Handlers.Map.Decontaminating -= MapEvents.OnDecon;
            Exiled.Events.Handlers.Warhead.Starting -= MapEvents.OnWarheadStart;
            Exiled.Events.Handlers.Warhead.Stopping -= MapEvents.OnWarheadCancelled;
            Exiled.Events.Handlers.Warhead.Detonated -= MapEvents.OnWarheadDetonation;
            
            Exiled.Events.Handlers.Server.WaitingForPlayers -= ServerEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= ServerEvents.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerEvents.OnRoundEnd;
            Exiled.Events.Handlers.Server.RespawningTeam -= ServerEvents.OnRespawn;
            Exiled.Events.Handlers.Server.ReportingCheater -= ServerEvents.OnCheaterReport;
            Exiled.Events.Handlers.Server.LocalReporting -= ServerEvents.OnReport;

            Exiled.Events.Handlers.Player.ItemUsed -= PlayerEvents.OnUsedItem;
            Exiled.Events.Handlers.Scp079.InteractingTesla -= PlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= PlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= PlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem -= PlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Hurting -= PlayerEvents.OnPlayerHurt;
            Exiled.Events.Handlers.Player.Dying -= PlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.InteractingElevator -= PlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing -= PlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= PlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified -= PlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left -= PlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole -= PlayerEvents.OnSetClass;

            MapEvents = null;
            ServerEvents = null;
            PlayerEvents = null;
            Singleton = null;
            base.OnDisabled();
        }

        public static string SecurityKey;

        public override void OnEnabled()
        {
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher == null)
                CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();
            Singleton = this;
            
            if (SecurityKey != "None")
            {
                // Start the HTTP server.
                Task.Factory.StartNew(() =>
                {
                    WebSocketSystem.Start();
                });
            }
            else
                Log.Warn("security_key is set to none plugin will not load due to security risks");

            Harmony = new Harmony("com.cedmod.querysystem");
            Harmony.PatchAll();
            
            MapEvents = new MapEvents();
            ServerEvents = new ServerEvents();
            PlayerEvents = new PlayerEvents();

            Exiled.Events.Handlers.Map.Decontaminating += MapEvents.OnDecon;
            Exiled.Events.Handlers.Warhead.Starting += MapEvents.OnWarheadStart;
            Exiled.Events.Handlers.Warhead.Stopping += MapEvents.OnWarheadCancelled;
            Exiled.Events.Handlers.Warhead.Detonated += MapEvents.OnWarheadDetonation;
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += ServerEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += ServerEvents.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += ServerEvents.OnRoundEnd;
            Exiled.Events.Handlers.Server.RespawningTeam += ServerEvents.OnRespawn;
            Exiled.Events.Handlers.Server.ReportingCheater += ServerEvents.OnCheaterReport;
            Exiled.Events.Handlers.Server.LocalReporting += ServerEvents.OnReport;

            Exiled.Events.Handlers.Player.ItemUsed += PlayerEvents.OnUsedItem;
            Exiled.Events.Handlers.Scp079.InteractingTesla += PlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += PlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += PlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem += PlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Hurting += PlayerEvents.OnPlayerHurt;
            Exiled.Events.Handlers.Player.Dying += PlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.InteractingElevator += PlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing += PlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += PlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified += PlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left += PlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole += PlayerEvents.OnSetClass;
            base.OnEnabled();
        }
    }
}