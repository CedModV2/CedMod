using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CedMod.INIT;
using GameCore;
using Grenades;
using MEC;
using Mirror;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CedMod.Handlers
{
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;

    /// <summary>
    /// Handles server-related events.
    /// </summary>
    public class Player
    {
        public void OnJoin(JoinedEventArgs ev)
        {
            Task.Factory.StartNew(() => { BanSystem.HandleJoin(ev); });
            foreach (string b in ConfigFile.ServerConfig.GetStringList("cm_nicknamefilter"))
            {
                if (ev.Player.Nickname.ToUpper().Contains(b.ToUpper()))
                {
                    ev.Player.ReferenceHub.nicknameSync.DisplayName = "Filtered name";
                }
            }

            if (!RoundSummary.RoundInProgress() && ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
                Timing.RunCoroutine(Playerjoinhandle(ev));
        }

        public static ItemType GetRandomItem()
        {
            Random random = new Random();
            int index = UnityEngine.Random.Range(0, CedModMain.items.Count);
            return CedModMain.items[index];
        }
        
        public IEnumerator<float> Playerjoinhandle(JoinedEventArgs ev)
        {
            ReferenceHub Player = ev.Player.ReferenceHub;
            yield return Timing.WaitForSeconds(0.5f);
            if (!RoundSummary.RoundInProgress())
            {
                Player.characterClassManager.SetPlayersClass(RoleType.Tutorial, Player.gameObject);
                ev.Player.IsGodModeEnabled = false;
                yield return Timing.WaitForSeconds(0.2f);
                ev.Player.Position = (new Vector3(176.2169f, 987.3649f,112.187f));
                yield return Timing.WaitForSeconds(0.2f);
                Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
                GrenadeManager gm = ev.Player.GrenadeManager;
                GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
                if (ball == null)
                {
                    yield return 0f;
                }
                ball.grenadeInstance.transform.localScale = new Vector3(3f,3f,3f);
                Grenade component = Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
                component.InitData(gm, spawnrand, Vector3.zero);
                NetworkServer.Spawn(component.gameObject);
            }
            yield return 1f;
        }

        public void OnLeave(LeftEventArgs ev)
        {
        }

        public void OnDying(DyingEventArgs ev)
        {
            Initializer.Logger.Debug("CMEVENT", "Dyingevent triggered");
            FriendlyFireAutoban.HandleKill(ev);
            Timing.RunCoroutine(PlayerStatsDieEv(ev));
        }

        IEnumerator<float> PlayerStatsDieEv(DyingEventArgs ev)
        {
            if (!RoundSummary.RoundInProgress())
                yield return 0f;
            if (FriendlyFireAutoban.IsTeakill(ev))
                Task.Factory.StartNew(() =>
                {
                    API.APIRequest("playerstats/addstat.php",
                        $"?rounds=0&kills=0&deaths=0&teamkills=1&alias={API.GetAlias()}&id={ev.Killer.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Killer.IPAddress}&username={ev.Killer.Nickname}");
                });
            else
                Task.Factory.StartNew(() =>
                {
                    API.APIRequest("playerstats/addstat.php",
                        $"?rounds=0&kills=1&deaths=0&teamkills=0&alias={API.GetAlias()}&id={ev.Killer.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Killer.IPAddress}&username={ev.Killer.Nickname}");
                });
            Task.Factory.StartNew(() =>
            {
                API.APIRequest("playerstats/addstat.php",
                    $"?rounds=0&kills=0&deaths=1&teamkills=0&alias={API.GetAlias()}&id={ev.Target.UserId}&dnt={Convert.ToInt32(ev.Target.ReferenceHub.serverRoles.DoNotTrack)}&ip={ev.Target.IPAddress}&username={ev.Target.Nickname}");
            });
            yield return 0f;
        }
    }
}