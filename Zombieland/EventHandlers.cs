using System.Collections.Generic;
using System.Linq;
using EXILED;
using EXILED.Extensions;
using MEC;

namespace CedMod.GameMode.ZombieLand
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
			Timing.RunCoroutine(Start(), "Countdown");
		}

		public IEnumerator<float> Start()
		{
			if (_plugin.GamemodeEnabled)
			{
				yield return Timing.WaitForSeconds(2f);
				_plugin.RoundStarted = true;
				IEnumerable<ReferenceHub> players1 = Player.GetHubs();
				List<ReferenceHub> players = players1.ToList();
				List<ReferenceHub> ntf = new List<ReferenceHub>();

				for (int i = 0; i < _plugin.MaxNtf && players.Count > 2; i++)
				{
					int r = _plugin.Gen.Next(players.Count);
					ntf.Add(players[r]);
					players.Remove(players[r]);
				}

				Timing.RunCoroutine(_plugin.Functions.SpawnMtf(ntf));
				Timing.RunCoroutine(_plugin.Functions.SpawnZombies(players));
				Timing.RunCoroutine(_plugin.Functions.Countdown(players), "Countdown");
				Timing.RunCoroutine(_plugin.Functions.CarePackage(), "CarePackage");
			}

			yield return 0;
		}

		public void OnRoundEnd()
		{
			_plugin.RoundStarted = false;
			Timing.KillCoroutines("Countdown");
			Timing.KillCoroutines("CarePackage");
		}

		public void OnTeamRespawn(ref TeamRespawnEvent ev)
		{
			if (!_plugin.RoundStarted)
				return;
			
			foreach (ReferenceHub hub in ev.ToRespawn.Take(ev.MaxRespawnAmt))
				hub.characterClassManager.SetPlayersClass(RoleType.Scp0492, hub.gameObject);
			ev.ToRespawn = new List<ReferenceHub>();
			ev.MaxRespawnAmt = 0;
		}

		public void OnPlayerJoin(PlayerJoinEvent ev)
		{
			if (!_plugin.GamemodeEnabled)
				return;
			
			if (!_plugin.RoundStarted)
			{
				PlayerManager.localPlayer.gameObject.GetComponent<Broadcast>().RpcClearElements();
				PlayerManager.localPlayer.gameObject.GetComponent<Broadcast>().RpcAddElement("<color=lime>Zombieland Gamemode is starting..</color>", 5, Broadcast.BroadcastFlags.Normal);
			}
			else
			{
				ev.Player.GetComponent<Broadcast>().TargetClearElements(ev.Player.scp079PlayerScript.connectionToClient);
				ev.Player.Broadcast(5, "<color=lime>Currently playing: Zombieland Gamemode.</color>");
			}
		}
	}
}