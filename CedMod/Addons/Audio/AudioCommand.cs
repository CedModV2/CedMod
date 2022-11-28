using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Mirror;
using PlayerRoles;
using RemoteAdmin;

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

            CedModPlayer player = CedModPlayer.Get(raSender.ReferenceHub);

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

                        hubPlayer.roleManager.ServerSetRole(RoleTypeId.Scp049, RoleChangeReason.RemoteAdmin);
                        hubPlayer.nicknameSync.SetNick($"Dummy player {id}");
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
            }

            response = "Done";
            return true;
        }
    }
}