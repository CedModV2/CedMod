using System.Collections.Generic;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.Outbreak
{
	public class Methods
	{
		private readonly Plugin plugin;
		public Methods(Plugin plugin) => this.plugin = plugin;

		public void EnableGamemode()
		{
			plugin.GamemodeEnabled = true;
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcAddElement("<color=green>Outbreak gamemode is starting next round..</color>", 5, Broadcast.BroadcastFlags.Normal);
		}

		public void DisableGamemode()
		{
			plugin.GamemodeEnabled = false;
			Timing.KillCoroutines("blackout");
		}
		public IEnumerator<float> SpawnAlphas()
		{
			yield return Timing.WaitForSeconds(1f);

			foreach (ReferenceHub player in Plugin.GetHubs())
			{
				if (!player.characterClassManager.IsAnyScp())
					continue;
				
				player.characterClassManager.SetPlayersClass(RoleType.Scp0492, player.gameObject);
				yield return Timing.WaitForSeconds(1.5f);

				player.playerMovementSync.OverridePosition(Plugin.GetRandomSpawnPoint(RoleType.Scp049),
					player.gameObject.transform.rotation.y);
				player.playerStats.maxHP = plugin.ZombieHealth;
				player.playerStats._health = plugin.ZombieHealth;
				Cassie.CassieMessage("warning . power system unstable . power Failure may .g2 .g1 .g2 .g4", true, true);
				Timing.WaitForSeconds(7.50f);
				Timing.RunCoroutine(Run(), "blackout");
			}
		}

		public IEnumerator<float> Run()
		{
			Timing.RunCoroutine(CedMod.Functions.LightsOut(false), "blackout");
			yield return Timing.WaitForSeconds(UnityEngine.Random.Range(60f, 200));
		}

		public IEnumerator<float> RespawnZombie(ReferenceHub hub)
		{
			yield return Timing.WaitForSeconds(1f);

			hub.characterClassManager.SetPlayersClass(RoleType.Scp0492, hub.gameObject);
		}
	}
}