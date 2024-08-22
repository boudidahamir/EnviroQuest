using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class progressbar : MonoBehaviour
{
    public float maximum;
    public Image mask1;
    public Image mask2;
    public GameObject tree1;
    public GameObject tree2;
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public GameObject player;

    // Start is called before the first frame update
    private void Awake()
    {
        player = gameObject.transform.parent.gameObject;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        treeGrow[] trees = GameObject.FindObjectsOfType<treeGrow>();
        foreach (treeGrow t in trees)
        {
            if (t.GetComponent<NetworkMatch>().matchId == player.gameObject.GetComponent<NetworkMatch>().matchId)
                {
                    if (t.gameObject.name.Equals("TreeGrow1(Clone)"))
                    {
                        tree1 = t.gameObject;
                    }
                    if (t.gameObject.name.Equals("TreeGrow2(Clone)"))
                    {
                        tree2 = t.gameObject;
                    }
                }
                else
                { Debug.Log("matchcontroller NOT Found" + false); }
        }
    GetCurrentfill(); 
        
    }

    [ClientCallback]
    void GetCurrentfill()
    {
            if (player.GetComponent<pickupmechanic>().team == 1)
            {
                float fillamount1 = tree1.GetComponent<treeGrow>().total / maximum;
                mask1.fillAmount = fillamount1;


                float fillamount2 = tree2.GetComponent<treeGrow>().total / maximum;
                mask2.fillAmount = fillamount2;
                text1.text = "team 1";
                text2.text = "team 2";
            }

            if (player.GetComponent<pickupmechanic>().team == 2)
            {
                float fillamount1 = tree1.GetComponent<treeGrow>().total / maximum;
                mask2.fillAmount = fillamount1;

                float fillamount2 = tree2.GetComponent<treeGrow>().total / maximum;
                mask1.fillAmount = fillamount2;

                text1.text = "team 2";
                text2.text = "team 1";

            }
    }
}
