using System.Collections.Generic;
using System.Linq;
using CedMod.Addons.QuerySystem.WS;
using Cryptography;
using Exiled.API.Features;
using Mirror;
using RemoteAdmin;
using UnityEngine;

namespace CedMod.Addons.QuerySystem
{
    public static class HandleQueryplayer
    {
        public static List<Player> PlayerDummies = new List<Player>();

        public static bool IsDummy(this GameObject player)
        {
            return PlayerDummies.Any(s => s.GameObject == player);
        }
        
        public static void CreateDummy(CmSender cmSender)
        {
            if (!ServerStatic.GetPermissionsHandler()._members.Any(s => s.Key == cmSender.senderId))
            {
                return;
            }
            var gameObject = Object.Instantiate(Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorNetworkManager.singleton.playerPrefab);

            var player = new Player(gameObject);
            ReferenceHub referenceHub = player.ReferenceHub;

            Log.Debug(referenceHub != null);

            gameObject.transform.localScale = new Vector3(1, 1, 1);
            gameObject.transform.position = new Vector3(0, 0, 0);

            referenceHub.queryProcessor.PlayerId = QueryProcessor._idIterator++;
            referenceHub.queryProcessor.NetworkPlayerId = referenceHub.queryProcessor.PlayerId;
            referenceHub.queryProcessor._ipAddress = "127.0.0.WAN";
            referenceHub.characterClassManager.CurClass = RoleType.None;
            player.Health = 1;
            referenceHub.nicknameSync.Network_myNickSync = cmSender.Nickname + "(RemoteCmds)";
            referenceHub.characterClassManager._privUserId = cmSender.senderId;
            referenceHub.characterClassManager.SyncedUserId = cmSender.senderId;
            referenceHub.characterClassManager.NetworkSyncedUserId = Sha.HashToString(Sha.Sha512(referenceHub.characterClassManager._privUserId));
            referenceHub.serverRoles.Group = ServerStatic.GetPermissionsHandler()._groups[ServerStatic.GetPermissionsHandler()._members.FirstOrDefault(s => s.Key == cmSender.senderId).Value];

            NetworkServer.Spawn(gameObject);
            Player.Dictionary.Add(gameObject, player);
            PlayerDummies.Add(player);
            PlayerManager.AddPlayer(gameObject, CustomNetworkManager.slots);
            MEC.Timing.CallDelayed(0.1f, () =>
            {
                referenceHub.playerMovementSync.OverridePosition(new Vector3(0, 0, 0), new PlayerMovementSync.PlayerRotation(0, 0));
            });
        }
    }
}