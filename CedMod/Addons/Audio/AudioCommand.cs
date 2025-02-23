using System;
using System.Collections.Generic;
using System.Linq;
using CentralAuth;
using CommandSystem;
using LabApi.Features.Permissions;
using MEC;
using Mirror;
using PlayerRoles;
using RemoteAdmin;
using VoiceChat;

namespace CedMod.Addons.Audio
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class AudioCommand : ICommand
    {
        public static Dictionary<FakeConnection, ReferenceHub> FakeConnections = new Dictionary<FakeConnection, ReferenceHub>();
        public static Dictionary<int, ReferenceHub> FakeConnectionsIds = new Dictionary<int, ReferenceHub>();

        public string Command { get; } = "audio";

        public string[] Aliases { get; } = new string[0];

        public string Description { get; } = "Audio Command";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender raSender))
            {
                response = "Console cant execute this command!";
                return false;
            }

            if (!sender.HasPermissions("cedmod.audio"))
            {
                response = "You dont have permission to run this command.";
                return false;
            }
            
            if (arguments.Count == 0)
            {
                response = "Args: audio fake {playerid} - spawns a dummyplayer\naudio enqueue {playerid} {cedmodfilename} [index] - enqueues a file on from the CedMod panel in the queue at the given index (index is optional)\naudio play {playerid} {index} - starts playing at the given index\naudio destroy {playerid} - destroys the player\ntesting volume {playerid} {volume} - sets the volume";
                return false;
            }

            switch (arguments.At(0).ToLower())
            {
                case "fake":
                    {
                        int id = int.Parse(arguments.At(1));

                        var newPlayer = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);

                        var fakeConnection = new FakeConnection(id);


                        var hubPlayer = newPlayer.GetComponent<ReferenceHub>();
                        FakeConnections.Add(fakeConnection, hubPlayer);
                        FakeConnectionsIds.Add(id, hubPlayer);

                        NetworkServer.AddPlayerForConnection(fakeConnection, newPlayer);
                        try
                        {
                            hubPlayer.authManager.UserId = $"player{id}@server";
                        }
                        catch (Exception e)
                        {
                        }
                        hubPlayer.authManager.InstanceMode = ClientInstanceMode.Host;
                        try
                        {
                            hubPlayer.nicknameSync.SetNick($"Dummy player {id}");
                        }
                        catch (Exception e)
                        {
                        }
                        Timing.CallDelayed(0.1f, () =>
                        {
                            hubPlayer.roleManager.ServerSetRole(RoleTypeId.CustomRole, RoleChangeReason.RemoteAdmin);
                        });
                    }
                    break;
                case "attach":
                {
                    int id = int.Parse(arguments.At(1));
                    FakeConnectionsIds.Add(id, ReferenceHub.AllHubs.Where(s => s.PlayerId == id).FirstOrDefault());
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        var audioPlayer = CustomAudioPlayer.Get(hub);
                    }
                }
                    break;
                case "enqueue":
                {
                    int id = int.Parse(arguments.At(1));
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        var audioPlayer = CustomAudioPlayer.Get(hub);
                        audioPlayer.Enqueue(arguments.At(2), arguments.Count >= 4 ? Convert.ToInt32(arguments.At(3)) : -1);
                    }
                }
                    break;
                case "play":
                    {
                        int id = int.Parse(arguments.At(1));
                        if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                        {
                            var audioPlayer = CustomAudioPlayer.Get(hub);
                            audioPlayer.Play(Convert.ToInt32(arguments.At(2)));
                        }
                    }
                    break;
                case "destroy":
                {
                    int id = int.Parse(arguments.At(1));
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        FakeConnections.Remove(FakeConnections.FirstOrDefault(s => s.Value == hub).Key);
                        FakeConnectionsIds.Remove(id);
                        NetworkServer.Destroy(hub.gameObject);
                    }
                } 
                    break;
                case "volume":
                {
                    int id = int.Parse(arguments.At(1));
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        var audioPlayer = CustomAudioPlayer.Get(hub);
                        audioPlayer.Volume = Convert.ToInt32(arguments.At(2));
                    }
                } 
                    break;
                case "loop":
                {
                    int id = int.Parse(arguments.At(1));
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        var audioPlayer = CustomAudioPlayer.Get(hub);
                        audioPlayer.Loop = !audioPlayer.Loop;
                    }
                } 
                    break;
                case "audiochannel":
                {
                    int id = int.Parse(arguments.At(1));
                    if (FakeConnectionsIds.TryGetValue(id, out ReferenceHub hub))
                    {
                        var audioPlayer = CustomAudioPlayer.Get(hub);
                        audioPlayer.BroadcastChannel = (VoiceChatChannel)Enum.Parse(typeof(VoiceChatChannel), arguments.At(2));
                    }
                } 
                    break;
            }

            response = "Done";
            return true;
        }
    }
}