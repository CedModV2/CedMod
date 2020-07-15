using GameCore;
using MEC;
using UnityEngine;

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
            if (ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
                GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        /// <inheritdoc cref="Events.Handlers.Server.OnEndingRound(EndingRoundEventArgs)"/>
        public void OnEndingRound(EndingRoundEventArgs ev)
        {
            Timing.KillCoroutines("airstrike");
        }
    }
}