using System.Collections.Generic;
using EXILED;
using EXILED.ApiObjects;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.GangWar
{
	public class Methods
	{
		private readonly GangWar plugin;
		public Methods(GangWar plugin) => this.plugin = plugin;
		
		public void EnableGamemode()
		{
			plugin.GamemodeEnabled = true;
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcAddElement("<color=green>Gangwar gamemode is enabled for the next round!</color>", 10, Broadcast.BroadcastFlags.Normal);
		}

		public void DisableGamemode()
		{
			plugin.GamemodeEnabled = false;
		}

		public IEnumerator<float> SpawnPlayers()
		{
			yield return Timing.WaitForSeconds(1f);

			List<ReferenceHub> players = Plugin.GetHubs();
			
			for (int i = 0; i < players.Count / 2; i++)
			{
				players[i].characterClassManager.SetPlayersClass(RoleType.ChaosInsurgency, players[i].gameObject);
				players.Remove(players[i]);
				Timing.RunCoroutine(SetInventory(players[i], 0.5f));
			}

			foreach (ReferenceHub hub in players)
			{
				hub.characterClassManager.SetPlayersClass(RoleType.NtfCommander, hub.gameObject);
				Timing.RunCoroutine(SetInventory(hub, 0.5f));
			}
		}

		public IEnumerator<float> SetInventory(ReferenceHub hub, float delay)
		{
			yield return Timing.WaitForSeconds(delay);
			
			hub.inventory.items.Clear();
			
			List<ItemType> items = new List<ItemType>
			{
				ItemType.Adrenaline,
				ItemType.Medkit,
				ItemType.GrenadeFlash,
				ItemType.GrenadeFrag,
				ItemType.Painkillers,
				ItemType.GunE11SR,
			};

			foreach (ItemType item in items)
			{
				hub.inventory.AddNewItem(item, 20);
			}
			hub.SetAmmo(AmmoType.Dropped5, 300);
			hub.SetAmmo(AmmoType.Dropped7, 300);
			hub.SetAmmo(AmmoType.Dropped9, 300);
		}
	}
}