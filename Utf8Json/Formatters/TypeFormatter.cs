// Decompiled with JetBrains decompiler
// Type: Utf8Json.Formatters.TypeFormatter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Text.RegularExpressions;

namespace Utf8Json.Formatters
{
  public sealed class TypeFormatter : IJsonFormatter<Type>, IJsonFormatter
  {
    public static readonly TypeFormatter Default = new TypeFormatter();
    private static readonly Regex SubtractFullNameRegex = new Regex(", Version=\\d+.\\d+.\\d+.\\d+, Culture=\\w+, PublicKeyToken=\\w+");
    private bool serializeAssemblyQualifiedName;
    private bool deserializeSubtractAssemblyQualifiedName;
    private bool throwOnError;

    public TypeFormatter()
      : this(true, true, true)
    {
    }

    public TypeFormatter(
      bool serializeAssemblyQualifiedName,
      bool deserializeSubtractAssemblyQualifiedName,
      bool throwOnError)
    {
      this.serializeAssemblyQualifiedName = serializeAssemblyQualifiedName;
      this.deserializeSubtractAssemblyQualifiedName = deserializeSubtractAssemblyQualifiedName;
      this.throwOnError = throwOnError;
    }

    public void Serialize(
      ref JsonWriter writer,
      Type value,
      IJsonFormatterResolver formatterResolver)
    {
      if (value == (Type) null)
        writer.WriteNull();
      else if (this.serializeAssemblyQualifiedName)
        writer.WriteString(value.AssemblyQualifiedName);
      else
        writer.WriteString(value.FullName);
    }

    public Type Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
      if (reader.ReadIsNull())
        return (Type) null;
      string str = reader.ReadString();
      if (this.deserializeSubtractAssemblyQualifiedName)
        str = TypeFormatter.SubtractFullNameRegex.Replace(str, "");
      return Type.GetType(str, this.throwOnError);
    }
  }
}
