// Decompiled with JetBrains decompiler
// Type: QueryRaReply
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using Utf8Json;

public readonly struct QueryRaReply : IEquatable<QueryRaReply>, IJsonSerializable
{
  public readonly string Text;
  public readonly bool Success;
  public readonly bool LogToConsole;
  public readonly string OverrideDisplay;

  [SerializationConstructor]
  public QueryRaReply(string text, bool success, bool logToConsole, string overrideDisplay)
  {
    this.Text = text;
    this.Success = success;
    this.LogToConsole = logToConsole;
    this.OverrideDisplay = overrideDisplay;
  }

  public bool Equals(QueryRaReply other)
  {
    return string.Equals(this.Text, other.Text) && this.Success == other.Success && this.LogToConsole == other.LogToConsole && string.Equals(this.OverrideDisplay, other.OverrideDisplay);
  }

  public override bool Equals(object obj)
  {
    return obj is QueryRaReply other && this.Equals(other);
  }

  public override int GetHashCode()
  {
    int num1 = (this.Text != null ? this.Text.GetHashCode() : 0) * 397;
    bool flag = this.Success;
    int hashCode1 = flag.GetHashCode();
    int num2 = (num1 ^ hashCode1) * 397;
    flag = this.LogToConsole;
    int hashCode2 = flag.GetHashCode();
    return (num2 ^ hashCode2) * 397 ^ (this.OverrideDisplay != null ? this.OverrideDisplay.GetHashCode() : 0);
  }

  public static bool operator ==(QueryRaReply left, QueryRaReply right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(QueryRaReply left, QueryRaReply right)
  {
    return !left.Equals(right);
  }
}
