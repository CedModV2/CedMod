// Decompiled with JetBrains decompiler
// Type: Broadcast
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class Broadcast : NetworkBehaviour
{
  private void Start()
  {
  }

  [TargetRpc]
  public void TargetAddElement(NetworkConnection conn, string data, uint time, bool monospaced)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(data);
    writer.WritePackedUInt32(time);
    writer.WriteBoolean(monospaced);
    this.SendTargetRPCInternal(conn, typeof (Broadcast), nameof (TargetAddElement), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcAddElement(string data, uint time, bool monospaced)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    writer.WriteString(data);
    writer.WritePackedUInt32(time);
    writer.WriteBoolean(monospaced);
    this.SendRPCInternal(typeof (Broadcast), nameof (RpcAddElement), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [TargetRpc]
  public void TargetClearElements(NetworkConnection conn)
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendTargetRPCInternal(conn, typeof (Broadcast), nameof (TargetClearElements), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  [ClientRpc]
  public void RpcClearElements()
  {
    NetworkWriter writer = NetworkWriterPool.GetWriter();
    this.SendRPCInternal(typeof (Broadcast), nameof (RpcClearElements), writer, 0);
    NetworkWriterPool.Recycle(writer);
  }

  private void MirrorProcessed()
  {
  }

  protected static void InvokeRpcRpcAddElement(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcAddElement called on server.");
    else
      ((Broadcast) obj).CallRpcAddElement(reader.ReadString(), reader.ReadPackedUInt32(), reader.ReadBoolean());
  }

  protected static void InvokeRpcRpcClearElements(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "RPC RpcClearElements called on server.");
    else
      ((Broadcast) obj).CallRpcClearElements();
  }

  protected static void InvokeRpcTargetAddElement(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetAddElement called on server.");
    else
      ((Broadcast) obj).CallTargetAddElement(ClientScene.readyConnection, reader.ReadString(), reader.ReadPackedUInt32(), reader.ReadBoolean());
  }

  protected static void InvokeRpcTargetClearElements(NetworkBehaviour obj, NetworkReader reader)
  {
    if (!NetworkClient.active)
      Debug.LogError((object) "TargetRPC TargetClearElements called on server.");
    else
      ((Broadcast) obj).CallTargetClearElements(ClientScene.readyConnection);
  }

  public void CallRpcAddElement(string data, uint time, bool monospaced)
  {
  }

  public void CallRpcClearElements()
  {
  }

  public void CallTargetAddElement(
    NetworkConnection conn,
    string data,
    uint time,
    bool monospaced)
  {
  }

  public void CallTargetClearElements(NetworkConnection conn)
  {
  }

  static Broadcast()
  {
    NetworkBehaviour.RegisterRpcDelegate(typeof (Broadcast), "RpcAddElement", new NetworkBehaviour.CmdDelegate(Broadcast.InvokeRpcRpcAddElement));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Broadcast), "RpcClearElements", new NetworkBehaviour.CmdDelegate(Broadcast.InvokeRpcRpcClearElements));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Broadcast), "TargetAddElement", new NetworkBehaviour.CmdDelegate(Broadcast.InvokeRpcTargetAddElement));
    NetworkBehaviour.RegisterRpcDelegate(typeof (Broadcast), "TargetClearElements", new NetworkBehaviour.CmdDelegate(Broadcast.InvokeRpcTargetClearElements));
  }
}
