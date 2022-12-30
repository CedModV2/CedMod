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
            List<ReferenceHub> result;
            try
            {
                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                    Log.Debug("Starting RAUtil");
                string text = RAUtils.FormatArguments(args, startindex);
                List<ReferenceHub> list = ListPool<ReferenceHub>.Shared.Rent();
                if (text.StartsWith("@", StringComparison.Ordinal))
                {
                    foreach (object obj in new Regex("@\"(.*?)\".|@[^\\s.]+\\.").Matches(text))
                    {
                        Match match = (Match)obj;
                        text = RAUtils.ReplaceFirst(text, match.Value, "");
                        string name = match.Value.Substring(1).Replace("\"", "").Replace(".", "");
                        List<ReferenceHub> list2 = (from ply in ReferenceHub.AllHubs
                            where ply.nicknameSync.MyNick.Equals(name)
                            select ply).ToList<ReferenceHub>();
                        if (list2.Count == 1 && !list.Contains(list2[0]))
                        {
                            list.Add(list2[0]);
                        }
                    }
                    newargs = text.Split(new char[]
                    {
                        ' '
                    }, keepEmptyEntries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
                    result = list;
                }
                else
                {
                    var tmpargs = new List<string>(args);
                    foreach (var tmp in args)
                    {
                        if (!tmp.Contains("-"))
                            continue;
                        var index = tmpargs.IndexOf(tmp);
                        tmpargs[index] = tmp.Replace("-", "");
                    }
                    if (tmpargs[startindex].Length > 0 && char.IsDigit(tmpargs[startindex][0]))
                    {
                        string[] array = args.At(startindex).Split(new char[]
                        {
                            '.'
                        });
                        for (int i = 0; i < array.Length; i++)
                        {
                            if (array[i] == "-1" || array[i] == "-2")
                                continue;
                            if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                Log.Debug($"RAUtil {array[i]}");
                            int playerId;
                            ReferenceHub item;
                            if (int.TryParse(array[i], out playerId) && ReferenceHub.TryGetHub(playerId, out item) && !list.Contains(item))
                            {
                                if (CedModMain.Singleton.Config.QuerySystem.Debug)
                                    Log.Debug($"RAUtil {array[i]} {item.nicknameSync.name}");
                                list.Add(item);
                            }
                        }
                    }
                    else
                    {
                        if (CedModMain.Singleton.Config.QuerySystem.Debug)
                            Log.Debug($"RAUtil skip");
                    }
                    newargs = ((args.Count > 1) ? RAUtils.FormatArguments(args, startindex + 1).Split(new char[]
                    {
                        ' '
                    }, keepEmptyEntries ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries) : null);
                    result = list;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                newargs = null;
                result = null;
            }

            __result = result;
            return false;
        }
    }
}