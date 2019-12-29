// Decompiled with JetBrains decompiler
// Type: RoundPlayerHistory
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

public class RoundPlayerHistory : MonoBehaviour
{
  public List<RoundPlayerHistory.PlayerHistoryLog> historyLogs = new List<RoundPlayerHistory.PlayerHistoryLog>();
  public static RoundPlayerHistory singleton;

  private void Awake()
  {
    RoundPlayerHistory.singleton = this;
  }

  public RoundPlayerHistory.PlayerHistoryLog GetData(int playerId)
  {
    foreach (RoundPlayerHistory.PlayerHistoryLog historyLog in this.historyLogs)
    {
      if (historyLog.PlayerId == playerId)
        return historyLog;
    }
    return (RoundPlayerHistory.PlayerHistoryLog) null;
  }

  public void SetData(
    int playerId,
    string newNick,
    int newPlayerId,
    string newUserId,
    int newConnectionStatus,
    int newAliveClass,
    int newCurrentClass,
    DateTime newStartTime,
    DateTime newStopTime)
  {
    int index1 = -1;
    if (playerId == -1)
    {
      this.historyLogs.Add(new RoundPlayerHistory.PlayerHistoryLog()
      {
        Nickname = "Player",
        PlayerId = 0,
        UserId = string.Empty,
        ConnectionStatus = 0,
        LastAliveClass = -1,
        CurrentClass = -1,
        ConnectionStart = DateTime.Now,
        ConnectionStop = new DateTime(0, 0, 0)
      });
      index1 = this.historyLogs.Count - 1;
    }
    else
    {
      for (int index2 = 0; index2 < this.historyLogs.Count; ++index2)
      {
        if (this.historyLogs[index2].PlayerId == playerId)
          index1 = index2;
      }
    }
    if (index1 < 0)
      return;
    if (newNick != string.Empty)
      this.historyLogs[index1].Nickname = newNick;
    if (newPlayerId != 0)
      this.historyLogs[index1].PlayerId = newPlayerId;
    if (newUserId != string.Empty)
      this.historyLogs[index1].UserId = newUserId;
    if (newConnectionStatus != 0)
      this.historyLogs[index1].ConnectionStatus = newConnectionStatus;
    if (newAliveClass != 0)
      this.historyLogs[index1].LastAliveClass = newAliveClass;
    if (newCurrentClass != 0)
      this.historyLogs[index1].CurrentClass = newCurrentClass;
    if (newStartTime.Year != 0)
      this.historyLogs[index1].ConnectionStart = newStartTime;
    if (newStopTime.Year == 0)
      return;
    this.historyLogs[index1].ConnectionStop = newStopTime;
  }

  [Serializable]
  public class PlayerHistoryLog
  {
    public string Nickname;
    public int PlayerId;
    public string UserId;
    public int ConnectionStatus;
    public int LastAliveClass;
    public int CurrentClass;
    public DateTime ConnectionStart;
    public DateTime ConnectionStop;
  }
}
