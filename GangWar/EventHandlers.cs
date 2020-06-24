using System;
using System.Collections.Generic;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.GangWar
{
	public class EventHandlers
	{
		private readonly GangWar _plugin;
		public EventHandlers(GangWar plugin) => _plugin = plugin;

		public void OnWaitingForPlayers()
		{
			_plugin.RoundStarted = false;
		}

		public void OnRoundStart()
		{
			Timing.RunCoroutine(Start(), "Gangwar");
		}
		public IEnumerator<float> Start()
		{
			if (_plugin.GamemodeEnabled)
			{
				try
				{
					_plugin.RoundStarted = true;
					Timing.RunCoroutine(_plugin.Functions.SpawnPlayers());
				}
				catch (Exception e)
				{
					Log.Error($"ROUND START ERROR< REEEE: {e}");
				}
			}

			yield return 0;
		}
		public void OnRoundEnd()
		{
			_plugin.RoundStarted = false;
			Timing.KillCoroutines("Gangwar");
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!_plugin.GamemodeEnabled) 
				return;
			
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcClearElements();
			if (_plugin.RoundStarted)
				ev.Player.Broadcast(5, "<color=green>Currently playing Gangwar Gamemode!</color>");
			else
				PlayerManager.localPlayer.GetComponent<Broadcast>()
					.RpcAddElement("<color=green>Gangwar is starting..</color>", 5, Broadcast.BroadcastFlags.Normal);
		}

		public void OnRespawn(ref TeamRespawnEvent ev)
		{
			if (!_plugin.RoundStarted)
				return;
			
			ev.IsChaos = RoundSummary.singleton.CountTeam(Team.MTF) > RoundSummary.singleton.CountTeam(Team.CHI);

			foreach (ReferenceHub hub in ev.ToRespawn)
				Timing.RunCoroutine(_plugin.Functions.SetInventory(hub, 2f));
		}
	}
}