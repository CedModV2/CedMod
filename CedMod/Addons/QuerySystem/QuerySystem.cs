using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using MEC;
using Object = UnityEngine.Object;

namespace CedMod.Addons.QuerySystem
{
    public class QuerySystem
    {
        public static QueryMapEvents QueryMapEvents;
        public static QueryServerEvents QueryServerEvents;
        public static QueryPlayerEvents QueryPlayerEvents;
        public static List<string> ReservedSlotUserids = new List<string>();
        public static string PanelUrl = "frikanweb.cedmod.nl";
    }
}