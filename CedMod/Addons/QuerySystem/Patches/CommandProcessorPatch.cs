using System;
using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem.WS;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using HarmonyLib;
using PluginAPI.Core;
using RemoteAdmin;
using RemoteAdmin.Communication;
using RemoteAdmin.Interfaces;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class CommandProcessorPatch
    {
        public static void Postfix(string q, CommandSender sender)
        {
            try
            {
                if (q.StartsWith("$", StringComparison.Ordinal))
                {
                   return;
                }
                
                WebSocketSystem.SendQueue.Enqueue(new QueryCommand()
                {
                    Recipient = "ALL",
                    Data = new Dictionary<string, string>()
                    {
                        {"Type", "OnAdminCommand"},
                        {"UserId", sender.SenderId},
                        {"UserName", sender.Nickname},
                        {"Command", q},
                        {
                            "Message", string.Concat(new string[]
                            {
                                $"{sender.Nickname} ({sender.SenderId})",
                                " used command: ",
                                q
                            })
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}