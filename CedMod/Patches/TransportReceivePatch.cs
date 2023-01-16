using System;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(NetworkConnection), nameof(NetworkConnection.TransportReceive))]
    public class TransportReceivePatch
    {
        public static bool Prefix(NetworkConnection __instance, ArraySegment<byte> buffer)
        {
            if (buffer.Count >= 2)
                return true;
            Debug.LogError((object) string.Format("ConnectionRecv {0} Message was too short (messages should start with message id)", (object) __instance));
            return false;
        }
    }
}