﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using CedMod.Addons.QuerySystem;
using CedMod.Addons.QuerySystem.WS;
using CedMod.ApiModals;
using CommandSystem;
using Exiled.Loader;
using MEC;
using Newtonsoft.Json;
using RoundRestarting;
using Serialization;
using UnityEngine;

namespace CedMod.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class ImportRaCommand : ICommand
    {
        public string Command { get; } = "cedmodimportra";

        public string[] Aliases { get; } = new string[]
        {
        };

        public string Description { get; } = "Import your RA configuration to CedMod, WARNING, will override current panel perms";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            ThreadDispatcher.ThreadDispatchQueue.Enqueue(() =>
            {
                List<SLPermissionEntry> Groups = new List<SLPermissionEntry>();
                foreach (var group in ServerStatic.GetPermissionsHandler()._groups)
                {
                    //var perms = Permissions.Groups.FirstOrDefault(s => s.Key == group.Key);
                    Groups.Add(new SLPermissionEntry()
                    {
                        Name = group.Key,
                        KickPower = group.Value.KickPower,
                        RequiredKickPower = group.Value.RequiredKickPower,
                        Hidden = group.Value.HiddenByDefault,
                        Cover = group.Value.Cover,
                        ReservedSlot = false,
                        BadgeText = group.Value.BadgeText,
                        BadgeColor = group.Value.BadgeColor,
                        RoleId = 0,
                        Permissions = (PlayerPermissions)group.Value.Permissions,
                        ExiledPermissions = new List<string>(),
                        //perms.Value == null ? new List<string>() : perms.Value.CombinedPermissions
                    });
                }

                WebSocketSystem.Enqueue(new QueryCommand()
                {
                    Recipient = "PANEL",
                    Data = new Dictionary<string, string>()
                    {
                        { "Message", "ImportRAResponse" },
                        { "Data", JsonConvert.SerializeObject(Groups) }
                    }
                });
            });
            response = "Done";
            return true;
        }
    }
}