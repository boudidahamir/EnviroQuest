using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class treeGrow : NetworkBehaviour
{
    [SyncVar] public float total;
    [SyncVar] public int poopcount;
    [SyncVar] public float watercount;
    void Start()
    {
        poopcount = 0;
        watercount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            total = (poopcount + watercount);
            if (total >= 1) { gameObject.transform.localScale = new Vector3(total, total, total); }
            RpcUpdateStat();
        }
    }

    [ServerCallback]
    void UpdateStat()
    {
        total = (poopcount + watercount);
    }

    [ClientRpc]
    void RpcUpdateStat()
    {
        total = (poopcount + watercount);
    }
}
