using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using HarmonyLib;
using RemoteAdmin;
using RemoteAdmin.Communication;
using RemoteAdmin.Interfaces;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class CommandProcessorPatch
    {
        public static bool Prefix(string q, CommandSender sender)
        {
            try
            {
                if (q.StartsWith("$", StringComparison.Ordinal))
                {
                    string[] source = q.Remove(0, 1).Split(' ');
                    int result;
                    IServerCommunication serverCommunication;
                    if (source.Length < 1 || !int.TryParse(source[0], out result) || !CommunicationProcessor.ServerCommunication.TryGetValue(result, out serverCommunication))
                        return false;
                    serverCommunication.ReceiveData(sender, string.Join(" ", ((IEnumerable<string>)source).Skip<string>(1)));
                }
                else
                {
                    PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
                    if (q.StartsWith("@", StringComparison.Ordinal))
                    {
                        if (!CommandProcessor.CheckPermissions(sender, "Admin Chat", PlayerPermissions.AdminChat, string.Empty))
                        {
                            playerCommandSender?.Processor.TargetAdminChatAccessDenied(playerCommandSender.Processor.connectionToClient);
                        }
                        else
                        {
                            q = q + " ~" + sender.Nickname;
                            foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
                            {
                                if ((referenceHub.serverRoles.AdminChatPerms || referenceHub.serverRoles.RaEverywhere) && !referenceHub.isDedicatedServer && referenceHub.Ready) 
                                    referenceHub.queryProcessor.TargetReply(referenceHub.queryProcessor.connectionToClient, q, true, false, string.Empty);
                            }
                        }
                    }
                    else
                    {
                        if (q.StartsWith("EXTERNALLOOKUP") && !q.StartsWith("EXTERNALLOOKUP "))
                            q = q.Replace("EXTERNALLOOKUP", "EXTERNALLOOKUP ").Replace(".", "");
                        string[] array = q.Trim().Split(QueryProcessor.SpaceArray, 512, StringSplitOptions.RemoveEmptyEntries);
                        ICommand command;
                        if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(array[0], out command))
                        {
                            try
                            {
                                string response;
                                bool success = command.Execute(array.Segment<string>(1), (ICommandSender)sender, out response);
                                if (string.IsNullOrEmpty(response))
                                    return false;
                                sender.RaReply(array[0].ToUpperInvariant() + "#" + response, success, true, "");
                            }
                            catch (Exception ex)
                            {
                                sender.RaReply(array[0].ToUpperInvariant() + "# Command execution failed! Error: " + Misc.RemoveStacktraceZeroes(ex.ToString()), false, true, "");
                            }
                        }
                        else
                            sender.RaReply("SYSTEM#Unknown command!", false, true, string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return true;
            }
            return false;
        }
    }
}