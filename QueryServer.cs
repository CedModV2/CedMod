// Decompiled with JetBrains decompiler
// Type: QueryServer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

internal class QueryServer
{
  internal int TimeoutThreshold = 10;
  private readonly int _port;
  private readonly bool _useV6;
  internal List<QueryUser> Users;
  private Thread _thr;
  private Thread _checkThr;
  internal Stopwatch Stopwatch;
  private TcpListener _listner;
  private TcpListener _listnerv6;
  private bool _serverStop;

  internal QueryServer(int p, bool v6)
  {
    this._port = p;
    this._useV6 = v6;
  }

  internal void StartServer()
  {
    this._serverStop = false;
    this.Stopwatch = new Stopwatch();
    this._thr = new Thread(new ThreadStart(this.StartUp))
    {
      IsBackground = true
    };
    this._thr.Start();
    this._checkThr = new Thread(new ThreadStart(this.CheckClients))
    {
      IsBackground = true,
      Priority = ThreadPriority.BelowNormal
    };
  }

  private void CheckClients()
  {
    while (!this._serverStop)
    {
      for (int index = this.Users.Count - 1; index >= 0; --index)
      {
        if (!this.Users[index].IsConnected())
        {
          ServerConsole.AddLog("Query user connected from " + this.Users[index].Ip + " timed out.");
          try
          {
            this.Users[index].CloseConn(false);
            this.Users.RemoveAt(index);
          }
          catch
          {
          }
        }
      }
      Thread.Sleep(10000);
    }
  }

  internal void StopServer()
  {
    ServerConsole.AddLog("Stopping query server...");
    this._checkThr.Abort();
    this._serverStop = true;
  }

  private void StartUp()
  {
    ServerConsole.AddLog("Starting query server on port " + (object) this._port + " TCP...");
    this.Users = new List<QueryUser>();
    this.Stopwatch.Start();
    this._checkThr.Start();
    try
    {
      this._listner = new TcpListener(IPAddress.Any, this._port);
      this._listner.Start();
      if (this._useV6)
      {
        this._listnerv6 = new TcpListener(IPAddress.IPv6Any, this._port);
        this._listnerv6.Start();
      }
      while (!this._serverStop)
      {
        if (this._listner.Pending())
          this.AcceptSocket(this._listner);
        else if (this._listnerv6.Pending())
          this.AcceptSocket(this._listnerv6);
        else
          Thread.Sleep(500);
      }
      this._listner.Stop();
      if (this._useV6)
        this._listnerv6.Stop();
      foreach (QueryUser user in this.Users)
        user.CloseConn(true);
      this.Users.Clear();
      ServerConsole.AddLog("Query server stopped.");
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Server ERROR: " + ex.StackTrace);
    }
  }

  private void AcceptSocket(TcpListener lst)
  {
    TcpClient c = lst.AcceptTcpClient();
    this.Users.Add(new QueryUser(this, c, c.Client.RemoteEndPoint.ToString()));
    ServerConsole.AddLog("New query connection from " + (object) c.Client.RemoteEndPoint + " on " + (object) c.Client.LocalEndPoint + ".");
  }
}
