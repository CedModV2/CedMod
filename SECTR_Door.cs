// Decompiled with JetBrains decompiler
// Type: SECTR_Door
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

[RequireComponent(typeof (Animator))]
[AddComponentMenu("SECTR/Audio/SECTR Door")]
public class SECTR_Door : MonoBehaviour
{
  [SECTR_ToolTip("The name of the control param in the door.")]
  public string ControlParam = "Open";
  [SECTR_ToolTip("The name of the control param that indicates if we are allowed to open.")]
  public string CanOpenParam = "CanOpen";
  [SECTR_ToolTip("The full name (layer and state) of the Open state in the Animation Controller.")]
  public string OpenState = "Base Layer.Open";
  [SECTR_ToolTip("The full name (layer and state) of the Closed state in the Animation Controller.")]
  public string ClosedState = "Base Layer.Closed";
  [SECTR_ToolTip("The full name (layer and state) of the Opening state in the Animation Controller.")]
  public string OpeningState = "Base Layer.Opening";
  [SECTR_ToolTip("The full name (layer and state) of the Closing state in the Animation Controller.")]
  public string ClosingState = "Base Layer.Closing";
  [SECTR_ToolTip("The full name (layer and state) of the Wating state in the Animation Controller.")]
  public string WaitingState = "Base Layer.Waiting";
  private int controlParam;
  private int canOpenParam;
  private int closedState;
  private int waitingState;
  private int openingState;
  private int openState;
  private int closingState;
  private int lastState;
  private Animator cachedAnimator;
  private int openCount;
  [SECTR_ToolTip("The portal this door affects (if any).")]
  public SECTR_Portal Portal;

  public void OpenDoor()
  {
    ++this.openCount;
  }

  public void CloseDoor()
  {
    --this.openCount;
  }

  public bool IsFullyOpen()
  {
    return this.cachedAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == this.openState;
  }

  public bool IsClosed()
  {
    return this.cachedAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == this.closedState;
  }

  protected virtual void OnEnable()
  {
    this.cachedAnimator = this.GetComponent<Animator>();
    this.controlParam = Animator.StringToHash(this.ControlParam);
    this.canOpenParam = Animator.StringToHash(this.CanOpenParam);
    this.closedState = Animator.StringToHash(this.ClosedState);
    this.waitingState = Animator.StringToHash(this.WaitingState);
    this.openingState = Animator.StringToHash(this.OpeningState);
    this.openState = Animator.StringToHash(this.OpenState);
    this.closingState = Animator.StringToHash(this.ClosingState);
  }

  private void Start()
  {
    if (this.controlParam != 0)
      this.cachedAnimator.SetBool(this.controlParam, false);
    if (this.canOpenParam != 0)
      this.cachedAnimator.SetBool(this.canOpenParam, false);
    if ((bool) (Object) this.Portal)
      this.Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, true);
    this.openCount = 0;
    this.lastState = this.closedState;
    this.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
  }

  private void Update()
  {
    bool flag = this.CanOpen();
    if (this.canOpenParam != 0)
      this.cachedAnimator.SetBool(this.canOpenParam, flag);
    if (this.controlParam != 0 && (flag || this.canOpenParam != 0))
      this.cachedAnimator.SetBool(this.controlParam, this.openCount > 0);
    int fullPathHash = this.cachedAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    if (fullPathHash != this.lastState)
    {
      if (fullPathHash == this.closedState)
        this.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
      if (fullPathHash == this.waitingState)
        this.SendMessage("OnWaiting", SendMessageOptions.DontRequireReceiver);
      else if (fullPathHash == this.openingState)
        this.SendMessage("OnOpening", SendMessageOptions.DontRequireReceiver);
      if (fullPathHash == this.openState)
        this.SendMessage("OnOpen", SendMessageOptions.DontRequireReceiver);
      else if (fullPathHash == this.closingState)
        this.SendMessage("OnClosing", SendMessageOptions.DontRequireReceiver);
      this.lastState = fullPathHash;
    }
    if (!(bool) (Object) this.Portal)
      return;
    this.Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, this.IsClosed());
  }

  protected virtual void OnTriggerEnter(Collider other)
  {
    ++this.openCount;
  }

  protected virtual void OnTriggerExit(Collider other)
  {
    --this.openCount;
  }

  protected virtual bool CanOpen()
  {
    return true;
  }
}
