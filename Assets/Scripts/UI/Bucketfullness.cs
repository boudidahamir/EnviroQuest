using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Bucketfullness : NetworkBehaviour
{
    public float maximum;
    public Image mask1;
    public float fullness;
    [SyncVar] public float fillamount1;
    // Start is called before the first frame update
    private void Awake()
    {
        
    }
    void Start()
    {
        fullness=1;
        fillamount1=1;
    }

    // Update is called once per frame
    void Update()
    {
        GetCurrentfill();
        if(isServer)
            CmdGetCurrentfill();
    }

    void GetCurrentfill()
    {
            fillamount1 = fullness / maximum;
            mask1.fillAmount = fillamount1;
    }

    [ClientRpc]
    void CmdGetCurrentfill()
    {
        fillamount1 = fullness / maximum;
        mask1.fillAmount = fillamount1;
    }

}
