using System.Collections.Generic;
using System.Linq;
using System.Text;
using CedMod.Addons.QuerySystem;
using LabApi.Events.Handlers;
using LabApi.Loader;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace CedMod.Error
{
    internal static class ErrorCollector
    {
        private static GameObject _obj;
        private static readonly List<string> Errors = new();

        private static PluginLibrary? _currentLibrary;

#if EXILED
        private const PluginLibrary TargetLibrary = PluginLibrary.Exiled;
#else
        private const PluginLibrary TargetLibrary = PluginLibrary.LabAPI;
#endif

        internal static void Setup()
        {
            _obj = new GameObject();
            Object.DontDestroyOnLoad(_obj);

            _obj.AddComponent<ContinuousError>();

            ServerEvents.WaitingForPlayers += WaitingForPlayers;
        }

        internal static void Destroy()
        {
            Object.Destroy(_obj);

            ServerEvents.WaitingForPlayers -= WaitingForPlayers;
        }

        internal static void Add(string content)
        {
            Errors.Add(content);
            SendErrors();
        }

        internal static void SendErrors(bool initial = false)
        {
            CheckLibrary();

            var allErrors = CollectAllErrors();
            if (allErrors.Count == 0)
            {
                if (initial)
                {
                    Logger.Info("No CedMod setup errors found during startup");
                }

                return;
            }

            var builder = new StringBuilder(" --------- [ CedMod setup error(s) ] --------- \n" +
                                            "One or more errors have been found in your CedMod setup. CedMod may not function properly until all errors are resolved!.\n\n");

            foreach (var error in allErrors)
            {
                builder.Append(" - ").AppendLine(error);
            }

            builder.AppendLine();
            builder.AppendLine(" ----------------------------------------------- ");

            Logger.Error(builder.ToString());
        }

        private static List<string> CollectAllErrors()
        {
            var allErrors = new List<string>(Errors);

            if (_currentLibrary != TargetLibrary)
            {
                allErrors.Add(
                    $"Wrong CedMod version was installed! You installed CedMod for {TargetLibrary} but should have installed CedMod for {_currentLibrary}. Make sure you followed the installation instructions in the README (https://github.com/CedModV2/CedMod), or on the panel, for the plugin library you are using.");
            }

            if (string.IsNullOrWhiteSpace(QuerySystem.QuerySystemKey))
            {
                allErrors.Add(
                    "CedMod requires additional Setup, the plugin will not function and some features will not work if the plugin is not setup.\nPlease follow the setup guide on https://cedmod.nl/Servers/Create");
            }

            return allErrors;
        }

        private static void CheckLibrary()
        {
            if (PluginLoader.EnabledPlugins.Select(plugin => plugin.Name)
                .Any(loweredName => loweredName == "Exiled Loader"))
            {
                _currentLibrary = PluginLibrary.Exiled;
                return;
            }

            _currentLibrary = PluginLibrary.LabAPI;
        }

        private static void WaitingForPlayers()
        {
            SendErrors(true);
            ServerEvents.WaitingForPlayers -= WaitingForPlayers;
        }
    }
}