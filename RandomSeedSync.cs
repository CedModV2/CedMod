// Decompiled with JetBrains decompiler
// Type: RandomSeedSync
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using GameCore;
using MEC;
using Mirror;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RandomSeedSync : NetworkBehaviour
{
  [SyncVar]
  public int seed = -1;
  private static int staticSeed;
  public bool mapGenerated;

  private void Start()
  {
    if (!this.isLocalPlayer || !NetworkServer.active)
      return;
    foreach (WorkStation workStation in Object.FindObjectsOfType<WorkStation>())
      workStation.Networkposition = new Offset()
      {
        position = workStation.transform.localPosition,
        rotation = workStation.transform.localRotation.eulerAngles,
        scale = Vector3.one
      };
    this.Networkseed = ConfigFile.ServerConfig.GetInt("map_seed", -1);
    while (NetworkServer.active && this.seed == -1)
      this.Networkseed = Random.Range(-999999999, 999999999);
  }

  private void Update()
  {
    if (this.seed == -1 || this.mapGenerated || !(this.name == "Host"))
      return;
    RandomSeedSync.staticSeed = this.seed;
    RandomSeedSync.DebugInfo("Seed received (" + (object) RandomSeedSync.staticSeed + ")", false, MessageImportance.Normal);
    this.mapGenerated = true;
    if (RandomSeedSync.GenerateLevel())
      this.Invoke("RefreshBounds", 10f);
    else
      Timing.RunCoroutine(this._RetryGeneration(), Segment.FixedUpdate);
  }

  private IEnumerator<float> _RetryGeneration()
  {
    RandomSeedSync randomSeedSync = this;
    for (int i = 1; i <= 10; ++i)
    {
      for (int w = 0; w < 100; ++w)
        yield return 0.0f;
      RandomSeedSync.DebugInfo("<color=red>Retrying... (Attempt " + (object) i + "/10)</color>", false, MessageImportance.MostImportant);
      if (RandomSeedSync.GenerateLevel())
      {
        randomSeedSync.Invoke("RefreshBounds", 10f);
        yield break;
      }
    }
    Debug.LogError((object) "Map generator fatal error. Could not find 'ImageGenerator'. Debug info:");
    GameObject gameObject = GameObject.Find("GameManager");
    RandomSeedSync.DebugInfo("<color=red>Game Manager: </color>" + ((Object) gameObject == (Object) null ? "[MISSING]" : "[FOUND]"), false, MessageImportance.MostImportant);
    if ((Object) gameObject != (Object) null)
    {
      RandomSeedSync.DebugInfo("<color=red>All Components: </color>" + (object) gameObject.GetComponents(typeof (Component)).Length, false, MessageImportance.MostImportant);
      RandomSeedSync.DebugInfo("<color=red>Image Generator Components: </color>" + (object) gameObject.GetComponents<ImageGenerator>().Length, false, MessageImportance.MostImportant);
    }
  }

  public static void DebugError(string error, bool isWarning = false)
  {
    RandomSeedSync.DebugInfo((isWarning ? "<color=orange>WARNING: " : "<color=red>FATAL ERROR: ") + error + "</color>", true, MessageImportance.MostImportant);
  }

  public static void DebugInfo(string txt, bool nospace, MessageImportance importance)
  {
    Console.AddDebugLog("MAPGEN", txt, importance, nospace);
  }

  private static void PrintExplanation(string zone)
  {
    RandomSeedSync.DebugError(zone + " procedural generation failed. Could not start the ImageGenerator method.", false);
  }

  private static bool GenerateLevel()
  {
    RandomSeedSync.DebugInfo("Level generator sequence starting...", false, MessageImportance.MostImportant);
    if (!NonFacilityCompatibility.currentSceneSettings.enableWorldGeneration)
    {
      RandomSeedSync.DebugInfo("World generation has been disabled for that scene.", true, MessageImportance.Normal);
      return true;
    }
    RandomSeedSync.DebugInfo("Generating zones...", false, MessageImportance.Normal);
    ImageGenerator zoneGenerator1 = ImageGenerator.ZoneGenerators[0];
    ImageGenerator zoneGenerator2 = ImageGenerator.ZoneGenerators[1];
    ImageGenerator zoneGenerator3 = ImageGenerator.ZoneGenerators[2];
    if ((Object) zoneGenerator1 != (Object) null)
    {
      string blackbox1;
      if (!zoneGenerator1.GenerateMap(RandomSeedSync.staticSeed, "LC", out blackbox1))
        RandomSeedSync.DebugError("LC procedural generation failed. Restoring the last blackbox message: \"" + blackbox1 + "\"", false);
      else
        RandomSeedSync.DebugInfo("LC procedural generation process finished.", true, MessageImportance.LessImportant);
      if ((Object) zoneGenerator2 != (Object) null)
      {
        string blackbox2;
        if (!zoneGenerator2.GenerateMap(RandomSeedSync.staticSeed + 1, "HC", out blackbox2))
          RandomSeedSync.DebugError("HC procedural generation failed. Restoring the last blackbox message: \"" + blackbox2 + "\"", false);
        else
          RandomSeedSync.DebugInfo("HC procedural generation process finished.", true, MessageImportance.LessImportant);
        if ((Object) zoneGenerator3 != (Object) null)
        {
          string blackbox3;
          if (!zoneGenerator3.GenerateMap(RandomSeedSync.staticSeed + 2, "EZ", out blackbox3))
            RandomSeedSync.DebugError("EZ procedural generation failed. Restoring the last blackbox message: \"" + blackbox3 + "\"", false);
          else
            RandomSeedSync.DebugInfo("EZ procedural generation process finished.", true, MessageImportance.LessImportant);
          RandomSeedSync.DebugInfo("Setting up doors...", true, MessageImportance.Normal);
          foreach (Door door in Object.FindObjectsOfType<Door>())
            door.UpdatePos();
          RandomSeedSync.DebugInfo("Adjusting the position of wall-mounted elements...", true, MessageImportance.LeastImportant);
          foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("DoorButton"))
          {
            try
            {
              gameObject.GetComponent<ButtonWallAdjuster>().Adjust();
              foreach (MonoBehaviour componentsInChild in gameObject.GetComponentsInChildren<ButtonWallAdjuster>())
                componentsInChild.Invoke("Adjust", 4f);
            }
            catch
            {
            }
          }
          RandomSeedSync.DebugInfo("Setting up lifts...", true, MessageImportance.Normal);
          foreach (Lift lift in Object.FindObjectsOfType<Lift>())
          {
            foreach (Lift.Elevator elevator in lift.elevators)
              elevator.SetPosition();
          }
          RandomSeedSync.DebugInfo("Searching for destroyed doors...", true, MessageImportance.LeastImportant);
          foreach (Door door in Object.FindObjectsOfType<Door>())
          {
            if (door.destroyed)
            {
              door.DestroyDoor(true);
            }
            else
            {
              door.SetActiveStatus(1);
              door.SetActiveStatus(0);
            }
            door.SetState(door.isOpen);
          }
          RandomSeedSync.DebugInfo("Setting up decorations...", true, MessageImportance.Normal);
          foreach (ClutterSpawner clutterSpawner in Object.FindObjectsOfType<ClutterSpawner>())
            clutterSpawner.GenerateClutter();
          if (NetworkServer.active)
          {
            RandomSeedSync.DebugInfo("Spawning items... [SERVER ONLY]", true, MessageImportance.Normal);
            PlayerManager.localPlayer.GetComponent<HostItemSpawner>().Spawn(RandomSeedSync.staticSeed);
          }
          RandomSeedSync.DebugInfo("Pre-baking terrain...", true, MessageImportance.LeastImportant);
          foreach (SECTR_Member sectrMember in Object.FindObjectsOfType<SECTR_Member>())
            sectrMember.UpdateViaScript();
          RandomSeedSync.DebugInfo("Setting up LCZ room navigation labels...", true, MessageImportance.LessImportant);
          Object.FindObjectOfType<LCZ_LabelManager>().RefreshLabels();
          RandomSeedSync.DebugInfo("Preparing interactable elements for SCP-079...", true, MessageImportance.LessImportant);
          foreach (Scp079Interactable scp079Interactable in Object.FindObjectsOfType<Scp079Interactable>())
            scp079Interactable.OnMapGenerate();
          Interface079.singleton.RefreshInteractables();
          RandomSeedSync.DebugInfo("Setting up lockers...", true, MessageImportance.Normal);
          if ((Object) LockerManager.singleton == (Object) null)
          {
            RandomSeedSync.DebugError("Setting up lockers failed.", false);
            return false;
          }
          LockerManager.singleton.Generate(RandomSeedSync.staticSeed + 3);
          RandomSeedSync.DebugInfo("Sequence of procedural level generation completed.", false, MessageImportance.MostImportant);
          return true;
        }
        RandomSeedSync.PrintExplanation("EZ");
        return false;
      }
      RandomSeedSync.PrintExplanation("HC");
      return false;
    }
    RandomSeedSync.PrintExplanation("LC");
    return false;
  }

  private void RefreshBounds()
  {
    foreach (SECTR_Member sectrMember in Object.FindObjectsOfType<SECTR_Member>())
      sectrMember.UpdateViaScript();
    RandomSeedSync.DebugInfo("Terrain baking complete.", false, MessageImportance.LeastImportant);
  }

  private void MirrorProcessed()
  {
  }

  public int Networkseed
  {
    get
    {
      return this.seed;
    }
    [param: In] set
    {
      this.SetSyncVar<int>(value, ref this.seed, 1UL);
    }
  }

  public override bool OnSerialize(NetworkWriter writer, bool forceAll)
  {
    bool flag = base.OnSerialize(writer, forceAll);
    if (forceAll)
    {
      writer.WritePackedInt32(this.seed);
      return true;
    }
    writer.WritePackedUInt64(this.syncVarDirtyBits);
    if (((long) this.syncVarDirtyBits & 1L) != 0L)
    {
      writer.WritePackedInt32(this.seed);
      flag = true;
    }
    return flag;
  }

  public override void OnDeserialize(NetworkReader reader, bool initialState)
  {
    base.OnDeserialize(reader, initialState);
    if (initialState)
    {
      this.Networkseed = reader.ReadPackedInt32();
    }
    else
    {
      if (((long) reader.ReadPackedUInt64() & 1L) == 0L)
        return;
      this.Networkseed = reader.ReadPackedInt32();
    }
  }
}
