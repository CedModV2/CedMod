using CedMod.INIT;
using EXILED;
using System.Collections.Generic;

namespace CedMod
{
    public class Misc
    {
        public Plugin plugin;
        public Misc(Plugin plugin) => this.plugin = plugin;
        bool opened = false;
        bool cassie = false;
        public Door[] array2 = null;
        public void OnRoundStart()
        {
            opened = false;
            cassie = false;
            array2 = UnityEngine.Object.FindObjectsOfType<Door>();
        }
        public void OnDoorAccess(ref DoorInteractionEvent ev) 
        {
            Initializer.logger.Warn("TEST", ev.Door.name);
                foreach (Door d in array2)
                {
                float y = d.transform.position.y;
                if (d.name.StartsWith("PrisonDoor") && ev.Door.name.StartsWith("PrisonDoor"))
                    {
                    Initializer.logger.Warn("TEST", d.DoorName + "doorname:" + d.name + "evname:" + ev.Door.name + "doorname:" + ev.Door.DoorName);
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
    }
}
