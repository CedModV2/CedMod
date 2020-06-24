using System.Collections.Generic;
using System.Linq;
using EXILED.Extensions;
using MEC;
using UnityEngine;

namespace CedMod.GameMode.Outbreak
{
	public class Methods
	{
		private readonly Plugin _plugin;
		public Methods(Plugin plugin) => _plugin = plugin;

		public void EnableGamemode()
		{
			_plugin.GamemodeEnabled = true;
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcAddElement("<color=green>Outbreak gamemode is starting next round..</color>", 5, Broadcast.BroadcastFlags.Normal);
		}

		public void DisableGamemode()
		{
			_plugin.GamemodeEnabled = false;
			Timing.KillCoroutines("blackout");
		}
		public IEnumerator<float> SpawnAlphas()
		{
			yield return Timing.WaitForSeconds(1f);
			IEnumerable<ReferenceHub> players1 = Player.GetHubs();
			List<ReferenceHub> players = players1.ToList();
			foreach (ReferenceHub player in players)
			{
				if (!player.characterClassManager.IsAnyScp())
					continue;
				
				player.characterClassManager.SetPlayersClass(RoleType.Scp0492, player.gameObject);
				yield return Timing.WaitForSeconds(1.5f);

				player.playerMovementSync.OverridePosition(Map.GetRandomSpawnPoint(RoleType.Scp049),
					player.gameObject.transform.rotation.y);
				player.playerStats.maxHP = _plugin.ZombieHealth;
				player.playerStats._health = _plugin.ZombieHealth;
				Cassie.CassieMessage("warning . power system unstable . power Failure may .g2 .g1 .g2 .g4", true, true);
				Timing.WaitForSeconds(7.50f);
				Timing.RunCoroutine(Run(), "blackout");
			}
		}

		public IEnumerator<float> Run()
		{
			Timing.RunCoroutine(Functions.LightsOut(false), "blackout");
			yield return Timing.WaitForSeconds(Random.Range(60f, 200));
		}

		public IEnumerator<float> RespawnZombie(ReferenceHub hub)
		{
			yield return Timing.WaitForSeconds(1f);

			hub.characterClassManager.SetPlayersClass(RoleType.Scp0492, hub.gameObject);
		}
	}
}