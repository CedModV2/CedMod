using EXILED;
using EXILED.Extensions;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CedMod
{
    public class FunctionsNonStatic
    {
        public Plugin plugin;
        public FunctionsNonStatic(Plugin plugin) => this.plugin = plugin;
        public static void Roundrestart()
        {
            Timing.KillCoroutines("airstrike");
        }
        public void Waitingforplayers()
        {
            if (GameCore.ConfigFile.ServerConfig.GetBool("cm_customloadingscreen", true))
            {
                GameObject.Find("StartRound").transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }
        public void SetScale(GameObject target, float x, float y, float z) //this code may have been yoinked
        {
            try
            {
                NetworkIdentity identity = target.GetComponent<NetworkIdentity>();


                target.transform.localScale = new Vector3(x, y ,z);

                ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
                destroyMessage.netId = identity.netId;


                foreach (GameObject player in PlayerManager.players)
                {
                    if (player == target)
                        continue;

                    NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;

                    playerCon.Send(destroyMessage, 0);

                    object[] parameters = new object[] { identity, playerCon };
                    typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
                }
            }
            catch (Exception e)
            {
                Log.Info($"Set Scale error: {e}");
            }
        }
    }
    public static class Functions
    {
        public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic |
                                 BindingFlags.Static | BindingFlags.Public;
            MethodInfo info = type.GetMethod(methodName, flags);
            info?.Invoke(null, param);
        }
        public static void BC(uint time, string msg)
        {
            foreach (ReferenceHub p in Player.GetHubs())
                p.Broadcast(time, msg, false);
        }
        public static void PlayAmbientSound(int id)
        {
            PlayerManager.localPlayer.GetComponent<AmbientSoundPlayer>().RpcPlaySound(Mathf.Clamp(id, 0, 31));
        }
        public enum GRENADE_ID
        {
            FRAG_NADE = 0,
            FLASH_NADE = 1,
            SCP018_NADE = 2
        }
        public static void SpawnGrenade(Vector3 position, bool isFlash = false, float fusedur = -1, ReferenceHub player = null, float scalex = 1, float scaley = 1, float scalez = 1)
        {
            if (player == null) player = ReferenceHub.GetHub(PlayerManager.localPlayer);
            var gm = player.GetComponent<Grenades.GrenadeManager>();
            Grenades.Grenade component = UnityEngine.Object.Instantiate(gm.availableGrenades[isFlash ? (int)GRENADE_ID.FLASH_NADE : (int)GRENADE_ID.FRAG_NADE].grenadeInstance).GetComponent<Grenades.Grenade>();
            if (fusedur != -1) component.fuseDuration = fusedur;
            component.FullInitData(gm, position, Quaternion.Euler(component.throwStartAngle), Vector3.zero, component.throwAngularVelocity);
            component.gameObject.transform.localScale = new Vector3(scalex, scaley, scalez);
            NetworkServer.Spawn(component.gameObject);
        }
        public static class OutsideRandomAirbombPos
        {
            public static List<Vector3> Load()
            {
                return new List<Vector3>{
                new Vector3(UnityEngine.Random.Range(175, 182),  984, UnityEngine.Random.Range( 25,  29)),
                new Vector3(UnityEngine.Random.Range(174, 182),  984, UnityEngine.Random.Range( 36,  39)),
                new Vector3(UnityEngine.Random.Range(174, 182),  984, UnityEngine.Random.Range( 36,  39)),
                new Vector3(UnityEngine.Random.Range(166, 174),  984, UnityEngine.Random.Range( 26,  39)),
                new Vector3(UnityEngine.Random.Range(169, 171),  987, UnityEngine.Random.Range(  9,  24)),
                new Vector3(UnityEngine.Random.Range(174, 175),  988, UnityEngine.Random.Range( 10,  -2)),
                new Vector3(UnityEngine.Random.Range(186, 174),  990, UnityEngine.Random.Range( -1,  -2)),
                new Vector3(UnityEngine.Random.Range(186, 189),  991, UnityEngine.Random.Range( -1, -24)),
                new Vector3(UnityEngine.Random.Range(186, 189),  991, UnityEngine.Random.Range( -1, -24)),
                new Vector3(UnityEngine.Random.Range(185, 189),  993, UnityEngine.Random.Range(-26, -34)),
                new Vector3(UnityEngine.Random.Range(180, 195),  995, UnityEngine.Random.Range(-36, -91)),
                new Vector3(UnityEngine.Random.Range(148, 179),  995, UnityEngine.Random.Range(-45, -72)),
                new Vector3(UnityEngine.Random.Range(118, 148),  995, UnityEngine.Random.Range(-47, -65)),
                new Vector3(UnityEngine.Random.Range( 83, 118),  995, UnityEngine.Random.Range(-47, -65)),
                new Vector3(UnityEngine.Random.Range( 13,  15),  995, UnityEngine.Random.Range(-18, -48)),
                new Vector3(UnityEngine.Random.Range( 84,  88),  988, UnityEngine.Random.Range(-67, -70)),
                new Vector3(UnityEngine.Random.Range( 68,  83),  988, UnityEngine.Random.Range(-52, -66)),
                new Vector3(UnityEngine.Random.Range( 53,  68),  988, UnityEngine.Random.Range(-53, -63)),
                new Vector3(UnityEngine.Random.Range( 12,  49),  988, UnityEngine.Random.Range(-47, -66)),
                new Vector3(UnityEngine.Random.Range( 38,  42),  988, UnityEngine.Random.Range(-40, -47)),
                new Vector3(UnityEngine.Random.Range( 38,  43),  988, UnityEngine.Random.Range(-32, -38)),
                new Vector3(UnityEngine.Random.Range(-25,  12),  988, UnityEngine.Random.Range(-50, -66)),
                new Vector3(UnityEngine.Random.Range(-26, -56),  988, UnityEngine.Random.Range(-50, -66)),
                new Vector3(UnityEngine.Random.Range( -3, -24), 1001, UnityEngine.Random.Range(-66, -73)),
                new Vector3(UnityEngine.Random.Range(  5,  28), 1001, UnityEngine.Random.Range(-66, -73)),
                new Vector3(UnityEngine.Random.Range( 29,  55), 1001, UnityEngine.Random.Range(-66, -73)),
                new Vector3(UnityEngine.Random.Range( 50,  54), 1001, UnityEngine.Random.Range(-49, -66)),
                new Vector3(UnityEngine.Random.Range( 24,  48), 1001, UnityEngine.Random.Range(-41, -46)),
                new Vector3(UnityEngine.Random.Range(  5,  24), 1001, UnityEngine.Random.Range(-41, -46)),
                new Vector3(UnityEngine.Random.Range( -4, -17), 1001, UnityEngine.Random.Range(-41, -46)),
                new Vector3(UnityEngine.Random.Range(  4,  -4), 1001, UnityEngine.Random.Range(-25, -40)),
                new Vector3(UnityEngine.Random.Range( 11, -11), 1001, UnityEngine.Random.Range(-18, -21)),
                new Vector3(UnityEngine.Random.Range(  3,  -3), 1001, UnityEngine.Random.Range( -4, -17)),
                new Vector3(UnityEngine.Random.Range(  2,  14), 1001, UnityEngine.Random.Range(  3,  -3)),
                new Vector3(UnityEngine.Random.Range( -1, -13), 1001, UnityEngine.Random.Range(  4,  -3))
            };
            }
        }
        internal static class Coroutines
        {
            public static bool isAirBombGoing = false;
            public static IEnumerator<float> AirSupportBomb(int waitforready = 5, int duration = 30)
            {
                Log.Info($"[AirSupportBomb] booting...");
                if (isAirBombGoing)
                {
                    Log.Info($"[Airbomb] already booted, cancel.");
                    yield break;
                }
                else
                {
                    isAirBombGoing = true;
                }

                    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("danger . outside zone emergency termination sequence activated .", false, true);
                    yield return Timing.WaitForSeconds(5f);

                Log.Info($"[AirSupportBomb] charging...");
                while (waitforready > 0)
                {
                    Functions.PlayAmbientSound(7);
                    waitforready--;
                    yield return Timing.WaitForSeconds(1f);
                }
                Timing.RunCoroutine(AirSupportstop(duration), "airstrike");
                Log.Info($"[AirSupportBomb] throwing...");
                int throwcount = 0;
                while (isAirBombGoing)
                {
                    List<Vector3> randampos = OutsideRandomAirbombPos.Load().OrderBy(x => Guid.NewGuid()).ToList();
                    foreach (var pos in randampos)
                    {
                        Functions.SpawnGrenade(pos, false, 0.1f);
                        yield return Timing.WaitForSeconds(0.1f);
                    }
                    throwcount++;
                    Log.Info($"[AirSupportBomb] throwcount:{throwcount}");
                    yield return Timing.WaitForSeconds(0.25f);
                }
                    PlayerManager.localPlayer.GetComponent<MTFRespawn>().RpcPlayCustomAnnouncement("outside zone termination completed .", false, true);

                Log.Info($"[AirSupportBomb] Ended.");
                yield break;
            }
            public static IEnumerator<float> AirSupportstop(int duration = 30)
            {
                yield return Timing.WaitForSeconds(duration);
                isAirBombGoing = false;
            }
        }
    }
}
