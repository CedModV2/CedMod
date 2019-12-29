// Decompiled with JetBrains decompiler
// Type: Utf8Json.Unity.UnityResolverGetFormatterHelper
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using Utf8Json.Formatters;

namespace Utf8Json.Unity
{
  internal static class UnityResolverGetFormatterHelper
  {
    private static readonly Dictionary<System.Type, int> lookup = new Dictionary<System.Type, int>(7)
    {
      {
        typeof (Vector2),
        0
      },
      {
        typeof (Vector3),
        1
      },
      {
        typeof (Vector4),
        2
      },
      {
        typeof (Quaternion),
        3
      },
      {
        typeof (Color),
        4
      },
      {
        typeof (Bounds),
        5
      },
      {
        typeof (Rect),
        6
      },
      {
        typeof (Vector2[]),
        7
      },
      {
        typeof (Vector3[]),
        8
      },
      {
        typeof (Vector4[]),
        9
      },
      {
        typeof (Quaternion[]),
        10
      },
      {
        typeof (Color[]),
        11
      },
      {
        typeof (Bounds[]),
        12
      },
      {
        typeof (Rect[]),
        13
      },
      {
        typeof (Vector2?),
        14
      },
      {
        typeof (Vector3?),
        15
      },
      {
        typeof (Vector4?),
        16
      },
      {
        typeof (Quaternion?),
        17
      },
      {
        typeof (Color?),
        18
      },
      {
        typeof (Bounds?),
        19
      },
      {
        typeof (Rect?),
        20
      }
    };

    internal static object GetFormatter(System.Type t)
    {
      int num;
      if (!UnityResolverGetFormatterHelper.lookup.TryGetValue(t, out num))
        return (object) null;
      switch (num)
      {
        case 0:
          return (object) new Vector2Formatter();
        case 1:
          return (object) new Vector3Formatter();
        case 2:
          return (object) new Vector4Formatter();
        case 3:
          return (object) new QuaternionFormatter();
        case 4:
          return (object) new ColorFormatter();
        case 5:
          return (object) new BoundsFormatter();
        case 6:
          return (object) new RectFormatter();
        case 7:
          return (object) new ArrayFormatter<Vector2>();
        case 8:
          return (object) new ArrayFormatter<Vector3>();
        case 9:
          return (object) new ArrayFormatter<Vector4>();
        case 10:
          return (object) new ArrayFormatter<Quaternion>();
        case 11:
          return (object) new ArrayFormatter<Color>();
        case 12:
          return (object) new ArrayFormatter<Bounds>();
        case 13:
          return (object) new ArrayFormatter<Rect>();
        case 14:
          return (object) new StaticNullableFormatter<Vector2>((IJsonFormatter<Vector2>) new Vector2Formatter());
        case 15:
          return (object) new StaticNullableFormatter<Vector3>((IJsonFormatter<Vector3>) new Vector3Formatter());
        case 16:
          return (object) new StaticNullableFormatter<Vector4>((IJsonFormatter<Vector4>) new Vector4Formatter());
        case 17:
          return (object) new StaticNullableFormatter<Quaternion>((IJsonFormatter<Quaternion>) new QuaternionFormatter());
        case 18:
          return (object) new StaticNullableFormatter<Color>((IJsonFormatter<Color>) new ColorFormatter());
        case 19:
          return (object) new StaticNullableFormatter<Bounds>((IJsonFormatter<Bounds>) new BoundsFormatter());
        case 20:
          return (object) new StaticNullableFormatter<Rect>((IJsonFormatter<Rect>) new RectFormatter());
        default:
          return (object) null;
      }
    }
  }
}
