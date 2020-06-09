using System;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.GangWar
{
	public class EventHandlers
	{
		private readonly GangWar plugin;
		public EventHandlers(GangWar plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers()
		{
			plugin.RoundStarted = false;
		}

		public void OnRoundStart()
		{
			try
			{
				if (!plugin.GamemodeEnabled)
					return;
				
				plugin.RoundStarted = true;
				Timing.RunCoroutine(plugin.Functions.SpawnPlayers());
			}
			catch (Exception e)
			{
				Plugin.Error($"ROUND START ERROR< REEEE: {e}");
			}
		}

		public void OnRoundEnd()
		{
			plugin.RoundStarted = false;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!plugin.GamemodeEnabled) 
				return;
			
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcClearElements();
			if (plugin.RoundStarted)
				ev.Player.Broadcast(5, "<color=green>Currently playing Gangwar Gamemode!</color>");
			else
				PlayerManager.localPlayer.GetComponent<Broadcast>()
					.RpcAddElement("<color=green>Gangwar is starting..</color>", 5, Broadcast.BroadcastFlags.Normal);
		}

		public void OnRespawn(ref TeamRespawnEvent ev)
		{
			if (!plugin.RoundStarted)
				return;
			
			ev.IsChaos = RoundSummary.singleton.CountTeam(Team.MTF) > RoundSummary.singleton.CountTeam(Team.CHI);

			foreach (ReferenceHub hub in ev.ToRespawn)
				Timing.RunCoroutine(plugin.Functions.SetInventory(hub, 2f));
		}
	}
}