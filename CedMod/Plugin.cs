using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using Exiled.API.Features;
using Exiled.Loader;
using HarmonyLib;
using UnityEngine;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Core.Extensions;
using PluginAPI.Enums;
using PluginAPI.Helpers;
using PluginAPI.Loader;
using PluginAPI.Loader.Features;
using Log = PluginAPI.Core.Log;
using Object = UnityEngine.Object;
using Paths = Exiled.API.Features.Paths;
using Server = PluginAPI.Core.Server;

namespace CedMod
{
    
#if EXILED
    public class CedModMain: Plugin<Config>
#else
    public class CedModMain
#endif
    {
        private Harmony _harmony;
        public static CedModMain Singleton;
        public static string FileHash { get; set; } = "";

        public static string GitCommitHash = String.Empty;
        public static string VersionIdentifier = String.Empty;

        public static string PluginConfigFolder = "";
        public static string PluginLocation = "";
        public static PluginDirectory GameModeDirectory;
        public static Assembly Assembly;
        public Thread CacheHandler;
        public static CancellationToken CancellationToken;
        public CancellationTokenSource CancellationTokenSource;
#if !EXILED
        public static PluginHandler Handler;
#endif

        public const string PluginVersion = "3.4.16";

#if !EXILED
        [PluginConfig]
        public Config Config;
#endif

#if EXILED
        public override string Name { get; } = "CedMod";
        public override string Prefix { get; } = "cm";
        public override string Author { get; } = "ced777ric#8321";
        public override Version Version { get; } = new Version(PluginVersion);
        public override Version RequiredExiledVersion { get; } = new Version(8, 0, 0);

        public override void OnEnabled()
        {
            LoadPlugin();
            base.OnEnabled();
        }
#endif

#if !EXILED
        [PluginPriority(LoadPriority.Lowest)]
        [PluginEntryPoint("CedMod", PluginVersion, "SCP:SL Moderation system https://cedmod.nl/About", "ced777ric#8321")]
#endif
        void LoadPlugin()
        {
#if !EXILED
            if (Config == null)
            {
                Timing.CallPeriodically(100000, 1, () => Log.Error("Failed to load CedMod, your CedMod config file is invalid. Please make sure the config.yml file is valid, the config.yml file is located in the CedMod folder inside the folder you installed the dll into, if the file does not contain valid yml, delete it, or resolve issues, and restart."));
                return;
            }
            
            if (!Config.IsEnabled)
                return;
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            bool loaded = (bool)loadProperty.GetValue(null);
            if (loaded)
            {
                Timing.CallPeriodically(100000, 1, () => Log.Error("It would appear that the NWApi tried loading CedMod twice, please ensure that you do not have CedMod installed twice"));
                return;
            }
            loadProperty.SetValue(null, true);
            Handler = PluginHandler.Get(this);
            Timing.CallDelayed(5, () =>
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(s => s.GetName().Name == "NWAPIPermissionSystem"))
                    Timing.CallPeriodically(100000, 1, () => Log.Error("You do not have the NWAPIPermissionSystem Installed, CedMod Requires the NWAPIPermission system in order to operate properly, please download it here: https://github.com/CedModV2/NWAPIPermissionSystem"));
                return;
            });

            PluginLocation = Handler.PluginFilePath;
            PluginConfigFolder = Handler.PluginDirectoryPath;
#else
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            bool loaded = (bool)loadProperty.GetValue(null);
            if (loaded)
            {
                Timing.CallPeriodically(100000, 1, () => Log.Error("It would appear that EXILED tried loading CedMod twice, please ensure that you do not have CedMod or EXILED installed twice"));
                return;
            }
            loadProperty.SetValue(null, true);
            PluginLocation = this.GetPath();
            PluginConfigFolder = Path.Combine(Paths.Configs, "CedMod");
            Log.Info($"Using {PluginConfigFolder} as CedMod data folder.");
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

            PluginAPI.Events.EventManager.RegisterEvents<Handlers.Player>(this);
            PluginAPI.Events.EventManager.RegisterEvents<Handlers.Server>(this);

