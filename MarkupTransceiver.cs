// Decompiled with JetBrains decompiler
// Type: MarkupTransceiver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using RemoteAdmin;
using System.Collections.Generic;
using UnityEngine;

public class MarkupTransceiver : NetworkBehaviour
{
  private readonly List<NetworkConnection> _conns = new List<NetworkConnection>();

  [ServerCallback]
  public void Transmit(string code, int[] playerIDs)
  {
    if (!NetworkServer.active)
      return;
    foreach (NetworkConnection target in this.GetTargets(playerIDs))
      this.TargetRpcReceiveData(target, code);
  }

  [ServerCallback]
  public void RequestStyleDownload(string url, int[] playerIDs)
  {
    if (!NetworkServer.active)
      return;
    foreach (NetworkConnection target in this.GetTargets(playerIDs))
      this.TargetRpcDownloadStyle(target, url);
  }

  [TargetRpc]
  private void TargetRpcDownloadStyle(NetworkConnection conn, string url)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(url);
    this.SendTargetRPCInternal(conn, typeof (MarkupTransceiver), nameof (TargetRpcDownloadStyle), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private IEnumerable<NetworkConnection> GetTargets(int[] playerIDs)
  {
    this._conns.Clear();
    foreach (GameObject player in PlayerManager.players)
    {
      QueryProcessor component = player.GetComponent<QueryProcessor>();
      foreach (int playerId in playerIDs)
      {
        if (component.PlayerId == playerId)
          this._conns.Add(component.connectionToClient);
      }
    }
    return (IEnumerable<NetworkConnection>) this._conns;
  }

  [TargetRpc]
  private void TargetRpcReceiveData(NetworkConnection target, string code)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(code);
    this.SendTargetRPCInternal(target, typeof (MarkupTransceiver), nameof (TargetRpcReceiveData), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcTargetRpcDownloadStyle(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetRpcDownloadStyle called on server.");
    else
      ((MarkupTransceiver) obj).CallTargetRpcDownloadStyle(ClientScene.readyConnection, reader.ReadString());
  }

  protected static void InvokeRpcTargetRpcReceiveData(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetRpcReceiveData called on server.");
    else
      ((MarkupTransceiver) obj).CallTargetRpcReceiveData(ClientScene.readyConnection, reader.ReadString());
  }

  public void CallTargetRpcDownloadStyle(NetworkConnection conn, string url)
  {
  }

  public void CallTargetRpcReceiveData(NetworkConnection target, string code)
  {
  }

  static MarkupTransceiver()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (MarkupTransceiver), "TargetRpcDownloadStyle", new NetworkBehaviour.CmdDelegate(MarkupTransceiver.InvokeRpcTargetRpcDownloadStyle));
    NetworkBehaviour.RegisterRpcDelegate(typeof (MarkupTransceiver), "TargetRpcReceiveData", new NetworkBehaviour.CmdDelegate(MarkupTransceiver.InvokeRpcTargetRpcReceiveData));
  }
}
