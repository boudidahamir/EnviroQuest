using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trapcontroller : NetworkBehaviour
{
    [SyncVar] float timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 5f;
    }
    void Update()
    {
        if(!isServer) { return; }

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 5f;
            CmdDestroyTool();
            destroytool();
        }
    }

    [ServerCallback]
    private void CmdDestroyTool()
    {
        NetworkServer.Destroy(gameObject);
    }
    [ClientRpc]
    private void destroytool()
    {
        Destroy(gameObject);
    }
}
