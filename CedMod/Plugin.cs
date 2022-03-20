using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CedMod.Addons.Events;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using HarmonyLib;
using UnityEngine;
using Exiled.API.Enums;
using Exiled.API.Features;
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

        public override Version RequiredExiledVersion { get; } = new Version(3, 0, 0);
        public override Version Version { get; } = new Version(3, 1, 0);
        public static string GitCommitHash = String.Empty;

        public override void OnEnabled()
        {
            _harmony = new Harmony("com.cedmod.patch");
            _harmony.PatchAll();
            
            using (Stream stream = Assembly.GetManifestResourceStream("CedMod.version.txt"))
            using (StreamReader reader = new StreamReader(stream))
            {
                GitCommitHash = reader.ReadToEnd();
            }

            if (Config.CedMod.AutoDownloadDependency)
            {
                if (!File.Exists(Application.dataPath + "/Managed/Newtonsoft.Json.dll"))
                {
                    WebClient wc = new WebClient();
                    Log.Error("Dependency missing, downloading...");
                    wc.DownloadFile("https://cdn.cedmod.nl/files/Newtonsoft.Json.dll", Application.dataPath + "/Managed/Newtonsoft.Json.dll");
                    Application.Quit();
                }
            
                if (!File.Exists(Exiled.API.Features.Paths.Dependencies + "/websocket-sharp.dll"))
                {
                    WebClient wc = new WebClient();
                    Log.Error("Dependency missing, downloading...");
                    wc.DownloadFile("https://cdn.cedmod.nl/files/websocket-sharp.dll", Paths.Dependencies + "/websocket-sharp.dll");
                    Application.Quit();
                }
            }

            Singleton = this;
            
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher == null)
                CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();

            if (Config.QuerySystem.SecurityKey != "None")
            {
                // Start the HTTP server.
                Task.Factory.StartNew(() =>
                {
                    WebSocketSystem.Start();
                });
            }
            else
                Log.Warn("security_key is set to none plugin will not load due to security risks");
            
            _server = new Handlers.Server();
            _player = new Handlers.Player();
            Exiled.Events.Handlers.Server.RestartingRound += _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting += _server.OnReport;
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

            Exiled.Events.Handlers.Player.ItemUsed += QuerySystem.QueryPlayerEvents.OnUsedItem;
            Exiled.Events.Handlers.Scp079.InteractingTesla += QuerySystem.QueryPlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension += QuerySystem.QueryPlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension += QuerySystem.QueryPlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem += QuerySystem.QueryPlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Dying += QuerySystem.QueryPlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.InteractingElevator += QuerySystem.QueryPlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing += QuerySystem.QueryPlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs += QuerySystem.QueryPlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified += QuerySystem.QueryPlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left += QuerySystem.QueryPlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole += QuerySystem.QueryPlayerEvents.OnSetClass;
            
            EventManager.EventManagerServerEvents = new EventManagerServerEvents();
            EventManager.EventManagerPlayerEvents = new EventManagerPlayerEvents();
            Exiled.Events.Handlers.Server.EndingRound += EventManager.EventManagerServerEvents.EndingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += EventManager.EventManagerServerEvents.WaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundEnded += EventManager.EventManagerServerEvents.EndRound;
            Exiled.Events.Handlers.Server.RestartingRound += EventManager.EventManagerServerEvents.RestartingRound;
            Exiled.Events.Handlers.Player.Joined += EventManager.EventManagerPlayerEvents.OnPlayerJoin;
            
            foreach (var plugin in Exiled.Loader.Loader.Plugins)
            {
                if (plugin.Name.StartsWith("Exiled.")) //dont check exiled itself
                    continue;
                Log.Debug($"Checking {plugin.Name} for CedMod-Events functionality", Config.EventManager.Debug);
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
            });
        }
        
        public override void OnDisabled()
        {
            _harmony.UnpatchAll();
            Singleton = null;
            
            Exiled.Events.Handlers.Server.RestartingRound -= _server.OnRoundRestart;
            Exiled.Events.Handlers.Server.LocalReporting -= _server.OnReport;
            //Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= server.OnSendingRemoteAdmin;
            
            Exiled.Events.Handlers.Player.Verified -= _player.OnJoin;
            Exiled.Events.Handlers.Player.Dying -= _player.OnDying;

            _server = null;
            _player = null;
            
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher != null)
                Object.Destroy(dispatcher);
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

            Exiled.Events.Handlers.Player.ItemUsed -= QuerySystem.QueryPlayerEvents.OnUsedItem;
            Exiled.Events.Handlers.Scp079.InteractingTesla -= QuerySystem.QueryPlayerEvents.On079Tesla;
            Exiled.Events.Handlers.Player.EscapingPocketDimension -= QuerySystem.QueryPlayerEvents.OnPocketEscape;
            Exiled.Events.Handlers.Player.EnteringPocketDimension -= QuerySystem.QueryPlayerEvents.OnPocketEnter;
            Exiled.Events.Handlers.Player.ThrowingItem -= QuerySystem.QueryPlayerEvents.OnGrenadeThrown;
            Exiled.Events.Handlers.Player.Dying -= QuerySystem.QueryPlayerEvents.OnPlayerDeath;
            Exiled.Events.Handlers.Player.InteractingElevator -= QuerySystem.QueryPlayerEvents.OnElevatorInteraction;
            Exiled.Events.Handlers.Player.Handcuffing -= QuerySystem.QueryPlayerEvents.OnPlayerHandcuffed;
            Exiled.Events.Handlers.Player.RemovingHandcuffs -= QuerySystem.QueryPlayerEvents.OnPlayerFreed;
            Exiled.Events.Handlers.Player.Verified -= QuerySystem.QueryPlayerEvents.OnPlayerJoin;
            Exiled.Events.Handlers.Player.Left -= QuerySystem.QueryPlayerEvents.OnPlayerLeave;
            Exiled.Events.Handlers.Player.ChangingRole -= QuerySystem.QueryPlayerEvents.OnSetClass;

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
            
            base.OnDisabled();
        }
    }
}