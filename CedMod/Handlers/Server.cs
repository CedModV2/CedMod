using System;
using CedMod.INIT;
using CommandSystem;
using Exiled.Events;
using GameCore;
using MEC;
using UnityEngine;
using Console = System.Console;

namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    public class Server
    {
        /// <inheritdoc cref="Events.Handlers.Server.OnWaitingForPlayers"/>
        public void OnWaitingForPlayers()
        {
            Initializer.Logger.Debug("WaitingForPlayers", "waitingforplayers event fired");
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
                GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        public void OnReport(LocalReportingEventArgs ev)
        {
            Initializer.Logger.Debug("LocalReport", "localreport event fired");
            sendDI();
        }

        private void sendDI()
        {
            try
            {
                DiscordIntegration_Plugin.ProcessSTT.SendData(CedModMain.config.ReportContent, CedModMain.config.ReportChannel);
            }
            catch (Exception e)
            {
            }
        }
        /// <inheritdoc cref="Events.Handlers.Server.OnEndingRound(EndingRoundEventArgs)"/>
        public void OnEndingRound(EndingRoundEventArgs ev)
        {
            Initializer.Logger.Debug("RoundEnd", "roundend event fired");
        }

        public void OnSendingRemoteAdmin(SendingRemoteAdminCommandEventArgs ev)
        {
            BanSystem.HandleRACommand(ev);
        }
    }
}