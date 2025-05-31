using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CedMod.Addons.AdminSitSystem;
using CedMod.Addons.Audio;
using CedMod.Addons.Events;
using CedMod.Addons.Events.Interfaces;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.Sentinal;
using CedMod.Addons.Sentinal.Patches;
using CedMod.Addons.StaffInfo;
using CedMod.Components;
using CentralAuth;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Loader;
using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Permissions;
using LabApi.Loader;
using LabApi.Loader.Features.Misc;
using LabApi.Loader.Features.Paths;
using LabApi.Loader.Features.Plugins;
using LabApi.Loader.Features.Plugins.Enums;
using MEC;
using Logger = LabApi.Features.Console.Logger;
using Object = UnityEngine.Object;
using Player = CedMod.Handlers.Player;
using Server = CedMod.Handlers.Server;

namespace CedMod
{
    
#if EXILED
    public class CedModMain: Exiled.API.Features.Plugin<Config>
#else
    public class CedModMain: LabApi.Loader.Features.Plugins.Plugin<Config>
#endif
    {
        private Harmony _harmony;
        public static CedModMain Singleton;
        public static string FileHash { get; set; } = "";

        public static string GitCommitHash = String.Empty;
        public static string VersionIdentifier = String.Empty;

        public static string PluginConfigFolder = "";
        public static string PluginLocation = "";
        public static Assembly Assembly;
        public Thread CacheHandler;
        public static CancellationToken CancellationToken;
        public CancellationTokenSource CancellationTokenSource;

        public const string PluginVersion = "3.4.24";

        public override string Name { get; } = "CedMod";
        public override string Author { get; } = "ced777ric#8321";
        public override Version Version { get; } = new Version(PluginVersion);

#if EXILED
        public override string Prefix { get; } = "cm";
        public override Version RequiredExiledVersion { get; } = new Version(8, 0, 0);
        public override PluginPriority Priority { get; } = PluginPriority.High;
        
        public override void OnEnabled()
        {
            LoadPlugin();
            base.OnEnabled();
        }
#else
        public override string Description { get; } = "Moderation system and admin tools";
        public override Version RequiredApiVersion { get; } = new Version(0, 0,0);
        public override LoadPriority Priority { get; } = LoadPriority.High;

        public override void Enable()
        {
            LoadPlugin();
        }
#endif

        public Handlers.Player PlayerEvents = new Player();
        public Handlers.Server ServerEvents = new Server();
        public QueryPlayerEvents QueryPlayerEvents = new QueryPlayerEvents();
        public QueryMapEvents QueryMapEvents = new QueryMapEvents();
        public QueryServerEvents QueryServerEvents = new QueryServerEvents();
        public EventManagerPlayerEvents EventManagerPlayerEvents = new EventManagerPlayerEvents();
        public EventManagerServerEvents EventManagerServerEvents = new EventManagerServerEvents();
        public TeslaGateHandler SentinalTeslaGateHandler = new TeslaGateHandler();
        public ItemPickupHandler SentinalItemPickupHandler = new ItemPickupHandler();
        public VoiceChatEvents SentinalVoicechatEvents = new VoiceChatEvents();
        public AutoUpdater AutoUpdater = null;
        public AdminSitHandler AdminSitHandlerEvents = null;
        public StaffInfoHandler StaffInfoHandler = null;
        
        public string GameModeDirectory { get; set; }
        Dictionary<Assembly, Plugin> LabApiPluginGamemodes = new Dictionary<Assembly, Plugin>();
        Dictionary<Plugin, string> LabApiPluginGamemodesPaths = new Dictionary<Plugin, string>();
        List<Assembly> ExiledPlugin = new List<Assembly>();
        
        void LoadPlugin()
        {
#if !EXILED
            if (Config == null)
            {
                Timing.CallPeriodically(100000, 1, () => Logger.Error("Failed to load CedMod, your CedMod config file is invalid. Please make sure the config.yml file is valid, the config.yml file is located in the CedMod folder inside the folder you installed the dll into, if the file does not contain valid yml, delete it, or resolve issues, and restart."));
                return;
            }
            
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            bool loaded = (bool)loadProperty.GetValue(null);
            if (loaded)
            {
                Timing.CallPeriodically(100000, 1, () => Logger.Error("It would appear that the LabApi tried loading CedMod twice, please ensure that you do not have CedMod installed twice"));
                return;
            }
            loadProperty.SetValue(null, true);
            
            PluginLocation = FilePath;
            PluginConfigFolder = this.GetConfigDirectory().FullName;
#else
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            bool loaded = (bool)loadProperty.GetValue(null);
            if (loaded)
            {
                Timing.CallPeriodically(100000, 1, () => Logger.Error("It would appear that EXILED tried loading CedMod twice, please ensure that you do not have CedMod or EXILED installed twice"));
                return;
            }
            loadProperty.SetValue(null, true);
            PluginLocation = this.GetPath();
            PluginConfigFolder = Path.Combine(Paths.Configs, "CedMod");
            Logger.Info($"Using {PluginConfigFolder} as CedMod data folder.");
#endif
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            if (!Directory.Exists(PluginConfigFolder))
            {
                Directory.CreateDirectory(PluginConfigFolder);
            }
            
            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedMod")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedMod"));
            }
            
            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedMod", "Internal")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedMod", "Internal"));
            }
            
            
            PlayerAuthenticationManager.OnInstanceModeChanged += HandleInstanceModeChange; 
            Assembly = Assembly.GetExecutingAssembly();
            CosturaUtility.Initialize();
            
            try
            {
                GameModeDirectory = Path.Combine(PluginConfigFolder, "CedModEvents");
                var file = File.Open(PluginLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                FileHash = GetHashCode(file, new MD5CryptoServiceProvider());
                file.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            try
            {
                _harmony = new Harmony("com.cedmod.patch");
                _harmony.PatchAll();
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to patch: {e.ToString()}");
                _harmony.UnpatchAll(_harmony.Id);
            }
            
            PermissionsManager.RegisterProvider<PermissionProvider>();
            CustomHandlersManager.RegisterEventsHandler(PlayerEvents);
            CustomHandlersManager.RegisterEventsHandler(ServerEvents);

            CustomHandlersManager.RegisterEventsHandler(QueryPlayerEvents);
            CustomHandlersManager.RegisterEventsHandler(QueryMapEvents);
            CustomHandlersManager.RegisterEventsHandler(QueryServerEvents);
            
            CustomHandlersManager.RegisterEventsHandler(EventManagerPlayerEvents);
            CustomHandlersManager.RegisterEventsHandler(EventManagerServerEvents);
            
            CustomHandlersManager.RegisterEventsHandler(SentinalTeslaGateHandler);
            CustomHandlersManager.RegisterEventsHandler(SentinalItemPickupHandler);
            CustomHandlersManager.RegisterEventsHandler(SentinalVoicechatEvents);
            
            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher == null)
                CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();

            AutoUpdater = Object.FindObjectOfType<AutoUpdater>();
            if (AutoUpdater == null)
                AutoUpdater = CustomNetworkManager.singleton.gameObject.AddComponent<AutoUpdater>();

            AdminSitHandlerEvents = Object.FindObjectOfType<AdminSitHandler>();
            if (AdminSitHandlerEvents == null)
                AdminSitHandlerEvents = CustomNetworkManager.singleton.gameObject.AddComponent<AdminSitHandler>();

            RemoteAdminModificationHandler remoteAdminModificationHandler = Object.FindObjectOfType<RemoteAdminModificationHandler>();
            if (remoteAdminModificationHandler == null)
                remoteAdminModificationHandler = CustomNetworkManager.singleton.gameObject.AddComponent<RemoteAdminModificationHandler>();

            if (Config.QuerySystem.StaffInfoSystem)
            {
                StaffInfoHandler staffInfoHandler = Object.FindObjectOfType<StaffInfoHandler>();
                if (staffInfoHandler == null)
                    staffInfoHandler = CustomNetworkManager.singleton.gameObject.AddComponent<StaffInfoHandler>();
            }
            
            SentinalBehaviour sentinalBehaviour = Object.FindObjectOfType<SentinalBehaviour>();
            if (sentinalBehaviour == null)
                sentinalBehaviour = CustomNetworkManager.singleton.gameObject.AddComponent<SentinalBehaviour>();

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

            if (File.Exists(Path.Combine(PluginConfigFolder, "CedMod", "dev.txt")))
            {
                Logger.Info("Plugin running as Dev");
                QuerySystem.IsDev = true;
                using (StreamReader reader = new StreamReader(Path.Combine(PluginConfigFolder, "CedMod", "dev.txt")))
                {
                    var txt = reader.ReadToEnd();
                    var lines = txt.Split(',');

                    if (lines.Length > 0)
                    {
                        foreach (var line in lines)
                        {
                            var trim = line.Trim();
                            if(trim.StartsWith("APIUrl:"))
                            {
                                API.DevUri = trim.Replace("APIUrl:", "");
                                continue;
                            }
                            if(trim.StartsWith("UseSSL:"))
                            {
                                QuerySystem.UseSSL = bool.Parse(trim.Replace("UseSSL:", ""));
                                continue;
                            }
                            if (trim.StartsWith("PanelUrl:"))
                            {
                                QuerySystem.DevPanelUrl = trim.Replace("PanelUrl:", "");
                            }
                        }
                    }
                }
                QuerySystem.CurrentMaster = QuerySystem.DevPanelUrl;
            }

            Singleton = this;

            if (File.Exists(Path.Combine(PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{ServerStatic.ServerPort}.txt")))
            {
                // Start the HTTP server.
                Task.Run(async () =>
                {
                    try
                    {
                        await VerificationChallenge.AwaitVerification();
                        await WebSocketSystem.Start();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.ToString());
                    }
                });
            }
            else
                Logger.Warn("Plugin is not setup properly, please use refer to the cedmod setup guide"); //todo link guide

            Shutdown.OnQuit += OnQuit;
            CacheHandler = new Thread(CedMod.CacheHandler.Loop);
            CacheHandler.Start();

            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedModEvents")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedModEvents"));
            }

            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedMod")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedMod"));
            }

            int successes = 0;

            foreach (var file in Directory.GetFiles(Path.Combine(PluginConfigFolder, "CedModEvents"), "*.dll"))
            {
                var assembly = Assembly.Load(File.ReadAllBytes(file));
                
                var types = assembly.GetTypes();

                foreach (var entryType in types)
                {
#if EXILED
                    if (IsDerivedFromExiledPlugin(entryType))
                    {
                        ExiledPlugin.Add(entryType.Assembly);
                        continue;
                    }
#endif
                    if (AssemblyUtils.HasMissingDependencies(assembly, file, out Type[]? missingTypes))
                        continue;

                    if (PluginLoader.Plugins.ContainsValue(assembly)) 
                        continue;
                    
                    try
                    {
                        if (!entryType.IsSubclassOf(typeof(Plugin)))
                            continue;

                        if (Activator.CreateInstance(entryType) is not Plugin plugin)
                            continue;

                        LabApiPluginGamemodes.Add(assembly, plugin);
                        LabApiPluginGamemodesPaths[plugin] = file;
                        Logger.Info($"[CedModEvents-LabApi] Successfully loaded {plugin.Name}");
                        successes++;
                    }
                    catch (Exception e)
                    {
                        Logger.Info($"Failed to load {entryType.FullName}.\n{e}");
                        continue;
                    }
                }
            }