            PluginAPI.Events.EventManager.RegisterEvents<QueryMapEvents>(this);
            PluginAPI.Events.EventManager.RegisterEvents<QueryServerEvents>(this);
            PluginAPI.Events.EventManager.RegisterEvents<QueryPlayerEvents>(this);

            PluginAPI.Events.EventManager.RegisterEvents<EventManagerServerEvents>(this);
            PluginAPI.Events.EventManager.RegisterEvents<EventManagerPlayerEvents>(this);

            PluginAPI.Events.EventManager.RegisterEvents<AutoUpdater>(this);
            PluginAPI.Events.EventManager.RegisterEvents<AdminSitHandler>(this);
            if (Config.QuerySystem.StaffInfoSystem)
                PluginAPI.Events.EventManager.RegisterEvents<StaffInfoHandler>(this);
            FactoryManager.RegisterPlayerFactory(this, new CedModPlayerFactory());

            try
            {
                GameModeDirectory = new PluginDirectory(Path.Combine(PluginConfigFolder, "CedModEvents"));
                var file = File.Open(PluginLocation, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                FileHash = GetHashCode(file, new MD5CryptoServiceProvider());
                file.Dispose();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            try
            {
                _harmony = new Harmony("com.cedmod.patch");
                _harmony.PatchAll();
            }
            catch (Exception e)
            {
                PluginAPI.Core.Log.Error($"Failed to patch: {e.ToString()}");
                _harmony.UnpatchAll(_harmony.Id);
                PluginAPI.Events.EventManager.UnregisterAllEvents(this);
            }

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
                Log.Info("Plugin running as Dev");
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

            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher == null)
                CustomNetworkManager.singleton.gameObject.AddComponent<ThreadDispatcher>();

            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            if (updater == null)
                updater = CustomNetworkManager.singleton.gameObject.AddComponent<AutoUpdater>();

            AdminSitHandler adminSitHandler = Object.FindObjectOfType<AdminSitHandler>();
            if (adminSitHandler == null)
                adminSitHandler = CustomNetworkManager.singleton.gameObject.AddComponent<AdminSitHandler>();

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
            

            if (File.Exists(Path.Combine(PluginConfigFolder, "CedMod", $"QuerySystemSecretKey-{Server.Port}.txt")))
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
                        Log.Error(e.ToString());
                    }
                });
            }
            else
                Log.Warning("Plugin is not setup properly, please use refer to the cedmod setup guide"); //todo link guide

            Shutdown.OnQuit += OnQuit;
            CacheHandler = new Thread(CedMod.CacheHandler.Loop);
            CacheHandler.Start();

            QuerySystem.QueryMapEvents = new QueryMapEvents();
            QuerySystem.QueryServerEvents = new QueryServerEvents();
            QuerySystem.QueryPlayerEvents = new QueryPlayerEvents();

            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedModEvents")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedModEvents"));
            }

            if (!Directory.Exists(Path.Combine(PluginConfigFolder, "CedMod")))
            {
                Directory.CreateDirectory(Path.Combine(PluginConfigFolder, "CedMod"));
            }

            int successes = 0;
            
            Dictionary<Type, PluginHandler> handlers = new Dictionary<Type, PluginHandler>();
            Dictionary<PluginHandler, string> pluginLocations = new Dictionary<PluginHandler, string>();

            List<Assembly> ExiledPlugin = new List<Assembly>();

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
                    if (!entryType.IsValidEntrypoint()) 
                        continue;

                    if (!AssemblyLoader.Plugins.ContainsKey(assembly)) 
                        AssemblyLoader.Plugins.Add(assembly, new Dictionary<Type, PluginHandler>());

                    if (!AssemblyLoader.Plugins[assembly].ContainsKey(entryType))
                    {
                        try
                        {
                            var plugin = Activator.CreateInstance(entryType);
                            var pluginType = new PluginHandler(GameModeDirectory, plugin, entryType, types);
                            pluginLocations.Add(pluginType, file);
                            handlers.Add(entryType, pluginType);
                            AssemblyLoader.Plugins[assembly].Add(entryType, pluginType);
                            AssemblyLoader.PluginToAssembly.Add(plugin, assembly);
                            successes++;
                        }
                        catch (Exception e)
                        {
                            Log.Info($"Failed to load {entryType.FullName}.\n{e}");
                            continue;
                        }
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
                    Log.Info($"[ExiledGameMode] Failed to load {plugin.Name}.\n{e}");
                    continue;
                }

                EventManager.AvailableEventPluginsExiled.Add(plugin);

                foreach (var type in exiled.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    if (Config.EventManager.Debug)
                        Log.Debug($"[ExiledGameMode] Checked {plugin.Name} for CedMod-Events functionality, IEvent inherited");
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        if (Config.EventManager.Debug) 
                            Log.Debug($"[ExiledGameMode] Checked {plugin.Name} Constructor is null, cannot continue");
                        continue;
                    }
                    else
                    {
                        IEvent @event = constructor.Invoke(null) as IEvent;
                        if (@event == null)
                            continue;

                        if (!@event.Config.IsEnabled)
                        {
                            if (Config.EventManager.Debug)
                                Log.Debug($"[ExiledGameMode] Checked {plugin.Name} IsEnabled is False, cannot continue");
                            continue;
                        }

                        EventManager.AvailableEvents.Add(@event);
                        Log.Info($"[ExiledGameMode] Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
                    }
                }
            }
