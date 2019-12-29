// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.MirrorIgnoranceServer
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Networking;
using Dissonance.Networking.Server;
using Mirror;
using System;
using System.Collections.Generic;

namespace Dissonance.Integrations.MirrorIgnorance
{
  public class MirrorIgnoranceServer : BaseServer<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>
  {
    private readonly List<NetworkConnection> _addedConnections = new List<NetworkConnection>();
    [NotNull]
    private readonly MirrorIgnoranceCommsNetwork _network;
    private static MirrorIgnoranceServer _instance;

    public MirrorIgnoranceServer([NotNull] MirrorIgnoranceCommsNetwork network)
    {
      if ((UnityEngine.Object) network == (UnityEngine.Object) null)
        throw new ArgumentNullException(nameof (network));
      this._network = network;
      MirrorIgnoranceServer._instance = this;
    }

    public override void Connect()
    {
      NetworkServer.RegisterHandler<DissonanceNetworkMessage>(new Action<NetworkConnection, DissonanceNetworkMessage>(this.OnMessageReceived), true);
      base.Connect();
    }

    private void OnMessageReceived(NetworkConnection source, DissonanceNetworkMessage msg)
    {
      using (msg)
        this.NetworkReceivedPacket(new MirrorConn(source), msg.Data);
    }

    protected override void AddClient([NotNull] ClientInfo<MirrorConn> client)
    {
      base.AddClient(client);
      if (!(client.PlayerName != this._network.PlayerName))
        return;
      this._addedConnections.Add(client.Connection.Connection);
    }

    public override void Disconnect()
    {
      base.Disconnect();
      NetworkServer.RegisterHandler<DissonanceNetworkMessage>(new Action<NetworkConnection, DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
      if (MirrorIgnoranceServer._instance != this)
        return;
      MirrorIgnoranceServer._instance = (MirrorIgnoranceServer) null;
    }

    protected override void ReadMessages()
    {
    }

    public static void ForceDisconnectClient(NetworkConnection connection)
    {
      if (MirrorIgnoranceServer._instance == null)
        return;
      MirrorIgnoranceServer._instance.ForceDisconnectClient(new MirrorConn(connection));
    }

    private void ForceDisconnectClient(MirrorConn conn)
    {
      int index = this._addedConnections.IndexOf(conn.Connection);
      if (index < 0)
        return;
      this._addedConnections.RemoveAt(index);
      this.ClientDisconnected(conn);
    }

    public override ServerState Update()
    {
      for (int index = this._addedConnections.Count - 1; index >= 0; --index)
      {
        if (!MirrorIgnoranceServer.IsConnected(this._addedConnections[index]))
        {
          this.ClientDisconnected(new MirrorConn(this._addedConnections[index]));
          this._addedConnections.RemoveAt(index);
        }
      }
      return base.Update();
    }

    private static bool IsConnected([NotNull] NetworkConnection conn)
    {
      return conn.isReady && NetworkServer.connections.ContainsKey(conn.connectionId);
    }

    protected override void SendReliable(MirrorConn connection, ArraySegment<byte> packet)
    {
      if (this.Send(packet, connection, (byte) 0))
        return;
      this.FatalError("Failed to send reliable packet (unknown Mirror error)");
    }

    protected override void SendUnreliable(MirrorConn connection, ArraySegment<byte> packet)
    {
      this.Send(packet, connection, (byte) 1);
    }

    private bool Send(ArraySegment<byte> packet, MirrorConn connection, byte channel)
    {
      if (this._network.PreprocessPacketToClient(packet, connection) || !MirrorIgnoranceServer.IsConnected(connection.Connection))
        return true;
      if (connection.Connection != null)
        return connection.Connection.Send<DissonanceNetworkMessage>(new DissonanceNetworkMessage(packet), (int) channel);
      this.Log.Error("Cannot send to a null destination");
      return false;
    }
  }
}
