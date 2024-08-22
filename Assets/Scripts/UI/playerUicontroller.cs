using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class playerUicontroller : NetworkBehaviour
{
    public RawImage GT1;
    public RawImage RT1;
    public RawImage GT2;
    public RawImage RT2;
    public Image playerposthis;
    public Image playerposEnemy;
    public int team;

    // Start is called before the first frame update
    void Start()
    {
        GT1 = GameObject.Find("target green t1").GetComponent<RawImage>();
        RT1 = GameObject.Find("target red t1").GetComponent<RawImage>();
        GT2 = GameObject.Find("target green t2").GetComponent<RawImage>();
        RT2 = GameObject.Find("target red t2").GetComponent<RawImage>();
        updateUi();
    }

    // Update is called once per frame
    void Update()
    {
        team = gameObject.GetComponent<pickupmechanic>().team;
        updateUi(); 
    }

    [ClientCallback]
    void updateUi()
    {
        if (isLocalPlayer)
        {
            playerposEnemy.enabled = false;
            playerposthis.enabled = true;
            if (team == 1)
            {
                GT1.enabled = true;
                RT1.enabled = false;
                GT2.enabled = false;
                RT2.enabled = true;
            }
            else if (team == 2)
            {
                GT1.enabled = false;
                RT1.enabled = true;
                GT2.enabled = true;
                RT2.enabled = false;
            }
        }
    }
}
