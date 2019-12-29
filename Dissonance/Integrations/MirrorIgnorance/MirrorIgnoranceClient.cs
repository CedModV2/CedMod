// Decompiled with JetBrains decompiler
// Type: Dissonance.Integrations.MirrorIgnorance.MirrorIgnoranceClient
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Dissonance.Networking;
using Mirror;
using System;

namespace Dissonance.Integrations.MirrorIgnorance
{
  public class MirrorIgnoranceClient : BaseClient<MirrorIgnoranceServer, MirrorIgnoranceClient, MirrorConn>
  {
    private readonly MirrorIgnoranceCommsNetwork _network;

    public MirrorIgnoranceClient([NotNull] MirrorIgnoranceCommsNetwork network)
      : base((ICommsNetworkState) network)
    {
      if ((UnityEngine.Object) network == (UnityEngine.Object) null)
        throw new ArgumentNullException(nameof (network));
      this._network = network;
    }

    public override void Connect()
    {
      if (!this._network.Mode.IsServerEnabled())
        NetworkClient.RegisterHandler<DissonanceNetworkMessage>(new Action<NetworkConnection, DissonanceNetworkMessage>(this.OnMessageReceived), true);
      this.Connected();
    }

    public override void Disconnect()
    {
      if (!this._network.Mode.IsServerEnabled())
        NetworkClient.RegisterHandler<DissonanceNetworkMessage>(new Action<NetworkConnection, DissonanceNetworkMessage>(MirrorIgnoranceCommsNetwork.NullMessageReceivedHandler), true);
      base.Disconnect();
    }

    private void OnMessageReceived(NetworkConnection source, DissonanceNetworkMessage msg)
    {
      using (msg)
        this.NetworkReceivedPacket(msg.Data);
    }

    protected override void ReadMessages()
    {
    }

    protected override void SendReliable(ArraySegment<byte> packet)
    {
      if (this.Send(packet, (byte) 0))
        return;
      this.FatalError("Failed to send reliable packet (unknown Mirror error)");
    }

    protected override void SendUnreliable(ArraySegment<byte> packet)
    {
      this.Send(packet, (byte) 1);
    }

    private bool Send(ArraySegment<byte> packet, byte channel)
    {
      return this._network.PreprocessPacketToServer(packet) || NetworkClient.connection.Send<DissonanceNetworkMessage>(new DissonanceNetworkMessage(packet), (int) channel);
    }
  }
}
