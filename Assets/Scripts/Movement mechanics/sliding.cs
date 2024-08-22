using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class sliding : MonoBehaviour
{
    private Transform spinningCylinder;
    public float movementSpeed = 4f;
    private Rigidbody rb;
    private Vector3 spinDirection;
    public Vector3 velocityslide;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        spinDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (spinDirection != Vector3.zero)
        {
            velocityslide = -spinDirection * movementSpeed;
        }
        else
            velocityslide= Vector3.zero;
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("RotatingPlatform"))
        {
            spinningCylinder = collision.gameObject.transform;

            //Debug.Log(spinningCylinder.rotation.eulerAngles.y);

            if (spinningCylinder.rotation.eulerAngles.y == 90 )
                spinDirection = new Vector3(0, 0, 1f);
            if (spinningCylinder.rotation.eulerAngles.y == 270)
                spinDirection = new Vector3(0, 0, -1f);
            if (spinningCylinder.rotation.eulerAngles.y == 0)
                spinDirection = new Vector3(-1f, 0, 0);
            if (spinningCylinder.rotation.eulerAngles.y == 180)
                spinDirection = new Vector3(1f, 0, 0);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("RotatingPlatform"))
        {
            spinDirection = Vector3.zero;
        }
    }
}