#if EXILED
            foreach (var exiled in ExiledPlugin)
            {
                var plugin = Loader.CreatePlugin(exiled);
                try
                {
                    plugin.OnEnabled();
                    plugin.OnRegisteringCommands();
                }
                catch (Exception e)
                {
                    Logger.Info($"[ExiledGameMode] Failed to load {plugin.Name}.\n{e}");
                    continue;
                }

                EventManager.AvailableEventPluginsExiled.Add(plugin);

                foreach (var type in exiled.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    if (Config.EventManager.Debug)
                        Logger.Debug($"[ExiledGameMode] Checked {plugin.Name} for CedMod-Events functionality, IEvent inherited");
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        if (Config.EventManager.Debug) 
                            Logger.Debug($"[ExiledGameMode] Checked {plugin.Name} Constructor is null, cannot continue");
                        continue;
                    }
                    else
                    {
                        if (plugin is not IEvent @event)
                            continue;
                        
                        if (@event == null)
                            continue;

                        if (!@event.EventConfig.IsEnabled)
                        {
                            if (Config.EventManager.Debug)
                                Logger.Debug($"[ExiledGameMode] Checked {plugin.Name} IsEnabled is False, cannot continue");
                            continue;
                        }

                        EventManager.AvailableEvents.Add(@event);
                        Logger.Info($"[ExiledGameMode] Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
                    }
                }
            }
