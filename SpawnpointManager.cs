// Decompiled with JetBrains decompiler
// Type: SpawnpointManager
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

public class SpawnpointManager : MonoBehaviour
{
  public GameObject GetRandomPosition(RoleType classID)
  {
    GameObject gameObject = (GameObject) null;
    Role role = GameObject.Find("Host").GetComponent<CharacterClassManager>().Classes.SafeGet(classID);
    if (classID == RoleType.Scp0492)
      return (GameObject) null;
    switch (role.team)
    {
      case Team.SCP:
        switch (classID)
        {
          case RoleType.Scp106:
            GameObject[] gameObjectsWithTag1 = GameObject.FindGameObjectsWithTag("SP_106");
            int index1 = Random.Range(0, gameObjectsWithTag1.Length);
            gameObject = gameObjectsWithTag1[index1];
            break;
          case RoleType.Scp049:
            GameObject[] gameObjectsWithTag2 = GameObject.FindGameObjectsWithTag("SP_049");
            int index2 = Random.Range(0, gameObjectsWithTag2.Length);
            gameObject = gameObjectsWithTag2[index2];
            break;
          case RoleType.Scp079:
            GameObject[] gameObjectsWithTag3 = GameObject.FindGameObjectsWithTag("SP_079");
            int index3 = Random.Range(0, gameObjectsWithTag3.Length);
            gameObject = gameObjectsWithTag3[index3];
            break;
          case RoleType.Scp096:
            GameObject[] gameObjectsWithTag4 = GameObject.FindGameObjectsWithTag("SCP_096");
            int index4 = Random.Range(0, gameObjectsWithTag4.Length);
            gameObject = gameObjectsWithTag4[index4];
            break;
          case RoleType.Scp93953:
          case RoleType.Scp93989:
            GameObject[] gameObjectsWithTag5 = GameObject.FindGameObjectsWithTag("SCP_939");
            int index5 = Random.Range(0, gameObjectsWithTag5.Length);
            gameObject = gameObjectsWithTag5[index5];
            break;
          default:
            GameObject[] gameObjectsWithTag6 = GameObject.FindGameObjectsWithTag("SP_173");
            int index6 = Random.Range(0, gameObjectsWithTag6.Length);
            gameObject = gameObjectsWithTag6[index6];
            break;
        }
        break;
      case Team.MTF:
        GameObject[] gameObjectsWithTag7 = GameObject.FindGameObjectsWithTag(classID == RoleType.FacilityGuard ? "SP_GUARD" : "SP_MTF");
        int index7 = Random.Range(0, gameObjectsWithTag7.Length);
        gameObject = gameObjectsWithTag7[index7];
        break;
      case Team.CHI:
        GameObject[] gameObjectsWithTag8 = GameObject.FindGameObjectsWithTag("SP_CI");
        int index8 = Random.Range(0, gameObjectsWithTag8.Length);
        gameObject = gameObjectsWithTag8[index8];
        break;
      case Team.RSC:
        GameObject[] gameObjectsWithTag9 = GameObject.FindGameObjectsWithTag("SP_RSC");
        int index9 = Random.Range(0, gameObjectsWithTag9.Length);
        gameObject = gameObjectsWithTag9[index9];
        break;
      case Team.CDP:
        GameObject[] gameObjectsWithTag10 = GameObject.FindGameObjectsWithTag("SP_CDP");
        int index10 = Random.Range(0, gameObjectsWithTag10.Length);
        gameObject = gameObjectsWithTag10[index10];
        break;
      case Team.TUT:
        gameObject = GameObject.Find("TUT Spawn");
        break;
    }
    return gameObject;
  }
}
