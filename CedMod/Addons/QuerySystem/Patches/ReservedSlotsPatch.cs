using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using CedMod.Addons.QuerySystem.WS;
using CentralAuth;
using Cryptography;
using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using Newtonsoft.Json;
using NorthwoodLib.Pools;
using SlProxy;

using static HarmonyLib.AccessTools;

namespace CedMod.Addons.QuerySystem.Patches
{
    [HarmonyPatch(typeof(CustomLiteNetLib4MirrorTransport),
        nameof(CustomLiteNetLib4MirrorTransport.ProcessConnectionRequest))]
    public static class AuthKickTranspiler
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions,
            ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(codeInstructions);

            int offset = 9;
            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Ldsfld && x.operand is FieldInfo { Name: "UseChallenge" }) + offset;
            
            Label ret = generator.DefineLabel();
            
            newInstructions.InsertRange(index, new[]
            {
                // if(!AuthKickTranspiler.MethodTest(request))
                new CodeInstruction(OpCodes.Ldarg_1).MoveLabelsFrom(newInstructions[index]),
                CodeInstruction.Call(typeof(AuthKickTranspiler), nameof(PreauthCheck), new []{ typeof(ConnectionRequest) }), 
                
                // return;
                new(OpCodes.Brfalse_S, ret),
            });
            
            newInstructions[^1].labels.Add(ret);
            
            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        public static bool PreauthCheck(ConnectionRequest request)
        {
            var reader = new NetDataReader(request.Data.RawData);
            reader._position = 34;
            var preauthdata = PreAuthModel.ReadPreAuth(reader);
            if (preauthdata == null)
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug($"Rejected preauth due to null data");
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Custom);
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(
                    $"[CedModAntiPreAuthSpam]\nYour connection has been rejected as the 'PreAuth' data sent from your client appears to be invalid, please restart your game or run 'ar' in your client console, You can usually open the client console by pressing ` or ~");

                CustomLiteNetLib4MirrorTransport.Rejected++;
                if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                    CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

                if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
                    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
                    ServerConsole.AddLog(
                        $"Security challenge response of incoming connection from endpoint {request.RemoteEndPoint} has been CustomLiteNetLib4MirrorTransport.Rejected (Failed extra CedMod verification 1).");
                return false;
            }

            if (PlayerAuthenticationManager.OnlineMode && !ECDSA.VerifyBytes(
                    $"{preauthdata.UserID};{preauthdata.Flags};{preauthdata.Region};{preauthdata.Expiration}",
                    preauthdata.Signature, ServerConsole.PublicKey))
            {
                if (CedModMain.Singleton.Config.CedMod.ShowDebug)
                    Logger.Debug($"Rejected preauth due to invalidity\n{preauthdata}");
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte)RejectionReason.Custom);
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put(
                    $"[CedModAntiPreAuthSpam]\nYour connection has been rejected as the 'PreAuth' data sent from your client appears to be invalid, please restart your game or run 'ar' in your client console, You can usually open the client console by pressing ` or ~");
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);

                CustomLiteNetLib4MirrorTransport.Rejected++;
                if (CustomLiteNetLib4MirrorTransport.Rejected > CustomLiteNetLib4MirrorTransport.RejectionThreshold)
                    CustomLiteNetLib4MirrorTransport.SuppressRejections = true;

                if (!CustomLiteNetLib4MirrorTransport.SuppressRejections &&
                    CustomLiteNetLib4MirrorTransport.DisplayPreauthLogs)
                    ServerConsole.AddLog(
                        $"Security challenge response of incoming connection from endpoint {request.RemoteEndPoint} has been CustomLiteNetLib4MirrorTransport.Rejected (Failed extra CedMod verification 2).");
                return false;
            }
            return true;
        }
    }
}