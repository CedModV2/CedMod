using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CedMod.INIT;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.Handlers;
using GameCore;
using HarmonyLib;
using Console = System.Console;

namespace CedMod.QuerySystem
{
    public class QuerySystem : Plugin<Config>
    {
        public MapEvents MapEvents;
        public ServerEvents ServerEvents;
        public PlayerEvents PlayerEvents;
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
            if (Config.NewWebSocketSystem)
                WebService.StopWebServer();
            else
                WebService.StopWebServer();
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += CommandHandler.HandleCommand;
            if (Config.NewWebSocketSystem)
            {
                Exiled.Events.Handlers.Map.Decontaminating -= MapEvents.OnDecon;
                Exiled.Events.Handlers.Warhead.Starting -= MapEvents.OnWarheadStart;
                Exiled.Events.Handlers.Warhead.Stopping -= MapEvents.OnWarheadCancelled;
                Exiled.Events.Handlers.Warhead.Detonated -= MapEvents.OnWarheadDetonation;

                Exiled.Events.Handlers.Server.SendingRemoteAdminCommand -= ServerEvents.OnCommand;
                Exiled.Events.Handlers.Server.WaitingForPlayers -= ServerEvents.OnWaitingForPlayers;
                Exiled.Events.Handlers.Server.SendingConsoleCommand -= ServerEvents.OnConsoleCommand;
                Exiled.Events.Handlers.Server.RoundStarted -= ServerEvents.OnRoundStart;
                Exiled.Events.Handlers.Server.RoundEnded -= ServerEvents.OnRoundEnd;
                Exiled.Events.Handlers.Server.RespawningTeam -= ServerEvents.OnRespawn;
                Exiled.Events.Handlers.Server.ReportingCheater -= ServerEvents.OnCheaterReport;

                Exiled.Events.Handlers.Player.UsingMedicalItem -= PlayerEvents.OnMedicalItem;
                Exiled.Events.Handlers.Scp079.InteractingTesla -= PlayerEvents.On079Tesla;
                Exiled.Events.Handlers.Player.EscapingPocketDimension -= PlayerEvents.OnPocketEscape;
                Exiled.Events.Handlers.Player.EnteringPocketDimension -= PlayerEvents.OnPocketEnter;
                Exiled.Events.Handlers.Player.ThrowingGrenade -= PlayerEvents.OnGrenadeThrown;
                Exiled.Events.Handlers.Player.Hurting -= PlayerEvents.OnPlayerHurt;
                Exiled.Events.Handlers.Player.Dying -= PlayerEvents.OnPlayerDeath;
                Exiled.Events.Handlers.Player.InteractingElevator -= PlayerEvents.OnElevatorInteraction;
                Exiled.Events.Handlers.Player.Handcuffing -= PlayerEvents.OnPlayerHandcuffed;
                Exiled.Events.Handlers.Player.RemovingHandcuffs -= PlayerEvents.OnPlayerFreed;
                Exiled.Events.Handlers.Player.Joined -= PlayerEvents.OnPlayerJoin;
                Exiled.Events.Handlers.Player.Left -= PlayerEvents.OnPlayerLeave;
                Exiled.Events.Handlers.Player.ChangingRole -= PlayerEvents.OnSetClass;
            }

            MapEvents = null;
            ServerEvents = null;
            PlayerEvents = null;
        }

        public static string SecurityKey;

