using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingDone : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public PlayerMovementAdvanced pm;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed = 3f;
    public float maxClimbTime = 0.75f;
    private float climbTimer;

    public float climbJumpUpForce = 15f;
    public float climbJumpBackForce = 15f;

    private bool climbing;

    [Header("ClimbJumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle = 30f;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Ledge Grabbing")]
    public Transform cam;

    public float moveToLedgeSpeed;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpForce;
    public float maxLedgeJumpUpSpeed;
    public float maxLedgeGrabDistance;

    public float minTimeOnLedge;
    private float timeOnLedge;

    private bool holding;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    public Transform currLedge;

    private RaycastHit ledgeHit;
    private Vector3 directionToLedge;
    private float distanceToLedge;

    [Header("Vaulting")]
    public float vaultDetectionLength;
    public bool topReached;
    public float vaultClimbSpeed;
    public float vaultJumpForwardForce;
    public float vaultJumpUpForce;
    public float vaultCooldown;

    bool readyToVault;
    bool vaultPerformed;
    bool midCheck;
    bool feetCheck;


    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // State 0 - Ledge Holding
        if (holding)
        {
            FreezeRigidbodyOnLedge();

            if (timeOnLedge > minTimeOnLedge && inputDirection != Vector2.zero) ExitLedgeHold();

            timeOnLedge += Time.deltaTime;

            /*
            Vector2 flatDirectionToLedge = new Vector2(directionToLedge.x, directionToLedge.z);
            float dotProduct = Vector2.Dot(inputDirection.normalized, flatDirectionToLedge.normalized);

            /// print("dot product: " + dotProduct);

            // -1 means 180* away, 0 means a right angle (so limit the angle somewhere between 0 and -1)
            /// if (dotProduct < 0) ExitLedgeHold();
            if (Input.GetKeyDown(KeyCode.S)) ExitLedgeHold();
            */
        }

        // State 1 - Vaultclimbing
        else if (topReached && Input.GetKey(KeyCode.W))
        {
            // start climbing (but using vault speed)
            if (!climbing && climbTimer > 0) StartClimbing();

            if (!readyToVault) readyToVault = true;
        }

        // State 2 - Climbing
        else if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            // timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimbing();
        }

        // State 3 - Exiting
        else if (exitingWall)
        {
            if (climbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }

        // State 4 - None
        else
        {
            if (climbing) StopClimbing();
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0 && !holding) ClimbJump();

        if (Input.GetKeyDown(jumpKey) && holding) LedgeJump();

        // start vault
        if (readyToVault)
        {
            if (!feetCheck && !vaultPerformed)
            {
                if (Input.GetKey(KeyCode.Mouse1)) Vault();
                else readyToVault = false;
            }
        }
    }

    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }

        // vaulting
        if(Physics.Raycast(transform.position, orientation.forward, detectionLength, whatIsWall))
            print("raycastCheck done");

        midCheck = Physics.Raycast(transform.position, orientation.forward, vaultDetectionLength, whatIsWall);
        feetCheck = Physics.Raycast(transform.position + new Vector3(0, -0.9f, 0), orientation.forward, vaultDetectionLength, whatIsWall);

        topReached = feetCheck && !midCheck;

        // ledge detection
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (ledgeHit.transform == null) return;

        directionToLedge = ledgeHit.transform.position - transform.position;
        distanceToLedge = directionToLedge.magnitude;

        if (lastLedge != null && ledgeHit.transform == lastLedge) return;

        if (ledgeDetected && distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();

    }

    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
        ///cam.DoShake(1, 1);
    }

    private void ClimbingMovement()
    {
        float speed = topReached ? vaultClimbSpeed : climbSpeed;
        rb.velocity = new Vector3(rb.velocity.x, speed, rb.velocity.z);
    }

    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;
    }

    private void ClimbJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }

    private void Vault()
    {
        print("vault!");

        Vector3 forceToAdd = orientation.forward * vaultJumpForwardForce + orientation.up * vaultJumpUpForce;

        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);

        vaultPerformed = true;

        Invoke(nameof(ResetVault), vaultCooldown);
    }

    private void ResetVault()
    {
        vaultPerformed = false;
        readyToVault = false;
    }

    private void LedgeJump()
    {
        Vector3 forceToAdd = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpForce;
        // evt. noch + orientation.forward * ledgeJumpForwardForce * 0.5f

        // print(forceToAdd.y + " compared to " + maxLedgeJumpUpSpeed);
        // if (forceToAdd.y > maxLedgeJumpUpSpeed) forceToAdd = new Vector3(forceToAdd.x, maxLedgeJumpUpSpeed, forceToAdd.z);

        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);

        ExitLedgeHold();
    }

    private void EnterLedgeHold()
    {
        holding = true;

        pm.restricted = true;
        pm.unlimited = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    bool touchingLedge;
    private void FreezeRigidbodyOnLedge()
    {
        Vector3 directionToLedge = currLedge.position - transform.position;

        if (directionToLedge.magnitude > maxLedgeGrabDistance && holding) ExitLedgeHold();

        // Move player towards ledge
        if (directionToLedge.magnitude > .8f)
        {
            // Vector3 directionToLedge = ledgeHit.transform.position - transform.position;
            // rb.velocity = directionToLedge.normalized * moveToLedgeSpeed;

            if(rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);

            /// The current problem is that I can't set the velocity from here, I can only add force
            /// -> but then the force is mainly upwards :D

            print("moving to ledge");
        }

        // Hold onto ledge
        else
        {
            if (pm.unlimited) pm.unlimited = false;
            if (!pm.freeze) pm.freeze = true;
            ///rb.velocity = Vector3.zero;
            print("hanging on ledge");
        }
    }

    private void ExitLedgeHold()
    {
        holding = false;
        timeOnLedge = 0;

        pm.freeze = false;
        pm.unlimited = false;
        pm.restricted = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.tag == "Ledge")
        {
            touchingLedge = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Ledge")
        {
            touchingLedge = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (currLedge == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currLedge.position, 1f);
    }
}