#endif
            
            foreach (var gamemode in handlers)
            {
                if (EventManager.AvailableEventPlugins.Contains(gamemode.Value))
                {
                    Log.Error($"[NWAPIGamemode] Found duplicate Event: {gamemode.Value.PluginName} Located at: {pluginLocations[gamemode.Value]} Please only have one of each Event installed");
                    continue;
                }
                EventManager.AvailableEventPlugins.Add(gamemode.Value);
                try
                {
                    gamemode.Value.Load();
                }
                catch (Exception e)
                {
                    Log.Info($"[NWAPIGamemode] Failed to load {gamemode.Value.PluginName}.\n{e}");
                    continue;
                }

                foreach (var type in gamemode.Key.Assembly.GetTypes().Where(x => typeof(IEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
                {
                    if (Config.EventManager.Debug)
                        Log.Debug($"[NWAPIGamemode] Checked {gamemode.Value.PluginName} for CedMod-Events functionality, IEvent inherited");
                    var constructor = type.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                    {
                        if (Config.EventManager.Debug)
                            Log.Debug($"[NWAPIGamemode] Checked {gamemode.Value.PluginName} Constructor is null, cannot continue");
                        continue;
                    }
                    else
                    {
                        IEvent @event = gamemode.Value._plugin as IEvent;
                        if (@event == null) 
                            continue;

                        if (!@event.Config.IsEnabled)
                        {
                            if (Config.EventManager.Debug)
                                Log.Debug($"[NWAPIGamemode] Checked {gamemode.Value.PluginName} IsEnabled is False, cannot continue");
                            continue;
                        }
                      
                        EventManager.AvailableEvents.Add(@event);
                        Log.Info($"[NWAPIGamemode] Successfully registered {@event.EventName} By {@event.EvenAuthor} ({@event.EventPrefix})");
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
                    Log.Info("Exit watcher enabled.");
                    //CedMod.CacheHandler.WaitForSecond(5, o => true);
                    for (int i = 0; i < 30 && ServerShutdown.ShutdownState != ServerShutdown.ServerShutdownState.Complete || i < 6; ++i)
                    {
                        Thread.Sleep(120);
                    }
                    var process = Process.GetCurrentProcess();
                    Log.Info($"Exit taking too long, killing {process.Id} {process.ProcessName} {process.StartInfo.Arguments}");
                    process.Kill();
                    Log.Info("Killed");
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }).Start();
            
            Log.Info("Server shutting down, stopping threads...");
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
            
            Log.Info("Shutdown WebsocketSystem");
            
            try
            {
                CedModMain.Singleton.CacheHandler = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Log.Info("CedMod Threads stopped.");
        }

        private void HandleInstanceModeChange(ReferenceHub arg1, ClientInstanceMode arg2)
        {
            if ((arg2 != ClientInstanceMode.Unverified || arg2 != ClientInstanceMode.Host) && AudioCommand.FakeConnectionsIds.ContainsValue(arg1))
            {
                Log.Info($"Replaced instancemode for dummy to host.");
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
            while (type != null)
            {
                type = type.BaseType;

                if (type?.IsGenericType ?? false)
                {
                    Type genericDef = type.GetGenericTypeDefinition();
                    if (genericDef == typeof(Plugin<>) || genericDef == typeof(Plugin<,>))
                        return true;
                }
            }

            return false;
        }
#else
        [PluginUnload]
#endif
        public void Disabled()
        {
            var loadProperty = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(s => s.GetName().Name == "CedModV3").GetType("CedMod.API").GetProperty("HasLoaded");
            loadProperty.SetValue(null, false);
            PlayerAuthenticationManager.OnInstanceModeChanged -= HandleInstanceModeChange;
            _harmony.UnpatchAll(_harmony.Id);
            
            PluginAPI.Events.EventManager.UnregisterEvents<Handlers.Player>(this);
            PluginAPI.Events.EventManager.UnregisterEvents<Handlers.Server>(this);

            PluginAPI.Events.EventManager.UnregisterEvents<QueryMapEvents>(this);
            PluginAPI.Events.EventManager.UnregisterEvents<QueryServerEvents>(this);
            PluginAPI.Events.EventManager.UnregisterEvents<QueryPlayerEvents>(this);

            PluginAPI.Events.EventManager.UnregisterEvents<EventManagerServerEvents>(this);
            PluginAPI.Events.EventManager.UnregisterEvents<EventManagerPlayerEvents>(this);

            PluginAPI.Events.EventManager.UnregisterEvents<AutoUpdater>(this);
            PluginAPI.Events.EventManager.UnregisterEvents<AdminSitHandler>(this);
            if (Config.QuerySystem.StaffInfoSystem)
                PluginAPI.Events.EventManager.UnregisterEvents<StaffInfoHandler>(this);
            
            Shutdown.OnQuit -= OnQuit;
            
            Singleton = null;

            ThreadDispatcher dispatcher = Object.FindObjectOfType<ThreadDispatcher>();
            if (dispatcher != null)
                Object.Destroy(dispatcher);
            
            AutoUpdater updater = Object.FindObjectOfType<AutoUpdater>();
            if (updater != null)
                Object.Destroy(updater);
            
            AdminSitHandler adminSitHandler = Object.FindObjectOfType<AdminSitHandler>();
            if (adminSitHandler != null)
                Object.Destroy(adminSitHandler);
            
            RemoteAdminModificationHandler remoteAdminModificationHandler = Object.FindObjectOfType<RemoteAdminModificationHandler>();
            if (remoteAdminModificationHandler != null)
                Object.Destroy(remoteAdminModificationHandler);
            
            SentinalBehaviour sentinalBehaviour = Object.FindObjectOfType<SentinalBehaviour>();
            if (sentinalBehaviour != null)
                Object.Destroy(sentinalBehaviour);

            if (Config.QuerySystem.StaffInfoSystem)
            {
                StaffInfoHandler staffInfoHandler = Object.FindObjectOfType<StaffInfoHandler>();
                if (staffInfoHandler != null)
                    Object.Destroy(staffInfoHandler);
            }

            WebSocketSystem.Stop();
            CacheHandler.Interrupt();
            
            QuerySystem.QueryMapEvents = null;
            QuerySystem.QueryServerEvents = null;
            QuerySystem.QueryPlayerEvents = null;
            
            EventManager.EventManagerServerEvents = null;
            EventManager.EventManagerPlayerEvents = null;

            foreach (var plugin in EventManager.AvailableEventPlugins)
            {
                try
                {
                    plugin.Unload();
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to disable GameMode: {plugin.PluginName}\n{e}");
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
                    Log.Error($"Failed to disable GameMode: {plugin.Name}\n{e}");
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