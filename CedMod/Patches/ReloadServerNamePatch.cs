using System;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using HarmonyLib;
using Mirror;
using PluginAPI.Core;
using UnityEngine;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public class ReloadServerNamePatch
    {
        public static void Postfix()
        {
            if (Verification.ServerId != 0)
            {
                ServerConsole._serverName += $"<color=#00000000><size=1>CedModVerification{Verification.ServerId}</size></color>";
            }
        }
    }
}