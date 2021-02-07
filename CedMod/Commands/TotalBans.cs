using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using CedMod.Handlers;
using CommandSystem;
using Exiled.Permissions.Extensions;
using Newtonsoft.Json;
using RemoteAdmin;
using UnityEngine;
using Player = Exiled.API.Features.Player;

namespace CedMod.Commands
{
    
    public class ApiBanResponse
    {
        public bool Success { get; set; }
        public List<BanModel> Message { get; set; }
    }

    public class BanModel
    {
        public int id { get; set; }
        public int AppealState { get; set; }
        public bool Recentban { get; set; }
        public string Banreason { get; set; }
        public string Adminname { get; set; }
        public Int64 Banduration { get; set; }
        public string Nickname { get; set; }
        public string Timestamp { get; set; }
        public string Alias { get; set; }
        public string Userid { get; set; }
        public string Banuserid { get; set; }
        public string Ip { get; set; }
        public Int64 Unixstamp { get; set; }
    }
    
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class TotalBansCommand : ICommand
    {
        public string Command { get; } = "totalbans";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Gives the ammount of bans on record";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender,
            out string response)
        {
            if (!sender.CheckPermission("cedmod.totalbans"))
            {
                response = "no permission";
                return false;
            }
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
                    string response1 = (string) API.APIRequest($"api/BanLog/UserId/{ply.UserId}", "", true);
                    if (response1.Contains("\"message\":\"Specified BanLog does not exist\""))
                    {
                        response = "No banlogs found!";
                        return true;
                    }
                    ApiBanResponse resp =
                        JsonConvert.DeserializeObject<ApiBanResponse>(response1);
                    response = resp.Message.Count().ToString();
                    return true;
                }
                catch (WebException ex)
                {
                    sender.Respond("Api request failed \n " + ex.Status + "\n" +
                                                                 new StreamReader(((HttpWebResponse) ex.Response).GetResponseStream()).ReadToEnd(), false);
                }
            }

            if (!Player.Get(sndr.SenderId).ReferenceHub.serverRoles.RemoteAdmin)
            {
                try
                {
                    ApiBanResponse resp =
                        JsonConvert.DeserializeObject<ApiBanResponse>(
                            (string) API.APIRequest($"api/BanLog/UserId/{sndr.SenderId}", "", true));
                    response = resp.Message.Count().ToString();
                    return true;
                }
                catch (WebException ex)
                {
                    Player.Get(sndr.SenderId).SendConsoleMessage("Api request failed \n " + ex.Status + "\n" +
                                                                 new StreamReader(((HttpWebResponse) ex.Response).GetResponseStream()).ReadToEnd(), "red");
                }
            }
            response = "Unable to select commandtype";
            return false;
        }
    }
}