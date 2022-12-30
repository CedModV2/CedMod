using System;
using Mirror;

namespace CedMod.Addons.Audio
{
    public class FakeConnection : NetworkConnectionToClient
    {
        public FakeConnection(int connectionId) : base(connectionId, false, 0f)
        {
            
        }

        public override string address
        {
            get
            {
                return "localhost";
            }
        }

        public override void Send(ArraySegment<byte> segment, int channelId = 0)
        {
        }

        public override void Disconnect()
        {
        }
    }
}