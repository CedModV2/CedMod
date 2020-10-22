using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CedMod.Handlers;
using CommandSystem;
using GameCore;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using Console = System.Console;
using Player = Exiled.API.Features.Player;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class PriorBansCommand : ICommand
    {
        public string Command { get; } = "priorbans";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the prior bans on record";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            CommandSender sndr = (sender as CommandSender);
            if (Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                if (arguments.IsEmpty())
                {
                    response = "Usage: totalbans <playerid>";
                    return false;
                }

                Player ply = Player.Get(arguments.At(0));
                try
                {
                    ApiBanResponse resp =
                        JsonConvert.DeserializeObject<ApiBanResponse>(
                            (string) API.APIRequest($"api/BanLog/UserId/{ply.UserId}", "", true));
                    foreach (BanModel ban in resp.Message)
                    {
                        sender.Respond($"\nIssuer :{ban.Adminname}" +
                                       $"\nReason {ban.Banreason}" +
                                       $"\nDuration {ban.Banduration}" +
                                       $"\nTimestamp {ban.Timestamp}", true);
                    }
                }
                catch (WebException ex)
                {
                    sender.Respond("Api request failed \n " + ex.Status + "\n" +
                                   new StreamReader(((HttpWebResponse) ex.Response).GetResponseStream()).ReadToEnd(), false);
                }

                response = "Done!";
                return true;
            }

            if (!Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                try
                {
                    ApiBanResponse resp =
                        JsonConvert.DeserializeObject<ApiBanResponse>(
                            (string) API.APIRequest($"api/BanLog/UserId/{sndr.SenderId}", "", true));
                    foreach (BanModel ban in resp.Message)
                    {
                        Player.Get(sndr.SenderId).SendConsoleMessage($"\nIssuer :{ban.Adminname}" +
                                                                     $"\nReason {ban.Banreason}" +
                                                                     $"\nDuration {ban.Banduration}" +
                                                                     $"\nTimestamp {ban.Timestamp}", "green");
                    }
                }
                catch (WebException ex)
                {
                    Player.Get(sndr.SenderId).SendConsoleMessage("Api request failed \n " + ex.Status + "\n" +
                                                                  new StreamReader(((HttpWebResponse) ex.Response).GetResponseStream()).ReadToEnd(), "red");
                }

                response = "Done!";
                return true;
            }

            response = "Unable to select commandtype";
            return false;
        }
    }
}