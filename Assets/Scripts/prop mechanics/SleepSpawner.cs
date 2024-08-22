using Mirror;
using UnityEngine;


public class SleepSpawner : NetworkBehaviour
{
    public Transform pos1;
    public Transform pos2;
    public Transform pos3;
    public Transform pos4;
    [SyncVar] public int random;
    [SyncVar] float timer;
    public GameObject sleepprefab;

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
                if (trap.GetComponent<NetworkMatch>().matchId == gameObject.GetComponent<NetworkMatch>().matchId && trap.gameObject.name.Equals("zzz display(Clone)"))
                {
                    found = true; break;
                }
            }

            if (found == false)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {

                    random = Random.Range(1, 4);
                    switch (random)
                    {
                        case 1:
                            {
                                GameObject sleep = Instantiate(sleepprefab, pos1.position, sleepprefab.transform.rotation);
                                sleep.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(sleep);

                                timer = 10f;
                                break;
                            }
                        case 2:
                            {
                                GameObject sleep = Instantiate(sleepprefab, pos2.position, sleepprefab.transform.rotation);
                                sleep.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(sleep);
                                timer = 10f;
                                break;
                            }
                        case 3:
                            {
                                GameObject sleep = Instantiate(sleepprefab, pos3.position, sleepprefab.transform.rotation);
                                sleep.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(sleep);
                                timer = 10f;
                                break;
                            }
                        case 4:
                            {
                                GameObject sleep = Instantiate(sleepprefab, pos4.position, sleepprefab.transform.rotation);
                                sleep.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                                NetworkServer.Spawn(sleep);
                                timer = 10f;
                                break;
                            }
                        default:
                            {
                                random = Random.Range(1, 5);
                                break;
                            }
                    }

                }
            }
        }
    }

}
