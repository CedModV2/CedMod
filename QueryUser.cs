// Decompiled with JetBrains decompiler
// Type: QueryUser
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

internal class QueryUser : IDisposable
{
  private readonly NetworkStream _s;
  private readonly QueryServer _server;
  private readonly Thread _thr;
  private readonly Thread _sol;
  private int _lastping;
  private bool _closing;
  private int _invalidPackets;
  private readonly string _querypassword;
  internal readonly string Ip;
  private bool _authenticated;
  private readonly UTF8Encoding _encoder;
  private readonly UserPrint _printer;
  internal ulong Permissions;
  internal byte KickPower;

  internal QueryUser(QueryServer s, TcpClient c, string ip)
  {
    this._s = c.GetStream();
    this._server = s;
    this.Ip = ip;
    this.Send("Welcome to SCP Secret Laboratory Query Protocol");
    this._thr = new Thread(new ThreadStart(this.Receive))
    {
      IsBackground = true
    };
    this._thr.Start();
    this._encoder = new UTF8Encoding();
    this._querypassword = ConfigFile.ServerConfig.GetString("administrator_query_password", "");
    this._lastping = Convert.ToInt32(this._server.Stopwatch.Elapsed.TotalSeconds) + 5;
    this._authenticated = false;
    this._printer = new UserPrint(this);
  }

  internal bool IsConnected()
  {
    return this._server.Stopwatch.Elapsed.TotalSeconds - (double) this._lastping < (double) this._server.TimeoutThreshold;
  }

  private void Receive()
  {
    this._s.ReadTimeout = 200;
    this._s.WriteTimeout = 200;
    while (!this._closing)
    {
      try
      {
        byte[] numArray = new byte[4096];
        int num;
        try
        {
          num = this._s.Read(numArray, 0, 4096);
        }
        catch
        {
          num = -1;
          Thread.Sleep(5);
        }
        if (num > -1)
        {
          foreach (byte[] bytes in AuthenticatedMessage.Decode(numArray))
          {
            string str = this._encoder.GetString(bytes);
            AuthenticatedMessage authenticatedMessage = (AuthenticatedMessage) null;
            string message1;
            try
            {
              message1 = str.Substring(0, str.LastIndexOf(';'));
            }
            catch
            {
              ++this._invalidPackets;
              message1 = str.TrimEnd(new char[1]);
              if (message1.EndsWith(";"))
                message1 = message1.Substring(0, message1.Length - 1);
            }
            if (this._invalidPackets >= 5)
            {
              if (!this._closing)
              {
                this.Send("Too many invalid packets sent.");
                ServerConsole.AddLog("Query connection from " + this.Ip + " dropped due to too many invalid packets sent.");
                this._server.Users.Remove(this);
                this.CloseConn(false);
                break;
              }
              break;
            }
            try
            {
              authenticatedMessage = AuthenticatedMessage.AuthenticateMessage(message1, TimeBehaviour.CurrentTimestamp(), this._querypassword);
            }
            catch (MessageAuthenticationFailureException ex)
            {
              this.Send("Message can't be authenticated - " + ex.Message);
              ServerConsole.AddLog("Query command from " + this.Ip + " can't be authenticated - " + ex.Message);
            }
            catch (MessageExpiredException ex)
            {
              this.Send("Message expired");
              ServerConsole.AddLog("Query command from " + this.Ip + " is expired.");
            }
            catch (Exception ex)
            {
              this.Send("Error during processing your message.");
              ServerConsole.AddLog("Query command from " + this.Ip + " can't be processed - " + ex.Message + ".");
            }
            if (authenticatedMessage != null)
            {
              if (!this._authenticated && authenticatedMessage.Administrator)
                this._authenticated = true;
              string message2 = authenticatedMessage.Message;
              string[] strArray = new string[0];
              if (message2.Contains(" "))
              {
                message2 = message2.Split(' ')[0];
                authenticatedMessage.Message.Substring(message2.Length + 1).Split(' ');
              }
              message2.ToLower();
              if (authenticatedMessage.Message == "Ping")
              {
                this._invalidPackets = 0;
                this._lastping = Convert.ToInt32(this._server.Stopwatch.Elapsed.TotalSeconds);
                this.Send("Pong");
              }
              else if (this.AdminCheck(authenticatedMessage.Administrator))
                ServerConsole.EnterCommand(authenticatedMessage.Message, (CommandSender) null);
            }
          }
        }
      }
      catch (SocketException ex)
      {
        ServerConsole.AddLog("Query connection from " + this.Ip + " dropped (SocketException).");
        if (this._closing)
          break;
        this._server.Users.Remove(this);
        this.CloseConn(false);
        break;
      }
      catch
      {
        ServerConsole.AddLog("Query connection from " + this.Ip + " dropped.");
        if (this._closing)
          break;
        this._server.Users.Remove(this);
        this.CloseConn(false);
        break;
      }
    }
  }

  private bool AdminCheck(bool admin)
  {
    if (!admin)
      this.Send("Access denied! You need to have administrator permissions.");
    return admin;
  }

  public void CloseConn(bool shuttingdown = false)
  {
    this._closing = true;
    if (shuttingdown)
      this.Send("Server is shutting down...");
    this._s.Close();
    this._thr?.Abort();
    this.Dispose();
  }

  public void Send(string msg)
  {
    msg = !this._authenticated || this._querypassword == "" || (this._querypassword == "none" || this._querypassword == null) ? AuthenticatedMessage.GenerateNonAuthenticatedMessage(msg) : AuthenticatedMessage.GenerateAuthenticatedMessage(msg, TimeBehaviour.CurrentTimestamp(), this._querypassword);
    this.Send(Utf8.GetBytes(msg));
  }

  public void Send(byte[] msg)
  {
    try
    {
      byte[] buffer = AuthenticatedMessage.Encode(msg);
      this._s.Write(buffer, 0, buffer.Length);
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog("Can't send query response to " + this.Ip + ": " + ex.StackTrace);
    }
  }

  public void Dispose()
  {
    this._s?.Dispose();
    if (!ServerConsole.ConsoleOutputs.Contains((IOutput) this._printer))
      return;
    ServerConsole.ConsoleOutputs.Remove((IOutput) this._printer);
  }
}