        public override void OnEnabled()
        {
            config = Config;
            // Load the event handlers.
            if (!Config.IsEnabled)
                return;
            SecurityKey =
                GameCore.ConfigFile.ServerConfig.GetString("cm_plugininterface_key", "None");
            if (SecurityKey == "None")
            {
                SecurityKey = Config.SecurityKey;
            }
            else
            {
                Initializer.Logger.Warn("PluginInterface",
                    "cm_plugininterface_key is depricated, please use the new exiled config system, this option will be removed in future release");
            }

            if (SecurityKey != "None")
            {
                // Start the HTTP server.
                if (Config.QueryOverride)
                {
                    harmony = new Harmony("com.cedmodWAPI.patch");
                    harmony.PatchAll();
                }

                if (Config.NewWebSocketSystem)
                    WS.WebSocketServer.Start(Config.Port);
                else
                    WebService.StartWebServer();
            }
            else
                Initializer.Logger.Warn("PluginInterface",
                    "cm_plugininterface_key is set to none plugin will nog load due to security risks");

            MapEvents = new MapEvents();
            ServerEvents = new ServerEvents();
            PlayerEvents = new PlayerEvents();
            Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += CommandHandler.HandleCommand;
            if (Config.NewWebSocketSystem)
            {
                Exiled.Events.Handlers.Map.Decontaminating += MapEvents.OnDecon;
                Exiled.Events.Handlers.Warhead.Starting += MapEvents.OnWarheadStart;
                Exiled.Events.Handlers.Warhead.Stopping += MapEvents.OnWarheadCancelled;
                Exiled.Events.Handlers.Warhead.Detonated += MapEvents.OnWarheadDetonation;

                Exiled.Events.Handlers.Server.SendingRemoteAdminCommand += ServerEvents.OnCommand;
                Exiled.Events.Handlers.Server.WaitingForPlayers += ServerEvents.OnWaitingForPlayers;
                Exiled.Events.Handlers.Server.SendingConsoleCommand += ServerEvents.OnConsoleCommand;
                Exiled.Events.Handlers.Server.RoundStarted += ServerEvents.OnRoundStart;
                Exiled.Events.Handlers.Server.RoundEnded += ServerEvents.OnRoundEnd;
                Exiled.Events.Handlers.Server.RespawningTeam += ServerEvents.OnRespawn;
                Exiled.Events.Handlers.Server.ReportingCheater += ServerEvents.OnCheaterReport;

                Exiled.Events.Handlers.Player.UsingMedicalItem += PlayerEvents.OnMedicalItem;
                Exiled.Events.Handlers.Scp079.InteractingTesla += PlayerEvents.On079Tesla;
                Exiled.Events.Handlers.Player.EscapingPocketDimension += PlayerEvents.OnPocketEscape;
                Exiled.Events.Handlers.Player.EnteringPocketDimension += PlayerEvents.OnPocketEnter;
                Exiled.Events.Handlers.Player.ThrowingGrenade += PlayerEvents.OnGrenadeThrown;
                Exiled.Events.Handlers.Player.Hurting += PlayerEvents.OnPlayerHurt;
                Exiled.Events.Handlers.Player.Dying += PlayerEvents.OnPlayerDeath;
                Exiled.Events.Handlers.Player.InteractingElevator += PlayerEvents.OnElevatorInteraction;
                Exiled.Events.Handlers.Player.Handcuffing += PlayerEvents.OnPlayerHandcuffed;
                Exiled.Events.Handlers.Player.RemovingHandcuffs += PlayerEvents.OnPlayerFreed;
                Exiled.Events.Handlers.Player.Joined += PlayerEvents.OnPlayerJoin;
                Exiled.Events.Handlers.Player.Left += PlayerEvents.OnPlayerLeave;
                Exiled.Events.Handlers.Player.ChangingRole += PlayerEvents.OnSetClass;
            }
        }

        public override void OnReloaded()
        {
        }

        public static string DecryptString(string cipherText, byte[] key, byte[] iv)
        {
            // Instantiate a new Aes object to perform string symmetric encryption
            Aes encryptor = Aes.Create();

            encryptor.Mode = CipherMode.CBC;
            //encryptor.KeySize = 256;
            //encryptor.BlockSize = 128;
            //encryptor.Padding = PaddingMode.Zeros;

            // Set key and IV
            encryptor.Key = key;
            encryptor.IV = iv;

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            // Instantiate a new encryptor from our Aes object
            ICryptoTransform aesDecryptor = encryptor.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aesDecryptor, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the decrypted byte array to string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the decrypted data as a string
            return plainText;
        }
    }
}