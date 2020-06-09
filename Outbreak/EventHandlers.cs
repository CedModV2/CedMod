using System.Collections.Generic;
using Dissonance;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.Outbreak
{
	public class EventHandlers
	{
		private readonly Plugin plugin;
		public EventHandlers(Plugin plugin) => this.plugin = plugin;

		public void OnWaitingForPlayers()
		{
			plugin.RoundStarted = false;
		}

		public void OnRoundStart()
		{
			if (!plugin.GamemodeEnabled)
				return;
			
			plugin.RoundStarted = true;

			Timing.RunCoroutine(plugin.Functions.SpawnAlphas());
		}

		public void OnRoundEnd()
		{
			plugin.RoundStarted = false;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!plugin.GamemodeEnabled)
				return;
			
			if (plugin.RoundStarted)
				ev.Player.Broadcast(5, "<color=green>Currently playing: Outbreak Gamemode.</color>");
			else
			{
				Broadcast broadcast = PlayerManager.localPlayer.gameObject.GetComponent<Broadcast>();
				broadcast.RpcClearElements();
				broadcast.RpcAddElement("<color=green>Outbreak Gamemode is starting..</color>", 5, Broadcast.BroadcastFlags.Normal);
			}
		}

		public void OnCheckRoundEnd(ref CheckRoundEndEvent ev)
		{
			if (!plugin.RoundStarted)
				return;
			
			int zCount = 0;
			int hCount = 0;

			foreach (ReferenceHub hub in Plugin.GetHubs())
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
			if (!plugin.RoundStarted)
				return;
			
			DamageTypes.DamageType type = ev.Info.GetDamageType();
			
			if (type != DamageTypes.Nuke && type != DamageTypes.Decont && type != DamageTypes.Tesla)
				Timing.RunCoroutine(plugin.Functions.RespawnZombie(ev.Player));
		}

		public void OnDoorInteraction(ref DoorInteractionEvent ev)
		{
			if (!plugin.RoundStarted)
				return;
			
			if (ev.Player.characterClassManager.CurClass == RoleType.Scp0492 && ev.Player.playerStats.maxHP == plugin.ZombieHealth && ev.Door.Networklocked && plugin.AlphasBreakDoors)
			{
				ev.Door.Networkdestroyed = true;
				if (ev.Door.GrenadesResistant)
					ev.Door.NetworkisOpen = true;
			}
		}
	}
}