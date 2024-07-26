using System;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(NetworkClient), nameof(NetworkClient.OnTransportData))]
    internal class TransportReceivePatch
    {
        public static bool Prefix(ArraySegment<byte> data, int channelId)
        {
            if (NetworkClient.connection != null)
            {
                if (!NetworkClient.unbatcher.AddBatch(data))
                {
                    Debug.LogWarning("NetworkClient: failed to add batch, disconnecting.");
                    Log.Warning("Skipping Mirror NetworkClient Disconnect call.");
                    //NetworkClient.connection.Disconnect();
                    return false;
                }
                
                ArraySegment<byte> arraySegment;
                double num;
                while (!NetworkClient.isLoadingScene && NetworkClient.unbatcher.GetNextMessage(out arraySegment, out num))
                {
                    using (NetworkReaderPooled networkReaderPooled = NetworkReaderPool.Get(arraySegment))
                    {
                        if (networkReaderPooled.Remaining < 2)
                        {
                            Debug.LogWarning("NetworkClient: received Message was too short (messages should start with message id)");
                            Log.Warning("Skipping Mirror NetworkClient Disconnect call.");
                            //NetworkClient.connection.Disconnect();
                            return false;
                        }
                        NetworkClient.connection.remoteTimeStamp = num;
                        if (!NetworkClient.UnpackAndInvoke(networkReaderPooled, channelId))
                        {
                            Debug.LogWarning("NetworkClient: failed to unpack and invoke message. Disconnecting.");
                            Log.Warning("Skipping Mirror NetworkClient Disconnect call.");
                            //NetworkClient.connection.Disconnect();
                            return false;
                        }
                        continue;
                    }
                }
                
                if (!NetworkClient.isLoadingScene && NetworkClient.unbatcher.BatchesCount > 0)
                {
                    Debug.LogError(string.Format("Still had {0} batches remaining after processing, even though processing was not interrupted by a scene change. This should never happen, as it would cause ever growing batches.\nPossible reasons:\n* A message didn't deserialize as much as it serialized\n*There was no message handler for a message id, so the reader wasn't read until the end.", NetworkClient.unbatcher.BatchesCount));
                    return false;
                }
            }
            else
            {
                Debug.LogError("Skipped Data message handling because connection is null.");
            }
            return false;
        }
    }
}