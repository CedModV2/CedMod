// Decompiled with JetBrains decompiler
// Type: Utf8Json.JsonFormatterAttribute
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json
{
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
  public class JsonFormatterAttribute : Attribute
  {
    public Type FormatterType { get; private set; }

    public object[] Arguments { get; private set; }

    public JsonFormatterAttribute(Type formatterType)
    {
      this.FormatterType = formatterType;
    }

    public JsonFormatterAttribute(Type formatterType, params object[] arguments)
    {
      this.FormatterType = formatterType;
      this.Arguments = arguments;
    }
  }
}
