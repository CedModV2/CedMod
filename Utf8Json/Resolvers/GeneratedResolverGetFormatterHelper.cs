// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.GeneratedResolverGetFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Authenticator;
using System;
using System.Collections.Generic;
using Utf8Json.Formatters;
using Utf8Json.Formatters.Authenticator;

namespace Utf8Json.Resolvers
{
  internal static class GeneratedResolverGetFormatterHelper
  {
    private static readonly Dictionary<Type, int> lookup = new Dictionary<Type, int>(17)
    {
      {
        typeof (AuthenticatorPlayerObject[]),
        0
      },
      {
        typeof (AuthenticatiorAuthReject[]),
        1
      },
      {
        typeof (ServerListItem[]),
        2
      },
      {
        typeof (List<string>),
        3
      },
      {
        typeof (AuthenticatiorAuthReject),
        4
      },
      {
        typeof (AuthenticatorPlayerObject),
        5
      },
      {
        typeof (AuthenticatorPlayerObjects),
        6
      },
      {
        typeof (AuthenticatorResponse),
        7
      },
      {
        typeof (QueryRaReply),
        8
      },
      {
        typeof (ServerListItem),
        9
      },
      {
        typeof (ServerList),
        10
      },
      {
        typeof (PlayerListSerialized),
        11
      },
      {
        typeof (RequestSignatureResponse),
        12
      },
      {
        typeof (PublicKeyResponse),
        13
      },
      {
        typeof (RenewResponse),
        14
      },
      {
        typeof (AuthenticateResponse),
        15
      },
      {
        typeof (ServerListSigned),
        16
      }
    };

    internal static object GetFormatter(Type t)
    {
      int num;
      if (!GeneratedResolverGetFormatterHelper.lookup.TryGetValue(t, out num))
        return (object) null;
      switch (num)
      {
        case 0:
          return (object) new ArrayFormatter<AuthenticatorPlayerObject>();
        case 1:
          return (object) new ArrayFormatter<AuthenticatiorAuthReject>();
        case 2:
          return (object) new ArrayFormatter<ServerListItem>();
        case 3:
          return (object) new ListFormatter<string>();
        case 4:
          return (object) new AuthenticatiorAuthRejectFormatter();
        case 5:
          return (object) new AuthenticatorPlayerObjectFormatter();
        case 6:
          return (object) new AuthenticatorPlayerObjectsFormatter();
        case 7:
          return (object) new AuthenticatorResponseFormatter();
        case 8:
          return (object) new QueryRaReplyFormatter();
        case 9:
          return (object) new ServerListItemFormatter();
        case 10:
          return (object) new ServerListFormatter();
        case 11:
          return (object) new PlayerListSerializedFormatter();
        case 12:
          return (object) new RequestSignatureResponseFormatter();
        case 13:
          return (object) new PublicKeyResponseFormatter();
        case 14:
          return (object) new RenewResponseFormatter();
        case 15:
          return (object) new AuthenticateResponseFormatter();
        case 16:
          return (object) new ServerListSignedFormatter();
        default:
          return (object) null;
      }
    }
  }
}
