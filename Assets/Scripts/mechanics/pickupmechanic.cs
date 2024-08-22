using EasyPrimitiveAnimals;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class pickupmechanic : NetworkBehaviour
{
    public InputManager inputManager;
    [SyncVar] public bool poop;
    [SyncVar] public bool water;
    [SyncVar] public bool fence;
    [SyncVar] public bool sleep;
    public GameObject poopUi;
    public GameObject waterUi;
    public GameObject fenceUi;
    public GameObject sleepUi;
    [SyncVar] public int team;
    public GameObject trapprefab;
    public Canvas bucketfullnes;
    public GameObject Bucketbar;
    public Camera itemcam;

    // Start is called before the first frame update
    void Start()
    {

        poop = false;
        water = false;
        fence = false;
        sleep = false;

        if(isLocalPlayer)
        {
            GameObject.Find("Main Camera").GetComponent<Camera>().GetUniversalAdditionalCameraData().cameraStack.Add(itemcam);

        }
        else {
            gameObject.GetComponentInChildren<progressbar>().enabled = false;
            gameObject.GetComponentInChildren<progressbar>().gameObject.GetComponent<Canvas>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            if (poop)
                poopUi.SetActive(true);
            if (water)
            {
                Bucketbar.SetActive(true);
                waterUi.SetActive(true);
            }
            if (fence)
                fenceUi.SetActive(true);
            if (sleep )
                sleepUi.SetActive(true);

            if (bucketfullnes.GetComponent<Bucketfullness>().fullness <= 0)
            {
                Bucketbar.SetActive(false);
                waterUi.SetActive(false);
                water = false;
            }
        }

    }

    
    private void OnCollisionStay(Collision collision)
    {
            //pickup poop
            if (collision.gameObject.name.StartsWith("poop") && poop == false && water == false && fence == false && sleep == false)
            {
                if (inputManager.PlayerPick() && collision.gameObject.GetComponent<poopdespawn>().team == team)
                {
                    CmdPickupPoop(collision.gameObject);
                    
                    PickupPoop(collision.gameObject);
                }
            }

            //pickup water
            if (collision.gameObject.name.StartsWith("water") && poop == false && fence == false && sleep == false)
            {
                if (inputManager.PlayerPick())
                {
                    CmdPickupWater();
                    PickupWater();
                }
            }

            //pickup sleep
            if (collision.gameObject.name.StartsWith("zzz display") && poop == false && water == false && fence == false && sleep == false)
            {
                if (inputManager.PlayerPick())
                {
                    CmdPickupSleep(collision.gameObject);
                    PickupSleep(collision.gameObject);
                }
            }

            //pickup fence
            if (collision.gameObject.name.StartsWith("fencedisplay") && poop == false && water == false && fence == false && sleep == false)
            {
                if (inputManager.PlayerPick())
                {
                    CmdPickupFence(collision.gameObject);
                    PickupFence(collision.gameObject);
                }
            }

            //when team 1 tree grow
            if (collision.gameObject.name.StartsWith("TreeGrow1") && (poop || water) && team == 1)
            {
                if (inputManager.PlayerPick())
                {
                    CmdTreeGrow(collision.gameObject);
                    TreeGrow(collision.gameObject);
                }
            }

            //when team 1 tree sabotage
            if (collision.gameObject.name.StartsWith("TreeGrow2") && fence && team == 1)
            {
                if (inputManager.PlayerPick())
                {
                    SabotageTree(collision.gameObject, -20f);
                    ClientSabotageTree(-20f);
                }
            }

            //when team 2 tree grow
            if (collision.gameObject.name.StartsWith("TreeGrow2") && (poop || water) && team == 2)
            {
                if (inputManager.PlayerPick())
                {
                    CmdTreeGrow(collision.gameObject);
                    TreeGrow(collision.gameObject);
                }
            }

            //when team 2 tree sabotage
            if (collision.gameObject.name.StartsWith("TreeGrow1") && fence && team == 2)
            {
                if (inputManager.PlayerPick())
                {
                    SabotageTree(collision.gameObject,20f);
                    ClientSabotageTree(20);
                }
            }

            //sabotage cow
            if (collision.gameObject.name.StartsWith("Cow") && sleep && collision.gameObject.GetComponent<AnimalController>().team != team)
            {
                if (inputManager.PlayerPick())
                {
                    CmdSabotageCow(collision.gameObject);
                    SabotageCow(collision.gameObject);
                }
            }

            if (collision.gameObject.CompareTag("obstacle") && water && !gameObject.GetComponent<PlayerController>().canMove)
            {
                CmdBucketFullnessDrop();
                BucketFullnessDrop();
            }
        
    }

    #region pickupwater
    [Command]
    private void CmdPickupWater()
    {
        water = true;
        bucketfullnes.GetComponent<Bucketfullness>().fullness = 1f;
    }
    [ClientCallback]
    private void PickupWater()
    {
        water = true;
        bucketfullnes.GetComponent<Bucketfullness>().fullness = 1f;
    }
    #endregion

    #region bucketfullness Drop
    [Command]
    private void CmdBucketFullnessDrop()
    {
        bucketfullnes.GetComponent<Bucketfullness>().fullness -= 0.01f;
    }

    [ClientCallback]
    private void BucketFullnessDrop()
    {
        bucketfullnes.GetComponent<Bucketfullness>().fullness -= 0.01f;
    }
    #endregion

    #region pickup poop

    [Command]
    private void CmdPickupPoop(GameObject poopObject)
    {
        poop = true;
        if (poopObject != null && poopObject.GetComponent<poopdespawn>() != null)
        {
            poopObject.GetComponent<poopdespawn>().ispicked = true;
        }
        else
        {
            Debug.LogError("poopObject or poopdespawn component is null.");
        }
    }
    
    private void PickupPoop(GameObject poopObject)
    {
        poop = true;
        poopObject.GetComponent<poopdespawn>().ispicked = true;
    }
    #endregion

    #region pickupsleep
    [Command]
    private void CmdPickupSleep(GameObject sleepObject)
    {
        sleep = true;
        sleepObject.GetComponent<axecontroller>().ispicked = true;
    }

    private void PickupSleep(GameObject sleepObject)
    {
        sleep = true;
        sleepObject.GetComponent<axecontroller>().ispicked = true;
    }
    #endregion

    #region pickup Fence
    [Command]
    private void CmdPickupFence(GameObject fenceObject)
    {
        fence = true;
        fenceObject.GetComponent<axecontroller>().ispicked = true;
    }

    private void PickupFence(GameObject fenceObject)
    {
        fence = true;
        fenceObject.GetComponent<axecontroller>().ispicked = true;
    }
    #endregion

    #region sabotagecow
    [Command]
    private void CmdSabotageCow(GameObject cow)
    {
        cow.GetComponent<AnimalController>().issleeping = true;
        cow.GetComponent<Transform>().position = cow.gameObject.GetComponent<Transform>().position + new Vector3(0, 0.62f, 0);
        cow.GetComponent<Transform>().Rotate(0, 0, 90);
        sleepUi.SetActive(false);
        sleep = false;
    }
    private void SabotageCow(GameObject cow)
    {
        cow.GetComponent<AnimalController>().issleeping = true;
        cow.GetComponent<Transform>().position = cow.gameObject.GetComponent<Transform>().position + new Vector3(0, 0.62f, 0);
        cow.GetComponent<Transform>().Rotate(0, 0, 90);
        sleepUi.SetActive(false);
        sleep = false;
    }
    #endregion

    #region treegrow
    [Command]
    private void CmdTreeGrow(GameObject tree)
    {
        Debug.Log(tree.GetComponent<NetworkMatch>().matchId);
            if (poop)
            {
                if (tree.GetComponent<treeGrow>().poopcount != 8)
                {
                    tree.GetComponent<treeGrow>().poopcount++;
                }
                poopUi.SetActive(false);
                poop = false;
            }

            if (water)
            {
                
                if (tree.GetComponent<treeGrow>().watercount != 8)
                {
                    tree.GetComponent<treeGrow>().watercount += bucketfullnes.GetComponent<Bucketfullness>().fullness;
                }
                Bucketbar.SetActive(false);
                waterUi.SetActive(false);
                water = false;
            }
    }

    private void TreeGrow(GameObject tree)
    {
        if (poop)
        {
            if (tree.GetComponent<treeGrow>().poopcount != 8)
            {
                tree.GetComponent<treeGrow>().poopcount++;
            }
            poopUi.SetActive(false);
            poop = false;
        }

        if (water)
        {

            if (tree.GetComponent<treeGrow>().watercount != 8)
            {
                tree.GetComponent<treeGrow>().watercount += bucketfullnes.GetComponent<Bucketfullness>().fullness;
            }
            Bucketbar.SetActive(false);
            waterUi.SetActive(false);
            water = false;
        }
    }
    #endregion

    #region sabotagetree
    [Command]
    private void SabotageTree(GameObject tree,float offset)
    {
        if (fence)
        {
            Vector3 position = tree.transform.position;
            position.y = 1.35f;
            GameObject trap = Instantiate(trapprefab, position, Quaternion.identity);
            trap.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
            NetworkServer.Spawn(trap);
            transform.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
            fenceUi.SetActive(false);
            fence = false;
        }
    }

    private void ClientSabotageTree(float offset)
    {
        if (fence)
        {
            transform.position = new Vector3(transform.position.x + offset, transform.position.y, transform.position.z);
            fenceUi.SetActive(false);
            fence = false;
        }
    }
    #endregion

}
