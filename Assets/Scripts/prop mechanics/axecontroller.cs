using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class axecontroller : NetworkBehaviour
{
    [SyncVar] float timer;
    float rotationSpeed = 80.0f;
    [SyncVar] public bool ispicked;
    public Canvas canvas;
    public RawImage image;
    // Start is called before the first frame update
    void Start()
    {
        timer = 5f;
        ispicked = false;
    }
    void Update()
    {
        if (!isServer)
        {
            return;
        }

        canvas.GetComponent<Canvas>().worldCamera = GameObject.Find("minimapcam").GetComponent<Camera>();
        image.transform.position = transform.position;
        if (!ispicked)
        {
            timer -= Time.deltaTime;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            if (timer < 0)
            {
                CmdDestroyTool();
                destroytool();
            }
        }
        else
        {
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
