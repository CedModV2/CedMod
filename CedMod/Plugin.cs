using System;
using System.Collections.Generic;
using CedMod.CedMod.INIT;
using CedMod.Commands;
using EXILED;
using GameCore;
using CommandSystem;
using Harmony;
using CommandHandler = CedMod.Commands.CedModCommandHandler;
using Log = EXILED.Log;

namespace CedMod
{
    public class Plugin : EXILED.Plugin
    {
        public BanSystem BanSystemEvents;
        public CommandHandler Commands;
        public FriendlyFireAutoBan FfaEvents;
        public FunctionsNonStatic FunctionsNonStatic;
        public PlayerJoinBc PlayerJoinBcEvents;
        public PlayerStatistics PlayerStats;

        public override string getName { get; } = "CedModV2";

        public override void OnEnable()
        {
            try
            {
                string geoString = "";
                List<string> geoList = ConfigFile.ServerConfig.GetStringList("bansystem_geo");
                foreach (string s in geoList)
                {
                    geoString = geoString + s + "+";
                }
                if (geoList != null)
                {
                    ServerConsole.AccessRestriction = true;
                }
                Log.Debug("Initializing event handlers..");
                //Set instance varible to a new instance, this should be nulled again in OnDisable
                BanSystemEvents = new BanSystem(this);
                //Hook the events you will be using in the plugin. You should hook all events you will be using here, all events should be unhooked in OnDisabled 
                Events.RemoteAdminCommandEvent += BanSystemEvents.OnCommand;
                Events.PlayerJoinEvent += BanSystemEvents.OnPlayerJoin;
                PlayerJoinBcEvents = new PlayerJoinBc(this);
                Events.PlayerJoinEvent += PlayerJoinBcEvents.OnPlayerJoin;
                FfaEvents = new FriendlyFireAutoBan(this);
                Events.RoundStartEvent += FfaEvents.OnRoundStart;
                Events.PlayerDeathEvent += FfaEvents.Ondeath;
                Events.ConsoleCommandEvent += FfaEvents.ConsoleCommand;
                Commands = new CommandHandler(this);
                Events.RoundEndEvent += Commands.OnRoundEnd;
                PlayerStats = new PlayerStatistics(this);
                Events.RoundEndEvent += PlayerStats.OnRoundEnd;
                Events.PlayerDeathEvent += PlayerStats.OnPlayerDeath;
                FunctionsNonStatic = new FunctionsNonStatic(this);
                Events.RoundRestartEvent += FunctionsNonStatic.Roundrestart;
                Events.WaitingForPlayersEvent += FunctionsNonStatic.Waitingforplayers;
                Log.Info("CedMod has loaded. c:");
                Initializer.Setup();
            }
            catch (Exception e)
            {
                //This try catch is redundant, as EXILED will throw an error before this block can, but is here as an example of how to handle exceptions/errors
                Log.Error($"There was an error loading the plugin: {e}");
            }
        }

        public override void OnDisable()
        {
            Events.RemoteAdminCommandEvent -= BanSystemEvents.OnCommand;
            Events.PlayerJoinEvent -= BanSystemEvents.OnPlayerJoin;
            Events.PlayerJoinEvent -= PlayerJoinBcEvents.OnPlayerJoin;
            Events.RoundStartEvent -= FfaEvents.OnRoundStart;
            Events.RoundEndEvent -= Commands.OnRoundEnd;
            Events.ConsoleCommandEvent -= FfaEvents.ConsoleCommand;
            Events.RoundEndEvent -= PlayerStats.OnRoundEnd;
            Events.PlayerDeathEvent -= PlayerStats.OnPlayerDeath;
            Events.RoundRestartEvent -= FunctionsNonStatic.Roundrestart;
            BanSystemEvents = null;
            PlayerJoinBcEvents = null;
            FfaEvents = null;
            Commands = null;
            PlayerStats = null;
            FunctionsNonStatic = null;

        }

        public override void OnReload()
        {
            //This is only fired when you use the EXILED reload command, the reload command will call OnDisable, OnReload, reload the plugin, then OnEnable in that order. There is no GAC bypass, so if you are updating a plugin, it must have a unique assembly name, and you need to remove the old version from the plugins folder
        }
    }
}