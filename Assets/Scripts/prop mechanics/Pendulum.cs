using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pendulum : NetworkBehaviour
{
	[SyncVar] public float speed = 1.5f;
    [SyncVar] public float limit = 75f; //Limit in degrees of the movement
    [SyncVar] public bool randomStart = false; //If you want to modify the start position
    [SyncVar] private float random = 0;

    // Start is called before the first frame update
    void Awake()
    {
		if(randomStart)
        {
            random = Random.Range(0f, 1f);
            OnServerRandom(random);
        }
			
	}

    // Update is called once per frame
    void Update()
    {
		float angle = limit * Mathf.Sin(Time.time + random * speed);
		transform.localRotation = Quaternion.Euler(0, 0, angle);
        OnServersyncMove();
	}

    [ServerCallback]
    void OnServerRandom(float rand)
    {
        random = rand;
    }

    [ServerCallback]
    void OnServersyncMove()
    {
        float angle = limit * Mathf.Sin(Time.time + random * speed);
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
