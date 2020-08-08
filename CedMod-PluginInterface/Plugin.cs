using System;
using System.Collections.Generic;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events;
using GameCore;
using HarmonyLib;

namespace CedMod.PluginInterface
{
    public class CedModPluginInterface : Plugin<Config>
    {
        public static Harmony harmony;
        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Default;

        /// <inheritdoc/>
        /// 
        public static Dictionary<QueryUser, string> autheduers = new Dictionary<QueryUser, string>();

        public override string Author { get; } = "ced777ric#0001";

        public override string Name { get; } = "CedMod-WebAPI";

        public override string Prefix { get; } = "cm_WAPI";

        public static Config config;
        public override void OnDisabled()
        {
            // Unload the event handlers.
            // Close the HTTP server.
            WebService.StopWebServer();
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= CommandHandler.HandleCommand;
        }

        public static string SecurityKey;
        public override void OnEnabled()
        {
            config = Config;
            // Load the event handlers.
            if (!Config.IsEnabled)
                return;
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += CommandHandler.HandleCommand;
            SecurityKey =
                GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "None");
            if (SecurityKey == "None")
            {
                SecurityKey = Config.SecurityKey;
            }
            else
            {
                Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is depricated, please use the new exiled config system, this opention will be removed in future release");
            }
            if (SecurityKey != "None")
            {
                // Start the HTTP server.
                harmony = new Harmony("com.cedmodAPI.patch");
                harmony.PatchAll();
                WebService.StartWebServer();
            }
            else
               Initializer.Logger.Warn("PluginInterface", "cm_plugininterface_key is set to none plugin will nog load due to security risks");
        }
        public override void OnReloaded()
        {}
    }
}