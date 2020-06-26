using System.Collections.Generic;
using EXILED.Extensions;
using MEC;
using UnityEngine;

namespace CedMod.GameMode.Survival
{
	public class Methods
	{
		private readonly Player _plugin;
		public Methods(Player plugin) => _plugin = plugin;

		public void EnableGamemode()
		{
			_plugin.GamemodeEnabled = true;
			PlayerManager.localPlayer.GetComponent<Broadcast>().RpcAddElement("<color=yellow>Survival of the Fittest gamemode is enabled for the next round!</color>", 10, Broadcast.BroadcastFlags.Normal);
		}

		public void DisableGamemode()
		{
			_plugin.GamemodeEnabled = false;
		}

		public IEnumerator<float> SpawnDbois(List<ReferenceHub> hubs)
		{
			foreach (ReferenceHub hub in hubs)
			{
				hub.characterClassManager.NetworkCurClass = RoleType.ClassD;
				hub.Broadcast(30, "<color=orange>You are a Dboi, you need to find a hiding place and pray you are the last person alive, the nuts wil be release at their chamber in 2 minutes.</color>");
				yield return Timing.WaitForOneFrame * 30;
				
				hub.inventory.Clear();
				hub.inventory.AddNewItem(ItemType.Flashlight);
                hub.inventory.AddNewItem(ItemType.KeycardO5);
				hub.playerMovementSync.OverridePosition(Map.GetRandomSpawnPoint(RoleType.Scp096), 0f);
			}
		}

		public IEnumerator<float> SpawnNuts(List<ReferenceHub> hubs)
		{
			foreach (ReferenceHub hub in hubs)
			{
				hub.characterClassManager.NetworkCurClass = RoleType.Scp173;
				hub.Broadcast(120, "<color=red>You are a Nut, once this notice disapears (2 minutes), you will be set loose to kill the Dbois!</color>");
                hub.serverRoles.BypassMode = true;
                yield return Timing.WaitForOneFrame * 30;
                hub.playerMovementSync.OverridePosition(Map.GetRandomSpawnPoint(RoleType.Scp173), 0f);
			}

			yield return Timing.WaitForSeconds(30f);
            Cassie.CassieMessage("90 seconds", false, false);
            yield return Timing.WaitForSeconds(30f);
            Cassie.CassieMessage("60 seconds", false, false);
            yield return Timing.WaitForSeconds(30f);
            Cassie.CassieMessage("30 seconds", false, false);
            yield return Timing.WaitForSeconds(18);
            Cassie.CassieMessage("10 . 9 . 8 . 7 . 6 . 5 . 4 . 3 . 2 . 1 . 0", false, false);
            yield return Timing.WaitForSeconds(12);
            Cassie.CassieMessage("ready or not here come the snap snap", false, false);
            foreach (Door door in Object.FindObjectsOfType<Door>())
            {
                if (door.DoorName == "173")
                {
                    door.Networklocked = true;
                    door.NetworkisOpen = true;
                    door.enabled = true;
                    door.DestroyDoor(true);
                    door.destroyed = true;
                }
            }// bruh
            foreach (Door door in Object.FindObjectsOfType<Door>())
            {
                if (door.DoorName == "173" || door.DoorName == "914" || door.DoorName == "CHECKPOINT_ENT" || door.DoorName == "GATE_A" || door.DoorName == "GATE_B" || door.DoorName == "NUKE_SURFACE")
                {
                }
                else
                {
                    door.Networklocked = false;
                    door.NetworkisOpen = false;
                    door.enabled = true;
                }
            }
            yield return Timing.WaitForSeconds(1f);
            Timing.RunCoroutine(DoBlackout(), "blackout");
            yield return Timing.WaitForSeconds(Random.Range(180f, 360f));
            Cassie.CassieMessage("pitch_.2 .g5 yield_0,5 .g4 pitch_1 low power system operational . opening entrance zone", false, true);
            foreach (Door door in Object.FindObjectsOfType<Door>())
            {
	            if (door.DoorName == "CHECKPOINT_ENT")
	            {
		            door.Networklocked = true;
		            door.NetworkisOpen = true;
		            door.enabled = false;
	            }
            }
            yield return Timing.WaitForSeconds(Random.Range(180f, 360f));
            Cassie.CassieMessage("pitch_.2 .g5 yield_0,5 .g4 pitch_1 high power system operational . opening surface zone gates", false, true);
            foreach (Door door in Object.FindObjectsOfType<Door>())
            {
	            if (door.DoorName == "GATE_A")
	            {
		            door.Networklocked = true;
		            door.NetworkisOpen = true;
		            door.enabled = false;
	            }
	            else
	            {
		            if (door.DoorName == "GATE_B")
		            {
			            door.Networklocked = true;
			            door.NetworkisOpen = true;
			            door.enabled = false;
		            }
	            }
            }
            yield return Timing.WaitForSeconds(300f);
            Cassie.CassieMessage("pitch_.2 .g5 yield_0,5 .g4 pitch_1 facility detonation in t minus 5 minutes", false, true);
            Map.Broadcast("<color=red>the facility will detonate in 5 minutes leave now or die :)</color>", 110);
            yield return Timing.WaitForSeconds(290f);
            Map.Broadcast("<color=red>10</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>9</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>8</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>7</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>6</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>5</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>4</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>3</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>2</color>", 1);
            yield return Timing.WaitForSeconds(1f);
            Map.Broadcast("<color=red>1</color>", 1);
            Map.DetonateNuke();
		}

		public IEnumerator<float> DoBlackout()
		{
			Generator079.mainGenerator.RpcCustomOverchargeForOurBeautifulModCreators(9.5f, false);
	        yield return Timing.WaitForSeconds(10f);
            Timing.RunCoroutine(DoBlackout(), "blackout");
        }

		public void DoSetup()
		{
			foreach (Door door in Object.FindObjectsOfType<Door>())
				if (door.DoorName == "CHECKPOINT_ENT" || door.DoorName == "173" || door.DoorName == "914" || door.DoorName == "GATE_A" || door.DoorName == "GATE_B" || door.DoorName == "NUKE_SURFACE")
				{
					door.Networklocked = true;
					door.NetworkisOpen = false;
                    door.enabled = false;
				}
			foreach (Pickup item in Object.FindObjectsOfType<Pickup>())
				item.Delete();
		}
	}
}