// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.MirrorIgnoranceCommsNetwork
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Datastructures;
using Dissonance.Extensions;
using Dissonance.Networking;
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Dissonance.Integrations.MirrorIgnorance
{
  [HelpURL("https://placeholder-software.co.uk/dissonance/docs/Basics/Quick-Start-MirrorIgnorance/")]
  public class MirrorIgnoranceCommsNetwork : BaseCommsNetwork<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn, Unit, Unit>
  {
    private readonly ConcurrentPool<byte[]> _loopbackBuffers = new ConcurrentPool<byte[]>(8, (Func<byte[]>) (() => new byte[1024]));
    private readonly List<ArraySegment<byte>> _loopbackQueue = new List<ArraySegment<byte>>();
    internal const byte ReliableSequencedChannel = 0;
    internal const byte UnreliableChannel = 1;

    protected override MirrorIgnoranceServer CreateServer(Unit details)
    {
      return new MirrorIgnoranceServer(this);
    }

    protected override MirrorIgnoranceClient CreateClient(Unit details)
    {
      return new MirrorIgnoranceClient(this);
    }

    protected override void Update()
    {
      if (this.IsInitialized)
      {
        if ((!((UnityEngine.Object) NetworkManager.singleton != (UnityEngine.Object) null) || !NetworkManager.singleton.isNetworkActive || !NetworkServer.active && !NetworkClient.active ? 0 : (!NetworkClient.active ? 1 : (NetworkClient.connection == null ? 0 : (NetworkClient.connection.isReady ? 1 : 0)))) != 0)
        {
          bool active1 = NetworkServer.active;
          bool active2 = NetworkClient.active;
          if (this.Mode.IsServerEnabled() != active1 || this.Mode.IsClientEnabled() != active2 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null)
          {
            if (active1 && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
              this.RunAsDedicatedServer(Unit.None);
            else if (active1 & active2)
              this.RunAsHost(Unit.None, Unit.None);
            else if (active1)
              this.RunAsDedicatedServer(Unit.None);
            else if (active2)
              this.RunAsClient(Unit.None);
          }
        }
        else if (this.Mode != NetworkMode.None)
        {
          this.Stop();
          this._loopbackQueue.Clear();
        }
        for (int index = 0; index < this._loopbackQueue.Count; ++index)
        {
          if (this.Client != null)
            this.Client.NetworkReceivedPacket(this._loopbackQueue[index]);
          this._loopbackBuffers.Put(this._loopbackQueue[index].Array);
        }
        this._loopbackQueue.Clear();
      }
      base.Update();
    }

    protected override void Initialize()
    {
      NetworkServer.RegisterHandler<DissonanceNetworkMessage>(new Action<NetworkConnection, DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
      base.Initialize();
    }

    internal bool PreprocessPacketToClient(ArraySegment<byte> packet, MirrorConn destination)
    {
      if (this.Server == null)
        throw this.Log.CreatePossibleBugException("server packet preprocessing running, but this peer is not a server", "8f9dc0a0-1b48-4a7f-9bb6-f767b2542ab1");
      if (this.Client == null || NetworkClient.connection != destination.Connection)
        return false;
      if (this.Client != null)
        this._loopbackQueue.Add(packet.CopyTo<byte>(this._loopbackBuffers.Get(), 0));
      return true;
    }

    internal bool PreprocessPacketToServer(ArraySegment<byte> packet)
    {
      if (this.Client == null)
        throw this.Log.CreatePossibleBugException("client packet processing running, but this peer is not a client", "dd75dce4-e85c-4bb3-96ec-3a3636cc4fbe");
      if (this.Server == null)
        return false;
      this.Server.NetworkReceivedPacket(new MirrorConn(NetworkClient.connection), packet);
      return true;
    }

    internal static void NullMessageReceivedHandler(
      NetworkConnection source,
      DissonanceNetworkMessage msg)
    {
      if (Logs.GetLogLevel(LogCategory.Network) <= Dissonance.LogLevel.Trace)
        Debug.Log((object) "Discarding Dissonance network message");
      msg.Dispose();
    }
  }
}
