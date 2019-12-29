// Decompiled with JetBrains decompiler
// Type: NineTailedFoxUnits
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using Mirror;
using UnityEngine;

public class NineTailedFoxUnits : NetworkBehaviour
{
  public string[] names;
  public SyncListString list;
  public static NineTailedFoxUnits host;

  private void AddUnit(string unit)
  {
    this.list.Add(unit);
  }

  private string GenerateName()
  {
    return this.names[Random.Range(0, this.names.Length)] + "-" + Random.Range(1, 20).ToString("00");
  }

  private void Start()
  {
    if (!this.isLocalPlayer)
      return;
    if (NetworkServer.active)
    {
      this.NewName();
      NineTailedFoxUnits.host = this;
    }
    else
      NineTailedFoxUnits.host = (NineTailedFoxUnits) null;
  }

  private void Update()
  {
  }

  public int NewName(out int number, out char letter)
  {
    int num = 0;
    string name;
    for (name = this.GenerateName(); this.list.Contains(name) && num < 100; name = this.GenerateName())
      ++num;
    letter = name.ToUpper()[0];
    number = int.Parse(name.Split('-')[1]);
    this.AddUnit(name);
    return this.list.Count - 1;
  }

  public int NewName()
  {
    return this.NewName(out int _, out char _);
  }

  public string GetNameById(int id)
  {
    return this.list[id];
  }

  public NineTailedFoxUnits()
  {
    this.list = new SyncListString();
    this.InitSyncObject((SyncObject) this.list);
  }

  private void MirrorProcessed()
  {
  }
}
