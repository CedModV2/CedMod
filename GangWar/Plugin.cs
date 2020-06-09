using EXILED;

namespace CedMod.GameMode.GangWar
{
	public class GangWar : EXILED.Plugin
	{
		public Methods Functions { get; private set; }
		public EventHandlers EventHandlers { get; private set; }
		public Commands Commands { get; private set; }

		public bool Enabled;

		internal bool GamemodeEnabled = false;
		internal bool RoundStarted = false;

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
			Events.RemoteAdminCommandEvent += Commands.OnRaCommand;
			Events.PlayerJoinEvent += EventHandlers.OnPlayerJoin;
			Events.TeamRespawnEvent += EventHandlers.OnRespawn;
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

		public override string getName { get; } = "GangWar";

		public void ReloadConfig()
		{
			Enabled = Config.GetBool("GangWar_enabled", true);
		}
	}
}