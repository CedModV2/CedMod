// Decompiled with JetBrains decompiler
// Type: ServerLogs
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using Mirror.LiteNetLib4Mirror;
using System;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerLogs : MonoBehaviour
{
  public static readonly string[] Txt = new string[7]
  {
    "Connection update",
    "Remote Admin",
    "Remote Admin - Misc",
    "Kill",
    "Game Event",
    "Internal",
    "Rate Limit"
  };
  public static readonly string[] Modulestxt = new string[7]
  {
    "Warhead",
    "Networking",
    "Class change",
    "Permissions",
    "Administrative",
    "Logger",
    "Data access"
  };
  private static readonly System.Collections.Generic.Queue<ServerLogs.ServerLog> Queue = new System.Collections.Generic.Queue<ServerLogs.ServerLog>();
  private static readonly object LockObject = new object();
  private Thread _appendThread;
  public static ServerLogs singleton;
  private int _port;
  private int _maxlen;
  private int _modulemaxlen;
  private static volatile bool _exit;
  private static volatile bool _write;
  private string _roundStartTime;

  private void Awake()
  {
    ServerLogs.singleton = this;
    foreach (string str in ServerLogs.Txt)
      this._maxlen = Math.Max(this._maxlen, str.Length);
    foreach (string str in ServerLogs.Modulestxt)
      this._modulemaxlen = Math.Max(this._modulemaxlen, str.Length);
    this._appendThread = new Thread(new ThreadStart(this.AppendLog))
    {
      Name = "Saving server logs to file",
      Priority = System.Threading.ThreadPriority.BelowNormal,
      IsBackground = true
    };
    this._appendThread.Start();
  }

  public static void AddLog(ServerLogs.Modules module, string msg, ServerLogs.ServerLogType type)
  {
    string time = TimeBehaviour.FormatTime("yyyy-MM-dd HH:mm:ss.fff zzz");
    lock (ServerLogs.LockObject)
      ServerLogs.Queue.Enqueue(new ServerLogs.ServerLog(msg, ServerLogs.Txt[(int) type], ServerLogs.Modulestxt[(int) module], time));
    ServerLogs._write = true;
  }

  private void Start()
  {
    this._port = (int) LiteNetLib4MirrorTransport.Singleton.port;
    this._roundStartTime = TimeBehaviour.FormatTime("yyyy-MM-dd HH.mm.ss");
  }

  private void OnDestroy()
  {
    ServerLogs._write = true;
    ServerLogs._exit = true;
  }

  private void AppendLog()
  {
    while (string.IsNullOrEmpty(this._roundStartTime))
      Thread.Sleep(100);
    StringBuilder stringBuilder = new StringBuilder();
    string str = this._port.ToString();
    ServerLogs.AddLog(ServerLogs.Modules.Logger, "Started logging. Game version: " + CustomNetworkManager.CompatibleVersions[0] + ", private beta: " + (CustomNetworkManager.isPrivateBeta ? "YES" : "NO") + ".", ServerLogs.ServerLogType.InternalMessage);
    while (NetworkServer.active)
    {
      Thread.Sleep(100);
      if (ServerLogs._write)
      {
        if (!Directory.Exists(FileManager.GetAppFolder(true, false, "")))
          break;
        if (!Directory.Exists(FileManager.GetAppFolder(true, false, "") + nameof (ServerLogs)))
          Directory.CreateDirectory(FileManager.GetAppFolder(true, false, "") + nameof (ServerLogs));
        if (!Directory.Exists(FileManager.GetAppFolder(true, false, "") + "ServerLogs/" + str))
          Directory.CreateDirectory(FileManager.GetAppFolder(true, false, "") + "ServerLogs/" + str);
        stringBuilder.Clear();
        lock (ServerLogs.LockObject)
        {
          ServerLogs.ServerLog element;
          while (ServerLogs.Queue.TryDequeue<ServerLogs.ServerLog>(out element))
            stringBuilder.Append(element.Time + " | " + ServerLogs.ToMax(element.Type, this._maxlen) + " | " + ServerLogs.ToMax(element.Module, this._modulemaxlen) + " | " + element.Content + Environment.NewLine);
        }
        using (StreamWriter streamWriter = new StreamWriter(FileManager.GetAppFolder(true, false, "") + "ServerLogs/" + str + "/Round " + this._roundStartTime + ".txt", true))
          streamWriter.Write(stringBuilder.ToString());
        ServerLogs._write = false;
        if (ServerLogs._exit)
          break;
      }
    }
  }

  private static string ToMax(string text, int max)
  {
    while (text.Length < max)
      text += " ";
    return text;
  }

  public enum ServerLogType : byte
  {
    ConnectionUpdate,
    RemoteAdminActivity_GameChanging,
    RemoteAdminActivity_Misc,
    KillLog,
    GameEvent,
    InternalMessage,
    RateLimit,
  }

  public enum Modules : byte
  {
    Warhead,
    Networking,
    ClassChange,
    Permissions,
    Administrative,
    Logger,
    DataAccess,
  }

  public readonly struct ServerLog : IEquatable<ServerLogs.ServerLog>
  {
    public readonly string Content;
    public readonly string Type;
    public readonly string Module;
    public readonly string Time;

    public ServerLog(string content, string type, string module, string time)
    {
      this.Content = content;
      this.Type = type;
      this.Module = module;
      this.Time = time;
    }

    public bool Equals(ServerLogs.ServerLog other)
    {
      return this.Content == other.Content && this.Type == other.Type && this.Module == other.Module && this.Time == other.Time;
    }

    public override bool Equals(object obj)
    {
      return obj is ServerLogs.ServerLog other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return (((this.Content != null ? this.Content.GetHashCode() : 0) * 397 ^ (this.Type != null ? this.Type.GetHashCode() : 0)) * 397 ^ (this.Module != null ? this.Module.GetHashCode() : 0)) * 397 ^ (this.Time != null ? this.Time.GetHashCode() : 0);
    }

    public static bool operator ==(ServerLogs.ServerLog left, ServerLogs.ServerLog right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(ServerLogs.ServerLog left, ServerLogs.ServerLog right)
    {
      return !left.Equals(right);
    }
  }
}
