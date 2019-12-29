// Decompiled with JetBrains decompiler
// Type: RoundSummary
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundSummary : NetworkBehaviour
{
  private bool roundEnded;
  public RoundSummary.SumInfo_ClassList classlistStart;
  public GameObject ui_root;
  public static bool RoundLock;
  public static RoundSummary singleton;
  public static int roundTime;
  public static float Damages;
  public static int Kills;
  public static int escaped_ds;
  public static int escaped_scientists;
  public static int kills_by_scp;
  public static int kills_by_frag;
  public static int changed_into_zombies;

  private void Start()
  {
    if (!NetworkServer.active)
      return;
    RoundSummary.roundTime = 0;
    RoundSummary.singleton = this;
    Timing.RunCoroutine(this._ProcessServerSideCode(), Segment.FixedUpdate);
    RoundSummary.kills_by_scp = 0;
    RoundSummary.escaped_ds = 0;
    RoundSummary.escaped_scientists = 0;
    RoundSummary.changed_into_zombies = 0;
    RoundSummary.Damages = 0.0f;
    RoundSummary.Kills = 0;
  }

  public void SetStartClassList(RoundSummary.SumInfo_ClassList info)
  {
    this.classlistStart = info;
  }

  public int CountRole(RoleType role)
  {
    return PlayerManager.players.Count<GameObject>((Func<GameObject, bool>) (player => (UnityEngine.Object) player != (UnityEngine.Object) null && player.GetComponent<CharacterClassManager>().CurClass == role));
  }

  public int CountTeam(Team team)
  {
    int num = 0;
    foreach (GameObject player in PlayerManager.players)
    {
      if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
      {
        CharacterClassManager component = player.GetComponent<CharacterClassManager>();
        if (component.Classes.CheckBounds(component.CurClass) && component.Classes[(int) component.CurClass].team == team)
          ++num;
      }
    }
    return num;
  }

  private IEnumerator<float> _ProcessServerSideCode()
  {
    RoundSummary roundSummary = this;
    while ((UnityEngine.Object) roundSummary != (UnityEngine.Object) null)
    {
      while (RoundSummary.RoundLock || !RoundSummary.RoundInProgress() || PlayerManager.players.Count < 2)
        yield return 0.0f;
      RoundSummary.SumInfo_ClassList newList = new RoundSummary.SumInfo_ClassList();
      foreach (GameObject player in PlayerManager.players)
      {
        if (!((UnityEngine.Object) player == (UnityEngine.Object) null))
        {
          CharacterClassManager component = player.GetComponent<CharacterClassManager>();
          if (component.Classes.CheckBounds(component.CurClass))
          {
            switch (component.Classes.SafeGet(component.CurClass).team)
            {
              case Team.SCP:
                if (component.CurClass == RoleType.Scp0492)
                {
                  ++newList.zombies;
                  continue;
                }
                ++newList.scps_except_zombies;
                continue;
              case Team.MTF:
                ++newList.mtf_and_guards;
                continue;
              case Team.CHI:
                ++newList.chaos_insurgents;
                continue;
              case Team.RSC:
                ++newList.scientists;
                continue;
              case Team.CDP:
                ++newList.class_ds;
                continue;
              default:
                continue;
            }
          }
        }
      }
      newList.warhead_kills = AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
      yield return float.NegativeInfinity;
      newList.time = (int) Time.realtimeSinceStartup;
      yield return float.NegativeInfinity;
      RoundSummary.roundTime = newList.time - roundSummary.classlistStart.time;
      int num1 = newList.mtf_and_guards + newList.scientists;
      int num2 = newList.chaos_insurgents + newList.class_ds;
      int num3 = newList.scps_except_zombies + newList.zombies;
      float num4 = roundSummary.classlistStart.class_ds == 0 ? 0.0f : (float) ((RoundSummary.escaped_ds + newList.class_ds) / roundSummary.classlistStart.class_ds);
      float num5 = roundSummary.classlistStart.scientists == 0 ? 1f : (float) ((RoundSummary.escaped_scientists + newList.scientists) / roundSummary.classlistStart.scientists);
      if (newList.class_ds == 0 && num1 == 0)
      {
        roundSummary.roundEnded = true;
      }
      else
      {
        int num6 = 0;
        if (num1 > 0)
          ++num6;
        if (num2 > 0)
          ++num6;
        if (num3 > 0)
          ++num6;
        if (num6 <= 1)
          roundSummary.roundEnded = true;
      }
      if (roundSummary.roundEnded)
      {
        RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;
        if (num1 > 0)
        {
          if (RoundSummary.escaped_ds == 0 && RoundSummary.escaped_scientists != 0)
            leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
        }
        else
          leadingTeam = RoundSummary.escaped_ds != 0 ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Anomalies;
        string str = "Round finished! Anomalies: " + (object) num3 + " | Chaos: " + (object) num2 + " | Facility Forces: " + (object) num1 + " | D escaped percentage: " + (object) num4 + " | S escaped percentage: : " + (object) num5;
        GameCore.Console.AddLog(str, Color.gray, false);
        ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
        byte i1;
        for (i1 = (byte) 0; i1 < (byte) 75; ++i1)
          yield return 0.0f;
        int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
        if ((UnityEngine.Object) roundSummary != (UnityEngine.Object) null)
          roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, newList, leadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, timeToRoundRestart);
        for (int i2 = 0; i2 < 50 * (timeToRoundRestart - 1); ++i2)
          yield return 0.0f;
        roundSummary.RpcDimScreen();
        for (i1 = (byte) 0; i1 < (byte) 50; ++i1)
          yield return 0.0f;
        PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
      }
    }
  }

  [ClientRpc]
  private void RpcShowRoundSummary(
    RoundSummary.SumInfo_ClassList list_start,
    RoundSummary.SumInfo_ClassList list_finish,
    RoundSummary.LeadingTeam leadingTeam,
    int e_ds,
    int e_sc,
    int scp_kills,
    int round_cd)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    GeneratedNetworkCode._WriteSumInfo_ClassList_RoundSummary(writer, list_start);
    GeneratedNetworkCode._WriteSumInfo_ClassList_RoundSummary(writer, list_finish);
    writer.WritePackedInt32((int) leadingTeam);
    writer.WritePackedInt32(e_ds);
    writer.WritePackedInt32(e_sc);
    writer.WritePackedInt32(scp_kills);
    writer.WritePackedInt32(round_cd);
    this.SendRPCInternal(typeof (RoundSummary), nameof (RpcShowRoundSummary), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  private void RpcDimScreen()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (RoundSummary), nameof (RpcDimScreen), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  public static bool RoundInProgress()
  {
    return (UnityEngine.Object) PlayerManager.localPlayer != (UnityEngine.Object) null && PlayerManager.localPlayer.GetComponent<CharacterClassManager>().RoundStarted && !RoundSummary.singleton.roundEnded && (UnityEngine.Object) AlphaWarheadController.Host != (UnityEngine.Object) null;
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcShowRoundSummary(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcShowRoundSummary called on server.");
    else
      ((RoundSummary) obj).CallRpcShowRoundSummary(GeneratedNetworkCode._ReadSumInfo_ClassList_RoundSummary(reader), GeneratedNetworkCode._ReadSumInfo_ClassList_RoundSummary(reader), (RoundSummary.LeadingTeam) reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadPackedInt32());
  }

  protected static void InvokeRpcRpcDimScreen(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcDimScreen called on server.");
    else
      ((RoundSummary) obj).CallRpcDimScreen();
  }

  public void CallRpcShowRoundSummary(
    RoundSummary.SumInfo_ClassList list_start,
    RoundSummary.SumInfo_ClassList list_finish,
    RoundSummary.LeadingTeam leadingTeam,
    int e_ds,
    int e_sc,
    int scp_kills,
    int round_cd)
  {
  }

  public void CallRpcDimScreen()
  {
  }

  static RoundSummary()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (RoundSummary), "RpcShowRoundSummary", new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeRpcRpcShowRoundSummary));
    NetworkBehaviour.RegisterRpcDelegate(typeof (RoundSummary), "RpcDimScreen", new NetworkBehaviour.CmdDelegate(RoundSummary.InvokeRpcRpcDimScreen));
  }

  public enum LeadingTeam
  {
    FacilityForces,
    ChaosInsurgency,
    Anomalies,
    Draw,
  }

  [Serializable]
  public struct SumInfo_ClassList : IEquatable<RoundSummary.SumInfo_ClassList>
  {
    public int class_ds;
    public int scientists;
    public int chaos_insurgents;
    public int mtf_and_guards;
    public int scps_except_zombies;
    public int zombies;
    public int warhead_kills;
    public int time;

    public bool Equals(RoundSummary.SumInfo_ClassList other)
    {
      return this.class_ds == other.class_ds && this.scientists == other.scientists && (this.chaos_insurgents == other.chaos_insurgents && this.mtf_and_guards == other.mtf_and_guards) && (this.scps_except_zombies == other.scps_except_zombies && this.zombies == other.zombies && this.warhead_kills == other.warhead_kills) && this.time == other.time;
    }

    public override bool Equals(object obj)
    {
      return obj is RoundSummary.SumInfo_ClassList other && this.Equals(other);
    }

    public override int GetHashCode()
    {
      return ((((((this.class_ds * 397 ^ this.scientists) * 397 ^ this.chaos_insurgents) * 397 ^ this.mtf_and_guards) * 397 ^ this.scps_except_zombies) * 397 ^ this.zombies) * 397 ^ this.warhead_kills) * 397 ^ this.time;
    }

    public static bool operator ==(
      RoundSummary.SumInfo_ClassList left,
      RoundSummary.SumInfo_ClassList right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(
      RoundSummary.SumInfo_ClassList left,
      RoundSummary.SumInfo_ClassList right)
    {
      return !left.Equals(right);
    }
  }
}
