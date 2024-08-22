using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class poopdespawn : NetworkBehaviour
{
    [SyncVar] float time;
    [SyncVar] public bool ispicked;
    [SyncVar] public int team;
    // Start is called before the first frame update
    void Start()
    {
        ispicked = false;
        time = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer)
        {
            return;
        }

        if (!ispicked)
        {
            time -= Time.deltaTime;
        }

        if(ispicked) 
        {
            CmdDestroyPoop();
            destroypoopClient();
        }

        if (time < 0 )
        {
            CmdDestroyPoop();
            destroypoopClient();
        }

    }

    [ServerCallback]
    private void CmdDestroyPoop()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void destroypoopClient()
    {
        Destroy(gameObject);
    }
}
