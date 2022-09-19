﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using UnityEngine;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(PermissionsHandler), nameof(PermissionsHandler.RefreshPermissions))]
    public static class RefreshPermissionsHandlerPatch
    {
        public static bool Prefix()
        {
            if (!WebSocketSystem.UseRa)
            {
                Task.Factory.StartNew(() => { WebSocketSystem.ApplyRa(); });
                return false;
            }

            return true;
        }
    }
}