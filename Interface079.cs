// Decompiled with JetBrains decompiler
// Type: Interface079
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interface079 : MonoBehaviour
{
  public float targetFov = 60f;
  public List<Interface079.Overcon> overconTemplates = new List<Interface079.Overcon>();
  public List<Interface079.Overcon> overconInstances = new List<Interface079.Overcon>();
  private KeyCode[] movementKeys = new KeyCode[4];
  public bool startupAnimation = true;
  public AnimationCurve overconScaleOverDistance = AnimationCurve.Constant(0.0f, 1f, 1f);
  private Queue<string> bigNotificationQueue = new Queue<string>();
  private Queue<string> smallNotificationQueue = new Queue<string>();
  public Camera cameraNormal;
  public Scp079Interactable[] allInteractables;
  public GameObject[] quicktpKeys;
  private KeyCode changeModeKey;
  private KeyCode movementModeKey;
  private string lockedDoorInfo;
  public GameObject overlayCamFeed;
  public GameObject overlaySurvMap;
  public GameObject overlayStartup;
  public GameObject overlaySpeaker;
  public bool mapEnabled;
  public float transitionInProgress;
  public static Interface079 singleton;
  public static Scp079PlayerScript lply;
  public Camera079 defaultCamera;
  public LayerMask roomDetectionMask;
  public LayerMask overconMask;
  public RectTransform minimapTransform;
  public RectTransform minimapOperationalTransform;
  public RectTransform minimapToolbar;
  [Tooltip("0 - HCZ | 1 - LCZ | 2 - EZ")]
  public RectTransform[] minimapsForZones;
  public AnimationCurve notificationVisibilityCurve;
  public GameObject warning;
  public bool mouseLookMode;
  [HideInInspector]
  public Quaternion prevCamRotation;
  private static bool debugMode;
  private int warns;
  private float remtim;

  private void LateUpdate()
  {
    this.UpdateSpeaker();
  }

  private void UpdateSpeaker()
  {
    if (!string.IsNullOrEmpty(Interface079.lply.Speaker))
    {
      this.overlaySpeaker.SetActive(true);
      this.overlaySurvMap.SetActive(false);
      this.overlayCamFeed.SetActive(false);
    }
    else
    {
      if (!this.overlaySpeaker.activeSelf)
        return;
      this.overlaySpeaker.SetActive(false);
      this.overlayCamFeed.SetActive(true);
    }
  }

  public void UseButton(int actionID)
  {
    if (actionID < 4 && !this.mapEnabled)
    {
      if ((UnityEngine.Object) Interface079.lply.nearestCameras[actionID] == (UnityEngine.Object) null)
        return;
      this.transitionInProgress = 10f;
      this.SwitchToCamera(Interface079.lply.nearestCameras[actionID].cameraId, true);
    }
    else if (actionID != 4)
    {
      if (actionID != 5)
        return;
      Interface079.lply.CmdInteract("STOPSPEAKER:", (GameObject) null);
    }
    else
      this.SwitchMode();
  }

  private void SwitchToCamera(ushort id, bool lookatRotation)
  {
    this.prevCamRotation = Interface079.lply.currentCamera.head.transform.rotation;
    GameCore.Console.AddDebugLog("SCP079", "Sent a camera change request: " + (object) id, MessageImportance.Normal, false);
    Interface079.lply.CmdSwitchCamera(id, lookatRotation);
  }

  public void SwitchMode()
  {
  }

  public void AddOvercon(
    int id,
    string name,
    Vector3 position,
    string info,
    Color colorA,
    Color colorE,
    string[] abilityNames = null,
    int[] ids = null,
    bool[] blockers = null)
  {
  }

  private void Awake()
  {
    Interface079.singleton = this;
    this.gameObject.SetActive(false);
  }

  private void Start()
  {
    this.cameraNormal.gameObject.SetActive(true);
    if (ServerStatic.IsDedicated)
      return;
    for (int index1 = 0; index1 < 4; ++index1)
    {
      TextMeshProUGUI componentsInChild = this.quicktpKeys[index1].GetComponentsInChildren<TextMeshProUGUI>(true)[1];
      KeyCode[] movementKeys = this.movementKeys;
      int index2 = index1;
      string[] strArray = new string[4]
      {
        "Move Forward",
        "Move Left",
        "Move Right",
        "Move Backward"
      };
      int key;
      KeyCode keyCode = (KeyCode) (key = (int) NewInput.GetKey(strArray[index1]));
      movementKeys[index2] = (KeyCode) key;
      string str = keyCode.ToString();
      componentsInChild.text = str;
    }
  }

  public void RefreshInteractables()
  {
    this.allInteractables = UnityEngine.Object.FindObjectsOfType<Scp079Interactable>();
  }

  public void NotifyNewLevel(int newLvl)
  {
    GameCore.Console.AddDebugLog("SCP079", "User has achieved a level of id " + (object) newLvl, MessageImportance.LeastImportant, false);
    this.bigNotificationQueue.Enqueue("NEW ACCESS TIER\nUNLOCKED");
  }

  public void NotifyMoreExp(string info)
  {
    this.smallNotificationQueue.Enqueue(info);
  }

  public void NotifyNotEnoughMana(int required, int cur)
  {
    GameCore.Console.AddDebugLog("SCP079", "Not enough mana (" + (object) cur + "/" + (object) required + ")", MessageImportance.LessImportant, false);
  }

  public void AddBigNotification(string text)
  {
    this.bigNotificationQueue.Enqueue(text);
  }

  private void ShowBigNotification(string text)
  {
  }

  private void ShowSmallNotification(string text)
  {
  }

  private void UpdateCameraPostProcessing()
  {
  }

  [Serializable]
  public class Overcon
  {
  }
}
