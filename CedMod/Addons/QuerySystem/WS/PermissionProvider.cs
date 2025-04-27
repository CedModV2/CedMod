using System.Collections.Generic;
using System.Linq;
using CedMod.ApiModals;
using LabApi.Features.Console;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using Utils.NonAllocLINQ;

namespace CedMod.Addons.QuerySystem.WS
{
    public class PermissionProvider: IPermissionsProvider
    {
        public static AutoSlPermsSlRequest Permissions { get; set; }

        public string[] GetPermissions(Player player)
        {
            string group = GetGroup(player.UserId);
            if (string.IsNullOrEmpty(group))
                return new string[0];
            List<string> perms = new List<string>();
            
            if (string.IsNullOrEmpty(group))
            {
                perms = Permissions.DefaultPermissions;
            }
            else if (Permissions.PermissionEntries.Any(s => s.Name == group))
            {
                var groupObject = Permissions.PermissionEntries.FirstOrDefault(s => s.Name == group);
                if (groupObject != null)
                    perms.AddRange(groupObject.ExiledPermissions);
            }

            return perms.ToArray();
        }

        public bool HasPermissions(Player player, params string[] permissions)
        {
            if (player.IsHost)
                return true;
            
            string group = GetGroup(player.UserId);
            if (string.IsNullOrEmpty(group))
                return false;
            List<string> perms = new List<string>();
            
            if (string.IsNullOrEmpty(group))
            {
                perms = Permissions.DefaultPermissions;
            }
            else if (Permissions.PermissionEntries.Any(s => s.Name == group))
            {
                var groupObject = Permissions.PermissionEntries.FirstOrDefault(s => s.Name == group);
                if (groupObject != null)
                    perms.AddRange(groupObject.ExiledPermissions);
            }
            
            foreach (var perm in permissions)
            {
                if (!CheckGroupPermission(perms, perm))
                    return false;
            }

            return true;
        }

        public bool HasAnyPermission(Player player, params string[] permissions)
        {
            if (player.IsHost)
                return true;
            
            string group = GetGroup(player.UserId);
            if (string.IsNullOrEmpty(group))
                return false;
            
            List<string> perms = new List<string>();
            
            if (string.IsNullOrEmpty(group))
            {
                perms = Permissions.DefaultPermissions;
            }
            else if (Permissions.PermissionEntries.Any(s => s.Name == group))
            {
                var groupObject = Permissions.PermissionEntries.FirstOrDefault(s => s.Name == group);
                perms.AddRange(groupObject.ExiledPermissions);
            }
            
            foreach (var perm in permissions)
            {
                if (CheckGroupPermission(perms, perm))
                    return true;
            }

            return false;
        }

        public void AddPermissions(Player player, params string[] permissions)
        {
            Logger.Warn("A plugin tried granting permissions, CedMod permission management does not support this.");
        }

        public void RemovePermissions(Player player, params string[] permissions)
        {
            Logger.Warn("A plugin tried granting permissions, CedMod permission management does not support this.");
        }

        private string GetGroup(string userId)
        {
            
            string group = "";
            if (ServerStatic.PermissionsHandler.Members.ContainsKey(userId))
            {
                group = ServerStatic.PermissionsHandler.Members[userId];
                
                if (CedModMain.Singleton.Config.Debug)
                    Logger.Debug($"Found in RA config {group}");
            }
            else
            {
                ReferenceHub hub = ReferenceHub.AllHubs.FirstOrDefault(s => s.authManager.UserId == userId);
                
                if (CedModMain.Singleton.Config.Debug)
                    Logger.Debug($"Found hubs {hub.PlayerId}");
                if (hub.serverRoles == null || hub.serverRoles.Group == null)
                    return null;
                
                UserGroup playerGroup = hub.serverRoles.Group;
                group = playerGroup.Name;
            }

            return group;
        }
        
        public static bool CheckGroupPermission(List<string> perms, string permission)
        {
            if (perms.Contains(".*"))
                return true;
            string[] sp = permission.Split('.');
            if (sp.Length != 1)
                if (perms.Contains(sp[0] + ".*"))
                    return true;
            if (sp.Length == 1)
                if (perms.Any(perm => perm.StartsWith(sp[0])))
                    return true;
            foreach(var perm in perms)
                if (perm == permission)
                    return true;

            return false;
        }
    }
}