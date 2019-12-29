using System;
using System.Collections;
using UnityEngine;

// Token: 0x02000139 RID: 313
public class DecalDestroyer : MonoBehaviour
{
    // Token: 0x06000791 RID: 1937 RVA: 0x0000E2A6 File Offset: 0x0000C4A6
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(this.lifeTime);
        UnityEngine.Object.Destroy(base.gameObject);
        yield break;
    }

    // Token: 0x040008EF RID: 2287
    public float lifeTime = 5f;
}
