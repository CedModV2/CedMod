using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.MutingAndIntercom;
using GameCore;
using HarmonyLib;
using NorthwoodLib.Pools;
using PluginAPI.Core;
using RemoteAdmin;
using UnityEngine;
using Utils;
using Log = PluginAPI.Core.Log;

namespace CedMod.Patches
{
    [HarmonyPatch(typeof(RAUtils), nameof(RAUtils.ProcessPlayerIdOrNamesList))]
    public static class RAProcessPlayerPatch
    {
        public static bool Prefix(ref List<ReferenceHub> __result, ArraySegment<string> args, int startindex, out string[] newargs, bool keepEmptyEntries = false)
        {
            List<ReferenceHub> result1;
            try
            {
                string str1 = RAUtils.FormatArguments(args, startindex);
                List<ReferenceHub> referenceHubList = ListPool<ReferenceHub>.Shared.Rent();
                if (str1.StartsWith("@", StringComparison.Ordinal))
                {
                    foreach (Match match in new Regex("@\"(.*?)\".|@[^\\s.]+\\.").Matches(str1))
                    {
                        str1 = RAUtils.ReplaceFirst(str1, match.Value, "");
                        string name = match.Value.Substring(1).Replace("\"", "").Replace(".", "");
                        List<ReferenceHub> list = Enumerable.Where<ReferenceHub>((IEnumerable<ReferenceHub>)ReferenceHub.AllHubs, (Func<ReferenceHub, bool>)(ply => ply.nicknameSync.MyNick.Equals(name))).ToList<ReferenceHub>();
                        if (list.Count == 1 && !referenceHubList.Contains(list[0]))
                            referenceHubList.Add(list[0]);
                    }

                    newargs = str1.Split(new char[1] { ' ' }, (StringSplitOptions)(keepEmptyEntries ? 0 : 1));
                }
                else if (args.At<string>(startindex).Length > 0)
                {
                    if (char.IsDigit(args.At<string>(startindex)[0]))
                    {
                        foreach (string s in args.At<string>(startindex).Split('.', StringSplitOptions.None))
                        {
                            if (s == "-1" || s == "-2")
                                continue;
                            int result;
                            ReferenceHub hub;
                            if (int.TryParse(s, out result) && ReferenceHub.TryGetHub(result, out hub) && !referenceHubList.Contains(hub))
                                referenceHubList.Add(hub);
                        }
                    }
                    else if (char.IsLetter(args.At<string>(startindex)[0]))
                    {
                        foreach (string str2 in args.At<string>(startindex).Split('.', StringSplitOptions.None))
                        {
                            if (str2 == "-1" || str2 == "-2")
                                continue;
                            
                            foreach (ReferenceHub allHub in ReferenceHub.AllHubs)
                            {
                                if (allHub.nicknameSync.MyNick.Equals(str2) && !referenceHubList.Contains(allHub))
                                    referenceHubList.Add(allHub);
                            }
                        }
                    }
                }
                
                string[] strArray;
                if (args.Count <= 1)
                    strArray = (string[])null;
                else
                    strArray = RAUtils.FormatArguments(args, startindex + 1).Split(new char[1]
                    {
                        ' '
                    }, (StringSplitOptions)(keepEmptyEntries ? 0 : 1));
                newargs = strArray;
                result1 = referenceHubList;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                newargs = null;
                result1 = null;
            }

            __result = result1;
            return false;
        }
    }
}