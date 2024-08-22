using Cinemachine;
using Mirror;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class PlayerController : NetworkBehaviour
{
    private Rigidbody rb;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 5.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    public InputManager inputManager;
    private Transform cameraTransform;
    public Transform camposition;
    public Animator animator;
    private float pushForce;
    private Vector3 pushDir;
    private bool slide;
    private bool wasStuned;
    [SyncVar] public bool canMove = true;
    private bool isStuned;
    public bool endgame = false;




    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        cameraTransform = Camera.main.transform;
        if (!isLocalPlayer)
        {
            gameObject.GetComponentInChildren<CinemachineVirtualCamera>().enabled = false;
            gameObject.GetComponentInChildren<CMVextension>().enabled = false;
            gameObject.GetComponentInChildren<Camera>().enabled = false;
            gameObject.GetComponentInChildren<CinemachineBrain>().enabled = false;
        }
    }
    void Update()
    {
        if (isLocalPlayer)
        {
            if (!endgame)
            {
                if (canMove)
                {
                    animator.SetBool("isdead", false);
                    if (groundedPlayer && playerVelocity.y < 0)
                    {
                        playerVelocity.y = 0f;
                    }

                    Vector2 movement = inputManager.GetPlayerMovement();
                    Vector3 move = new Vector3(movement.x, 0f, movement.y);
                    move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
                    move.y = 0f;

                    if (move != Vector3.zero)
                    {
                        animator.SetBool("isrunning", true);
                        transform.forward = move;

                        if (movement.y > 0f)
                        {
                            camposition.localPosition = new Vector3(0f, 4f, -5f);
                        }
                        else if (movement.y < 0f)
                        {
                            camposition.localPosition = new Vector3(0f, 4f, 5f);
                        }
                        if (movement.x < 0f)
                        {
                            camposition.localPosition = new Vector3(-5f, 4f, 0f);
                        }
                        else if (movement.x > 0f)
                        {
                            camposition.localPosition = new Vector3(5f, 4f, 0f);
                        }

                        if (movement.y > 0f && movement.x < 0f)
                        {
                            camposition.localPosition = new Vector3(-3.5f, 4f, -3.5f);
                        }
                        else if (movement.y > 0f && movement.x > 0f)
                        {
                            camposition.localPosition = new Vector3(3.5f, 4f, -3.5f);
                        }

                        if (movement.y < 0f && movement.x < 0f)
                        {
                            camposition.localPosition = new Vector3(-3.5f, 4f, 3.5f);
                        }
                        else if (movement.y < 0f && movement.x > 0f)
                        {
                            camposition.localPosition = new Vector3(3.5f, 4f, 3.5f);
                        }
                    }
                    else
                    {
                        animator.SetBool("isrunning", false);
                    }

                    if (inputManager.PlayerJump() && groundedPlayer)
                    {
                        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
                    }

                    if (inputManager.PlayerAccelaration())
                    {
                        playerSpeed = 10f;
                    }
                    else
                    {
                        playerSpeed = 5f;
                    }

                    playerVelocity.y += gravityValue * Time.deltaTime;
                    rb.velocity = move * playerSpeed + new Vector3(0, playerVelocity.y, 0) - gameObject.GetComponent<sliding>().velocityslide;
                }
                else
                {
                    animator.SetBool("isdead", true);
                    animator.SetBool("isrunning", false);
                }
            }
            else
            {
                animator.SetBool("isrunning", false);
            }
        }
    }
    private void OnDrawGizmos()
    {
        float raycastDistance = 0.1f;
        bool isHit = Physics.Raycast(transform.position, Vector3.down, raycastDistance);

        // Set Gizmos color based on whether the ray hits something
        Gizmos.color = isHit ? Color.green : Color.red;

        // Draw a raycast line from the object's position downward
        Gizmos.DrawRay(transform.position, Vector3.down * raycastDistance);
    }
    public void HitPlayer(Vector3 velocityF, float time)
    {
        if (canMove)
        {
            rb.velocity = velocityF;
            pushForce = velocityF.magnitude;
            pushDir = Vector3.Normalize(velocityF);
            StartCoroutine(Decrease(velocityF.magnitude, time));
        }
    }
    private IEnumerator Decrease(float value, float duration)
    {
        if (isStuned)
            wasStuned = true;
        isStuned = true;
        canMove = false;

        float delta = 0;
        delta = value / duration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            yield return null;
            if (!slide)
            {
                pushForce = pushForce - Time.deltaTime * delta;
                pushForce = pushForce < 0 ? 0 : pushForce;
            }
            rb.AddForce(new Vector3(0, -3 * GetComponent<Rigidbody>().mass, 0)); 
        }

        if (wasStuned)
        {
            wasStuned = false;
        }
        else
        {
            isStuned = false;
            canMove = true;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            groundedPlayer = true;
        }
        if (collision.gameObject.name.StartsWith("Gate"))
        {
            collision.gameObject.GetComponent<BoxCollider>().isTrigger = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundedPlayer = false;
        }
        if (collision.gameObject.name.StartsWith("Gate"))
        {
            collision.gameObject.GetComponent<BoxCollider>().isTrigger = false;
        }
    }

    [Command]
    void CmdselectChat()
    {
        canMove = false;
    }

    public void selectChat()
    {
        canMove = false;
        CmdselectChat();
    }

    [Command]
    void CmdDeselectChat()
    {
        canMove = true;
    }

    public void DeselectChat()
    {
        canMove = true;
        CmdDeselectChat();
    }

}
