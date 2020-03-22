using EXILED;
using EXILED.Extensions;
using Grenades;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace CedMod
{
    public class Misc
    {
        internal static List<string> cassieAnnounce = new List<string>()
        {
         "THE ALPHA WARHEAD HAS BEEN DESTROYED",
         "AWAITINGRECONTAINMENT SCP 4 20 J",
         "ARREST CLASSD",
         "MTFUNIT IS CORRUPTED",
         "DEFENSE AGAINST THE DARK FORCE",
         "THE G O C XMAS_HASENTERED MTFUNIT",
         "XMAS_BOUNCYBALLS",
         "MEMORY AGENT IS IN FACILITY",
         "YOU ALL FAILED",
         "FEMUR BREAKER DOES NOT WORK",
         "SOME LABORATORY THIS IS",
         "FACILITY POWER LEVEL IS OVER 9000",
         "L S HAS SCP 2 6 8",
         "THE CHAOSINSURGENCY IS NOW THE G O C",
         "THE GROUND IS NOW AN ANOMALY",
         "KILL ALL SCIENTISTS",
         "RUN FOR YOUR ESCAPE",
         "BELIEVE IN THE HELICOPTER",
         "INTERCOM IS READY",
         "GET A REAL G F TODAY",
         "TUESDAY IS THE WORST DAY",
         "I NEED YOUR CREDIT CARD INFORMATION",
         "INSTALLING O5 COMMAND IN FACILITY",
         "THE MILITARY ARE DEAD",
         "EXECUTIVE DEACTIVATED",
         "SERPENTS HAND IS ON SCP 4 20 J",
         "VIRUS INFECTION HAS TURNED ON THE ALPHA WARHEAD",
         "WELCOME TO WEDNESDAY",
         "ATTENTION I HAVE ACTIVATED THE WAY ON SURFACE",
         "MONDAY IS NOT REAL",
         "DATA ANALYSIS COMPLETED . . INITIALIZING SURFACE WARHEAD",
         "INTERCOM IS UNABLE TO BE PAUSED",
         "THE PLAGUE IS REAL",
         "SITE SECURITY IS DEAD",
         "PLEASE RESPAWN",
         "REPEAT RADIATION QUERY",
         "YEAR IS OVER",
         "DIGITAL DEVICE IS KILL",
         "A B C D E F G H I J K",
         "SIGMA WOOD",
         "WEAPONS ARE NOT WELCOME",
         "IT IS WEDNESDAY MY FACILITY PERSONNEL"
        };
        public List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();
        private System.Random rand = new System.Random();
        public Plugin plugin;
        public Misc(Plugin plugin) => this.plugin = plugin;
        bool opened = false;
        bool cassie = false;
        public Door[] array2 = null;
        bool dofunny()
        {
            DateTime utcNow = DateTime.UtcNow;
            if (utcNow.Day == 1 && utcNow.Month == 4)
            {
                return true;
            }
            else
            {
                if (utcNow.Day == 2 && utcNow.Month == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public void OnRoundStart()
        {
            opened = false;
            cassie = false;
            array2 = UnityEngine.Object.FindObjectsOfType<Door>();
            if (dofunny())
            {
                Coroutines.Add(Timing.RunCoroutine(randomAnnoucements()));
            }
        }
        public void OnRoundEnd()
        {
            if (dofunny())
            {
                foreach (CoroutineHandle handle in Coroutines)
                    Timing.KillCoroutines(handle);
                float rand = UnityEngine.Random.Range(1, 100);
                if (rand <= 50)
                {
                    foreach (ReferenceHub hub in Player.GetHubs())
                    {
                        GrenadeManager gm = hub.GetComponent<GrenadeManager>();
                        GrenadeSettings grenade = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.GrenadeFrag);
                        if (grenade == null)
                        {
                            return;
                        }
                        Grenade component = UnityEngine.Object.Instantiate(grenade.grenadeInstance).GetComponent<Grenade>();
                        component.InitData(gm, Vector3.zero, Vector3.zero, 0f);
                        NetworkServer.Spawn(component.gameObject);
                    }
                }
                else
                {
                    Cassie.CassieMessage("XMAS_BOUNCYBALLS", false, false);
                    foreach (ReferenceHub hub in Player.GetHubs())
                    {
                        Vector3 spawnrand = new Vector3(UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f), UnityEngine.Random.Range(0f, 2f));
                        GrenadeManager gm = hub.GetComponent<GrenadeManager>();
                        GrenadeSettings ball = gm.availableGrenades.FirstOrDefault(g => g.inventoryID == ItemType.SCP018);
                        if (ball == null)
                        {
                            return;
                        }
                        Grenade component = UnityEngine.Object.Instantiate(ball.grenadeInstance).GetComponent<Scp018Grenade>();
                        component.InitData(gm, spawnrand, Vector3.zero);
                        float size = UnityEngine.Random.Range(1f, 5f);
                        component.gameObject.transform.localScale = new Vector3(size, size, size);
                        NetworkServer.Spawn(component.gameObject);
                    }
                }
            }

        }
        public void OnDoorAccess(ref DoorInteractionEvent ev) // dont ask me why i did this i was bored
        {
                foreach (Door d in array2)
                {
                float y = d.transform.position.y;
                if (d.name.StartsWith("PrisonDoor") && ev.Door.name.StartsWith("PrisonDoor"))
                    {
                        if (GameCore.ConfigFile.ServerConfig.GetBool("cm_dclassdooropen", false) && !opened)
                        {
                            if (!d.isOpen)
                            {
                                d.ChangeState(true);
                            }
                        }
                        if (GameCore.ConfigFile.ServerConfig.GetBool("cm_dclasscassieannounce", false) && !cassie)
                        {
                            cassie = true;
                            EXILED.Extensions.Cassie.CassieMessage("pitch_.1 .g2 .g4 pitch_.9 alert . d class and scp breach JAM_050_6 detected .g2 .g5", false, true);
                        }
                    }
                }
                opened = true;
        }
        public IEnumerator<float> randomAnnoucements()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(rand.Next(200, 600));
                int announcementid = rand.Next(0, cassieAnnounce.Count - 1);
                Cassie.CassieMessage(cassieAnnounce[announcementid], true, false);
            }
        }
    }
}
