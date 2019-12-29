// Decompiled with JetBrains decompiler
// Type: GameCore.RoundStart
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using MEC;
using Mirror;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GameCore
{
  public class RoundStart : NetworkBehaviour
  {
    [SyncVar]
    public short Timer = -2;
    internal static readonly Stopwatch RoundStartTimer = new Stopwatch();
    public static RoundStart singleton;
    public static bool RoundJustStarted;
    public static bool LobbyLock;
    private bool _antiNoclassStarted;

    public static TimeSpan RoundLenght
    {
      get
      {
        return RoundStart.RoundStartTimer.Elapsed;
      }
    }

    internal static bool AntiNoclass { get; private set; }

    static RoundStart()
    {
      SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(RoundStart.OnSceneLoaded);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      RoundStart.RoundStartTimer.Reset();
    }

    private void Start()
    {
      if (this.isServer)
        RoundStart.RoundJustStarted = true;
      this.GetComponent<RectTransform>().localPosition = Vector3.zero;
    }

    private IEnumerator<float> _AntiNonclass()
    {
      for (ushort i = 0; i < (ushort) 500; ++i)
        yield return float.NegativeInfinity;
      RoundStart.AntiNoclass = true;
    }

    private IEnumerator<float> AntiFloorStuck()
    {
      for (byte i = 0; i < (byte) 250; ++i)
        yield return 0.0f;
      RoundStart.RoundJustStarted = false;
    }

    private void Awake()
    {
      RoundStart.singleton = this;
      RoundStart.AntiNoclass = false;
      if (!this.isServer)
        return;
      RoundStart.RoundJustStarted = true;
    }

    private void Update()
    {
    }

    private void FixedUpdate()
    {
      if (!NetworkServer.active || this._antiNoclassStarted || this.Timer != (short) -1)
        return;
      this._antiNoclassStarted = true;
      RoundStart.RoundJustStarted = true;
      Timing.RunCoroutine(this._AntiNonclass(), Segment.FixedUpdate);
      Timing.RunCoroutine(this.AntiFloorStuck(), Segment.FixedUpdate);
    }

    private void MirrorProcessed()
    {
    }

    public short NetworkTimer
    {
      get
      {
        return this.Timer;
      }
      [param: In] set
      {
        this.SetSyncVar<short>(value, ref this.Timer, 1UL);
      }
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
      bool flag = base.OnSerialize(writer, forceAll);
      if (forceAll)
      {
        writer.WriteInt16(this.Timer);
        return true;
      }
      writer.WritePackedUInt64(this.syncVarDirtyBits);
      if (((long) this.syncVarDirtyBits & 1L) != 0L)
      {
        writer.WriteInt16(this.Timer);
        flag = true;
      }
      return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
      base.OnDeserialize(reader, initialState);
      if (initialState)
      {
        this.NetworkTimer = reader.ReadInt16();
      }
      else
      {
        if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
          return;
        this.NetworkTimer = reader.ReadInt16();
      }
    }
  }
}
