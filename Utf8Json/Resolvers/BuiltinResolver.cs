// Decompiled with JetBrains decompiler
// Type: Utf8Json.Resolvers.BuiltinResolver
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers
{
  public sealed class BuiltinResolver : IJsonFormatterResolver
  {
    public static readonly IJsonFormatterResolver Instance = (IJsonFormatterResolver) new BuiltinResolver();

    private BuiltinResolver()
    {
    }

    public IJsonFormatter<T> GetFormatter<T>()
    {
      return BuiltinResolver.FormatterCache<T>.formatter;
    }

    private static class FormatterCache<T>
    {
      public static readonly IJsonFormatter<T> formatter = (IJsonFormatter<T>) BuiltinResolver.BuiltinResolverGetFormatterHelper.GetFormatter(typeof (T));
    }

    internal static class BuiltinResolverGetFormatterHelper
    {
      private static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
      {
        {
          typeof (short),
          (object) Int16Formatter.Default
        },
        {
          typeof (int),
          (object) Int32Formatter.Default
        },
        {
          typeof (long),
          (object) Int64Formatter.Default
        },
        {
          typeof (ushort),
          (object) UInt16Formatter.Default
        },
        {
          typeof (uint),
          (object) UInt32Formatter.Default
        },
        {
          typeof (ulong),
          (object) UInt64Formatter.Default
        },
        {
          typeof (float),
          (object) SingleFormatter.Default
        },
        {
          typeof (double),
          (object) DoubleFormatter.Default
        },
        {
          typeof (bool),
          (object) BooleanFormatter.Default
        },
        {
          typeof (byte),
          (object) ByteFormatter.Default
        },
        {
          typeof (sbyte),
          (object) SByteFormatter.Default
        },
        {
          typeof (short?),
          (object) NullableInt16Formatter.Default
        },
        {
          typeof (int?),
          (object) NullableInt32Formatter.Default
        },
        {
          typeof (long?),
          (object) NullableInt64Formatter.Default
        },
        {
          typeof (ushort?),
          (object) NullableUInt16Formatter.Default
        },
        {
          typeof (uint?),
          (object) NullableUInt32Formatter.Default
        },
        {
          typeof (ulong?),
          (object) NullableUInt64Formatter.Default
        },
        {
          typeof (float?),
          (object) NullableSingleFormatter.Default
        },
        {
          typeof (double?),
          (object) NullableDoubleFormatter.Default
        },
        {
          typeof (bool?),
          (object) NullableBooleanFormatter.Default
        },
        {
          typeof (byte?),
          (object) NullableByteFormatter.Default
        },
        {
          typeof (sbyte?),
          (object) NullableSByteFormatter.Default
        },
        {
          typeof (DateTime),
          (object) ISO8601DateTimeFormatter.Default
        },
        {
          typeof (TimeSpan),
          (object) ISO8601TimeSpanFormatter.Default
        },
        {
          typeof (DateTimeOffset),
          (object) ISO8601DateTimeOffsetFormatter.Default
        },
        {
          typeof (DateTime?),
          (object) new StaticNullableFormatter<DateTime>(ISO8601DateTimeFormatter.Default)
        },
        {
          typeof (TimeSpan?),
          (object) new StaticNullableFormatter<TimeSpan>(ISO8601TimeSpanFormatter.Default)
        },
        {
          typeof (DateTimeOffset?),
          (object) new StaticNullableFormatter<DateTimeOffset>(ISO8601DateTimeOffsetFormatter.Default)
        },
        {
          typeof (string),
          (object) NullableStringFormatter.Default
        },
        {
          typeof (char),
          (object) CharFormatter.Default
        },
        {
          typeof (char?),
          (object) NullableCharFormatter.Default
        },
        {
          typeof (Decimal),
          (object) DecimalFormatter.Default
        },
        {
          typeof (Decimal?),
          (object) new StaticNullableFormatter<Decimal>(DecimalFormatter.Default)
        },
        {
          typeof (Guid),
          (object) GuidFormatter.Default
        },
        {
          typeof (Guid?),
          (object) new StaticNullableFormatter<Guid>(GuidFormatter.Default)
        },
        {
          typeof (Uri),
          (object) UriFormatter.Default
        },
        {
          typeof (Version),
          (object) VersionFormatter.Default
        },
        {
          typeof (StringBuilder),
          (object) StringBuilderFormatter.Default
        },
        {
          typeof (BitArray),
          (object) BitArrayFormatter.Default
        },
        {
          typeof (Type),
          (object) TypeFormatter.Default
        },
        {
          typeof (byte[]),
          (object) ByteArrayFormatter.Default
        },
        {
          typeof (short[]),
          (object) Int16ArrayFormatter.Default
        },
        {
          typeof (int[]),
          (object) Int32ArrayFormatter.Default
        },
        {
          typeof (long[]),
          (object) Int64ArrayFormatter.Default
        },
        {
          typeof (ushort[]),
          (object) UInt16ArrayFormatter.Default
        },
        {
          typeof (uint[]),
          (object) UInt32ArrayFormatter.Default
        },
        {
          typeof (ulong[]),
          (object) UInt64ArrayFormatter.Default
        },
        {
          typeof (float[]),
          (object) SingleArrayFormatter.Default
        },
        {
          typeof (double[]),
          (object) DoubleArrayFormatter.Default
        },
        {
          typeof (bool[]),
          (object) BooleanArrayFormatter.Default
        },
        {
          typeof (sbyte[]),
          (object) SByteArrayFormatter.Default
        },
        {
          typeof (char[]),
          (object) CharArrayFormatter.Default
        },
        {
          typeof (string[]),
          (object) NullableStringArrayFormatter.Default
        },
        {
          typeof (List<short>),
          (object) new ListFormatter<short>()
        },
        {
          typeof (List<int>),
          (object) new ListFormatter<int>()
        },
        {
          typeof (List<long>),
          (object) new ListFormatter<long>()
        },
        {
          typeof (List<ushort>),
          (object) new ListFormatter<ushort>()
        },
        {
          typeof (List<uint>),
          (object) new ListFormatter<uint>()
        },
        {
          typeof (List<ulong>),
          (object) new ListFormatter<ulong>()
        },
        {
          typeof (List<float>),
          (object) new ListFormatter<float>()
        },
        {
          typeof (List<double>),
          (object) new ListFormatter<double>()
        },
        {
          typeof (List<bool>),
          (object) new ListFormatter<bool>()
        },
        {
          typeof (List<byte>),
          (object) new ListFormatter<byte>()
        },
        {
          typeof (List<sbyte>),
          (object) new ListFormatter<sbyte>()
        },
        {
          typeof (List<DateTime>),
          (object) new ListFormatter<DateTime>()
        },
        {
          typeof (List<char>),
          (object) new ListFormatter<char>()
        },
        {
          typeof (List<string>),
          (object) new ListFormatter<string>()
        },
        {
          typeof (ArraySegment<byte>),
          (object) ByteArraySegmentFormatter.Default
        },
        {
          typeof (ArraySegment<byte>?),
          (object) new StaticNullableFormatter<ArraySegment<byte>>(ByteArraySegmentFormatter.Default)
        }
      };

      internal static object GetFormatter(Type t)
      {
        object obj;
        return BuiltinResolver.BuiltinResolverGetFormatterHelper.formatterMap.TryGetValue(t, out obj) ? obj : (object) null;
      }
    }
  }
}
