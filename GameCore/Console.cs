// Decompiled with JetBrains decompiler
// Type: GameCore.Console
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using CedMod;
using Cryptography;
using MEC;
using Mirror;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace GameCore
{
  public class Console : MonoBehaviour
  {
    private string _response = string.Empty;
    public CommandHint[] hints;
    public static string[] StartupArgs;
    internal static HttpQueryMode HttpMode;
    internal static bool LockHttpMode;
    private static ConsoleCommandSender _ccs;
    private static AsymmetricKeyParameter _publicKey;
    private string _content;

    public static Console singleton { get; private set; }

    internal static AsymmetricCipherKeyPair SessionKeys { get; private set; }

    public static DistributionPlatform Platform { get; private set; }

    private void Awake()
    {
      Console._ccs = new ConsoleCommandSender();
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
      if ((UnityEngine.Object) Console.singleton == (UnityEngine.Object) null)
        Console.singleton = this;
      else
        UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.gameObject);
    }

    private void Start()
    {
      SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
      Initializer.Setup();
      Console.AddLog("Hi there! Initializing console...", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
      Console.AddLog("Done! Type 'help' to print the list of available commands.", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
      Console.StartupArgs = Environment.GetCommandLineArgs();
      if ((((IEnumerable<string>) Console.StartupArgs).Any<string>((Func<string, bool>) (arg => string.Equals(arg, "-lockhttpmode", StringComparison.OrdinalIgnoreCase))) ? 1 : (File.Exists(FileManager.GetAppFolder(true, false, "") + "LockHttpMode.txt") ? 1 : 0)) != 0)
      {
        Console.LockHttpMode = true;
        Console.AddLog("HTTP mode locked", Color.gray, false);
      }
      if ((((IEnumerable<string>) Console.StartupArgs).Any<string>((Func<string, bool>) (arg => string.Equals(arg, "-httpclient", StringComparison.OrdinalIgnoreCase))) ? 1 : (File.Exists(FileManager.GetAppFolder(true, false, "") + "HttpClient.txt") ? 1 : 0)) != 0)
      {
        Console.HttpMode = HttpQueryMode.HttpClient;
        Console.AddLog("HTTP mode switched to HttpClient", Color.gray, false);
      }
      Timing.RunCoroutine(this._RefreshCentralServers(), Segment.FixedUpdate);
      Console.AddLog("Generatig session keys...", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
      Console.SessionKeys = ECDSA.GenerateKeys(384);
      Console.AddLog("Session keys generated (ECDSA)!", Color.green, false);
      Console.Platform = DistributionPlatform.Dedicated;
      Console.AddLog("Running as headless dedicated server. Skipping distribution platform detection.", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      Console.AddLog("Scene Manager: Loaded scene '" + scene.name + "' [" + scene.path + "]", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
      this.RefreshConsoleScreen();
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
    }

    private void FixedUpdate()
    {
    }

    private void RefreshConsoleScreen()
    {
    }

    public static void AddDebugLog(
      string debugKey,
      string message,
      MessageImportance importance,
      bool nospace = false)
    {
      Color32 color;
      if (!ConsoleDebugMode.CheckImportance(debugKey, importance, out color))
        return;
      Console.AddLog("[DEBUG_" + debugKey + "] " + message, (Color) color, nospace);
    }

    public static void AddLog(string text, Color c, bool nospace = false)
    {
      if (!ServerStatic.IsDedicated)
        return;
      ServerConsole.AddLog(text);
    }

    public static GameObject FindConnectedRoot(NetworkConnection conn)
    {
      try
      {
        GameObject gameObject = conn.playerController.gameObject;
        if (gameObject.CompareTag("Player"))
          return gameObject;
      }
      catch
      {
        return (GameObject) null;
      }
      return (GameObject) null;
    }

    public string TypeCommand(string cmd, CommandSender sender = null)
    {
      if (cmd.StartsWith(".") && cmd.Length > 1)
      {
        Console.AddLog("Sending command to server: " + cmd.Substring(1), (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
        PlayerManager.localPlayer.GetComponent<GameConsoleTransmission>().SendToServer(cmd.Substring(1));
        return string.Empty;
      }
      if (cmd.StartsWith("/") && cmd.Length > 1)
      {
        if (NetworkServer.active)
          CommandProcessor.ProcessQuery(cmd.Substring(1), sender ?? (CommandSender) Console._ccs);
        return string.Empty;
      }
      this._response = string.Empty;
      string[] strArray = cmd.Split(' ');
      cmd = strArray[0].ToUpper();
      uint stringHash = PrivateImplementationDetails.ComputeStringHash(cmd);
      if (stringHash <= 1458105184U)
      {
        if (stringHash <= 715817306U)
        {
          if (stringHash <= 388265658U)
          {
            if (stringHash <= 172932033U)
            {
              if (stringHash != 27894855U)
              {
                if (stringHash != 172932033U || !(cmd == "LIST"))
                  goto label_216;
                else
                  goto label_197;
              }
              else if (cmd == "CONFIG")
              {
                if (strArray.Length < 2)
                {
                  this.TypeCommand("HELP CONFIG", (CommandSender) null);
                  goto label_217;
                }
                else
                {
                  string upper = strArray[1].ToUpper();
                  if (upper == "RELOAD" || upper == "R" || upper == "RLD")
                  {
                    ConfigFile.ReloadGameConfigs(false);
                    ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath);
                    ServerStatic.SharedGroupsConfig = ConfigSharing.Paths[4] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt");
                    ServerStatic.SharedGroupsMembersConfig = ConfigSharing.Paths[5] == null ? (YamlConfig) null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt");
                    ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
                    Console.AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                    goto label_217;
                  }
                  else if (upper == "PATH")
                  {
                    Console.AddLog("Configuration file path: <i>" + FileManager.GetAppFolder(true, true, "") + "</i>", (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                    Console.AddLog("<i>No visible drive letter means the root game directory.</i>", (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                    goto label_217;
                  }
                  else if (upper == "VALUE")
                  {
                    if (strArray.Length < 3)
                    {
                      Console.AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", (Color) new Color32(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                      goto label_217;
                    }
                    else
                    {
                      Console.AddLog("The value of <i>'" + strArray[2].ToUpper() + "'</i> is: " + ConfigFile.ServerConfig.GetString(strArray[2].ToUpper(), "<color=ff0>DENIED: Entered key does not exists</color>"), (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                      goto label_217;
                    }
                  }
                  else
                    goto label_217;
                }
              }
              else
                goto label_216;
            }
            else if (stringHash != 317471912U)
            {
              if (stringHash != 388265658U || !(cmd == "CENTRAL"))
                goto label_216;
              else
                goto label_206;
            }
            else if (cmd == "SRVCFG")
            {
              Console.AddLog("Requesting server config...", Color.yellow, false);
              PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerConfig();
              goto label_217;
            }
            else
              goto label_216;
          }
          else if (stringHash <= 518955878U)
          {
            if (stringHash != 452872333U)
            {
              if (stringHash != 518955878U || !(cmd == "KH"))
                goto label_216;
              else
                goto label_205;
            }
            else if (cmd == "LENNY")
            {
              Console.AddLog("<size=450>( ͡° ͜ʖ ͡°)</size>\n\n", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 180, byte.MaxValue), false);
              goto label_217;
            }
            else
              goto label_216;
          }
          else if (stringHash != 620754425U)
          {
            if (stringHash != 715817306U || !(cmd == "ADMINME"))
              goto label_216;
          }
          else if (cmd == "PL")
            goto label_197;
          else
            goto label_216;
        }
        else if (stringHash <= 892818482U)
        {
          if (stringHash <= 786957503U)
          {
            if (stringHash != 729404925U)
            {
              if (stringHash != 786957503U || !(cmd == "CSRV"))
                goto label_216;
              else
                goto label_206;
            }
            else if (!(cmd == "OVERRIDE"))
              goto label_216;
          }
          else if (stringHash != 844380939U)
          {
            if (stringHash == 892818482U && cmd == "ROUNDRESTART")
            {
              bool flag = false;
              PlayerStats component = PlayerManager.localPlayer.GetComponent<PlayerStats>();
              if (component.isLocalPlayer && component.isServer)
              {
                flag = true;
                Console.AddLog("The round is about to restart! Please wait..", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                component.Roundrestart();
              }
              if (!flag)
              {
                Console.AddLog("You're not owner of this server!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                goto label_217;
              }
              else
                goto label_217;
            }
            else
              goto label_216;
          }
          else if (cmd == "HELLO")
          {
            Console.AddLog("Hello World!", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
            goto label_217;
          }
          else
            goto label_216;
        }
        else if (stringHash <= 1083443582U)
        {
          if (stringHash != 912447482U)
          {
            if (stringHash != 1083443582U || !(cmd == "KHASH"))
              goto label_216;
            else
              goto label_205;
          }
          else if (cmd == "HELP")
          {
            if (strArray.Length > 1)
            {
              string upper = strArray[1].ToUpper();
              foreach (CommandHint hint in this.hints)
              {
                if (!(hint.name != upper))
                {
                  Console.AddLog(hint.name + " - " + hint.fullDesc, (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                  this.RefreshConsoleScreen();
                  return this._response;
                }
              }
              Console.AddLog("Help for command '" + upper + "' does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
              this.RefreshConsoleScreen();
              return this._response;
            }
            Console.AddLog("List of available commands:\n", (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
            foreach (CommandHint hint in this.hints)
              Console.AddLog(hint.name + " - " + hint.shortDesc, (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
            Console.AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
            this.RefreshConsoleScreen();
            goto label_217;
          }
          else
            goto label_216;
        }
        else if (stringHash != 1190553394U)
        {
          if (stringHash != 1458105184U || !(cmd == "ID"))
            goto label_216;
          else
            goto label_196;
        }
        else if (cmd == "ITEMLIST")
        {
          string str = "offline";
          int result = 1;
          if (strArray.Length >= 2 && !int.TryParse(strArray[1], out result))
          {
            Console.AddLog("Please enter correct page number!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
            return this._response;
          }
          Inventory component = PlayerManager.localPlayer.GetComponent<Inventory>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
          {
            str = "none";
            if (result < 1)
            {
              Console.AddLog("Page '" + (object) result + "' does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
              this.RefreshConsoleScreen();
              return this._response;
            }
            Item[] availableItems = component.availableItems;
            for (int index = 10 * (result - 1); index < 10 * result; ++index)
            {
              if (10 * (result - 1) > availableItems.Length)
              {
                Console.AddLog("Page '" + (object) result + "' does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                break;
              }
              if (index < availableItems.Length)
                Console.AddLog("ITEM#" + index.ToString("000") + " : " + availableItems[index].label, (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
              else
                break;
            }
          }
          if (str != "none")
          {
            Console.AddLog(str == "offline" ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
            goto label_217;
          }
          else
            goto label_217;
        }
        else
          goto label_216;
        GameObject gameObject = GameObject.Find("Host");
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null && gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
        {
          ServerRoles component = gameObject.GetComponent<ServerRoles>();
          if (!component.PublicKeyAccepted)
          {
            Console.AddLog("Authentication wasn't performed. Is the server running in online mode?", Color.red, false);
            return string.Empty;
          }
          component.RemoteAdmin = true;
          component.OverwatchPermitted = true;
          component.Permissions = ServerStatic.PermissionsHandler.FullPerm;
          component.TargetOpenRemoteAdmin(component.connectionToClient, false);
          Console.AddLog("Remote admin enabled for you.", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
          return string.Empty;
        }
        goto label_217;
      }
      else
      {
        if (stringHash <= 2305816734U)
        {
          if (stringHash > 1750141176U)
          {
            switch (stringHash)
            {
              case 2038653189:
                if (cmd == "EXIT")
                  break;
                goto label_216;
              case 2198250801:
                if (cmd == "COLORS")
                  goto label_187;
                else
                  goto label_216;
              case 2228862006:
                if (!(cmd == "QUIT"))
                  goto label_216;
                else
                  break;
              case 2305816734:
                if (cmd == "MYID")
                  goto label_196;
                else
                  goto label_216;
              default:
                goto label_216;
            }
            Console.AddLog("<size=50>GOODBYE!</size>", (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
            this.Invoke("QuitGame", 1f);
            goto label_217;
          }
          else
          {
            switch (stringHash)
            {
              case 1490170220:
                if (cmd == "KEYHASH")
                  goto label_205;
                else
                  goto label_216;
              case 1664031467:
                if (cmd == "REPORT")
                {
                  if (SceneManager.GetActiveScene().name.Contains("Facility"))
                  {
                    if (strArray.Length < 2)
                    {
                      Console.AddLog("Syntax: \"report <playerid> <reason>\"", Color.red, false);
                      return this._response;
                    }
                    if (strArray.Length >= 3)
                    {
                      string str = strArray[2];
                      if (strArray.Length > 3)
                      {
                        for (int index = 3; index < strArray.Length; ++index)
                          str = str + " " + strArray[index];
                      }
                      strArray[2] = str;
                    }
                    int result;
                    if (!int.TryParse(strArray[1], out result))
                      return this._response;
                    PlayerManager.localPlayer.GetComponent<CheaterReport>().Report(result, strArray[2]);
                    goto label_217;
                  }
                  else
                    goto label_217;
                }
                else
                  goto label_216;
              case 1708783731:
                if (cmd == "CS")
                  goto label_206;
                else
                  goto label_216;
              case 1750141176:
                if (cmd == "DEBUG")
                {
                  int num = 4;
                  if (strArray.Length == 1)
                  {
                    string message = "Welcome to Debug Mode. The following modules were found:";
                    string[] keys;
                    string[] descriptions;
                    ConsoleDebugMode.GetList(out keys, out descriptions);
                    for (int index = 0; index < keys.Length; ++index)
                      message = message + "\n- <b>" + keys[index] + "</b> - " + descriptions[index];
                    Console.AddDebugLog("MODE", message, MessageImportance.MostImportant, false);
                    goto label_217;
                  }
                  else if (strArray.Length == 2)
                  {
                    Console.AddDebugLog("MODE", ConsoleDebugMode.ConsoleGetLevel(strArray[1]), MessageImportance.MostImportant, false);
                    goto label_217;
                  }
                  else if (strArray.Length >= 3)
                  {
                    int result;
                    if (!int.TryParse(strArray[2], out result) && result >= 0 && result <= num)
                    {
                      Console.AddDebugLog("MODE", "Could not change the Debug Mode importance: '" + strArray[2] + "' is supposed to be a integer value between 0 and " + (object) num + ".", MessageImportance.MostImportant, false);
                      goto label_217;
                    }
                    else
                    {
                      strArray[1] = strArray[1].ToUpper();
                      if (ConsoleDebugMode.ChangeImportance(strArray[1], result))
                      {
                        Console.AddDebugLog("MODE", "Debug Level was modified. " + ConsoleDebugMode.ConsoleGetLevel(strArray[1]), MessageImportance.MostImportant, false);
                        goto label_217;
                      }
                      else
                      {
                        Console.AddDebugLog("MODE", "Could not change the Debug Mode importance: Module '" + strArray[1] + "' could not be found.", MessageImportance.MostImportant, false);
                        goto label_217;
                      }
                    }
                  }
                  else
                    goto label_217;
                }
                else
                  goto label_216;
              default:
                goto label_216;
            }
          }
        }
        else if (stringHash <= 2748412071U)
        {
          if (stringHash <= 2385991020U)
          {
            if (stringHash != 2377374125U)
            {
              if (stringHash == 2385991020U && cmd == "SEED")
              {
                GameObject gameObject = GameObject.Find("Host");
                Console.AddLog("Map seed is: <b>" + ((UnityEngine.Object) gameObject == (UnityEngine.Object) null ? "NONE" : gameObject.GetComponent<RandomSeedSync>().seed.ToString()) + "</b>", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                goto label_217;
              }
              else
                goto label_216;
            }
            else if (cmd == "GROUPS")
            {
              Console.AddLog("Requesting server groups...", Color.yellow, false);
              PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerGroups();
              goto label_217;
            }
            else
              goto label_216;
          }
          else if (stringHash != 2674786406U)
          {
            if (stringHash == 2748412071U && cmd == "CLASSLIST")
            {
              string str = "offline";
              int result = 1;
              if (strArray.Length >= 2 && !int.TryParse(strArray[1], out result))
              {
                Console.AddLog("Please enter correct page number!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                return this._response;
              }
              CharacterClassManager component = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
              if ((UnityEngine.Object) component != (UnityEngine.Object) null)
              {
                str = "none";
                if (result < 1)
                {
                  Console.AddLog("Page '" + (object) result + "' does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                  this.RefreshConsoleScreen();
                  return this._response;
                }
                Role[] classes = component.Classes;
                for (int index = 10 * (result - 1); index < 10 * result; ++index)
                {
                  if (10 * (result - 1) > classes.Length)
                  {
                    Console.AddLog("Page '" + (object) result + "' does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                    break;
                  }
                  if (index < classes.Length)
                    Console.AddLog("CLASS#" + index.ToString("000") + " : " + classes[index].fullName, (Color) new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), false);
                  else
                    break;
                }
              }
              if (str != "none")
              {
                Console.AddLog(str == "offline" ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                goto label_217;
              }
              else
                goto label_217;
            }
            else
              goto label_216;
          }
          else if (cmd == "BAN")
          {
            if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
              return this._response;
            if (strArray.Length < 3)
            {
              Console.AddLog("Syntax: BAN [player kick / ip] [minutes]", (Color) new Color32(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue), false);
              using (Dictionary<int, NetworkConnection>.ValueCollection.Enumerator enumerator = NetworkServer.connections.Values.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  NetworkConnection current = enumerator.Current;
                  string str = string.Empty;
                  GameObject connectedRoot = Console.FindConnectedRoot(current);
                  if ((UnityEngine.Object) connectedRoot != (UnityEngine.Object) null)
                    str = connectedRoot.GetComponent<NicknameSync>().MyNick;
                  if (str == string.Empty)
                    Console.AddLog("Player :: " + current.address, (Color) new Color32((byte) 160, (byte) 128, (byte) 128, byte.MaxValue), true);
                  else
                    Console.AddLog("Player :: " + str + " :: " + current.address, (Color) new Color32((byte) 128, (byte) 160, (byte) 128, byte.MaxValue), true);
                }
                goto label_217;
              }
            }
            else
            {
              int result;
              if (!int.TryParse(strArray[2], out result))
              {
                Console.AddLog("Parse error: [minutes] - has to be an integer.", (Color) new Color32(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                goto label_217;
              }
              else
              {
                bool flag = false;
                foreach (NetworkConnection conn in NetworkServer.connections.Values)
                {
                  GameObject connectedRoot = Console.FindConnectedRoot(conn);
                  if (conn.address.Contains(strArray[1], StringComparison.OrdinalIgnoreCase) || !((UnityEngine.Object) connectedRoot == (UnityEngine.Object) null) && connectedRoot.GetComponent<NicknameSync>().MyNick.Contains(strArray[1], StringComparison.OrdinalIgnoreCase))
                  {
                    flag = true;
                    PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(connectedRoot, result, string.Empty, "Administrator");
                    Console.AddLog("Player banned.", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                  }
                }
                if (!flag)
                {
                  Console.AddLog("Player not found.", (Color) new Color32(byte.MaxValue, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                  goto label_217;
                }
                else
                  goto label_217;
              }
            }
          }
          else
            goto label_216;
        }
        else if (stringHash <= 3439379241U)
        {
          if (stringHash != 2898977996U)
          {
            if (stringHash != 3439379241U || !(cmd == "PLAYERS"))
              goto label_216;
            else
              goto label_197;
          }
          else if (cmd == "KEY")
          {
            GameObject localPlayer = PlayerManager.localPlayer;
            if (localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey == null)
            {
              Console.AddLog("Encryption key: (null) - session not encrypted (probably due to online mode disabled).", Color.grey, false);
              goto label_217;
            }
            else
            {
              Console.AddLog("Encryption key (KEEP SECRET!): " + BitConverter.ToString(localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey), Color.grey, false);
              goto label_217;
            }
          }
          else
            goto label_216;
        }
        else if (stringHash != 3494757443U)
        {
          if (stringHash != 3611046620U)
          {
            if (stringHash != 3888318712U || !(cmd == "COLOR"))
              goto label_216;
          }
          else if (cmd == "GIVE")
          {
            if (!(bool) (PlayerManager.localPlayer.GetComponent<CharacterClassManager>().isServer ? (UnityEngine.Object) PlayerManager.localPlayer : (UnityEngine.Object) null))
            {
              Console.AddLog("You're not owner of this server!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
              goto label_217;
            }
            else
            {
              int result;
              if (strArray.Length < 2 || !int.TryParse(strArray[1], out result))
              {
                Console.AddLog("Second argument has to be a number!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                goto label_217;
              }
              else
              {
                string str = "offline";
                Inventory component = PlayerManager.localPlayer.GetComponent<Inventory>();
                if ((UnityEngine.Object) component != (UnityEngine.Object) null)
                {
                  str = "online";
                  if (component.availableItems.Length > result)
                  {
                    component.AddNewItem((ItemType) result, -4.656647E+11f, 0, 0, 0);
                    goto label_217;
                  }
                  else
                    Console.AddLog("Failed to add ITEM#" + result.ToString("000") + " - item does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                }
                if (str == "offline" || str == "online")
                {
                  Console.AddLog(str == "offline" ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
                  goto label_217;
                }
                else
                {
                  Console.AddLog("ITEM#" + result.ToString("000") + " has been added!", (Color) new Color32((byte) 0, byte.MaxValue, (byte) 0, byte.MaxValue), false);
                  goto label_217;
                }
              }
            }
          }
          else
            goto label_216;
        }
        else if (cmd == "CONTACT")
        {
          Console.AddLog("Requesting server-owner's contact email...", Color.yellow, false);
          PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestContactEmail();
          goto label_217;
        }
        else
          goto label_216;
label_187:
        bool flag1 = strArray.Length > 1 && string.Equals(strArray[1], "LIST", StringComparison.OrdinalIgnoreCase);
        bool flag2 = strArray.Length > 1 && string.Equals(strArray[1], "ALL", StringComparison.OrdinalIgnoreCase) || strArray.Length > 2 && string.Equals(strArray[2], "ALL", StringComparison.OrdinalIgnoreCase);
        Console.AddLog("Available colors:", Color.gray, false);
        string text = string.Empty;
        foreach (ServerRoles.NamedColor namedColor in PlayerManager.localPlayer.GetComponent<ServerRoles>().NamedColors)
        {
          if (!namedColor.Restricted | flag2)
          {
            if (flag1)
              Console.AddLog("<color=#" + namedColor.ColorHex + ">" + namedColor.Name + (namedColor.Restricted ? "*" : string.Empty) + " - #" + namedColor.ColorHex + "</color>", Color.white, false);
            else
              text = text + "<color=#" + namedColor.ColorHex + ">" + namedColor.Name + (namedColor.Restricted ? "*" : string.Empty) + "</color>    ";
          }
        }
        if (!flag1)
        {
          Console.AddLog(text, Color.white, false);
          goto label_217;
        }
        else
          goto label_217;
      }
label_196:
      Console.AddLog("Your Player ID on the current server: " + (object) PlayerManager.localPlayer.GetComponent<QueryProcessor>().PlayerId, Color.green, false);
      goto label_217;
label_197:
      List<GameObject> players = PlayerManager.players;
      Console.AddLog(string.Format("List of players ({0}):", (object) players.Count), Color.cyan, false);
      using (List<GameObject>.Enumerator enumerator = players.GetEnumerator())
      {
        while (enumerator.MoveNext())
        {
          GameObject current = enumerator.Current;
          Console.AddLog("- " + (current.GetComponent<NicknameSync>().MyNick ?? "(no nickname)") + ": " + (current.GetComponent<CharacterClassManager>().UserId ?? "(no User ID)"), Color.gray, false);
        }
        goto label_217;
      }
label_205:
      Console.AddLog("SHA256 hash of Central Server Public Key: " + Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey))), Color.green, false);
      goto label_217;
label_206:
      string str1 = ((IEnumerable<string>) CentralServer.Servers).Aggregate<string>((Func<string, string, string>) ((current, adding) => current = current + ", " + adding));
      Console.AddLog("Use \"" + strArray[0].ToUpper() + " -r\" to change to different central server.", Color.gray, false);
      Console.AddLog("Use \"" + strArray[0].ToUpper() + " -t\" to change to TEST central server.", Color.gray, false);
      Console.AddLog("Use \"" + strArray[0].ToUpper() + " -s CentralServerNameHere\" to change to specified central server.", Color.gray, false);
      if (strArray.Length > 1)
      {
        string upper = strArray[1].ToUpper();
        if (!(upper == "-R"))
        {
          if (!(upper == "-T"))
          {
            if ((string.Equals(strArray[1], "-S", StringComparison.OrdinalIgnoreCase) || string.Equals(strArray[1], "-FS", StringComparison.OrdinalIgnoreCase)) && strArray.Length == 3)
            {
              if (!CentralServer.Servers.Contains<string>(strArray[2].ToUpper()) && !string.Equals(strArray[1], "-FS", StringComparison.OrdinalIgnoreCase))
              {
                Console.AddLog("Server " + strArray[2].ToUpper() + " is not on the list. Use " + strArray[0].ToUpper() + " -fs " + strArray[2].ToUpper() + " to force the change.", Color.red, false);
                return this._response;
              }
              CentralServer.SelectedServer = strArray[2].ToUpper();
              CentralServer.StandardUrl = "https://" + strArray[2].ToUpper() + ".scpslgame.com/";
              CentralServer.TestServer = false;
              Console.AddLog("--- Central server changed to " + strArray[2].ToUpper() + " ---", Color.green, false);
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
      Console.AddLog("Selected central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.green, false);
      Console.AddLog("All central servers: " + str1, Color.green, false);
      goto label_217;
label_216:
      Console.AddLog("Command " + cmd + " does not exist!", (Color) new Color32(byte.MaxValue, (byte) 180, (byte) 0, byte.MaxValue), false);
label_217:
      return this._response;
    }

    public void ProceedButton()
    {
    }

    public void ToggleConsole()
    {
    }

    private IEnumerator<float> _RefreshCentralServers()
    {
      Console console = this;
      while ((UnityEngine.Object) console != (UnityEngine.Object) null)
      {
        for (int i = 0; i < 4500; ++i)
          yield return 0.0f;
        new Thread((ThreadStart) (() => CentralServer.RefreshServerList(true, false))).Start();
      }
    }

    private IEnumerator<float> _RefreshPublicKey()
    {
      string key = CentralServerKeyCache.ReadCache();
      string cacheHash = string.Empty;
      if (!string.IsNullOrEmpty(key))
      {
        Console._publicKey = ECDSA.PublicKeyFromString(key);
        cacheHash = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey)));
        Console.AddLog("Loaded central server public key from cache.\nSHA256 of public key: " + cacheHash, Color.gray, false);
      }
      while (!CentralServer.ServerSelected)
        yield return Timing.WaitForSeconds(1f);
      UnityWebRequest www2 = UnityWebRequest.Get(CentralServer.StandardUrl + "v2/publickey.php");
      bool flag;
      try
      {
        yield return Timing.WaitUntilDone((AsyncOperation) www2.SendWebRequest());
        try
        {
          if (!string.IsNullOrEmpty(www2.error))
          {
            Console.AddLog("Can't refresh central server public key - " + www2.error, Color.red, false);
            flag = false;
            goto label_15;
          }
          else
          {
            try
            {
              PublicKeyResponse publicKeyResponse = JsonSerialize.FromJson<PublicKeyResponse>(www2.downloadHandler.text);
              if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
              {
                Console.AddLog("Can't refresh central server public key - invalid signature!", Color.red, false);
                flag = false;
                goto label_15;
              }
              else
              {
                Console._publicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
                ServerConsole.PublicKey = Console._publicKey;
                string str = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(Console._publicKey)));
                Console.AddLog("Downloaded public key from central server.\nSHA256 of public key: " + str, Color.green, false);
                if (str != cacheHash)
                  CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
                else
                  Console.AddLog("SHA256 of cached key matches, no need to update cache.", Color.grey, false);
              }
            }
            catch (Exception ex)
            {
              Console.AddLog("Public key response deserialization error: " + ex.Message, Color.red, false);
              Console.AddLog(ex.StackTrace, Color.red, false);
            }
          }
        }
        catch
        {
          Console.AddLog("Can't refresh central server public key!", Color.red, false);
        }
        goto label_17;
label_15:
        goto label_16;
      }
      finally
      {
        www2?.Dispose();
      }
label_17:
      www2 = (UnityWebRequest) null;
      yield break;
label_16:
      return flag;
    }

    private void QuitGame()
    {
      Timing.RunCoroutine(this.Shutdown());
    }

    private IEnumerator<float> Shutdown()
    {
      foreach (GameObject player in PlayerManager.players)
      {
        NetworkBehaviour component = player.GetComponent<NetworkBehaviour>();
        component.GetComponent<CharacterClassManager>().DisconnectClient(component.connectionToClient, "Server shutting down.");
      }
      yield return Timing.WaitForSeconds(1f);
      Application.Quit();
    }
  }
}
