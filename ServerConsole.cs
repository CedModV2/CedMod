// Decompiled with JetBrains decompiler
// Type: ServerConsole
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Authenticator;
using Cryptography;
using GameCore;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Utils.CommandInterpolation;

public class ServerConsole : MonoBehaviour, IDisposable
{
  private static string _serverName = string.Empty;
  private static readonly Func<float, float> _roundNormal = new Func<float, float>(Mathf.Round);
  private static readonly Func<float, float> _roundCeil = new Func<float, float>(Mathf.Ceil);
  private static readonly Func<float, float> _roundFloor = new Func<float, float>(Mathf.Floor);
  private static readonly Func<float, float, float> _pow = new Func<float, float, float>(Mathf.Pow);
  public static bool FriendlyFire = false;
  public static bool WhiteListEnabled = false;
  public static bool AccessRestriction = false;
  private static bool _accepted = true;
  private static readonly Queue<string> PrompterQueue = new Queue<string>();
  private static readonly PlayerListSerialized PlayersListRaw = new PlayerListSerialized(new List<string>());
  private static string _verificationPlayersList = string.Empty;
  public static ServerConsole singleton;
  public static int Port;
  private static int _logId;
  private static bool _disposing;
  public static Process ConsoleId;
  public static string Session;
  public static string Password;
  public static string Ip;
  public static AsymmetricKeyParameter PublicKey;
  public static bool Update;
  public static bool ScheduleTokenRefresh;
  public static bool RateLimitKick;
  internal static bool PlayersListChanged;
  internal static bool EnforceSameIp;
  internal static bool EnforceSameAsn;
  internal static bool SkipEnforcementForLocalAddresses;
  private static bool _printedNotVerifiedMessage;
  private static bool _emailSet;
  private static CustomNetworkManager _cnm;
  private static ServerConsoleSender _scs;
  public static ConcurrentBag<CharacterClassManager> NewPlayers;
  internal static List<IOutput> ConsoleOutputs;
  internal static int PlayersAmount;
  private bool _errorSent;
  private static float _playersListRefresh;
  private static Thread _checkProcessThread;
  private static Thread _queueThread;
  private static Thread _refreshPublicKeyThread;
  private static Thread _refreshPublicKeyOnceThread;
  private static Thread _refreshServerListThread;
  private static Thread _verificationRequestThread;

  public InterpolatedCommandFormatter NameFormatter { get; private set; }

  public static void ReloadServerName()
  {
    ServerConsole._serverName = ConfigFile.ServerConfig.GetString("server_name", "My Server Name") + (ConfigFile.ServerConfig.GetBool("cm_tracking", true) ? "<color=#ffffff00><size=1>SMCedMod</size></color>" : string.Empty);
  }

  public void Dispose()
  {
    ServerConsole._disposing = true;
    if (ServerConsole._checkProcessThread != null && ServerConsole._checkProcessThread.IsAlive)
      ServerConsole._checkProcessThread.Abort();
    if (ServerConsole._verificationRequestThread != null && ServerConsole._verificationRequestThread.IsAlive)
      ServerConsole._verificationRequestThread.Abort();
    if (ServerConsole._refreshPublicKeyThread != null && ServerConsole._refreshPublicKeyThread.IsAlive)
      ServerConsole._refreshPublicKeyThread.Abort();
    if (ServerConsole._refreshPublicKeyOnceThread != null && ServerConsole._refreshPublicKeyOnceThread.IsAlive)
      ServerConsole._refreshPublicKeyOnceThread.Abort();
    if (ServerConsole._verificationRequestThread != null && ServerConsole._verificationRequestThread.IsAlive)
      ServerConsole._verificationRequestThread.Abort();
    if (ServerStatic.KeepSession || !Directory.Exists("SCPSL_Data/Dedicated/" + ServerConsole.Session))
      return;
    Directory.Delete("SCPSL_Data/Dedicated/" + ServerConsole.Session, true);
  }

