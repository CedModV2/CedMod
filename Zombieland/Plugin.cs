using System;
using EXILED;

namespace CedMod.GameMode.ZombieLand
{
	public class Plugin : EXILED.Plugin
	{
		public Methods Functions { get; private set; }
		public EventHandlers EventHandlers { get; private set; }
		public Commands Commands { get; private set; }

		public bool Enabled;
		public int MaxNtf;
		public int NtfHealth;

		public bool GamemodeEnabled;
		public bool RoundStarted;
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
			Events.TeamRespawnEvent += EventHandlers.OnTeamRespawn;
			Events.RemoteAdminCommandEvent += Commands.OnRaCommand;
			Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
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

		public override string getName { get; } = "Zombieland";

		public void ReloadConfig()
		{
			Enabled = Config.GetBool("Zombieland_enabled", true);
			MaxNtf = Config.GetInt("Zombieland_max_ntf", 3);
			NtfHealth = Config.GetInt("Zombieland_ntf_health", 500);
		}
	}
}