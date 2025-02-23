using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using CedMod.Addons.StaffInfo;
using CommandSystem;
using LabApi.Features.Wrappers;
using Newtonsoft.Json;
using RemoteAdmin;
using Utils;
using Utils.NonAllocLINQ;

namespace CedMod.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WarnCommand : ICommand, IUsageProvider
    {
        public string Command { get; } = "warn";

        public string[] Aliases { get; } = new string[]
        {
            "addwarning",
            "addwarn",
            "issuewarn",
            "issuewarning"
        };

        public string Description { get; } = "Warns a player, Warn reason can be multiple words.";
        
        public string[] Usage { get; } = new string[]
        {
            "%player%",
            "%reason%"
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count <= 1)
            {
                response = "Missing arguments, warn <player> <reason>\nReason can be multiple words";
                return false;
            }

            var plrs = RAUtils.ProcessPlayerIdOrNamesList(arguments, 0, out var newgArs);
            
            string reason = arguments.Skip(1).Aggregate((current, n) => current + " " + n);
            string senderId = "";

            if (sender is PlayerCommandSender commandSender)
                senderId = commandSender.SenderId;

            if (sender is CmSender queryCommandSender)
                senderId = queryCommandSender.SenderId;

            Task.Run(async () =>
            {
                foreach (var plr in plrs)
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("X-ServerIp", ServerConsole.Ip);
                            await VerificationChallenge.AwaitVerification();
                            var response = await client.PostAsync($"http{(QuerySystem.UseSSL ? "s" : "")}://" + QuerySystem.CurrentMaster + $"/Api/v3/Punishment/IssueWarn/{QuerySystem.QuerySystemKey}?userId={plr.authManager.UserId}&issuer={senderId}", new StringContent(JsonConvert.SerializeObject(new Dictionary<string, string> { { "Reason", reason } })));
                            var responseString = await response.Content.ReadAsStringAsync();
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
                                {
                                    sender.Respond(responseString, true);
                                    if (CedModMain.Singleton.Config.QuerySystem.StaffInfoSystem)
                                    {
                                        StaffInfoHandler.StaffData.ForEach(s => s.Value.Remove(plr.authManager.UserId));
                                        StaffInfoHandler.Requested.ForEach(s => s.Value.Remove(plr.authManager.UserId));
                                        StaffInfoHandler.Singleton.RequestInfo(Player.Get(sender), Player.Get(plr));
                                    }
                                });
                            }
                            else
                            {
                                ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { sender.Respond($"{response.StatusCode} - {responseString}"); });
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        ThreadDispatcher.ThreadDispatchQueue.Enqueue(() => { sender.Respond(e.ToString()); });
                    }
                }
            });

            response = "Attempting to issue warning, please wait...";
            return true;
        }
    }
}