#endif
            
            foreach (var gamemode in LabApiPluginGamemodes)
            {
                if (EventManager.AvailableEventPlugins.Contains(gamemode.Value))
                {
                    Logger.Error($"[NWAPIGamemode] Found duplicate Event: {gamemode.Value.Name} Located at: {LabApiPluginGamemodesPaths[gamemode.Value]} Please only have one of each Event installed");
                    continue;
                }
                EventManager.AvailableEventPlugins.Add(gamemode.Value);
                try
                {
                    gamemode.Value.LoadConfigs();
                    gamemode.Value.Enable();
                    gamemode.Value.RegisterCommands();
                }
                catch (Exception e)
                {
                    Logger.Info($"[CedModEvents-LabApi] Failed to load {gamemode.Value.Name}.\n{e}");
                    continue;
                }

                foreach (var type in gamemode.Key.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    if (Config.EventManager.Debug)
                        Logger.Debug($"[CedModEvents-LabApi] Checked {gamemode.Value.Name} for CedMod-Events functionality, IEvent inherited");
                    var constructor = Activator.CreateInstance(type);
                    if (constructor == null || constructor is not IEvent @event)
                    {
                        if (Config.EventManager.Debug)
                            Logger.Debug($"[CedModEvents-LabApi] Checked {gamemode.Value.Name} Constructor is null, cannot continue");
                        continue;
                    }
                    else
                    {
                        if (@event == null) 
                            continue;

                        if (!@event.EventConfig.IsEnabled)
                        {
                            if (Config.EventManager.Debug)
                                Logger.Debug($"[CedModEvents-LabApi] Checked {gamemode.Value.Name} IsEnabled is False, cannot continue");
                            continue;
                        }
                      
                        EventManager.AvailableEvents.Add(@event);
                        Logger.Info($"[CedModEvents-LabApi] Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
                    }
                }
            }

            Task.Run(Verification.ObtainId);
            Task.Run(() => ServerPreferences.ResolvePreferences(true));
        }

        private void OnQuit()
        {
            new Thread(s =>
            {
                try
                {
                    Logger.Info("Exit watcher enabled.");
                    //CedMod.CacheHandler.WaitForSecond(5, o => true);
                    for (int i = 0; i < 30 && ServerShutdown.ShutdownState != ServerShutdown.ServerShutdownState.Complete || i < 6; ++i)
                    {
                        Thread.Sleep(120);
                    }
                    var process = Process.GetCurrentProcess();
                    Logger.Info($"Exit taking too long, killing {process.Id} {process.ProcessName} {process.StartInfo.Arguments}");
                    process.Kill();
                    Logger.Info("Killed");
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
            }).Start();
            
            Logger.Info("Server shutting down, stopping threads...");
            CancellationTokenSource.Cancel();
            try
            {
                WebSocketSystem.Reconnect = false;
                WebSocketSystem.Stop().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Logger.Info("Shutdown WebsocketSystem");
            
            try
            {
                CedModMain.Singleton.CacheHandler = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Logger.Info("CedMod Threads stopped.");
        }

        private void HandleInstanceModeChange(ReferenceHub arg1, ClientInstanceMode arg2)
        {
            if ((arg2 != ClientInstanceMode.Unverified || arg2 != ClientInstanceMode.Host) && AudioCommand.FakeConnectionsIds.ContainsValue(arg1))
            {
                Logger.Info($"Replaced instancemode for dummy to host.");
                arg1.authManager.InstanceMode = ClientInstanceMode.Host;
            }
        }

#if EXILED
        public override void OnDisabled()
        {
            Disabled();
            base.OnDisabled();
        }
        
        private bool IsDerivedFromExiledPlugin(Type type)
        {
            while (type is not null)
            {
                type = type.BaseType;

                if (type?.IsGenericType ?? false)
                {
                    Type genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(Exiled.API.Features.Plugin<>) || genericDef == typeof(Plugin<,>))
                        return true;
                }
            }

            return false;
        }
#else
        public override void Disable()
        {
            Disabled();
        }
#endif
        public void Disabled()
        {
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            loadProperty.SetValue(null, false);
            PlayerAuthenticationManager.OnInstanceModeChanged -= HandleInstanceModeChange;
            _harmony.UnpatchAll(_harmony.Id);
            
            CustomHandlersManager.UnregisterEventsHandler(PlayerEvents);
            CustomHandlersManager.UnregisterEventsHandler(ServerEvents);

            CustomHandlersManager.UnregisterEventsHandler(QueryPlayerEvents);
            CustomHandlersManager.UnregisterEventsHandler(QueryMapEvents);
            CustomHandlersManager.UnregisterEventsHandler(QueryServerEvents);
            
            CustomHandlersManager.UnregisterEventsHandler(EventManagerPlayerEvents);
            CustomHandlersManager.UnregisterEventsHandler(EventManagerServerEvents);
            
            CustomHandlersManager.UnregisterEventsHandler(SentinalTeslaGateHandler);
            CustomHandlersManager.UnregisterEventsHandler(SentinalItemPickupHandler);
            CustomHandlersManager.UnregisterEventsHandler(SentinalVoicechatEvents);
            Shutdown.OnQuit -= OnQuit;
            
            Singleton = null;

            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher != null)
                Object.Destroy(dispatcher);
            
            if (AutoUpdater != null)
                Object.Destroy(AutoUpdater);
            
            if (AdminSitHandlerEvents != null)
                Object.Destroy(AdminSitHandlerEvents);
            
            RemoteAdminModificationHandler remoteAdminModificationHandler = Object.FindObjectOfType<RemoteAdminModificationHandler>();
            if (remoteAdminModificationHandler != null)
                Object.Destroy(remoteAdminModificationHandler);
            
            SentinalBehaviour sentinalBehaviour = Object.FindObjectOfType<SentinalBehaviour>();
            if (sentinalBehaviour != null)
                Object.Destroy(sentinalBehaviour);

            if (Config.QuerySystem.StaffInfoSystem)
            {
                if (StaffInfoHandler != null)
                    Object.Destroy(StaffInfoHandler);
            }

            WebSocketSystem.Stop();
            CacheHandler.Interrupt();

            foreach (var plugin in EventManager.AvailableEventPlugins)
            {
                try
                {
                    plugin.UnregisterCommands();
                    plugin.Disable();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to disable GameMode: {plugin.Name}\n{e}");
                }
            }

#if  EXILED
            foreach (var plugin in EventManager.AvailableEventPluginsExiled)
            {
                try
                {
                    plugin.OnDisabled();
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to disable GameMode: {plugin.Name}\n{e}");
                }
            }
#endif
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
        
        internal static string GetHashCode(string text, HashAlgorithm cryptoService)
        {
            using (cryptoService)
            {
                var hash = cryptoService.ComputeHash(Encoding.UTF8.GetBytes(text));
                var hashString = Convert.ToBase64String(hash);
                return hashString.TrimEnd('=');
            }
        }
    }
}