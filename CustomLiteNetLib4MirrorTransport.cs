// Decompiled with JetBrains decompiler
// Type: CustomLiteNetLib4MirrorTransport
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Cryptography;
using GameCore;
using LiteNetLib;
using LiteNetLib.Utils;
using Mirror.LiteNetLib4Mirror;
using System;
using System.Collections.Generic;
using System.Net;

public class CustomLiteNetLib4MirrorTransport : LiteNetLib4MirrorTransport
{
  private static readonly NetDataWriter RequestWriter = new NetDataWriter();
  public static GeoblockingMode Geoblocking = GeoblockingMode.None;
  public static readonly Dictionary<IPEndPoint, PreauthItem> UserIds = new Dictionary<IPEndPoint, PreauthItem>();
  public static readonly HashSet<string> UserRateLimit = new HashSet<string>();
  public static readonly HashSet<string> IpRateLimit = new HashSet<string>();
  public static readonly HashSet<string> GeoblockingList = new HashSet<string>();
  public static bool UserRateLimiting;
  public static bool IpRateLimiting;
  public static bool UseGlobalBans;
  public static bool GeoblockIgnoreWhitelisted;
  public static RejectionReason LastRejectionReason;
  public static string LastCustomReason;
  public static long LastBanExpiration;

  protected override void ProcessConnectionRequest(ConnectionRequest request)
  {
    try
    {
      byte result1;
      byte result2;
      if (!request.Data.TryGetByte(out result1) || !request.Data.TryGetByte(out result2) || ((int) result1 != (int) CustomNetworkManager.Major || (int) result2 != (int) CustomNetworkManager.Minor))
      {
        CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
        CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 3);
        request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
      }
      else
      {
        if (CustomLiteNetLib4MirrorTransport.IpRateLimiting)
        {
          if (CustomLiteNetLib4MirrorTransport.IpRateLimit.Contains(request.RemoteEndPoint.Address.ToString()))
          {
            ServerConsole.AddLog(string.Format("Incoming connection from endpoint {0} rejected due to exceeding the rate limit.", (object) request.RemoteEndPoint));
            ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Incoming connection from endpoint {0} rejected due to exceeding the rate limit.", (object) request.RemoteEndPoint), ServerLogs.ServerLogType.RateLimit);
            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 12);
            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
            return;
          }
          CustomLiteNetLib4MirrorTransport.IpRateLimit.Add(request.RemoteEndPoint.Address.ToString());
        }
        if (!CharacterClassManager.OnlineMode)
        {
          KeyValuePair<BanDetails, BanDetails> keyValuePair = BanHandler.QueryBan((string) null, request.RemoteEndPoint.Address.ToString());
          if (keyValuePair.Value != null)
          {
            ServerConsole.AddLog(string.Format("Player tried to connect from banned endpoint {0}.", (object) request.RemoteEndPoint));
            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 6);
            CustomLiteNetLib4MirrorTransport.RequestWriter.Put(keyValuePair.Value.Expires);
            CustomLiteNetLib4MirrorTransport.RequestWriter.Put(keyValuePair.Value?.Reason ?? string.Empty);
            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
          }
          else
            request.Accept();
        }
        else
        {
          string result3;
          if (!request.Data.TryGetString(out result3) || result3 == string.Empty)
          {
            CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
            CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 5);
            request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
          }
          else
          {
            ulong result4;
            byte result5;
            string result6;
            byte[] result7;
            if (!request.Data.TryGetULong(out result4) || !request.Data.TryGetByte(out result5) || (!request.Data.TryGetString(out result6) || !request.Data.TryGetBytesWithLength(out result7)))
            {
              CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
              CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 4);
              request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
            }
            else
            {
              CentralAuthPreauthFlags flags = (CentralAuthPreauthFlags) result5;
              try
              {
                if (!ECDSA.VerifyBytes(string.Format("{0};{1};{2};{3}", (object) result3, (object) result5, (object) result6, (object) result4), result7, ServerConsole.PublicKey))
                {
                  ServerConsole.AddLog(string.Format("Player from endpoint {0} sent preauthentication token with invalid digital signature.", (object) request.RemoteEndPoint));
                  CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                  CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 2);
                  request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                }
                else if (TimeBehaviour.CurrentUnixTimestamp > result4)
                {
                  ServerConsole.AddLog(string.Format("Player from endpoint {0} sent expired preauthentication token.", (object) request.RemoteEndPoint));
                  ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
                  CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                  CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 11);
                  request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                }
                else
                {
                  if (CustomLiteNetLib4MirrorTransport.UserRateLimiting)
                  {
                    if (CustomLiteNetLib4MirrorTransport.UserRateLimit.Contains(result3))
                    {
                      ServerConsole.AddLog(string.Format("Incoming connection from {0} ({1}) rejected due to exceeding the rate limit.", (object) result3, (object) request.RemoteEndPoint));
                      ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("Incoming connection from endpoint {0} ({1}) rejected due to exceeding the rate limit.", (object) result3, (object) request.RemoteEndPoint), ServerLogs.ServerLogType.RateLimit);
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 12);
                      request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                      return;
                    }
                    CustomLiteNetLib4MirrorTransport.UserRateLimit.Add(result3);
                  }
                  if (flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreBans) || !ServerStatic.GetPermissionsHandler().IsVerified)
                  {
                    KeyValuePair<BanDetails, BanDetails> keyValuePair = BanHandler.QueryBan(result3, request.RemoteEndPoint.Address.ToString());
                    if (keyValuePair.Key != null || keyValuePair.Value != null)
                    {
                      ServerConsole.AddLog(string.Format("{0} {1} tried to connect from {2} endpoint {3}.", keyValuePair.Key == null ? (object) "Player" : (object) "Banned player", (object) result3, keyValuePair.Value == null ? (object) "" : (object) "banned ", (object) request.RemoteEndPoint));
                      ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("{0} {1} tried to connect from {2} endpoint {3}.", keyValuePair.Key == null ? (object) "Player" : (object) "Banned player", (object) result3, keyValuePair.Value == null ? (object) "" : (object) "banned ", (object) request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate);
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 6);
                      NetDataWriter requestWriter = CustomLiteNetLib4MirrorTransport.RequestWriter;
                      BanDetails key = keyValuePair.Key;
                      long num = key != null ? key.Expires : keyValuePair.Value.Expires;
                      requestWriter.Put(num);
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Put(keyValuePair.Key?.Reason ?? keyValuePair.Value?.Reason ?? string.Empty);
                      request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                      return;
                    }
                  }
                  if (flags.HasFlagFast(CentralAuthPreauthFlags.GloballyBanned) && (ServerStatic.PermissionsHandler.IsVerified || CustomLiteNetLib4MirrorTransport.UseGlobalBans))
                  {
                    ServerConsole.AddLog(string.Format("Player {0} ({1}) kicked due to an active global ban.", (object) result3, (object) request.RemoteEndPoint));
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 8);
                    request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                  }
                  else if ((!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreWhitelist) || !ServerStatic.GetPermissionsHandler().IsVerified) && !WhiteList.IsWhitelisted(result3))
                  {
                    ServerConsole.AddLog(string.Format("Player {0} tried joined from endpoint {1}, but is not whitelisted.", (object) result3, (object) request.RemoteEndPoint));
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 7);
                    request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                  }
                  else if (CustomLiteNetLib4MirrorTransport.Geoblocking != GeoblockingMode.None && (!flags.HasFlagFast(CentralAuthPreauthFlags.IgnoreGeoblock) || !ServerStatic.PermissionsHandler.BanTeamBypassGeo) && (!CustomLiteNetLib4MirrorTransport.GeoblockIgnoreWhitelisted || !WhiteList.IsOnWhitelist(result3)) && (CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Whitelist && !CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(result6.ToUpper()) || CustomLiteNetLib4MirrorTransport.Geoblocking == GeoblockingMode.Blacklist && CustomLiteNetLib4MirrorTransport.GeoblockingList.Contains(result6.ToUpper())))
                  {
                    ServerConsole.AddLog(string.Format("Player {0} ({1}) tried joined from blocked country {2}.", (object) result3, (object) request.RemoteEndPoint, (object) result6.ToUpper()));
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                    CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 9);
                    request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
                  }
                  else
                  {
                    int num = CustomNetworkManager.slots;
                    if (flags.HasFlagFast(CentralAuthPreauthFlags.ReservedSlot) && ServerStatic.PermissionsHandler.BanTeamSlots)
                      num = LiteNetLib4MirrorNetworkManager.singleton.maxConnections;
                    else if (ConfigFile.ServerConfig.GetBool("use_reserved_slots", true) && ReservedSlot.HasReservedSlot(result3))
                      num += CustomNetworkManager.reservedSlots;
                    if (LiteNetLib4MirrorCore.Host.PeersCount < num)
                    {
                      if (CustomLiteNetLib4MirrorTransport.UserIds.ContainsKey(request.RemoteEndPoint))
                        CustomLiteNetLib4MirrorTransport.UserIds[request.RemoteEndPoint].SetUserId(result3);
                      else
                        CustomLiteNetLib4MirrorTransport.UserIds.Add(request.RemoteEndPoint, new PreauthItem(result3));
                      request.Accept();
                      ServerConsole.AddLog(string.Format("Player {0} preauthenticated from endpoint {1}.", (object) result3, (object) request.RemoteEndPoint));
                      ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Format("{0} preauthenticated from endpoint {1}.", (object) result3, (object) request.RemoteEndPoint), ServerLogs.ServerLogType.ConnectionUpdate);
                    }
                    else
                    {
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                      CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 1);
                      request.Reject(CustomLiteNetLib4MirrorTransport.RequestWriter);
                    }
                  }
                }
              }
              catch (Exception ex)
              {
                ServerConsole.AddLog(string.Format("Player from endpoint {0} sent an invalid preauthentication token. {1}", (object) request.RemoteEndPoint, (object) ex.Message));
                CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
                CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 2);
                request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
              }
            }
          }
        }
      }
    }
    catch (Exception ex)
    {
      ServerConsole.AddLog(string.Format("Player from endpoint {0} failed to preauthenticate: {1}", (object) request.RemoteEndPoint, (object) ex.Message));
      CustomLiteNetLib4MirrorTransport.RequestWriter.Reset();
      CustomLiteNetLib4MirrorTransport.RequestWriter.Put((byte) 4);
      request.RejectForce(CustomLiteNetLib4MirrorTransport.RequestWriter);
    }
  }

  protected override void GetConnectData(NetDataWriter writer)
  {
  }

  protected override void OnConncetionRefused(DisconnectInfo disconnectinfo)
  {
    byte result;
    if (disconnectinfo.AdditionalData.TryGetByte(out result))
    {
      CustomLiteNetLib4MirrorTransport.LastRejectionReason = (RejectionReason) result;
      if (result == (byte) 6 && !disconnectinfo.AdditionalData.TryGetLong(out CustomLiteNetLib4MirrorTransport.LastBanExpiration))
        CustomLiteNetLib4MirrorTransport.LastBanExpiration = 0L;
      if (result != (byte) 6 && result != (byte) 10 || disconnectinfo.AdditionalData.TryGetString(out CustomLiteNetLib4MirrorTransport.LastCustomReason))
        return;
      CustomLiteNetLib4MirrorTransport.LastCustomReason = string.Empty;
    }
    else
      CustomLiteNetLib4MirrorTransport.LastRejectionReason = RejectionReason.NotSpecified;
  }
}
