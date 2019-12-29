// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.DynamicObjectTypeFallbackFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Utf8Json.Internal;
using Utf8Json.Internal.Emit;
using Utf8Json.Resolvers.Internal;

namespace Utf8Json.Formatters
{
  public sealed class DynamicObjectTypeFallbackFormatter : IJsonFormatter<object>, IJsonFormatter
  {
    private readonly ThreadsafeTypeKeyHashTable<KeyValuePair<object, DynamicObjectTypeFallbackFormatter.SerializeMethod>> serializers = new ThreadsafeTypeKeyHashTable<KeyValuePair<object, DynamicObjectTypeFallbackFormatter.SerializeMethod>>(4, 0.75f);
    private readonly IJsonFormatterResolver[] innerResolvers;

    public DynamicObjectTypeFallbackFormatter(params IJsonFormatterResolver[] innerResolvers)
    {
      this.innerResolvers = innerResolvers;
    }

    public void Serialize(
      ref Utf8Json.JsonWriter writer,
      object value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == null)
      {
        writer.WriteNull();
      }
      else
      {
        Type type1 = value.GetType();
        if (type1 == typeof (object))
        {
          writer.WriteBeginObject();
          writer.WriteEndObject();
        }
        else
        {
          KeyValuePair<object, DynamicObjectTypeFallbackFormatter.SerializeMethod> keyValuePair;
          if (!this.serializers.TryGetValue(type1, out keyValuePair))
          {
            lock (this.serializers)
            {
              if (!this.serializers.TryGetValue(type1, out keyValuePair))
              {
                object key = (object) null;
                foreach (IJsonFormatterResolver innerResolver in this.innerResolvers)
                {
                  key = innerResolver.GetFormatterDynamic(type1);
                  if (key != null)
                    break;
                }
                if (key == null)
                  throw new FormatterNotRegisteredException(type1.FullName + " is not registered in this resolver. resolvers:" + string.Join(", ", ((IEnumerable<IJsonFormatterResolver>) this.innerResolvers).Select<IJsonFormatterResolver, string>((Func<IJsonFormatterResolver, string>) (x => x.GetType().Name)).ToArray<string>()));
                Type type2 = type1;
                DynamicMethod dynamicMethod = new DynamicMethod(nameof (Serialize), (Type) null, new Type[4]
                {
                  typeof (object),
                  typeof (Utf8Json.JsonWriter).MakeByRefType(),
                  typeof (object),
                  typeof (IJsonFormatterResolver)
                }, type1.Module, true);
                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.EmitLdarg(0);
                ilGenerator.Emit(OpCodes.Castclass, typeof (IJsonFormatter<>).MakeGenericType(type2));
                ilGenerator.EmitLdarg(1);
                ilGenerator.EmitLdarg(2);
                ilGenerator.EmitUnboxOrCast(type2);
                ilGenerator.EmitLdarg(3);
                ilGenerator.EmitCall(DynamicObjectTypeBuilder.EmitInfo.Serialize(type2));
                ilGenerator.Emit(OpCodes.Ret);
                keyValuePair = new KeyValuePair<object, DynamicObjectTypeFallbackFormatter.SerializeMethod>(key, (DynamicObjectTypeFallbackFormatter.SerializeMethod) ((MethodInfo) dynamicMethod).CreateDelegate(typeof (DynamicObjectTypeFallbackFormatter.SerializeMethod)));
                this.serializers.TryAdd(type2, keyValuePair);
              }
            }
          }
          keyValuePair.Value(keyValuePair.Key, ref writer, value, formatterResolver);
        }
      }
    }

    public object Deserialize(ref Utf8Json.JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      return PrimitiveObjectFormatter.Default.Deserialize(ref reader, formatterResolver);
    }

    private delegate void SerializeMethod(
      object dynamicFormatter,
      ref Utf8Json.JsonWriter writer,
      object value,
      IJsonFormatterResolver formatterResolver);
  }
}
