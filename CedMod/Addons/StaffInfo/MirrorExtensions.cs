using System;
using Mirror;
using PluginAPI.Core;

namespace CedMod.Addons.StaffInfo
{
    public static class MirrorExtensions
    {
        public static void SendFakeDisplayName(this Player player, Player target, string value)
        {
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            PrepareWriter(writer, player.ReferenceHub.nicknameSync.netIdentity, player.ReferenceHub.nicknameSync, (writer) =>
            {
                writer.WriteULong(16UL);
                writer.WriteString(value);
            });

            target.Connection.Send(new EntityStateMessage()
            {
                netId = player.ReferenceHub.nicknameSync.netId,
                payload = writer.ToArraySegment()
            });

            NetworkWriterPool.Return(writer);
        }
        
        public static void SendFakeViewInfo(this Player player, Player target, PlayerInfoArea infoArea)
        {
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            PrepareWriter(writer, player.ReferenceHub.nicknameSync.netIdentity, player.ReferenceHub.nicknameSync, (writer) =>
                {
                    writer.WriteULong(4UL);
                    writer.WriteInt((int)infoArea);
                });

            target.Connection.Send(new EntityStateMessage()
            {
                netId = player.ReferenceHub.nicknameSync.netId,
                payload = writer.ToArraySegment()
            });

            NetworkWriterPool.Return(writer);
        }

        public static void SendFakeCustomInfo(this Player player, Player target, string value)
        {
            NetworkWriterPooled writer = NetworkWriterPool.Get();
            PrepareWriter(writer, player.ReferenceHub.nicknameSync.netIdentity, player.ReferenceHub.nicknameSync, (writer) =>
                {
                    writer.WriteULong(2UL);
                    writer.WriteString(value);
                });

            target.Connection.Send(new EntityStateMessage()
            {
                netId = player.ReferenceHub.nicknameSync.netId,
                payload = writer.ToArraySegment()
            });

            NetworkWriterPool.Return(writer);
        }

        public static void PrepareWriter(NetworkWriter writer, NetworkIdentity identity, NetworkBehaviour behaviour,
           Action<NetworkWriter> writeSyncdata)
        {
            //prepare the writer with the dirtymask
            WriteBehaviourMask(writer, identity, behaviour);

            //initial write
            int headerPosition = writer.Position;
            writer.WriteByte(0);
            int contentPosition = writer.Position;

            //serialize deltas as per mirrors packet order
            behaviour.SerializeObjectsDelta(writer);

            //write our dirty bit and value
            writeSyncdata.Invoke(writer);

            int endPosition = writer.Position;

            //end the packet
            writer.Position = headerPosition;
            int size = endPosition - contentPosition;
            byte safety = (byte)(size & 0xFF);
            writer.WriteByte(safety);
            writer.Position = endPosition;
        }

        public static void WriteBehaviourMask(NetworkWriter writer, NetworkIdentity identity, NetworkBehaviour behaviour)
        {
            var netId = behaviour.netIdentity;
            var behaviourIndex = netId.NetworkBehaviours.IndexOf(behaviour);

            ulong mask = 0;
            mask |= 1UL << behaviourIndex;
            Compression.CompressVarUInt(writer, mask);
        }
    }
}