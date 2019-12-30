using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CedMod;
using Cryptography;
using MEC;
using Mirror;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace GameCore
{
    // Token: 0x020004A6 RID: 1190
    public class Console : MonoBehaviour
    {
        // Token: 0x1700033F RID: 831
        // (get) Token: 0x06001B8E RID: 7054
        // (set) Token: 0x06001B8F RID: 7055
        public static Console singleton { get; private set; }

        // Token: 0x17000340 RID: 832
        // (get) Token: 0x06001B90 RID: 7056
        // (set) Token: 0x06001B91 RID: 7057
        internal static AsymmetricCipherKeyPair SessionKeys { get; private set; }

        // Token: 0x17000341 RID: 833
        // (get) Token: 0x06001B92 RID: 7058
        // (set) Token: 0x06001B93 RID: 7059
        public static DistributionPlatform Platform { get; private set; }

        // Token: 0x06001B94 RID: 7060
        private void Awake()
        {
            Console._ccs = new ConsoleCommandSender();
            UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
            if (Console.singleton == null)
            {
                Console.singleton = this;
                return;
            }
            UnityEngine.Object.DestroyImmediate(base.gameObject);
        }

        // Token: 0x06001B95 RID: 7061
        private void Start()
        {
            SceneManager.sceneLoaded += this.OnSceneLoaded;
            Initializer.Setup();
            Console.AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
            Console.AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
            Console.StartupArgs = Environment.GetCommandLineArgs();
            if (Console.StartupArgs.Any((string arg) => string.Equals(arg, "-lockhttpmode", StringComparison.OrdinalIgnoreCase)) || File.Exists(FileManager.GetAppFolder(true, false, "") + "LockHttpMode.txt"))
            {
                Console.LockHttpMode = true;
                Console.AddLog("HTTP mode locked", Color.gray, false);
            }
            if (Console.StartupArgs.Any((string arg) => string.Equals(arg, "-httpclient", StringComparison.OrdinalIgnoreCase)) || File.Exists(FileManager.GetAppFolder(true, false, "") + "HttpClient.txt"))
            {
                Console.HttpMode = HttpQueryMode.HttpClient;
                Console.AddLog("HTTP mode switched to HttpClient", Color.gray, false);
            }
            Timing.RunCoroutine(this._RefreshCentralServers(), Segment.FixedUpdate);
            Console.AddLog("Generatig session keys...", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
            Console.SessionKeys = ECDSA.GenerateKeys(384);
            Console.AddLog("Session keys generated (ECDSA)!", Color.green, false);
            Console.Platform = DistributionPlatform.Dedicated;
            Console.AddLog("Running as headless dedicated server. Skipping distribution platform detection.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
        }

        // Token: 0x06001B96 RID: 7062
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Console.AddLog(string.Concat(new string[]
            {
                "Scene Manager: Loaded scene '",
                scene.name,
                "' [",
                scene.path,
                "]"
            }), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
            this.RefreshConsoleScreen();
        }

        // Token: 0x06001B97 RID: 7063
        private void Update()
        {
        }

        // Token: 0x06001B98 RID: 7064
        private void LateUpdate()
        {
        }

        // Token: 0x06001B99 RID: 7065
        private void FixedUpdate()
        {
        }

        // Token: 0x06001B9A RID: 7066
        private void RefreshConsoleScreen()
        {
        }

        // Token: 0x06001B9B RID: 7067
        public static void AddDebugLog(string debugKey, string message, MessageImportance importance, bool nospace = false)
        {
            Color32 c;
            if (ConsoleDebugMode.CheckImportance(debugKey, importance, out c))
            {
                Console.AddLog("[DEBUG_" + debugKey + "] " + message, c, nospace);
            }
        }

        // Token: 0x06001B9C RID: 7068
        public static void AddLog(string text, Color c, bool nospace = false)
        {
            if (ServerStatic.IsDedicated)
            {
                ServerConsole.AddLog(text);
                return;
            }
        }

        // Token: 0x06001B9D RID: 7069
        public static GameObject FindConnectedRoot(NetworkConnection conn)
        {
            try
            {
                GameObject gameObject = conn.playerController.gameObject;
                if (gameObject.CompareTag("Player"))
                {
                    return gameObject;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        // Token: 0x06001B9E RID: 7070
        public string TypeCommand(string cmd, CommandSender sender = null)
        {
            if (cmd.StartsWith(".") && cmd.Length > 1)
            {
                Console.AddLog("Sending command to server: " + cmd.Substring(1), new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                PlayerManager.localPlayer.GetComponent<GameConsoleTransmission>().SendToServer(cmd.Substring(1));
                return string.Empty;
            }
            if (cmd.StartsWith("/") && cmd.Length > 1)
            {
                if (NetworkServer.active)
                {
                    CommandProcessor.ProcessQuery(cmd.Substring(1), sender ?? Console._ccs);
                }
                return string.Empty;
            }
            this._response = string.Empty;
            string[] array = cmd.Split(new char[]
            {
                ' '
            });
            cmd = array[0].ToUpper();
            uint num = PrivateImplementationDetails.ComputeStringHash(cmd);
            if (num <= 1458105184U)
            {
                if (num <= 715817306U)
                {
                    if (num <= 388265658U)
                    {
                        if (num <= 172932033U)
                        {
                            if (num != 27894855U)
                            {
                                if (num != 172932033U)
                                {
                                    goto IL_19C5;
                                }
                                if (!(cmd == "LIST"))
                                {
                                    goto IL_19C5;
                                }
                                goto IL_1627;
                            }
                            else
                            {
                                if (!(cmd == "CONFIG"))
                                {
                                    goto IL_19C5;
                                }
                                if (array.Length < 2)
                                {
                                    this.TypeCommand("HELP CONFIG", null);
                                    goto IL_19F5;
                                }
                                string a = array[1].ToUpper();
                                if (a == "RELOAD" || a == "R" || a == "RLD")
                                {
                                    ConfigFile.ReloadGameConfigs(false);
                                    ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath);
                                    ServerStatic.SharedGroupsConfig = ((ConfigSharing.Paths[4] == null) ? null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt"));
                                    ServerStatic.SharedGroupsMembersConfig = ((ConfigSharing.Paths[5] == null) ? null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt"));
                                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                                    Console.AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                                    goto IL_19F5;
                                }
                                if (a == "PATH")
                                {
                                    Console.AddLog("Configuration file path: <i>" + FileManager.GetAppFolder(true, true, "") + "</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                                    Console.AddLog("<i>No visible drive letter means the root game directory.</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                                    goto IL_19F5;
                                }
                                if (!(a == "VALUE"))
                                {
                                    goto IL_19F5;
                                }
                                if (array.Length < 3)
                                {
                                    Console.AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
                                    goto IL_19F5;
                                }
                                Console.AddLog("The value of <i>'" + array[2].ToUpper() + "'</i> is: " + ConfigFile.ServerConfig.GetString(array[2].ToUpper(), "<color=ff0>DENIED: Entered key does not exists</color>"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                                goto IL_19F5;
                            }
                        }
                        else if (num != 317471912U)
                        {
                            if (num != 388265658U)
                            {
                                goto IL_19C5;
                            }
                            if (!(cmd == "CENTRAL"))
                            {
                                goto IL_19C5;
                            }
                            goto IL_1740;
                        }
                        else
                        {
                            if (cmd == "SRVCFG")
                            {
                                Console.AddLog("Requesting server config...", Color.yellow, false);
                                PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerConfig();
                                goto IL_19F5;
                            }
                            goto IL_19C5;
                        }
                    }
                    else if (num <= 518955878U)
                    {
                        if (num != 452872333U)
                        {
                            if (num != 518955878U)
                            {
                                goto IL_19C5;
                            }
                            if (!(cmd == "KH"))
                            {
                                goto IL_19C5;
                            }
                            goto IL_1712;
                        }
                        else
                        {
                            if (cmd == "LENNY")
                            {
                                Console.AddLog("<size=450>( ͡° ͜ʖ ͡°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue), false);
                                goto IL_19F5;
                            }
                            goto IL_19C5;
                        }
                    }
                    else if (num != 620754425U)
                    {
                        if (num != 715817306U)
                        {
                            goto IL_19C5;
                        }
                        if (!(cmd == "ADMINME"))
                        {
                            goto IL_19C5;
                        }
                    }
                    else
                    {
                        if (!(cmd == "PL"))
                        {
                            goto IL_19C5;
                        }
                        goto IL_1627;
                    }
                }
                else if (num <= 892818482U)
                {
                    if (num <= 786957503U)
                    {
                        if (num != 729404925U)
                        {
                            if (num != 786957503U)
                            {
                                goto IL_19C5;
                            }
                            if (!(cmd == "CSRV"))
                            {
                                goto IL_19C5;
                            }
                            goto IL_1740;
                        }
                        else if (!(cmd == "OVERRIDE"))
                        {
                            goto IL_19C5;
                        }
                    }
                    else if (num != 844380939U)
                    {
                        if (num != 892818482U || !(cmd == "ROUNDRESTART"))
                        {
                            goto IL_19C5;
                        }
                        bool flag = false;
                        PlayerStats component = PlayerManager.localPlayer.GetComponent<PlayerStats>();
                        if (component.isLocalPlayer && component.isServer)
                        {
                            flag = true;
                            Console.AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                            component.Roundrestart();
                        }
                        if (!flag)
                        {
                            Console.AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        goto IL_19F5;
                    }
                    else
                    {
                        if (cmd == "HELLO")
                        {
                            Console.AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        goto IL_19C5;
                    }
                }
                else if (num <= 1083443582U)
                {
                    if (num != 912447482U)
                    {
                        if (num != 1083443582U)
                        {
                            goto IL_19C5;
                        }
                        if (!(cmd == "KHASH"))
                        {
                            goto IL_19C5;
                        }
                        goto IL_1712;
                    }
                    else
                    {
                        if (!(cmd == "HELP"))
                        {
                            goto IL_19C5;
                        }
                        if (array.Length > 1)
                        {
                            string text = array[1].ToUpper();
                            foreach (CommandHint commandHint in this.hints)
                            {
                                if (!(commandHint.name != text))
                                {
                                    Console.AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                                    this.RefreshConsoleScreen();
                                    return this._response;
                                }
                            }
                            Console.AddLog("Help for command '" + text + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            this.RefreshConsoleScreen();
                            return this._response;
                        }
                        Console.AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                        foreach (CommandHint commandHint2 in this.hints)
                        {
                            Console.AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
                        }
                        Console.AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                        this.RefreshConsoleScreen();
                        goto IL_19F5;
                    }
                }
                else if (num != 1190553394U)
                {
                    if (num != 1458105184U)
                    {
                        goto IL_19C5;
                    }
                    if (!(cmd == "ID"))
                    {
                        goto IL_19C5;
                    }
                    goto IL_15F9;
                }
                else
                {
                    if (!(cmd == "ITEMLIST"))
                    {
                        goto IL_19C5;
                    }
                    string a2 = "offline";
                    int num2 = 1;
                    if (array.Length >= 2 && !int.TryParse(array[1], out num2))
                    {
                        Console.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                        return this._response;
                    }
                    Inventory component2 = PlayerManager.localPlayer.GetComponent<Inventory>();
                    if (component2 != null)
                    {
                        a2 = "none";
                        if (num2 < 1)
                        {
                            Console.AddLog("Page '" + num2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            this.RefreshConsoleScreen();
                            return this._response;
                        }
                        Item[] availableItems = component2.availableItems;
                        for (int i = 10 * (num2 - 1); i < 10 * num2; i++)
                        {
                            if (10 * (num2 - 1) > availableItems.Length)
                            {
                                Console.AddLog("Page '" + num2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                break;
                            }
                            if (i >= availableItems.Length)
                            {
                                break;
                            }
                            Console.AddLog("ITEM#" + i.ToString("000") + " : " + availableItems[i].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                        }
                    }
                    if (a2 != "none")
                    {
                        Console.AddLog((a2 == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                        goto IL_19F5;
                    }
                    goto IL_19F5;
                }
                GameObject gameObject = GameObject.Find("Host");
                if (!(gameObject != null) || !gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
                {
                    goto IL_19F5;
                }
                ServerRoles component3 = gameObject.GetComponent<ServerRoles>();
                if (!component3.PublicKeyAccepted)
                {
                    Console.AddLog("Authentication wasn't performed. Is the server running in online mode?", Color.red, false);
                    return string.Empty;
                }
                component3.RemoteAdmin = true;
                component3.OverwatchPermitted = true;
                component3.Permissions = ServerStatic.PermissionsHandler.FullPerm;
                component3.TargetOpenRemoteAdmin(component3.connectionToClient, false);
                Console.AddLog("Remote admin enabled for you.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                return string.Empty;
            }
            else
            {
                if (num <= 2305816734U)
                {
                    if (num > 1750141176U)
                    {
                        if (num <= 2198250801U)
                        {
                            if (num != 2038653189U)
                            {
                                if (num != 2198250801U)
                                {
                                    goto IL_19C5;
                                }
                                if (!(cmd == "COLORS"))
                                {
                                    goto IL_19C5;
                                }
                                goto IL_1473;
                            }
                            else if (!(cmd == "EXIT"))
                            {
                                goto IL_19C5;
                            }
                        }
                        else if (num != 2228862006U)
                        {
                            if (num != 2305816734U)
                            {
                                goto IL_19C5;
                            }
                            if (!(cmd == "MYID"))
                            {
                                goto IL_19C5;
                            }
                            goto IL_15F9;
                        }
                        else if (!(cmd == "QUIT"))
                        {
                            goto IL_19C5;
                        }
                        Console.AddLog("<size=50>GOODBYE!</size>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                        base.Invoke("QuitGame", 1f);
                        goto IL_19F5;
                    }
                    if (num <= 1664031467U)
                    {
                        if (num != 1490170220U)
                        {
                            if (num != 1664031467U || !(cmd == "REPORT"))
                            {
                                goto IL_19C5;
                            }
                            if (!SceneManager.GetActiveScene().name.Contains("Facility"))
                            {
                                goto IL_19F5;
                            }
                            if (array.Length < 2)
                            {
                                Console.AddLog("Syntax: \"report <playerid> <reason>\"", Color.red, false);
                                return this._response;
                            }
                            if (array.Length >= 3)
                            {
                                string text2 = array[2];
                                if (array.Length > 3)
                                {
                                    for (int j = 3; j < array.Length; j++)
                                    {
                                        text2 = text2 + " " + array[j];
                                    }
                                }
                                array[2] = text2;
                            }
                            int playerId;
                            if (!int.TryParse(array[1], out playerId))
                            {
                                return this._response;
                            }
                            PlayerManager.localPlayer.GetComponent<CheaterReport>().Report(playerId, array[2]);
                            goto IL_19F5;
                        }
                        else
                        {
                            if (!(cmd == "KEYHASH"))
                            {
                                goto IL_19C5;
                            }
                            goto IL_1712;
                        }
                    }
                    else if (num != 1708783731U)
                    {
                        if (num != 1750141176U || !(cmd == "DEBUG"))
                        {
                            goto IL_19C5;
                        }
                        int num3 = 4;
                        if (array.Length == 1)
                        {
                            string text3 = "Welcome to Debug Mode. The following modules were found:";
                            string[] array2;
                            string[] array3;
                            ConsoleDebugMode.GetList(out array2, out array3);
                            for (int k = 0; k < array2.Length; k++)
                            {
                                text3 = string.Concat(new string[]
                                {
                                    text3,
                                    "\n- <b>",
                                    array2[k],
                                    "</b> - ",
                                    array3[k]
                                });
                            }
                            Console.AddDebugLog("MODE", text3, MessageImportance.MostImportant, false);
                            goto IL_19F5;
                        }
                        if (array.Length == 2)
                        {
                            Console.AddDebugLog("MODE", ConsoleDebugMode.ConsoleGetLevel(array[1]), MessageImportance.MostImportant, false);
                            goto IL_19F5;
                        }
                        if (array.Length < 3)
                        {
                            goto IL_19F5;
                        }
                        int num4;
                        if (!int.TryParse(array[2], out num4) && num4 >= 0 && num4 <= num3)
                        {
                            Console.AddDebugLog("MODE", string.Concat(new object[]
                            {
                                "Could not change the Debug Mode importance: '",
                                array[2],
                                "' is supposed to be a integer value between 0 and ",
                                num3,
                                "."
                            }), MessageImportance.MostImportant, false);
                            goto IL_19F5;
                        }
                        array[1] = array[1].ToUpper();
                        if (ConsoleDebugMode.ChangeImportance(array[1], num4))
                        {
                            Console.AddDebugLog("MODE", "Debug Level was modified. " + ConsoleDebugMode.ConsoleGetLevel(array[1]), MessageImportance.MostImportant, false);
                            goto IL_19F5;
                        }
                        Console.AddDebugLog("MODE", "Could not change the Debug Mode importance: Module '" + array[1] + "' could not be found.", MessageImportance.MostImportant, false);
                        goto IL_19F5;
                    }
                    else
                    {
                        if (!(cmd == "CS"))
                        {
                            goto IL_19C5;
                        }
                        goto IL_1740;
                    }
                }
                else if (num <= 2748412071U)
                {
                    if (num <= 2385991020U)
                    {
                        if (num != 2377374125U)
                        {
                            if (num == 2385991020U && cmd == "SEED")
                            {
                                GameObject gameObject2 = GameObject.Find("Host");
                                Console.AddLog("Map seed is: <b>" + ((gameObject2 == null) ? "NONE" : gameObject2.GetComponent<RandomSeedSync>().seed.ToString()) + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                                goto IL_19F5;
                            }
                            goto IL_19C5;
                        }
                        else
                        {
                            if (cmd == "GROUPS")
                            {
                                Console.AddLog("Requesting server groups...", Color.yellow, false);
                                PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerGroups();
                                goto IL_19F5;
                            }
                            goto IL_19C5;
                        }
                    }
                    else if (num != 2674786406U)
                    {
                        if (num != 2748412071U || !(cmd == "CLASSLIST"))
                        {
                            goto IL_19C5;
                        }
                        string a3 = "offline";
                        int num5 = 1;
                        if (array.Length >= 2 && !int.TryParse(array[1], out num5))
                        {
                            Console.AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            return this._response;
                        }
                        CharacterClassManager component4 = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
                        if (component4 != null)
                        {
                            a3 = "none";
                            if (num5 < 1)
                            {
                                Console.AddLog("Page '" + num5 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                this.RefreshConsoleScreen();
                                return this._response;
                            }
                            Role[] classes = component4.Classes;
                            for (int l = 10 * (num5 - 1); l < 10 * num5; l++)
                            {
                                if (10 * (num5 - 1) > classes.Length)
                                {
                                    Console.AddLog("Page '" + num5 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                                    break;
                                }
                                if (l >= classes.Length)
                                {
                                    break;
                                }
                                Console.AddLog("CLASS#" + l.ToString("000") + " : " + classes[l].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                            }
                        }
                        if (a3 != "none")
                        {
                            Console.AddLog((a3 == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        goto IL_19F5;
                    }
                    else
                    {
                        if (!(cmd == "BAN"))
                        {
                            goto IL_19C5;
                        }
                        if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
                        {
                            return this._response;
                        }
                        if (array.Length < 3)
                        {
                            Console.AddLog("Syntax: BAN [player kick / ip] [minutes]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
                            using (Dictionary<int, NetworkConnection>.ValueCollection.Enumerator enumerator = NetworkServer.connections.Values.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    NetworkConnection networkConnection = enumerator.Current;
                                    string text4 = string.Empty;
                                    GameObject gameObject3 = Console.FindConnectedRoot(networkConnection);
                                    if (gameObject3 != null)
                                    {
                                        text4 = gameObject3.GetComponent<NicknameSync>().MyNick;
                                    }
                                    if (text4 == string.Empty)
                                    {
                                        Console.AddLog("Player :: " + networkConnection.address, new Color32(160, 128, 128, byte.MaxValue), true);
                                    }
                                    else
                                    {
                                        Console.AddLog("Player :: " + text4 + " :: " + networkConnection.address, new Color32(128, 160, 128, byte.MaxValue), true);
                                    }
                                }
                                goto IL_19F5;
                            }
                        }
                        int duration;
                        if (!int.TryParse(array[2], out duration))
                        {
                            Console.AddLog("Parse error: [minutes] - has to be an integer.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        bool flag2 = false;
                        foreach (NetworkConnection networkConnection2 in NetworkServer.connections.Values)
                        {
                            GameObject gameObject4 = Console.FindConnectedRoot(networkConnection2);
                            if (networkConnection2.address.Contains(array[1], StringComparison.OrdinalIgnoreCase) || (!(gameObject4 == null) && gameObject4.GetComponent<NicknameSync>().MyNick.Contains(array[1], StringComparison.OrdinalIgnoreCase)))
                            {
                                flag2 = true;
                                PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(gameObject4, duration, string.Empty, "Administrator");
                                Console.AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                            }
                        }
                        if (!flag2)
                        {
                            Console.AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        goto IL_19F5;
                    }
                }
                else if (num <= 3439379241U)
                {
                    if (num != 2898977996U)
                    {
                        if (num != 3439379241U)
                        {
                            goto IL_19C5;
                        }
                        if (!(cmd == "PLAYERS"))
                        {
                            goto IL_19C5;
                        }
                        goto IL_1627;
                    }
                    else
                    {
                        if (!(cmd == "KEY"))
                        {
                            goto IL_19C5;
                        }
                        goto IL_16C0;
                    }
                }
                else if (num != 3494757443U)
                {
                    if (num != 3611046620U)
                    {
                        if (num != 3888318712U)
                        {
                            goto IL_19C5;
                        }
                        if (!(cmd == "COLOR"))
                        {
                            goto IL_19C5;
                        }
                    }
                    else
                    {
                        if (!(cmd == "GIVE"))
                        {
                            goto IL_19C5;
                        }
                        if (!(PlayerManager.localPlayer.GetComponent<CharacterClassManager>().isServer ? PlayerManager.localPlayer : null))
                        {
                            Console.AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        int num6;
                        if (array.Length < 2 || !int.TryParse(array[1], out num6))
                        {
                            Console.AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        string a4 = "offline";
                        Inventory component5 = PlayerManager.localPlayer.GetComponent<Inventory>();
                        if (component5 != null)
                        {
                            a4 = "online";
                            if (component5.availableItems.Length > num6)
                            {
                                component5.AddNewItem((ItemType)num6, -4.65664672E+11f, 0, 0, 0);
                                goto IL_19F5;
                            }
                            Console.AddLog("Failed to add ITEM#" + num6.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                        }
                        if (a4 == "offline" || a4 == "online")
                        {
                            Console.AddLog((a4 == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
                            goto IL_19F5;
                        }
                        Console.AddLog("ITEM#" + num6.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue), false);
                        goto IL_19F5;
                    }
                }
                else
                {
                    if (cmd == "CONTACT")
                    {
                        Console.AddLog("Requesting server-owner's contact email...", Color.yellow, false);
                        PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestContactEmail();
                        goto IL_19F5;
                    }
                    goto IL_19C5;
                }
            IL_1473:
                bool flag3 = array.Length > 1 && string.Equals(array[1], "LIST", StringComparison.OrdinalIgnoreCase);
                bool flag4 = (array.Length > 1 && string.Equals(array[1], "ALL", StringComparison.OrdinalIgnoreCase)) || (array.Length > 2 && string.Equals(array[2], "ALL", StringComparison.OrdinalIgnoreCase));
                Console.AddLog("Available colors:", Color.gray, false);
                string text5 = string.Empty;
                foreach (ServerRoles.NamedColor namedColor in PlayerManager.localPlayer.GetComponent<ServerRoles>().NamedColors)
                {
                    if (!namedColor.Restricted || flag4)
                    {
                        if (flag3)
                        {
                            Console.AddLog(string.Concat(new string[]
                            {
                                "<color=#",
                                namedColor.ColorHex,
                                ">",
                                namedColor.Name,
                                namedColor.Restricted ? "*" : string.Empty,
                                " - #",
                                namedColor.ColorHex,
                                "</color>"
                            }), Color.white, false);
                        }
                        else
                        {
                            text5 = string.Concat(new string[]
                            {
                                text5,
                                "<color=#",
                                namedColor.ColorHex,
                                ">",
                                namedColor.Name,
                                namedColor.Restricted ? "*" : string.Empty,
                                "</color>    "
                            });
                        }
                    }
                }
                if (!flag3)
                {
                    Console.AddLog(text5, Color.white, false);
                    goto IL_19F5;
                }
                goto IL_19F5;
            }
        IL_15F9:
            Console.AddLog("Your Player ID on the current server: " + PlayerManager.localPlayer.GetComponent<QueryProcessor>().PlayerId, Color.green, false);
            goto IL_19F5;
        IL_1627:
            List<GameObject> players = PlayerManager.players;
            Console.AddLog(string.Format("List of players ({0}):", players.Count), Color.cyan, false);
            using (List<GameObject>.Enumerator enumerator2 = players.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    GameObject gameObject5 = enumerator2.Current;
                    Console.AddLog("- " + (gameObject5.GetComponent<NicknameSync>().MyNick ?? "(no nickname)") + ": " + (gameObject5.GetComponent<CharacterClassManager>().UserId ?? "(no User ID)"), Color.gray, false);
                }
                goto IL_19F5;
            }
        IL_16C0:
            GameObject localPlayer = PlayerManager.localPlayer;
            if (localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey == null)
            {
                Console.AddLog("Encryption key: (null) - session not encrypted (probably due to online mode disabled).", Color.grey, false);
                goto IL_19F5;
            }
            Console.AddLog("Encryption key (KEEP SECRET!): " + BitConverter.ToString(localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey), Color.grey, false);
            goto IL_19F5;
        IL_1712:
            Console.AddLog("SHA256 hash of Central Server Public Key: " + Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey))), Color.green, false);
            goto IL_19F5;
        IL_1740:
            string str = CentralServer.Servers.Aggregate((string current, string adding) => current = current + ", " + adding);
            Console.AddLog("Use \"" + array[0].ToUpper() + " -r\" to change to different central server.", Color.gray, false);
            Console.AddLog("Use \"" + array[0].ToUpper() + " -t\" to change to TEST central server.", Color.gray, false);
            Console.AddLog("Use \"" + array[0].ToUpper() + " -s CentralServerNameHere\" to change to specified central server.", Color.gray, false);
            if (array.Length > 1)
            {
                string a5 = array[1].ToUpper();
                if (!(a5 == "-R"))
                {
                    if (!(a5 == "-T"))
                    {
                        if ((string.Equals(array[1], "-S", StringComparison.OrdinalIgnoreCase) || string.Equals(array[1], "-FS", StringComparison.OrdinalIgnoreCase)) && array.Length == 3)
                        {
                            if (!CentralServer.Servers.Contains(array[2].ToUpper()) && !string.Equals(array[1], "-FS", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.AddLog(string.Concat(new string[]
                                {
                                    "Server ",
                                    array[2].ToUpper(),
                                    " is not on the list. Use ",
                                    array[0].ToUpper(),
                                    " -fs ",
                                    array[2].ToUpper(),
                                    " to force the change."
                                }), Color.red, false);
                                return this._response;
                            }
                            CentralServer.SelectedServer = array[2].ToUpper();
                            CentralServer.StandardUrl = "https://" + array[2].ToUpper() + ".scpslgame.com/";
                            CentralServer.TestServer = false;
                            Console.AddLog("--- Central server changed to " + array[2].ToUpper() + " ---", Color.green, false);
                        }
                    }
                    else
                    {
                        CentralServer.SelectedServer = "TEST";
                        CentralServer.MasterUrl = "https://test.scpslgame.com/";
                        CentralServer.StandardUrl = "https://test.scpslgame.com/";
                        CentralServer.TestServer = true;
                        Console.AddLog("--- Central server changed to TEST SERVER ---", Color.green, false);
                    }
                }
                else
                {
                    CentralServer.ChangeCentralServer(false);
                    Console.AddLog("--- Central server changed ---", Color.green, false);
                }
            }
            Console.AddLog("Master central server: " + CentralServer.MasterUrl, Color.green, false);
            Console.AddLog(string.Concat(new string[]
            {
                "Selected central server: ",
                CentralServer.SelectedServer,
                " (",
                CentralServer.StandardUrl,
                ")"
            }), Color.green, false);
            Console.AddLog("All central servers: " + str, Color.green, false);
            goto IL_19F5;
        IL_19C5:
            Console.AddLog("Command " + cmd + " does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue), false);
        IL_19F5:
            return this._response;
        }

        // Token: 0x06001B9F RID: 7071
        public void ProceedButton()
        {
        }

        // Token: 0x06001BA0 RID: 7072
        public void ToggleConsole()
        {
        }

        // Token: 0x06001BA1 RID: 7073
        private IEnumerator<float> _RefreshCentralServers()
        {
            while (this != null)
            {
                int num;
                for (int i = 0; i < 4500; i = num + 1)
                {
                    yield return 0f;
                    num = i;
                }
                new Thread(delegate ()
                {
                    CentralServer.RefreshServerList(true, false);
                }).Start();
            }
            yield break;
        }

        // Token: 0x06001BA2 RID: 7074
        private IEnumerator<float> _RefreshPublicKey()
        {
            string text = CentralServerKeyCache.ReadCache();
            string cacheHash = string.Empty;
            if (!string.IsNullOrEmpty(text))
            {
                Console._publicKey = ECDSA.PublicKeyFromString(text);
                cacheHash = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey)));
                Console.AddLog("Loaded central server public key from cache.\nSHA256 of public key: " + cacheHash, Color.gray, false);
            }
            while (!CentralServer.ServerSelected)
            {
                yield return Timing.WaitForSeconds(1f);
            }
            using (UnityWebRequest www = UnityWebRequest.Get(CentralServer.StandardUrl + "v2/publickey.php"))
            {
                yield return Timing.WaitUntilDone(www.SendWebRequest());
                try
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Console.AddLog("Can't refresh central server public key - " + www.error, Color.red, false);
                        yield break;
                    }
                    try
                    {
                        PublicKeyResponse publicKeyResponse = JsonSerialize.FromJson<PublicKeyResponse>(www.downloadHandler.text);
                        if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
                        {
                            Console.AddLog("Can't refresh central server public key - invalid signature!", Color.red, false);
                            yield break;
                        }
                        Console._publicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
                        ServerConsole.PublicKey = Console._publicKey;
                        string text2 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey)));
                        Console.AddLog("Downloaded public key from central server.\nSHA256 of public key: " + text2, Color.green, false);
                        if (text2 != cacheHash)
                        {
                            CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
                        }
                        else
                        {
                            Console.AddLog("SHA256 of cached key matches, no need to update cache.", Color.grey, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.AddLog("Public key response deserialization error: " + ex.Message, Color.red, false);
                        Console.AddLog(ex.StackTrace, Color.red, false);
                    }
                }
                catch
                {
                    Console.AddLog("Can't refresh central server public key!", Color.red, false);
                }
            }
            UnityWebRequest www2 = null;
            yield break;
            yield break;
        }

        // Token: 0x06001BA3 RID: 7075
        private void QuitGame()
        {
            Timing.RunCoroutine(this.Shutdown());
        }

        // Token: 0x060024E4 RID: 9444
        private IEnumerator<float> Shutdown()
        {
            foreach (GameObject gameObject in PlayerManager.players)
            {
                NetworkBehaviour component = gameObject.GetComponent<NetworkBehaviour>();
                component.GetComponent<CharacterClassManager>().DisconnectClient(component.connectionToClient, "Server shutting down.");
            }
            yield return Timing.WaitForSeconds(1f);
            Application.Quit();
            yield break;
        }

        // Token: 0x04001CFB RID: 7419
        public CommandHint[] hints;

        // Token: 0x04001CFD RID: 7421
        public static string[] StartupArgs;

        // Token: 0x04001CFE RID: 7422
        internal static HttpQueryMode HttpMode;

        // Token: 0x04001CFF RID: 7423
        internal static bool LockHttpMode;

        // Token: 0x04001D01 RID: 7425
        private static ConsoleCommandSender _ccs;

        // Token: 0x04001D02 RID: 7426
        private static AsymmetricKeyParameter _publicKey;

        // Token: 0x04001D04 RID: 7428
        private string _content;

        // Token: 0x04001D05 RID: 7429
        private string _response = string.Empty;
    }
}
