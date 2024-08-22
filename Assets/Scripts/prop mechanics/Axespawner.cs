
using UnityEngine;
using Mirror;


public class Axespawner : NetworkBehaviour
{
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    [SyncVar] float timer;
    [SyncVar] public int random;
    public GameObject axeprefab;
    // Start is called before the first frame update
    void Start()
    {
        timer = 10f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {

            bool found = false;
            axecontroller[] traps = GameObject.FindObjectsOfType<axecontroller>();

            foreach (axecontroller trap in traps)
            {
                if (trap.GetComponent<NetworkMatch>().matchId == gameObject.GetComponent<NetworkMatch>().matchId && trap.gameObject.name.Equals("fencedisplay(Clone)"))
                {
                    found = true; break;
                }
            }
            if (!found)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {

                    random = Random.Range(1, 4);
                    switch (random)
                    {
                        case 1:
                            {
                                GameObject axe = Instantiate(axeprefab, pos1.position, axeprefab.transform.rotation);
                                axe.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(axe);
                                timer = 10f;
                                break;
                            }
                        case 2:
                            {
                                GameObject axe = Instantiate(axeprefab, pos2.position, axeprefab.transform.rotation);
                                axe.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(axe);
                                timer = 10f;
                                break;
                            }
                        case 3:
                            {
                                GameObject axe = Instantiate(axeprefab, pos3.position, axeprefab.transform.rotation);
                                axe.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(axe);
                                timer = 10f;
                                break;
                            }
                        default:
                            {
                                random = Random.Range(1, 4);
                                break;
                            }
                    }

                }
            }
        }
    }
}
