using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using MEC;
using Object = UnityEngine.Object;

namespace CedMod
{

    public class CedModMain : Plugin<Config>
    {
        private Handlers.Server _server;
        private Handlers.Player _player;
        private Harmony _harmony;
        public static CedModMain Singleton;
        
        public override PluginPriority Priority { get; } = PluginPriority.First;

        public override string Author { get; } = "ced777ric#8321";

        public override string Name { get; } = "CedMod";

        public override string Prefix { get; } = "cm";

        public override Version RequiredExiledVersion { get; } = new Version(5, 0, 0);
        public override Version Version { get; } = new Version(3, 2, 0);
        public static string FileHash { get; set; } = "";

        public static string GitCommitHash = String.Empty;
        public static string VersionIdentifier = String.Empty;

        public override void OnEnabled()
        {
            CosturaUtility.Initialize();

            try
            {
                var file = File.Open(this.GetPath(), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                FileHash = GetHashCode(file, new MD5CryptoServiceProvider());
                file.Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            
            _harmony = new Harmony("com.cedmod.patch");
            _harmony.PatchAll();
            
            using (Stream stream = Assembly.GetManifestResourceStream("CedMod.version.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                GitCommitHash = reader.ReadToEnd();
            }
            
            using (Stream stream = Assembly.GetManifestResourceStream("CedMod.versionIdentifier.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                VersionIdentifier = reader.ReadToEnd();
            }
            
            if (File.Exists(Path.Combine(Paths.Configs, "CedMod", "dev.txt")))
            {
                Log.Info("Plugin running as Dev");
                QuerySystem.CurrentMaster = QuerySystem.DevPanelUrl;
                QuerySystem.PanelUrl = QuerySystem.CurrentMaster;
            }

            Singleton = this;
            
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher == null)
                CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();
            
            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            if (updater == null)
                updater = CustomNetworkManager.singleton.gameObject.AddComponent<AutoUpdater>();

            if (File.Exists(Path.Combine(Paths.Configs, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt")))
            {
                // Start the HTTP server.
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        WebSocketSystem.Start();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                });
            }
            else
                Log.Warn("Plugin is not setup properly, please use refer to the cedmod setup guide"); //todo link guide
            
            _server = new Handlers.Server();
            _player = new Handlers.Player();
            Exiled.Events.Handlers.Server.RestartingRound += _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.RestartingRound += updater.RoundRestart;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified += _player.OnJoin;
            Exiled.Events.Handlers.Player.Dying += _player.OnDying;

            QuerySystem.QueryMapEvents = new QueryMapEvents();
            QuerySystem.QueryServerEvents = new QueryServerEvents();
            QuerySystem.QueryPlayerEvents = new QueryPlayerEvents();

            //Exiled.Events.Handlers.Map.Decontaminating += MapEvents.OnDecon;
            Exiled.Events.Handlers.Warhead.Starting += QuerySystem.QueryMapEvents.OnWarheadStart;
            Exiled.Events.Handlers.Warhead.Stopping += QuerySystem.QueryMapEvents.OnWarheadCancelled;
            Exiled.Events.Handlers.Warhead.Detonated += QuerySystem.QueryMapEvents.OnWarheadDetonation;
            
            Exiled.Events.Handlers.Server.WaitingForPlayers += QuerySystem.QueryServerEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += QuerySystem.QueryServerEvents.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += QuerySystem.QueryServerEvents.OnRoundEnd;
            Exiled.Events.Handlers.Server.RespawningTeam += QuerySystem.QueryServerEvents.OnRespawn;
            Exiled.Events.Handlers.Server.ReportingCheater += QuerySystem.QueryServerEvents.OnCheaterReport;
            Exiled.Events.Handlers.Server.LocalReporting += QuerySystem.QueryServerEvents.OnReport;
            
            Exiled.Events.Handlers.Scp079.InteractingTesla += QuerySystem.QueryPlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += QuerySystem.QueryPlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += QuerySystem.QueryPlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem += QuerySystem.QueryPlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Dying += QuerySystem.QueryPlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.Hurting += QuerySystem.QueryPlayerEvents.OnPlayerHurt;
            Exiled.Events.Handlers.Player.InteractingElevator += QuerySystem.QueryPlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing += QuerySystem.QueryPlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += QuerySystem.QueryPlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified += QuerySystem.QueryPlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left += QuerySystem.QueryPlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole += QuerySystem.QueryPlayerEvents.OnSetClass;
            Exiled.Events.Handlers.Player.Escaping += QuerySystem.QueryPlayerEvents.OnEscape;
            
            EventManager.EventManagerServerEvents = new EventManagerServerEvents();
            EventManager.EventManagerPlayerEvents = new EventManagerPlayerEvents();
            Exiled.Events.Handlers.Server.EndingRound += EventManager.EventManagerServerEvents.EndingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += EventManager.EventManagerServerEvents.WaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded += EventManager.EventManagerServerEvents.EndRound;
            Exiled.Events.Handlers.Server.RestartingRound += EventManager.EventManagerServerEvents.RestartingRound;
            Exiled.Events.Handlers.Player.Joined += EventManager.EventManagerPlayerEvents.OnPlayerJoin;
            
            if (!Directory.Exists(Path.Combine(Paths.Plugins, "CedModEvents")))
            {
                Directory.CreateDirectory(Path.Combine(Paths.Plugins, "CedModEvents"));
            }
            
            if (!Directory.Exists(Path.Combine(Paths.Configs, "CedMod")))
            {
                Directory.CreateDirectory(Path.Combine(Paths.Configs, "CedMod"));
            }
            
            foreach (var file in Directory.GetFiles(Path.Combine(Paths.Plugins, "CedModEvents"), "*.dll"))
            {
                var assembly = Loader.LoadAssembly(file);
                var plugin = Loader.CreatePlugin(assembly);
                if (EventManager.AvailableEventPlugins.Contains(plugin))
                {
                    Log.Error($"Found duplicate Event: {plugin.Name} Located at: {file} Please only have one of each Event installed");
                    continue;
                }
                EventManager.AvailableEventPlugins.Add(plugin);
                try
                {
                    plugin.OnEnabled();
                    plugin.OnRegisteringCommands();
                }
                catch (Exception e)
                {
                    Log.Info($"Failed to load {plugin.Name}.\n{e}");
                    continue;
                }

                foreach (var type in plugin.Assembly.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    Log.Debug($"Checked {plugin.Name} for CedMod-Events functionality, IEvent inherited", Config.EventManager.Debug);
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        Log.Debug($"Checked {plugin.Name} Constructor is null, cannot continue", Config.EventManager.Debug);
                        continue;
                    }
                    else
                    {
                        IEvent @event = constructor.Invoke(null) as IEvent;
                        if (@event == null) 
                            continue;
                        EventManager.AvailableEvents.Add(@event);
                        Log.Info($"Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
                    }
                }
            }

            base.OnEnabled();
        }
        
        public override void OnReloaded()
        {
            Timing.CallDelayed(2, () =>
            {
                ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
                if (dispatcher == null)
                    CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();
                
                AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
                if (updater == null)
                    CustomNetworkManager.singleton.gameObject.AddComponent<AutoUpdater>();
            });
        }
        
        public override void OnDisabled()
        {
            _harmony.UnpatchAll();
            Singleton = null;
            
            Exiled.Events.Handlers.Server.RestartingRound -= _server.OnRoundRestart;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified -= _player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= _player.OnDying;

            _server = null;
            _player = null;
            
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher != null)
                Object.Destroy(dispatcher);
            
            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            Exiled.Events.Handlers.Server.RestartingRound -= updater.RoundRestart;
            if (updater == null)
                Object.Destroy(updater);
            WebSocketSystem.Stop();

            //Exiled.Events.Handlers.Map.Decontaminating -= MapEvents.OnDecon;
            Exiled.Events.Handlers.Warhead.Starting -= QuerySystem.QueryMapEvents.OnWarheadStart;
            Exiled.Events.Handlers.Warhead.Stopping -= QuerySystem.QueryMapEvents.OnWarheadCancelled;
            Exiled.Events.Handlers.Warhead.Detonated -= QuerySystem.QueryMapEvents.OnWarheadDetonation;
            
            Exiled.Events.Handlers.Server.WaitingForPlayers -= QuerySystem.QueryServerEvents.OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= QuerySystem.QueryServerEvents.OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= QuerySystem.QueryServerEvents.OnRoundEnd;
            Exiled.Events.Handlers.Server.RespawningTeam -= QuerySystem.QueryServerEvents.OnRespawn;
            Exiled.Events.Handlers.Server.ReportingCheater -= QuerySystem.QueryServerEvents.OnCheaterReport;
            Exiled.Events.Handlers.Server.LocalReporting -= QuerySystem.QueryServerEvents.OnReport;
            
            Exiled.Events.Handlers.Scp079.InteractingTesla -= QuerySystem.QueryPlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= QuerySystem.QueryPlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= QuerySystem.QueryPlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem -= QuerySystem.QueryPlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Dying -= QuerySystem.QueryPlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.Hurting -= QuerySystem.QueryPlayerEvents.OnPlayerHurt;
            Exiled.Events.Handlers.Player.InteractingElevator -= QuerySystem.QueryPlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing -= QuerySystem.QueryPlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= QuerySystem.QueryPlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified -= QuerySystem.QueryPlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left -= QuerySystem.QueryPlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole -= QuerySystem.QueryPlayerEvents.OnSetClass;
            Exiled.Events.Handlers.Player.Escaping -= QuerySystem.QueryPlayerEvents.OnEscape;

            QuerySystem.QueryMapEvents = null;
            QuerySystem.QueryServerEvents = null;
            QuerySystem.QueryPlayerEvents = null;
            
            Exiled.Events.Handlers.Server.EndingRound -= EventManager.EventManagerServerEvents.EndingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= EventManager.EventManagerServerEvents.WaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded -= EventManager.EventManagerServerEvents.EndRound;
            Exiled.Events.Handlers.Server.RestartingRound -= EventManager.EventManagerServerEvents.RestartingRound;
            Exiled.Events.Handlers.Player.Joined -= EventManager.EventManagerPlayerEvents.OnPlayerJoin;
            EventManager.EventManagerServerEvents = null;
            EventManager.EventManagerPlayerEvents = null;

            foreach (var plugin in EventManager.AvailableEventPlugins)
            {
                try
                {
                    plugin.OnUnregisteringCommands();
                    plugin.OnDisabled();
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to disable GameMode: {plugin}\n{e}");
                }
            }
            
            base.OnDisabled();
        }
        
        internal static string GetHashCode(Stream stream, HashAlgorithm cryptoService)
        {
            using (cryptoService)
            {
                var hash = cryptoService.ComputeHash(stream);
                var hashString = Convert.ToBase64String(hash);
                return hashString.TrimEnd('=');
            }
        }
    }
}