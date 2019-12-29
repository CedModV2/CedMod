// Decompiled with JetBrains decompiler
// Type: RemoteAdmin.LargeDataPrinter
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using ZXing;

namespace RemoteAdmin
{
  public class LargeDataPrinter : MonoBehaviour
  {
    private const int Size = 700;
    internal static LargeDataPrinter Singleton;
    private static BarcodeWriter _barcodeWriter;
    public GameObject Panel;

    public void OnEnable()
    {
    }

    private void Update()
    {
    }
  }
}