  private void Start()
  {
    ServerConsole._scs = new ServerConsoleSender();
    ServerConsole._cnm = this.GetComponent<CustomNetworkManager>();
    ServerConsole.ConsoleOutputs = new List<IOutput>();
    ServerConsole.NewPlayers = new ConcurrentBag<CharacterClassManager>();
    this.NameFormatter = new InterpolatedCommandFormatter(4)
    {
      StartClosure = '{',
      EndClosure = '}',
      Escape = '\\',
      ArgumentSplitter = ',',
      Commands = new Dictionary<string, Func<List<string>, string>>()
      {
        {
          "ip",
          (Func<List<string>, string>) (args => ServerConsole.Ip)
        },
        {
          "port",
          (Func<List<string>, string>) (args => LiteNetLib4MirrorTransport.Singleton.port.ToString())
        },
        {
          "number",
          (Func<List<string>, string>) (args => ((int) LiteNetLib4MirrorTransport.Singleton.port - 7776).ToString())
        },
        {
          "version",
          (Func<List<string>, string>) (args => CustomNetworkManager.CompatibleVersions[0])
        },
        {
          "player_count",
          (Func<List<string>, string>) (args => PlayerManager.players.Count.ToString())
        },
        {
          "full_player_count",
          (Func<List<string>, string>) (args =>
          {
            int count = PlayerManager.players.Count;
            if (count != CustomNetworkManager.TypedSingleton.ReservedMaxPlayers)
              return string.Format("{0}/{1}", (object) count, (object) CustomNetworkManager.TypedSingleton.ReservedMaxPlayers);
            switch (args.Count)
            {
              case 1:
                return "FULL";
              case 2:
                return this.NameFormatter.ProcessExpression(args[1]);
              default:
                throw new ArgumentOutOfRangeException(nameof (args), (object) args, "Invalid arguments. Use: full_player_count OR full_player_count,[full]");
            }
          })
        },
        {
          "max_players",
          (Func<List<string>, string>) (args => CustomNetworkManager.TypedSingleton.ReservedMaxPlayers.ToString())
        },
        {
          "round_duration_minutes",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => Mathf.FloorToInt((float) RoundSummary.roundTime / 60f).ToString())))
        },
        {
          "round_duration_seconds",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => (RoundSummary.roundTime % 60).ToString())))
        },
        {
          "kills",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.Kills.ToString())))
        },
        {
          "kills_frag",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.kills_by_frag.ToString())))
        },
        {
          "alive_role",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count != 2)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use: alive_role,[role ID]");
            RoleType role;
            if (!Enum.TryParse<RoleType>(this.NameFormatter.ProcessExpression(args[1]), out role))
              throw new CommandInputException("role ID", (object) args[1], "Could not parse.");
            return ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (s => s.CountRole(role).ToString()));
          })
        },
        {
          "alive_team",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count != 2)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use: alive_team,[team ID]");
            Team team;
            if (!Enum.TryParse<Team>(this.NameFormatter.ProcessExpression(args[1]), out team))
              throw new CommandInputException("team ID", (object) args[1], "Could not parse.");
            return ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (s => s.CountTeam(team).ToString()));
          })
        },
        {
          "zombies_recalled",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.changed_into_zombies.ToString())))
        },
        {
          "scp_counter",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => string.Format("{0}/{1}", (object) (summary.CountTeam(Team.SCP) - summary.CountRole(RoleType.Scp0492)), (object) summary.classlistStart.scps_except_zombies))))
        },
        {
          "scp_start",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => summary.classlistStart.scps_except_zombies.ToString())))
        },
        {
          "scp_killed",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => (summary.classlistStart.scps_except_zombies - summary.CountTeam(Team.SCP) - summary.CountRole(RoleType.Scp0492)).ToString())))
        },
        {
          "scp_kills",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.kills_by_scp.ToString())))
        },
        {
          "classd_counter",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => string.Format("{0}/{1}", (object) RoundSummary.escaped_ds, (object) summary.classlistStart.class_ds))))
        },
        {
          "classd_start",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => summary.classlistStart.class_ds.ToString())))
        },
        {
          "classd_escaped",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.escaped_ds.ToString())))
        },
        {
          "scientist_counter",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => string.Format("{0}/{1}", (object) RoundSummary.escaped_scientists, (object) summary.classlistStart.scientists))))
        },
        {
          "scientist_start",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => summary.classlistStart.scientists.ToString())))
        },
        {
          "scientist_escaped",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => RoundSummary.escaped_scientists.ToString())))
        },
        {
          "mtf_respawns",
          (Func<List<string>, string>) (args => ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (summary => NineTailedFoxUnits.host.list.Count.ToString())))
        },
        {
          "warhead_detonated",
          (Func<List<string>, string>) (args =>
          {
            switch (args.Count)
            {
              case 1:
                return ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (s => !AlphaWarheadController.Host.detonated ? string.Empty : "☢ WARHEAD DETONATED ☢"));
              case 3:
                return ServerConsole.GetRoundInfo((Func<RoundSummary, string>) (s => this.NameFormatter.ProcessExpression(args[AlphaWarheadController.Host.detonated ? 1 : 2])));
              default:
                throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use: warhead_detonated OR warhead_detonated,[detonated],[undetonated]");
            }
          })
        },
        {
          "random",
          (Func<List<string>, string>) (args =>
          {
            float result1;
            float result2;
            switch (args.Count)
            {
              case 2:
                result1 = 0.0f;
                string s1 = this.NameFormatter.ProcessExpression(args[1]);
                if (!float.TryParse(s1, out result2))
                  throw new CommandInputException("max", (object) s1, "Could not parse.");
                break;
              case 3:
                string s2 = this.NameFormatter.ProcessExpression(args[1]);
                if (!float.TryParse(s2, out result1))
                  throw new CommandInputException("min", (object) s2, "Could not parse.");
                string s3 = this.NameFormatter.ProcessExpression(args[2]);
                if (!float.TryParse(s3, out result2))
                  throw new CommandInputException("max", (object) s3, "Could not parse.");
                break;
              default:
                throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use: random,[max] OR random,[min],[max]");
            }
            return UnityEngine.Random.Range(result1, result2).ToString();
          })
        },
        {
          "random_list",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count < 3)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use: random_list,[entry 1],[entry 2]...");
            return this.NameFormatter.ProcessExpression(args[UnityEngine.Random.Range(1, args.Count)]);
          })
        },
        {
          "constant_e",
          (Func<List<string>, string>) (args => 2.718282f.ToString())
        },
        {
          "constant_pi",
          (Func<List<string>, string>) (args => 3.141593f.ToString())
        },
        {
          "add",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<float>("add", (IReadOnlyList<string>) args, (Func<float, float, float>) ((a, b) => a + b)))
        },
        {
          "subtract",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<float>("subtract", (IReadOnlyList<string>) args, (Func<float, float, float>) ((a, b) => a - b)))
        },
        {
          "multiply",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<float>("multiply", (IReadOnlyList<string>) args, (Func<float, float, float>) ((a, b) => a * b)))
        },
        {
          "division",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<float>("division", (IReadOnlyList<string>) args, (Func<float, float, float>) ((a, b) => a / b)))
        },
        {
          "power",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<float>("power", (IReadOnlyList<string>) args, ServerConsole._pow))
        },
        {
          "log",
          (Func<List<string>, string>) (args =>
          {
            float result1;
            float result2;
            switch (args.Count)
            {
              case 2:
                string s1 = this.NameFormatter.ProcessExpression(args[1]);
                if (!float.TryParse(s1, out result1))
                  throw new CommandInputException("value", (object) s1, "Could not parse.");
                result2 = 10f;
                break;
              case 3:
                string s2 = this.NameFormatter.ProcessExpression(args[1]);
                if (!float.TryParse(s2, out result1))
                  throw new CommandInputException("value", (object) s2, "Could not parse.");
                string s3 = this.NameFormatter.ProcessExpression(args[2]);
                if (!float.TryParse(s3, out result2))
                  throw new CommandInputException("base", (object) s3, "Could not parse.");
                break;
              default:
                throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use log,[value] OR log,[value],[base]");
            }
            return Mathf.Log(result1, result2).ToString();
          })
        },
        {
          "ln",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count < 2)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use ln,[value]");
            string s = this.NameFormatter.ProcessExpression(args[1]);
            float result;
            if (!float.TryParse(s, out result))
              throw new CommandInputException("value", (object) s, "Error parsing value.");
            return Mathf.Log(result).ToString();
          })
        },
        {
          "round",
          (Func<List<string>, string>) (args => this.StandardizedFloatRound("round", (IReadOnlyList<string>) args, ServerConsole._roundNormal))
        },
        {
          "round_up",
          (Func<List<string>, string>) (args => this.StandardizedFloatRound("round_up", (IReadOnlyList<string>) args, ServerConsole._roundCeil))
        },
        {
          "round_down",
          (Func<List<string>, string>) (args => this.StandardizedFloatRound("round_down", (IReadOnlyList<string>) args, ServerConsole._roundFloor))
        },
        {
          "equals",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count != 3)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use equals,[object A],[object B]");
            return (args[1] == args[2]).ToString();
          })
        },
        {
          "greater",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<bool>("greater", (IReadOnlyList<string>) args, (Func<float, float, bool>) ((a, b) => (double) a > (double) b)))
        },
        {
          "lesser",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<bool>("lesser", (IReadOnlyList<string>) args, (Func<float, float, bool>) ((a, b) => (double) a < (double) b)))
        },
        {
          "greater_or_equal",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<bool>("greater_or_equal", (IReadOnlyList<string>) args, (Func<float, float, bool>) ((a, b) => (double) a >= (double) b)))
        },
        {
          "lesser_or_equal",
          (Func<List<string>, string>) (args => this.StandardizedFloatComparison<bool>("lesser_or_equal", (IReadOnlyList<string>) args, (Func<float, float, bool>) ((a, b) => (double) a <= (double) b)))
        },
        {
          "not",
          (Func<List<string>, string>) (args =>
          {
            if (args.Count != 2)
              throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use not,[value]");
            string str = this.NameFormatter.ProcessExpression(args[1]);
            bool result;
            if (!bool.TryParse(str, out result))
              throw new CommandInputException("value", (object) str, "Error parsing value.");
            return (!result).ToString();
          })
        },
        {
          "or",
          (Func<List<string>, string>) (args => this.StandardizedBoolComparison<bool>("or", (IReadOnlyList<string>) args, (Func<bool, bool, bool>) ((a, b) => a | b)))
        },
        {
          "xor",
          (Func<List<string>, string>) (args => this.StandardizedBoolComparison<bool>("xor", (IReadOnlyList<string>) args, (Func<bool, bool, bool>) ((a, b) => a ^ b)))
        },
        {
          "and",
          (Func<List<string>, string>) (args => this.StandardizedBoolComparison<bool>("and", (IReadOnlyList<string>) args, (Func<bool, bool, bool>) ((a, b) => a & b)))
        },
        {
          "if",
          (Func<List<string>, string>) (args =>
          {
            string str1;
            string empty;
            switch (args.Count)
            {
              case 3:
                str1 = args[2];
                empty = string.Empty;
                break;
              case 4:
                str1 = args[2];
                empty = args[3];
                break;
              default:
                throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use if,[condition],[action] OR if,[condition],[action],[else action]");
            }
            string str2 = this.NameFormatter.ProcessExpression(args[1]);
            bool result;
            if (!bool.TryParse(str2, out result))
              throw new CommandInputException("condition", (object) str2, "Could not parse.");
            return this.NameFormatter.ProcessExpression(result ? str1 : empty);
          })
        }
      }
    };
    if (!ServerStatic.IsDedicated)
      return;
    ServerConsole._logId = 0;
    ServerConsole._accepted = true;
    if (string.IsNullOrEmpty(ServerConsole.Session))
      ServerConsole.Session = "default";
    if (Directory.Exists("SCPSL_Data/Dedicated/" + ServerConsole.Session) && Environment.GetCommandLineArgs().Contains<string>("-nodedicateddelete"))
    {
      foreach (string file in Directory.GetFiles("SCPSL_Data/Dedicated/" + ServerConsole.Session))
        File.Delete(file);
    }
    else if (Directory.Exists("SCPSL_Data/Dedicated/" + ServerConsole.Session))
      Directory.Delete("SCPSL_Data/Dedicated/" + ServerConsole.Session, true);
    Directory.CreateDirectory("SCPSL_Data/Dedicated/" + ServerConsole.Session);
    FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
    fileSystemWatcher.Path = "SCPSL_Data/Dedicated/" + ServerConsole.Session;
    fileSystemWatcher.NotifyFilter = NotifyFilters.FileName;
    fileSystemWatcher.Created += (FileSystemEventHandler) ((sender, args) =>
    {
      if (!args.Name.Contains("cs") || !args.Name.Contains("mapi"))
        return;
      new Thread((ThreadStart) (() => ServerConsole.ReadLog(args.FullPath))).Start();
    });
    fileSystemWatcher.EnableRaisingEvents = true;
    ServerConsole._queueThread = new Thread(new ThreadStart(this.Prompt))
    {
      Priority = System.Threading.ThreadPriority.Lowest,
      IsBackground = true,
      Name = "Dedicated server console output"
    };
    ServerConsole._queueThread.Start();
    if (!ServerStatic.ProcessIdPassed)
      return;
    ServerConsole._checkProcessThread = new Thread(new ThreadStart(ServerConsole.CheckProcess))
    {
      Priority = System.Threading.ThreadPriority.Lowest,
      IsBackground = true,
      Name = "Dedicated server console running check"
    };
    ServerConsole._checkProcessThread.Start();
  }

  private void FixedUpdate()
  {
    if ((double) ServerConsole._playersListRefresh < 5.0)
    {
      ServerConsole._playersListRefresh += Time.fixedDeltaTime;
    }
    else
    {
      if (!ServerConsole.PlayersListChanged)
        return;
      ServerConsole.PlayersListChanged = false;
      ServerConsole._playersListRefresh = 0.0f;
      try
      {
        foreach (GameObject player in PlayerManager.players)
        {
          if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
          {
            CharacterClassManager component = player.GetComponent<CharacterClassManager>();
            if (!((UnityEngine.Object) component == (UnityEngine.Object) null) && component.IsVerified && !string.IsNullOrEmpty(component.UserId) && (!component.isLocalPlayer || !ServerStatic.IsDedicated))
              ServerConsole.PlayersListRaw.objects.Add(component.UserId);
          }
        }
        ServerConsole._verificationPlayersList = JsonSerialize.ToJson<PlayerListSerialized>(ServerConsole.PlayersListRaw);
        ServerConsole.PlayersListRaw.objects.Clear();
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("[VERIFICATION] Exception in Players Online processing: " + ex.Message);
        ServerConsole.AddLog(ex.StackTrace);
      }
    }
  }

  private string StandardizedBoolComparison<T>(
    string source,
    IReadOnlyList<string> args,
    Func<bool, bool, T> comparison)
  {
    bool result;
    return this.StandardizedComparison<bool, T>(source, args, (Func<string, (bool, bool)>) (arg => (bool.TryParse(arg, out result), result)), comparison);
  }

  private string StandardizedFloatComparison<T>(
    string source,
    IReadOnlyList<string> args,
    Func<float, float, T> comparison)
  {
    float result;
    return this.StandardizedComparison<float, T>(source, args, (Func<string, (bool, float)>) (arg => (float.TryParse(arg, out result), result)), comparison);
  }

  private string StandardizedComparison<TArg, TResult>(
    string source,
    IReadOnlyList<string> args,
    Func<string, (bool success, TArg value)> parse,
    Func<TArg, TArg, TResult> comparison)
  {
    // ISSUE: unable to decompile the method.
  }

  private string StandardizedFloatRound(
    string source,
    IReadOnlyList<string> args,
    Func<float, float> rounder)
  {
    float result1;
    int result2;
    switch (((IReadOnlyCollection<string>) args).get_Count())
    {
      case 2:
        string s1 = this.NameFormatter.ProcessExpression(args.get_Item(1));
        if (!float.TryParse(s1, out result1))
          throw new CommandInputException("value", (object) s1, "Could not parse.");
        result2 = 0;
        break;
      case 3:
        string s2 = this.NameFormatter.ProcessExpression(args.get_Item(1));
        if (!float.TryParse(s2, out result1))
          throw new CommandInputException("value", (object) s2, "Could not parse.");
        string s3 = this.NameFormatter.ProcessExpression(args.get_Item(1));
        if (!int.TryParse(s3, out result2))
          throw new CommandInputException("precision", (object) s3, "Could not parse.");
        break;
      default:
        throw new CommandInputException(nameof (args), (object) args, "Invalid arguments. Use " + source + ",[value] OR " + source + ",[value],[precision]");
    }
    float num = Mathf.Pow(10f, (float) result2);
    return (rounder(result1 * num) / num).ToString((IFormatProvider) CultureInfo.InvariantCulture);
  }

  private static string GetRoundInfo(Func<RoundSummary, string> getter)
  {
    return !((UnityEngine.Object) RoundSummary.singleton == (UnityEngine.Object) null) ? getter(RoundSummary.singleton) : "-";
  }

  public string RefreshServerName()
  {
    return this.NameFormatter.ProcessExpression(ServerConsole._serverName);
  }

  public string RefreshServerNameSafe()
  {
    string result;
    if (this.NameFormatter.TryProcessExpression(ServerConsole._serverName, "server name", out result))
      return result;
    ServerConsole.AddLog(result);
    return "Command errored";
  }

  private void Awake()
  {
    ServerConsole.singleton = this;
  }

  private static void ReadLog(string path)
  {
    try
    {
      if (!File.Exists(path))
        return;
      string str1 = path.Remove(0, path.IndexOf("cs", StringComparison.Ordinal));
      string q = string.Empty;
      string str2 = string.Empty;
      try
      {
        str2 = "Error while reading the file: " + str1;
        string cmds;
        using (StreamReader streamReader = new StreamReader("SCPSL_Data/Dedicated/" + ServerConsole.Session + "/" + str1))
        {
          cmds = streamReader.ReadToEnd();
          str2 = "Error while dedecting 'terminator end-of-message' signal.";
          if (cmds.Contains("terminator"))
            cmds = cmds.Remove(cmds.LastIndexOf("terminator", StringComparison.Ordinal));
          str2 = "Error while sending message.";
          q = ServerConsole.EnterCommand(cmds, (CommandSender) null);
          try
          {
            str2 = "Error while closing the file: " + str1 + " :: " + cmds;
          }
          catch
          {
            str2 = "Error while closing the file.";
          }
        }
        try
        {
          str2 = "Error while deleting the file: " + str1 + " :: " + cmds;
        }
        catch (Exception ex)
        {
          UnityEngine.Debug.LogException(ex);
          str2 = "Error while deleting the file.";
        }
        File.Delete("SCPSL_Data/Dedicated/" + ServerConsole.Session + "/" + str1);
      }
      catch
      {
        UnityEngine.Debug.LogError((object) ("Error in server console: " + str2));
      }
      if (string.IsNullOrEmpty(q))
        return;
      ServerConsole.AddLog(q);
    }
    catch (Exception ex)
    {
      UnityEngine.Debug.LogException(ex);
    }
  }

  private static void CheckProcess()
  {
    while (!ServerConsole._disposing)
    {
      Thread.Sleep(4000);
      if (ServerConsole.ConsoleId == null || ServerConsole.ConsoleId.HasExited)
      {
        ServerConsole.DisposeStatic();
        ServerConsole.TerminateProcess();
      }
    }
  }

  private void Prompt()
  {
    while (!ServerConsole._disposing)
    {
      if (ServerConsole.PrompterQueue.Count == 0 || !ServerConsole._accepted)
      {
        Thread.Sleep(25);
      }
      else
      {
        string str = ServerConsole.PrompterQueue.Dequeue();
        if (!this._errorSent || !str.Contains("Could not update the session - Server is not verified."))
        {
          this._errorSent = true;
          StreamWriter streamWriter = new StreamWriter("SCPSL_Data/Dedicated/" + ServerConsole.Session + "/sl" + (object) ServerConsole._logId + ".mapi");
          ++ServerConsole._logId;
          streamWriter.WriteLine(str);
          streamWriter.Close();
        }
      }
    }
  }

  public void OnDestroy()
  {
    this.Dispose();
  }

  public void OnApplicationQuit()
  {
    this.Dispose();
  }

  public static void DisposeStatic()
  {
    ServerConsole.singleton.Dispose();
  }

  public static void AddLog(string q)
  {
    ServerConsole.PrintOnOutputs(q);
    if (ServerStatic.IsDedicated)
      ServerConsole.PrompterQueue.Enqueue(q);
    else
      GameCore.Console.AddLog(q, Color.grey, false);
  }

  public static string GetClientInfo(NetworkConnection conn)
  {
    GameObject connectedRoot = GameCore.Console.FindConnectedRoot(conn);
    return connectedRoot.GetComponent<NicknameSync>().MyNick + " ( " + connectedRoot.GetComponent<CharacterClassManager>().UserId + " | " + conn.address + " )";
  }

  public static string GetClientInfo(GameObject gameObject)
  {
    return gameObject.GetComponent<NicknameSync>().MyNick + " ( " + gameObject.GetComponent<CharacterClassManager>().UserId + " | " + gameObject.GetComponent<NetworkBehaviour>().connectionToClient.address + " )";
  }

  public static void Disconnect(GameObject player, string message)
  {
    if ((UnityEngine.Object) player == (UnityEngine.Object) null)
      return;
    NetworkBehaviour component1 = player.GetComponent<NetworkBehaviour>();
    if ((UnityEngine.Object) component1 == (UnityEngine.Object) null)
      return;
    CharacterClassManager component2 = player.GetComponent<CharacterClassManager>();
    if ((UnityEngine.Object) component2 == (UnityEngine.Object) null)
    {
      component1.connectionToClient.Disconnect();
      component1.connectionToClient.Dispose();
    }
    else
      component2.DisconnectClient(component1.connectionToClient, message);
  }

  public static void Disconnect(NetworkConnection conn, string message)
  {
    GameObject connectedRoot = GameCore.Console.FindConnectedRoot(conn);
    if ((UnityEngine.Object) connectedRoot == (UnityEngine.Object) null)
    {
      conn.Disconnect();
      conn.Dispose();
    }
    else
      ServerConsole.Disconnect(connectedRoot, message);
  }

  private static void ColorText(string text)
  {
    UnityEngine.Debug.Log((object) ("<color=" + ServerConsole.GetColor(text) + ">" + text + "</color>"), (UnityEngine.Object) null);
  }

  private static string GetColor(string text)
  {
    ushort num = 9;
    if (text.Contains("LOGTYPE"))
    {
      try
      {
        string str = text.Remove(0, text.IndexOf("LOGTYPE", StringComparison.Ordinal) + 7);
        num = ushort.Parse(str.Contains("-") ? str.Remove(0, 1) : str);
      }
      catch
      {
        num = (ushort) 9;
      }
    }
    switch (num)
    {
      case 0:
        return "#000";
      case 1:
        return "#183487";
      case 2:
        return "#0b7011";
      case 3:
        return "#0a706c";
      case 4:
        return "#700a0a";
      case 5:
        return "#5b0a40";
      case 6:
        return "#aaa800";
      case 7:
        return "#afafaf";
      case 8:
        return "#5b5b5b";
      case 9:
        return "#0055ff";
      case 10:
        return "#10ce1a";
      case 11:
        return "#0fc7ce";
      case 12:
        return "#ce0e0e";
      case 13:
        return "#c70dce";
      case 14:
        return "#ffff07";
      case 15:
        return "#e0e0e0";
      default:
        return "#fff";
    }
  }

  internal static string EnterCommand(string cmds, CommandSender sender = null)
  {
    string text = "Command accepted.";
    string[] args = cmds.Split(' ');
    if (args.Length == 0)
      return text;
    string cmd = args[0];
    string upper = cmd.ToUpper();
    if (!(upper == "FORCESTART"))
    {
      if (!(upper == "SNR") && !(upper == "STOPNEXTROUND"))
      {
        if (upper == "CONFIG")
        {
          if (File.Exists(FileManager.GetAppFolder(true, true, "") + "config_gameplay.txt"))
            Application.OpenURL(FileManager.GetAppFolder(true, true, "") + "config_gameplay.txt");
          else
            text = "Config file not found!";
        }
        else if (cmd.StartsWith("!") && cmd.Length > 1)
        {
          text = "Sending command to central servers...";
          new Thread((ThreadStart) (() => ServerConsole.RunCentralServerCommand(cmd.Substring(1).ToLower(), args.Length == 1 ? "" : ((IEnumerable<string>) args).Skip<string>(1).Aggregate<string>((Func<string, string, string>) ((current, next) => current + " " + next)))))
          {
            IsBackground = true,
            Priority = System.Threading.ThreadPriority.AboveNormal,
            Name = "SCP:SL Central server command execution"
          }.Start();
        }
        else
          text = GameCore.Console.singleton.TypeCommand(cmds, sender ?? (CommandSender) ServerConsole._scs);
      }
      else
      {
        ServerStatic.StopNextRound = !ServerStatic.StopNextRound;
        text = "Server " + (ServerStatic.StopNextRound ? "WILL" : "WON'T") + " stop after next round.";
      }
    }
    else
      text = CharacterClassManager.ForceRoundStart() ? "Forced round start." : "Failed to force start.LOGTYPE14";
    sender?.Print(text);
    return text;
  }

  public void RunServer()
  {
    if (ServerConsole._verificationRequestThread != null && ServerConsole._verificationRequestThread.IsAlive)
      ServerConsole._verificationRequestThread.Abort();
    ServerConsole._verificationRequestThread = new Thread(new ThreadStart(this.RefreshServerData))
    {
      IsBackground = true,
      Priority = System.Threading.ThreadPriority.AboveNormal,
      Name = "SCP:SL Server list thread"
    };
    ServerConsole._verificationRequestThread.Start();
  }

  internal static void RunRefreshPublicKey()
  {
    if (ServerConsole._refreshPublicKeyThread != null && ServerConsole._refreshPublicKeyThread.IsAlive)
      ServerConsole._refreshPublicKeyThread.Abort();
    ServerConsole._refreshPublicKeyThread = new Thread(new ThreadStart(ServerConsole.RefreshPublicKey))
    {
      IsBackground = true,
      Priority = System.Threading.ThreadPriority.Normal,
      Name = "SCP:SL Public key refreshing"
    };
    ServerConsole._refreshPublicKeyThread.Start();
  }

  internal static void RunRefreshPublicKeyOnce()
  {
    if (ServerConsole._refreshPublicKeyOnceThread != null && ServerConsole._refreshPublicKeyOnceThread.IsAlive)
      ServerConsole._refreshPublicKeyOnceThread.Abort();
    ServerConsole._refreshPublicKeyOnceThread = new Thread(new ThreadStart(ServerConsole.RefreshPublicKeyOnce))
    {
      IsBackground = true,
      Priority = System.Threading.ThreadPriority.AboveNormal,
      Name = "SCP:SL Public key refreshing ON DEMAND"
    };
    ServerConsole._refreshPublicKeyOnceThread.Start();
  }

  internal static void RunRefreshCentralServers()
  {
    if (ServerConsole._refreshServerListThread != null)
    {
      try
      {
        if (ServerConsole._refreshServerListThread.IsAlive)
          ServerConsole._refreshServerListThread.Abort();
        ServerConsole._refreshServerListThread = (Thread) null;
      }
      catch
      {
      }
    }
    ServerConsole._refreshServerListThread = new Thread((ThreadStart) (() => CentralServer.RefreshServerList(true, true)))
    {
      IsBackground = true,
      Priority = System.Threading.ThreadPriority.BelowNormal,
      Name = "SCP:SL Server list refreshing"
    };
    ServerConsole._refreshServerListThread.Start();
  }

  private static void RefreshPublicKey()
  {
    string key = CentralServerKeyCache.ReadCache();
    string empty = string.Empty;
    string str1 = string.Empty;
    if (!string.IsNullOrEmpty(key))
    {
      ServerConsole.PublicKey = ECDSA.PublicKeyFromString(key);
      empty = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey)));
      ServerConsole.AddLog("Loaded central server public key from cache.\nSHA256 of public key: " + empty);
    }
    ServerConsole.AddLog("Downloading public key from central server...");
    while (!ServerConsole._disposing)
    {
      try
      {
        PublicKeyResponse publicKeyResponse = JsonSerialize.FromJson<PublicKeyResponse>(HttpQuery.Get(CentralServer.StandardUrl + "v2/publickey.php"));
        if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
        {
          GameCore.Console.AddLog("Can't refresh central server public key - invalid signature!", Color.red, false);
          Thread.Sleep(360000);
          continue;
        }
        ServerConsole.PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
        string str2 = Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey)));
        if (str2 != str1)
        {
          str1 = str2;
          ServerConsole.AddLog("Downloaded public key from central server.\nSHA256 of public key: " + str2);
          if (str2 != empty)
            CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
          else
            ServerConsole.AddLog("SHA256 of cached key matches, no need to update cache.");
        }
        else
          ServerConsole.AddLog("Refreshed public key of central server - key hash not changed.");
      }
      catch (Exception ex)
      {
        ServerConsole.AddLog("Can't refresh central server public key - " + ex.Message);
      }
      Thread.Sleep(360000);
    }
  }

  private static void RefreshPublicKeyOnce()
  {
    try
    {
      PublicKeyResponse publicKeyResponse = JsonSerialize.FromJson<PublicKeyResponse>(HttpQuery.Get(CentralServer.StandardUrl + "v2/publickey.php"));
      if (!ECDSA.Verify(publicKeyResponse.key, publicKeyResponse.signature, CentralServerKeyCache.MasterKey))
      {
        GameCore.Console.AddLog("Can't refresh central server public key - invalid signature!", Color.red, false);
      }
      else
      {
        ServerConsole.PublicKey = ECDSA.PublicKeyFromString(publicKeyResponse.key);
        ServerConsole.AddLog("Downloaded public key from central server.\nSHA256 of public key: " + Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(ServerConsole.PublicKey))));
        CentralServerKeyCache.SaveCache(publicKeyResponse.key, publicKeyResponse.signature);
      }
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Can't refresh central server public key - " + ex.Message);
    }
  }

  private static void RunCentralServerCommand(string cmd, string args)
  {
    cmd = cmd.ToLower();
    List<string> stringList = new List<string>()
    {
      "ip=" + ServerConsole.Ip,
      "port=" + (object) ServerConsole.Port,
      "cmd=" + Misc.Base64Encode(cmd),
      "args=" + Misc.Base64Encode(args)
    };
    if (!string.IsNullOrEmpty(ServerConsole.Password))
      stringList.Add("passcode=" + ServerConsole.Password);
    try
    {
      string str = HttpQuery.Post(CentralServer.MasterUrl + "centralcommands/" + cmd + ".php", HttpQuery.ToPostArgs((IEnumerable<string>) stringList));
      ServerConsole.AddLog("[" + cmd + "] " + str);
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Could not execute the central server command \"" + cmd + "\" - (LOCAL EXCEPTION) " + ex.Message + "LOGTYPE-4");
    }
  }

  internal static void RefreshEmailSetStatus()
  {
    ServerConsole._emailSet = !string.IsNullOrEmpty(ConfigFile.ServerConfig.GetString("contact_email", ""));
  }

  private void RefreshServerData()
  {
    bool flag1 = true;
    byte num1 = 0;
    ServerConsole.RefreshEmailSetStatus();
    ServerConsole.RefreshToken(true);
    while (!ServerConsole._disposing)
    {
      ++num1;
      if (!flag1 && string.IsNullOrEmpty(ServerConsole.Password) && num1 < (byte) 15)
      {
        if (num1 == (byte) 5 || num1 == (byte) 12 || ServerConsole.ScheduleTokenRefresh)
          ServerConsole.RefreshToken(false);
      }
      else
      {
        flag1 = false;
        ServerConsole.Update = ServerConsole.Update || num1 == (byte) 10;
        string str1 = string.Empty;
        try
        {
          int count = ServerConsole.NewPlayers.Count;
          int num2 = 0;
          List<AuthenticatorPlayerObject> authenticatorPlayerObjectList = new List<AuthenticatorPlayerObject>();
          while (!ServerConsole.NewPlayers.IsEmpty)
          {
            ++num2;
            if (num2 <= count + 30)
            {
              try
              {
                CharacterClassManager result;
                if (ServerConsole.NewPlayers.TryTake(out result))
                {
                  if ((UnityEngine.Object) result != (UnityEngine.Object) null)
                    authenticatorPlayerObjectList.Add(new AuthenticatorPlayerObject(result.UserId, result.Connection == null || string.IsNullOrEmpty(result.Connection.address) ? "N/A" : result.Connection.address, result.RequestIp, result.Asn, result.AuthTokenSerial, result.VacSession));
                }
              }
              catch (Exception ex)
              {
                ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Player (inside of loop) processing: " + ex.Message);
                ServerConsole.AddLog(ex.StackTrace);
              }
            }
            else
              break;
          }
          str1 = JsonSerialize.ToJson<AuthenticatorPlayerObjects>(new AuthenticatorPlayerObjects(authenticatorPlayerObjectList.ToArray()));
        }
        catch (Exception ex)
        {
          ServerConsole.AddLog("[VERIFICATION THREAD] Exception in New Players processing: " + ex.Message);
          ServerConsole.AddLog(ex.StackTrace);
        }
        List<string> stringList1;
        if (!ServerConsole.Update)
        {
          stringList1 = new List<string>()
          {
            "ip=" + ServerConsole.Ip,
            "players=" + (object) ServerConsole.PlayersAmount + "/" + (object) CustomNetworkManager.slots,
            "newPlayers=" + str1,
            "port=" + (object) LiteNetLib4MirrorTransport.Singleton.port,
            "version=2",
            "enforceSameIp=" + ServerConsole.EnforceSameIp.ToString(),
            "enforceSameAsn=" + ServerConsole.EnforceSameAsn.ToString()
          };
        }
        else
        {
          List<string> stringList2 = new List<string>();
          stringList2.Add("ip=" + ServerConsole.Ip);
          stringList2.Add("players=" + (object) ServerConsole.PlayersAmount + "/" + (object) CustomNetworkManager.slots);
          stringList2.Add("playersList=" + ServerConsole._verificationPlayersList);
          stringList2.Add("newPlayers=" + str1);
          stringList2.Add("port=" + (object) LiteNetLib4MirrorTransport.Singleton.port);
          stringList2.Add("pastebin=" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT"));
          stringList2.Add("gameVersion=" + CustomNetworkManager.CompatibleVersions[0]);
          stringList2.Add("version=2");
          stringList2.Add("update=1");
          stringList2.Add("info=" + Misc.Base64Encode(this.RefreshServerNameSafe()).Replace('+', '-'));
          List<string> stringList3 = stringList2;
          bool flag2 = CustomNetworkManager.isPrivateBeta;
          string str2 = "privateBeta=" + flag2.ToString();
          stringList3.Add(str2);
          List<string> stringList4 = stringList2;
          flag2 = ServerStatic.PermissionsHandler.StaffAccess;
          string str3 = "staffRA=" + flag2.ToString();
          stringList4.Add(str3);
          stringList2.Add("friendlyFire=" + ServerConsole.FriendlyFire.ToString());
          stringList2.Add("geoblocking=" + (object) (byte) CustomLiteNetLib4MirrorTransport.Geoblocking);
          stringList2.Add("modded=" + CustomNetworkManager.Modded.ToString());
          stringList2.Add("whitelist=" + ServerConsole.WhiteListEnabled.ToString());
          stringList2.Add("accessRestriction=" + ServerConsole.AccessRestriction.ToString());
          stringList2.Add("emailSet=" + ServerConsole._emailSet.ToString());
          stringList2.Add("enforceSameIp=" + ServerConsole.EnforceSameIp.ToString());
          stringList2.Add("enforceSameAsn=" + ServerConsole.EnforceSameAsn.ToString());
          stringList1 = stringList2;
        }
        List<string> stringList5 = stringList1;
        if (!string.IsNullOrEmpty(ServerConsole.Password))
          stringList5.Add("passcode=" + ServerConsole.Password);
        ServerConsole.Update = false;
        if (!AuthenticatorQuery.SendData((IEnumerable<string>) stringList5) && !ServerConsole._printedNotVerifiedMessage)
        {
          ServerConsole._printedNotVerifiedMessage = true;
          ServerConsole.AddLog("Your server won't be visible on the public server list - (" + ServerConsole.Ip + ")LOGTYPE-8");
          if (!ServerConsole._emailSet)
          {
            ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server. LOGTYPE-8");
          }
          else
          {
            ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information: LOGTYPE-8");
            ServerConsole.AddLog("- IP address of server (probably " + ServerConsole.Ip + ") LOGTYPE-8");
            ServerConsole.AddLog("- is this static or dynamic IP address (most of home adresses are dynamic) LOGTYPE-8");
            ServerConsole.AddLog("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf LOGTYPE-8");
            ServerConsole.AddLog("send us that information to: server.verification@scpslgame.com (server.verification at scpslgame.com) LOGTYPE-8");
            ServerConsole.AddLog("if you can't see the AT sign in console (in above line): server.verification AT scpslgame.com LOGTYPE-8");
            ServerConsole.AddLog("email must be sent from email address set as \"contact_email\" in your config file (current value: " + ConfigFile.ServerConfig.GetString("contact_email", "") + "). LOGTYPE-8");
          }
        }
        else
          ServerConsole._printedNotVerifiedMessage = true;
      }
      if (num1 >= (byte) 15)
        num1 = (byte) 0;
      Thread.Sleep(5000);
      if (ServerConsole.ScheduleTokenRefresh || num1 == (byte) 0)
        ServerConsole.RefreshToken(false);
    }
  }

  private static void PrintOnOutputs(string text)
  {
    if (ServerConsole.ConsoleOutputs == null)
      return;
    foreach (IOutput consoleOutput in ServerConsole.ConsoleOutputs)
      consoleOutput.Print(text);
  }

  private static void RefreshToken(bool init = false)
  {
    ServerConsole.ScheduleTokenRefresh = false;
    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
    if (!File.Exists(path))
      return;
    StreamReader streamReader = new StreamReader(path);
    string str = streamReader.ReadToEnd().Trim();
    if (!init && string.IsNullOrEmpty(ServerConsole.Password) && !string.IsNullOrEmpty(str))
      ServerConsole.AddLog("Verification token loaded! Server probably will be listed on public list.");
    if (ServerConsole.Password != str)
    {
      ServerConsole.AddLog("Verification token reloaded.");
      ServerConsole.Update = true;
    }
    ServerConsole.Password = str;
    ServerStatic.PermissionsHandler.SetServerAsVerified();
    streamReader.Close();
  }

  private static void TerminateProcess()
  {
    ServerStatic.IsDedicated = false;
    UnityEngine.Debug.Log((object) "Process terminated");
    Application.Quit();
  }
}
