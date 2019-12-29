// Decompiled with JetBrains decompiler
// Type: ServerTime
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;

public class ServerTime : NetworkBehaviour
{
  [SyncVar]
  public int timeFromStartup;
  private CharacterClassManager _ccm;
  public static int time;
  private const int allowedDeviation = 2;
  private bool _rateLimit;

  public static bool CheckSynchronization(int myTime)
  {
    int num = Mathf.Abs(myTime - ServerTime.time);
    if (num > 2)
      Console.AddLog("Damage sync error.", (Color) new Color32(byte.MaxValue, (byte) 200, (byte) 0, byte.MaxValue), false);
    return num <= 2;
  }

  private void Update()
  {
    this._rateLimit = false;
    if (!this._ccm.IsHost)
      return;
    ServerTime.time = this.timeFromStartup;
  }

  private void Start()
  {
    this._ccm = this.GetComponent<CharacterClassManager>();
    if (!this.isLocalPlayer || !this.isServer)
      return;
    this.InvokeRepeating("IncreaseTime", 1f, 1f);
  }

  private void IncreaseTime()
  {
    this.TransmitData(this.timeFromStartup + 1);
  }

  [ClientCallback]
  private void TransmitData(int timeFromStartup)
  {
    if (!NetworkClient.active)
      return;
    this.CmdSetTime(timeFromStartup);
  }

  [Command]
  private void CmdSetTime(int t)
  {
    if (this.isServer)
    {
      this.CallCmdSetTime(t);
    }
    else
    {
      NetworkWriter writer = NetworkWriterPool.GetWriter();
      writer.WritePackedInt32(t);
      this.SendCommandInternal(typeof (ServerTime), nameof (CmdSetTime), writer, 0);
      NetworkWriterPool.Recycle(writer);
    }
  }

  private void MirrorProcessed()
  {
  }

  public int NetworktimeFromStartup
  {
    get
    {
      return this.timeFromStartup;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.timeFromStartup, 1UL);
    }
  }

  protected static void InvokeCmdCmdSetTime(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkServer.active)
      Debug.LogError((object) "Command CmdSetTime called on client.");
    else
      ((ServerTime) obj).CallCmdSetTime(reader.ReadPackedInt32());
  }

  public void CallCmdSetTime(int t)
  {
    if (this._rateLimit)
      return;
    this._rateLimit = true;
    this.NetworktimeFromStartup = t;
  }

  static ServerTime()
  {
    NetworkBehaviour.RegisterCommandDelegate(typeof (ServerTime), "CmdSetTime", new NetworkBehaviour.CmdDelegate(ServerTime.InvokeCmdCmdSetTime));
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32(this.timeFromStartup);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.timeFromStartup);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.NetworktimeFromStartup = reader.ReadPackedInt32();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.NetworktimeFromStartup = reader.ReadPackedInt32();
    }
  }
}
