// Decompiled with JetBrains decompiler
// Type: SyncListUshort
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;

public class SyncListUshort : SyncList<ushort>
{
  protected override void SerializeItem(NetworkWriter writer, ushort item)
  {
    writer.WriteUInt16(item);
  }

  protected override ushort DeserializeItem(NetworkReader reader)
  {
    return reader.ReadUInt16();
  }
}
