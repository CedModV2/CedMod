// Decompiled with JetBrains decompiler
// Type: CheaterReport
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using Mirror;
using RemoteAdmin;
using Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class CheaterReport : NetworkBehaviour
{
  private int reportedPlayersAmount;
  private float lastReport;
  private HashSet<int> reportedPlayers;
  private RateLimit _commandRateLimit;

  private void Start()
  {
    this._commandRateLimit = this.GetComponent<PlayerRateLimitHandler>().RateLimits[2];
  }

  internal void Report(int playerId, string reason)
  {
    GameObject gameObject = PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (pl => pl.GetComponent<QueryProcessor>().PlayerId == playerId));
    if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
    {
      GameCore.Console.AddLog("[REPORTING] Can't find player with that PlayerID.", Color.red, false);
    }
    else
    {
      gameObject.GetComponent<CharacterClassManager>().CheatReported = true;
      this.CmdReport(playerId, reason, ECDSA.SignBytes(gameObject.GetComponent<CharacterClassManager>().UserId + ";" + reason, GameCore.Console.SessionKeys.Private));
    }
  }

  [Command]
  internal void CmdReport(int playerId, string reason, byte[] signature)
  {
    if (this.isServer)
    {
      this.CallCmdReport(playerId, reason, signature);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(playerId);
      writer.WriteString(reason);
      writer.WriteBytesAndSize(signature);
      this.SendCommandInternal(typeof (CheaterReport), nameof (CmdReport), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void IssueReport(
    GameConsoleTransmission reporter,
    string reporterSteamId,
    string reportedSteamId,
    string reportedAuth,
    string reportedIp,
    string reporterAuth,
    string reporterIp,
    ref string reason,
    ref byte[] signature,
    string reporterPublicKey,
    int reportedId)
  {
    try
    {
      if (reporterSteamId == reportedSteamId)
      {
        this.TargetReportUpdate(this.connectionToClient, reportedId, false);
        reporter.SendToClient(this.connectionToClient, "You can't report yourself!" + Environment.NewLine, "yellow");
      }
      else
      {
        string str = HttpQuery.Post(CentralServer.StandardUrl + "ingamereport.php", string.Format("reporterAuth={0}&reporterIp={1}&reportedAuth={2}&reportedIp={3}&reason={4}&signature={5}&reporterKey={6}&token={7}&port={8}&serverIp={9}", (object) Misc.Base64Encode(reporterAuth), (object) reporterIp, (object) Misc.Base64Encode(reportedAuth), (object) reportedIp, (object) Misc.Base64Encode(reason), (object) Convert.ToBase64String(signature), (object) Misc.Base64Encode(reporterPublicKey), (object) ServerConsole.Password, (object) ServerConsole.Port, (object) ServerConsole.Ip));
        if ((UnityEngine.Object) reporter == (UnityEngine.Object) null)
          return;
        if (!(str == "OK"))
        {
          if (!(str == "ReportedUserIDAlreadyReported"))
          {
            if (str == "RateLimited")
            {
              this.TargetReportUpdate(this.connectionToClient, reportedId, false);
              reporter.SendToClient(this.connectionToClient, "You are Ratelimited! Try again tomorrow." + Environment.NewLine, "red");
            }
            else
            {
              this.TargetReportUpdate(this.connectionToClient, reportedId, false);
              reporter.SendToClient(this.connectionToClient, "Error during **PROCESSING** player report:" + Environment.NewLine + str, "red");
            }
          }
          else
          {
            this.TargetReportUpdate(this.connectionToClient, reportedId, false);
            reporter.SendToClient(this.connectionToClient, "A report for this User ID already exists!" + Environment.NewLine, "yellow");
          }
        }
        else
        {
          this.reportedPlayers.Add(reportedId);
          reporter.SendToClient(this.connectionToClient, "Player report successfully sent.", "green");
        }
      }
    }
    catch (Exception ex)
    {
      GameCore.Console.AddLog("[HOST] Error during **SENDING** player report:" + Environment.NewLine + ex.Message, Color.red, false);
      if ((UnityEngine.Object) reporter == (UnityEngine.Object) null)
        return;
      this.TargetReportUpdate(this.connectionToClient, reportedId, false);
      reporter.SendToClient(this.connectionToClient, "Error during **SENDING** player report.", "yellow");
    }
  }

  [TargetRpc]
  private void TargetReportUpdate(NetworkConnection conn, int playerId, bool status)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WritePackedInt32(playerId);
    writer.WriteBoolean(status);
    this.SendTargetRPCInternal(conn, typeof (CheaterReport), nameof (TargetReportUpdate), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeCmdCmdReport(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdReport called on client.");
    else
      ((CheaterReport) obj).CallCmdReport(reader.ReadPackedInt32(), reader.ReadString(), reader.ReadBytesAndSize());
  }

  public void CallCmdReport(int playerId, string reason, byte[] signature)
  {
    if (!this._commandRateLimit.CanExecute(true) || reason == null || signature == null)
      return;
    float num = Time.time - this.lastReport;
    if ((double) num < 2.0)
    {
      this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Reporting rate limit exceeded (1).", "red");
    }
    else
    {
      if ((double) num > 60.0)
        this.reportedPlayersAmount = 0;
      if (this.reportedPlayersAmount > 5)
        this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Reporting rate limit exceeded (2).", "red");
      else if (!ServerStatic.GetPermissionsHandler().IsVerified || string.IsNullOrEmpty(ServerConsole.Password))
      {
        this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Server is not verified - you can't use report feature on this server.", "red");
      }
      else
      {
        GameObject gameObject = PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (pl => pl.GetComponent<QueryProcessor>().PlayerId == playerId));
        if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
        {
          this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Can't find player with that PlayerID.", "red");
        }
        else
        {
          CharacterClassManager reportedCcm = gameObject.GetComponent<CharacterClassManager>();
          CharacterClassManager reporterCcm = this.GetComponent<CharacterClassManager>();
          if (this.reportedPlayers == null)
            this.reportedPlayers = new HashSet<int>();
          if (this.reportedPlayers.Contains(playerId))
            this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] You have already reported that player.", "red");
          else if (string.IsNullOrEmpty(reportedCcm.UserId))
            this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Failed: User ID of reported player is null.", "red");
          else if (string.IsNullOrEmpty(reporterCcm.UserId))
            this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Failed: your User ID of is null.", "red");
          else if (!ECDSA.VerifyBytes(reportedCcm.UserId + ";" + reason, signature, this.GetComponent<ServerRoles>().PublicKey))
          {
            this.GetComponent<GameConsoleTransmission>().SendToClient(this.connectionToClient, "[REPORTING] Invalid report signature.", "red");
          }
          else
          {
            this.lastReport = Time.time;
            ++this.reportedPlayersAmount;
            GameCore.Console.AddLog("Player " + reporterCcm.GetComponent<NicknameSync>().MyNick + "(" + reporterCcm.UserId + ") reported player " + reportedCcm.GetComponent<NicknameSync>().MyNick + "(" + reportedCcm.UserId + ") with reason " + reason + ".", Color.gray, false);
            new Thread((ThreadStart) (() => this.IssueReport(this.GetComponent<GameConsoleTransmission>(), reporterCcm.UserId, reportedCcm.UserId, reportedCcm.AuthToken, reportedCcm.connectionToClient.address, reporterCcm.AuthToken, reporterCcm.connectionToClient.address, ref reason, ref signature, ECDSA.KeyToString(this.GetComponent<ServerRoles>().PublicKey), playerId)))
            {
              Priority = System.Threading.ThreadPriority.Lowest,
              IsBackground = true,
              Name = ("Reporting player - " + reportedCcm.UserId + " by " + reporterCcm.UserId)
            }.Start();
          }
        }
      }
    }
  }

  protected static void InvokeRpcTargetReportUpdate(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetReportUpdate called on server.");
    else
      ((CheaterReport) obj).CallTargetReportUpdate(ClientScene.readyConnection, reader.ReadPackedInt32(), reader.ReadBoolean());
  }

  public void CallTargetReportUpdate(NetworkConnection conn, int playerId, bool status)
  {
    GameObject gameObject = PlayerManager.players.FirstOrDefault<GameObject>((Func<GameObject, bool>) (pl => pl.GetComponent<QueryProcessor>().PlayerId == playerId));
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    gameObject.GetComponent<CharacterClassManager>().CheatReported = status;
  }

  static CheaterReport()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (CheaterReport), "CmdReport", new NetworkBehaviour.CmdDelegate(CheaterReport.InvokeCmdCmdReport));
    NetworkBehaviour.RegisterRpcDelegate(typeof (CheaterReport), "TargetReportUpdate", new NetworkBehaviour.CmdDelegate(CheaterReport.InvokeRpcTargetReportUpdate));
  }
}
