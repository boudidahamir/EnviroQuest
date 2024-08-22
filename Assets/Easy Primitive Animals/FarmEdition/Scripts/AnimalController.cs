using Mirror;
using System.Collections;
using UnityEngine;

namespace EasyPrimitiveAnimals
{
    public class AnimalController : NetworkBehaviour
    {
        public GameObject FrontLegL;
        public GameObject FrontLegR;
        public GameObject RearLegL;
        public GameObject RearLegR;
        public GameObject prefabpoop;


        private Vector3 legStartPosA = new Vector3(10.0f, 0f, 0f);
        private Vector3 legEndPosA = new Vector3(-10.0f, 0f, 0f);

        private Vector3 legStartPosB = new Vector3(-10.0f, 0f, 0f);
        private Vector3 legEndPosB = new Vector3(10.0f, 0f, 0f);

        private float rotSpeed;

        public float moveAngle = 90f;
        public float movSpeed = 1f;

        [SyncVar] private bool canRotate = true;
        [SyncVar] private bool canPeck = true;
        [SyncVar] private float pooptimer;
        [SyncVar] public int team;
        [SyncVar] public bool issleeping=false;
        [SyncVar] private float sleepingtimer=5f;
        public GameObject zzz;
        public Material lefteyeball;
        public Material righteyeball;
        public GameObject Reye;
        public GameObject Leye;

        private void Start()
        {
            if(gameObject.name.StartsWith("cow"))
                zzz = transform.GetChild(1).gameObject;

            if (isServer)
            {
                pooptimer = Random.Range(5, 10);
                if (!this.gameObject.CompareTag("Chicken"))
                {
                    FrontLegL = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_FL").gameObject;
                    FrontLegR = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_FR").gameObject;
                    RearLegL = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_RL").gameObject;
                    RearLegR = transform.Find("BaseAnimal").transform.Find("Legs").transform.Find("EPA_RR").gameObject;

                    rotSpeed = movSpeed * 4;
                }
            }
        }

        private void Update()
        {
            if (!isServer)
                return;

                if (!this.gameObject.CompareTag("Chicken"))
                {
                    if (!this.gameObject.CompareTag("Sheep"))
                    {
                        if(zzz != null)
                        {
                            if (issleeping)
                            {
                                zzz.SetActive(true);
                            }
                            else
                                zzz.SetActive(false);
                        }

                        setzzzactive();
                    }

                    if (!issleeping)
                    {
                        Quaternion legAngleFromA = Quaternion.Euler(this.legStartPosA);
                        Quaternion legAngleToA = Quaternion.Euler(this.legEndPosA);
                        Quaternion legAngleFromB = Quaternion.Euler(this.legStartPosB);
                        Quaternion legAngleToB = Quaternion.Euler(this.legEndPosB);

                        float lerp = 0.5f * (1.0f + Mathf.Sin(Mathf.PI * Time.realtimeSinceStartup * this.rotSpeed));

                        FrontLegL.transform.localRotation = Quaternion.Lerp(legAngleFromA, legAngleToA, lerp);
                        FrontLegR.transform.localRotation = Quaternion.Lerp(legAngleFromB, legAngleToB, lerp);

                        RearLegL.transform.localRotation = Quaternion.Lerp(legAngleFromB, legAngleToB, lerp);
                        RearLegR.transform.localRotation = Quaternion.Lerp(legAngleFromA, legAngleToA, lerp);


                        transform.Translate((Vector3.forward * Time.deltaTime) * movSpeed);
                    }
                }
                else
                {
                    if (Random.Range(0, 100) > 50 && canPeck)
                    {
                        StartCoroutine(TimeToPeck());
                    }
                }

                if (this.gameObject.CompareTag("Cow") && !issleeping)
                {
                    pooptimer -= Time.deltaTime;
                    if (pooptimer < 0)
                    {
                        GameObject poop = Instantiate(prefabpoop, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.1f, gameObject.transform.position.z), prefabpoop.transform.rotation);
                        poop.GetComponent<NetworkMatch>().matchId = gameObject.GetComponent<NetworkMatch>().matchId;
                        NetworkServer.Spawn(poop);
                        pooptimer = Random.Range(5, 10);
                        if (team == 1)
                        {
                            poop.GetComponent<poopdespawn>().team = 1;
                        }
                        if (team == 2)
                        {
                            poop.GetComponent<poopdespawn>().team = 2;
                        }
                    }
                }

                if (issleeping)
                {
                    canRotate = false;
                    Reye.GetComponent<Renderer>().material = righteyeball;
                    Leye.GetComponent<Renderer>().material = lefteyeball;
                    sleepingtimer -= Time.deltaTime;
                    if (sleepingtimer < 0)
                    {
                        gameObject.transform.Rotate(0, 0, -90);
                        gameObject.transform.position = gameObject.transform.position - new Vector3(0, 0.62f, 0);
                        Reye.GetComponent<Renderer>().material = lefteyeball;
                        Leye.GetComponent<Renderer>().material = righteyeball;
                        sleepingtimer = 5f;
                        issleeping = false;
                        sleepsyncro(issleeping);
                        canRotate = true;
                    }
                }
            
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Ground") && canRotate) 
            {
                StartCoroutine(SpinMeRound());
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if((collision.gameObject.CompareTag("fence") || collision.gameObject.CompareTag("farmwall")) && canRotate)
            {
                StartCoroutine(SpinMeRound());
            }
        }

        private IEnumerator SpinMeRound()
        {
            
            canRotate = false;
            this.transform.rotation *= Quaternion.Euler(0, moveAngle, 0);
            yield return new WaitForSeconds(1f);
            canRotate = true;
        }

        private IEnumerator TimeToPeck()
        {
            canPeck = false;

            this.transform.eulerAngles = new Vector3(45f, transform.eulerAngles.y, transform.eulerAngles.z);

            yield return new WaitForSeconds(0.2f);

            this.transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, transform.eulerAngles.z);

            yield return new WaitForSeconds(Random.Range(3f, 7f));

            canPeck = true;
        }

        [ClientRpc]
        void setzzzactive()
        {
            if (zzz != null)
            {
                if (issleeping)
                {
                    zzz.SetActive(true);
                }
                else
                    zzz.SetActive(false);
            }
        }

        [ClientRpc]
        void sleepsyncro(bool sleep)
        {
            issleeping=sleep;
        }
    }

    

    
}
