using System;
using System.Text;
using CentralAuth;
using Hints;
using LabApi.Features.Wrappers;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace FakeSyncTest
{
    public class Plugin: LabApi.Loader.Features.Plugins.Plugin
    {
        public override void Enable()
        {
            StaticUnityMethods.OnUpdate += Update;
        }

        private void Update()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var hub in ReferenceHub.AllHubs)
            {
                builder.Clear();
                if (hub.authManager.InstanceMode == ClientInstanceMode.ReadyClient)
                {
                    foreach (var hubTarget in ReferenceHub.AllHubs)
                    {
                        var roleColor = PlayerRoleLoader.TryGetRoleTemplate(hubTarget.roleManager.PreviouslySentRole.TryGetValue(hub.netId, out var role) ? role : RoleTypeId.Filmmaker, out FpcStandardRoleBase roleData) ? roleData.RoleColor : Color.gray;
                        var voice = hubTarget.roleManager.CurrentRole is FpcStandardRoleBase fpcStandard ? fpcStandard.VoiceModule : null;
                        builder.Append($"Server: [<color={hubTarget.roleManager.CurrentRole.RoleColor.ToHex()}>{hubTarget.roleManager.CurrentRole.RoleName}</color>] Local: <color={roleColor.ToHex()}>{hubTarget.nicknameSync.DisplayName}</color>{(voice == null || !voice.ServerIsSending ? "" : $" <color=green>{voice.CurrentChannel}</color>")}\n");
                    }
                    
                    if (Player.TryGet(hub.PlayerId, out var player))
                    {
                        player.SendHint(builder.ToString(), 0.1f);
                    }
                }
            }
        }

        public override void Disable()
        {
            
        }

        public override string Name { get; } = "FakeSyncTest";
        public override string Description { get; } = "Way to test the fakesync system";
        public override string Author { get; } = "John CedMod";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredApiVersion { get; } = new Version(1, 0, 0);
    }
}