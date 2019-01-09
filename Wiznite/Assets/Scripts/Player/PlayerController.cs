using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;
using UdpNetwork;
using Newtonsoft.Json;

public class PlayerController : MonoBehaviour
{

    public float speed = 7f, rotSpeed = 7f, hitdistance = 1f;
    private Vector3 moveInput;
    private Camera mainCamera;
    private Animator animator;
    public LayerMask layer;
    public GameObject attack;
    private Vector3 oldPosition;

    private bool isknockback = false;
    private float knockBackForce = 1000;
    private float knockBackTime = 1;
    private float knockBackCounter = 0;
    private Rigidbody impactTarget;
    private Vector3 impact;

    float timeToWait = 0.0f;
    public float timeToFire;

    UdpClientController udp = ClientInformation.UdpClientController;

    // Use this for initialization
    void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
        animator = GetComponent<Animator>();
        animator.SetBool("Idle", true);
        oldPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!animator.GetBool("Attacking") && !isknockback)
        {
            Movement();
            movementAnimation();
        }

        //Check if is grounded or not
        if (isGrounded())
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        else
        {
            GetComponent<Rigidbody>().constraints &= ~RigidbodyConstraints.FreezePositionY;
            FallForce();
        }

        //Fire
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetBool("Attacking", true);
        }

        //Knockback
        if (Time.time < knockBackCounter)
        {
            isknockback = true;
            impactTarget.AddForce(impact, ForceMode.VelocityChange);
        }
        else
            isknockback = false;

        Message msg = new Message();
        PlayerInfo info = new PlayerInfo();
        info.Id = udp.Player.Id;
        info.X = transform.position.x;
        info.Y = transform.position.y;
        info.Z = transform.position.z;
        info.RotX = transform.eulerAngles.x;
        info.RotY = transform.eulerAngles.y;
        info.RotZ = transform.eulerAngles.z;
        msg.MessageType = MessageType.PlayerMovement;
        msg.Description = JsonConvert.SerializeObject(info);
        udp.Player.Messages.Enqueue(msg);
        udp.SendPlayerMessageMulticast();
    }

    private void Fire()
    {
        GameObject attack1 = Instantiate(attack, this.transform.position, Quaternion.identity);
        attack1.GetComponent<SpellController>().Velocity = this.transform.forward;

        Message msg = new Message();
        /* PlayerInfo info = new PlayerInfo();
        info.Id = udp.Player.Id;
        info.X = transform.position.x;
        info.Y = transform.position.y;
        info.Z = transform.position.z;
        info.RotX = transform.eulerAngles.x;
        info.RotY = transform.eulerAngles.y;
        info.RotZ = transform.eulerAngles.z;*/
        msg.MessageType = MessageType.PlayerAttack;
        //msg.Description = JsonConvert.SerializeObject(info);
        udp.Player.Messages.Enqueue(msg);
        udp.SendPlayerMessageMulticast();
    }

    private void DeactivateAttack()
    {
        animator.SetBool("Attacking", false);
    }

    public void KnockBack(Vector3 direction)
    {
        impact = new Vector3(direction.x, 0.0f, direction.z) * knockBackForce * Time.deltaTime;
        knockBackCounter = Time.time + 0.25f;
    }

    private void Movement()
    {
        oldPosition = transform.position;
        //movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        moveInput = new Vector3(h, 0.0f, v) * speed;
        transform.position += moveInput * Time.deltaTime;

        //rotation
        Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLength;

        if (groundPlane.Raycast(cameraRay, out rayLength))
        {
            Vector3 pointToLook = cameraRay.GetPoint(rayLength);
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    private void movementAnimation()
    {
        Vector3 direction = transform.position - oldPosition;
        float forwardTest = Vector3.Dot(direction.normalized, transform.forward.normalized);
        float sideTest = Vector3.Dot(direction.normalized, Vector3.Cross(transform.forward, transform.up).normalized);

        //animations
        if (forwardTest != 0 || sideTest != 0)
        {
            if (forwardTest != 0)
            {
                animator.SetBool("Idle", false);
                if (forwardTest > 0)
                {
                    animator.SetBool("Forward", true);
                    animator.SetBool("Back", false);
                }
                else
                {
                    animator.SetBool("Forward", false);
                    animator.SetBool("Back", true);
                }
            }
            if (sideTest != 0)
            {
                animator.SetBool("Idle", false);
                if (sideTest < 0)
                {
                    animator.SetBool("Right", true);
                    animator.SetBool("Left", false);
                }
                else
                {
                    animator.SetBool("Right", false);
                    animator.SetBool("Left", true);
                }
            }
        }
        else if (forwardTest == 0 && sideTest == 0)
            animator.SetBool("Idle", true);
    }

    void FallForce()
    {
        float fallY;
        float airVelocity = -9.8f;
        Vector3 gravity = Physics.gravity * Time.deltaTime;
        airVelocity += gravity.y;
        fallY = airVelocity * Time.deltaTime;

        transform.position = Vector3.MoveTowards(transform.position, transform.position - new Vector3(0f, fallY, 0f), Time.deltaTime * airVelocity);
    }

    private bool isGrounded()
    {
        //Debug.DrawLine(transform.position, transform.position + Vector3.down, Color.cyan);
        //return Physics.Raycast(transform.position + Vector3.down, -transform.forward, hitdistance, layer);
        return Physics.Linecast(transform.position, transform.position + Vector3.down, layer);
    }
}
