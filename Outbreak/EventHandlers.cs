using System.Collections.Generic;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.Outbreak
{
	public class EventHandlers
	{
		private readonly Plugin _plugin;
		public EventHandlers(Plugin plugin) => _plugin = plugin;

		public void OnWaitingForPlayers()
		{
			_plugin.RoundStarted = false;
		}

		public void OnRoundStart()
		{
			Timing.RunCoroutine(Start(), "Outbreak");
		}

		public void OnRoundEnd()
		{
			_plugin.RoundStarted = false;
			Timing.KillCoroutines("Outbreak");
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!_plugin.GamemodeEnabled)
				return;
			
			if (_plugin.RoundStarted)
				ev.Player.Broadcast(5, "<color=green>Currently playing: Outbreak Gamemode.</color>");
			else
			{
				Broadcast broadcast = PlayerManager.localPlayer.gameObject.GetComponent<Broadcast>();
				broadcast.RpcClearElements();
				broadcast.RpcAddElement("<color=green>Outbreak Gamemode is starting..</color>", 5, Broadcast.BroadcastFlags.Normal);
			}
		}
		public IEnumerator<float> Start()
		{
			if (_plugin.GamemodeEnabled)
			{
				yield return Timing.WaitForSeconds(2f);
				_plugin.RoundStarted = true;

				Timing.RunCoroutine(_plugin.Functions.SpawnAlphas());
			}

			yield return 0;
		}

		public void OnCheckRoundEnd(ref CheckRoundEndEvent ev)
		{
			if (!_plugin.RoundStarted)
				return;
			
			int zCount = 0;
			int hCount = 0;

			IEnumerable<ReferenceHub> players1 = Player.GetHubs();
			List<ReferenceHub> players = players1.ToList();
			foreach (ReferenceHub hub in players)
			{
				if (hub.characterClassManager.CurClass == RoleType.Scp0492)
					zCount++;
				else if (hub.characterClassManager.IsHuman())
					hCount++;
			}

			if (zCount == 0)
			{
				ev.LeadingTeam = RoundSummary.LeadingTeam.FacilityForces;
				ev.ForceEnd = true;
			}
			else if (hCount == 0)
			{
				ev.LeadingTeam = RoundSummary.LeadingTeam.Anomalies;
				ev.ForceEnd = true;
			}
			else
			{
				ev.Allow = false;
			}
		}

		public void OnPlayerDeath(ref PlayerDeathEvent ev)
		{
			if (!_plugin.RoundStarted)
				return;
			
			DamageTypes.DamageType type = ev.Info.GetDamageType();
			
			if (type != DamageTypes.Nuke && type != DamageTypes.Decont && type != DamageTypes.Tesla)
				Timing.RunCoroutine(_plugin.Functions.RespawnZombie(ev.Player));
		}

		public void OnDoorInteraction(ref DoorInteractionEvent ev)
		{
			if (!_plugin.RoundStarted)
				return;
			
			if (ev.Player.characterClassManager.CurClass == RoleType.Scp0492 && ev.Player.playerStats.maxHP == _plugin.ZombieHealth && ev.Door.Networklocked && _plugin.AlphasBreakDoors)
			{
				ev.Door.Networkdestroyed = true;
				if (ev.Door.GrenadesResistant)
					ev.Door.NetworkisOpen = true;
			}
		}
	}
}