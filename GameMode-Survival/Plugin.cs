using System;
using EXILED;

namespace CedMod.GameMode.Survival
{
	public class Player : EXILED.Plugin
	{
		public Methods Functions { get; private set; }
		public EventHandlers EventHandlers { get; private set; }
		public Commands Commands { get; private set; }

		public bool Enabled;
		public int MaxNuts;

		public bool RoundStarted = false;
		public bool GamemodeEnabled = false;
		public Random Gen = new Random();

		public override void OnEnable()
		{
			ReloadConfig();
			if (!Enabled)
				return;

			EventHandlers = new EventHandlers(this);
			Functions = new Methods(this);
			Commands = new Commands(this);

			Events.WaitingForPlayersEvent += EventHandlers.OnWaitingForPlayers;
			Events.RoundStartEvent += EventHandlers.OnRoundStart;
			Events.RoundEndEvent += EventHandlers.OnRoundEnd;
			Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
			Events.TeamRespawnEvent += EventHandlers.OnTeamRespawn;
			Events.RemoteAdminCommandEvent += Commands.OnRaCommand;
            Events.TriggerTeslaEvent += EventHandlers.OnTesla;

        }

		public override void OnDisable()
		{
			Events.WaitingForPlayersEvent -= EventHandlers.OnWaitingForPlayers;
			Events.RoundStartEvent -= EventHandlers.OnRoundStart;
			Events.RoundEndEvent -= EventHandlers.OnRoundEnd;
			Events.RemoteAdminCommandEvent -= Commands.OnRaCommand;

			EventHandlers = null;
			Functions = null;
			Commands = null;
		}

		public override void OnReload()
		{
		}

		public override string getName { get; } = "CedMod.GameMode.Survival";

		public void ReloadConfig()
		{
			Enabled = Config.GetBool("Survival_enabled", true);
			MaxNuts = Config.GetInt("Survival_max_nuts", 3);
		}
	}
}