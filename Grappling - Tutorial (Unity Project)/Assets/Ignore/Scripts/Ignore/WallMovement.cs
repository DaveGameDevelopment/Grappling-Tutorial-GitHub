using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallMovement : MonoBehaviour
{
    /*
    public Transform orientation;

    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce = 200f;
    public float wallRunJumpUpForce = 5f;
    public float wallRunJumpSideForce = 5f;
    public float pushToWallForce = 100f;
    public float maxWallRunTime = 1f;
    private float wallRunTimer;

    /// booleans set by PlayerInput script
    public bool upwardsRunning;
    public bool downwardsRunning;

    [Header("Climbing")]
    public float climbForce = 200f;
    public float climbJumpUpForce = 15f;
    public float climbJumpBackForce = 15f;
    public float maxClimbYSpeed = 5f;
    public float maxClimbTime = 0.75f;

    private float climbTimer;
    // is true if player hits a new wall or has sucessfully exited the old one
    private bool readyToClimb;

    [Header("BackWallMovement")]
    public float backWallJumpUpForce = 5f;
    public float backWallJumpForwardForce = 12f;

    [Header("Limitations")]
    public bool doJumpOnEndOfTimer = false;
    public bool resetDoubleJumpsOnNewWall = true;
    public bool resetDoubleJumpsOnEveryWall = false;
    public int allowedWallJumps = 1;
    public int allowedClimbJumps = 1;

    [Header("Detection")]
    public float doubleRayCheckDistance = 0.1f;
    public float wallDistanceSide = 0.7f;
    public float wallDistanceFront = 1f;
    public float wallDistanceBack = 1f;
    public float minJumpHeight = 2f;
    public float exitWallTime = 0.2f;

    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity = false;
    public float customGravity = 0f;
    public float yDrossleSpeed = 0.2f;

    [Header("References")]
    private PlayerMovement pm;
    private PlayerInput pi;
    private PlayerCam cam;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private RaycastHit frontWallHit;
    private RaycastHit backWallHit;

    private bool wallLeft;
    private bool wallRight;

    [HideInInspector] public bool wallFront;
    [HideInInspector] public bool wallBack;

    private bool exitingWall;

    private bool wallRemembered;
    private Transform lastWall;

    private int wallJumpsDone;
    private int climbJumpsDone;

    private Rigidbody rb;

    private Vector2 input;

    public State state;
    public enum State
    {
        wallrunning,
        climbing,
        sliding,
        exiting,
        none
    }

    public TextMeshProUGUI text_wallState;

    private void Start()
    {
        if (whatIsWall.value == 0)
            whatIsWall = LayerMask.GetMask("Default");

        if (whatIsGround.value == 0)
            whatIsGround = LayerMask.GetMask("Default");

        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        pi = GetComponent<PlayerInput>();
        cam = GetComponent<PlayerCam>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();

        if (wallRunTimer < 0 && pm.wallrunning)
        {
            wallRunTimer = 0;

            if (doJumpOnEndOfTimer)
                WallJump();

            else
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
                StopWallRun();
            }
        }

        // handle wall-exiting
        if (exitWallTimer > 0)
        {
            exitWallTimer -= Time.deltaTime;
            ///pm.restricted = true;
        }

        if (exitWallTimer <= 0 && exitingWall)
        {
            exitingWall = false;
            ///pm.restricted = false;

            // reset readyToClimb when player has sucessfully exited the wall
            ResetReadyToClimb();
        }

        // if grounded, next wall is a new one
        if (pm.grounded && lastWall != null)
            lastWall = null;

        input = pi.movementInput;

        if (text_wallState != null)
            text_wallState.SetText(state.ToString());
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning && !exitingWall)
            WallRunningMovement();

        if (pm.climbing && !exitingWall)
            ClimbingMovement();
    }

    #region StateMachine

    private void StateMachine()
    {
        bool sideWall = wallLeft || wallRight;
        bool noInput = input.x == 0 && input.y == 0;

        bool climbing = wallFront && input.y > 0;

        // State 1 - Wallrunning
        if (sideWall && input.y > 0 && CanWallRun() && !exitingWall)
        {
            state = State.wallrunning;

            if (!pm.wallrunning) StartWallRun();

            wallRunTimer -= Time.deltaTime;
        }

        // State 2 - Climbing
        else if (climbing && readyToClimb && !exitingWall)
        {
            state = State.climbing;

            if (readyToClimb && !pm.climbing)
                StartClimbing();

            if (climbTimer > 0 && !exitingWall)
                StartClimbing();

            if (climbTimer > 0 && pm.climbing) climbTimer -= Time.deltaTime;

            if (climbTimer < 0 && pm.climbing)
            {
                climbTimer = -1;

                StopClimbing();
            }
        }

        // State 3 - Sliding
        // Ok, here in normal language:
        // wallback + back input, or sidewalls with specific side and no forward input, or wallfront without timer
        else if ((wallBack && input.y < 0) || (((wallLeft && input.x < 0) || (wallRight && input.x > 0)) && input.y <= 0) || (climbing && climbTimer <= 0))
        {
            state = State.sliding;

            // bug fix
            if (pm.wallrunning)
                StopWallRun();
        }

        // State 4 - Exiting
        // no input
        else if (exitingWall)
        {
            state = State.exiting;

            pm.restricted = true;

            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();
        }

        else
        {
            state = State.none;

            // exit out of WallRun or Climb when active

            if (pm.wallrunning)
                StopWallRun();

            if (pm.climbing)
                StopClimbing();
        }

        if (state != State.exiting && pm.restricted)
            pm.restricted = false;
    }

    #endregion

    #region WallChecks and Remembering

    /// do all of the raycasts
    private void CheckForWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistanceSide, whatIsWall);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistanceSide, whatIsWall);
        wallFront = Physics.Raycast(transform.position, orientation.forward, out frontWallHit, wallDistanceFront, whatIsWall);
        wallBack = Physics.Raycast(transform.position, -orientation.forward, out backWallHit, wallDistanceBack, whatIsWall);

        // reset readyToClimb and wallJumps whenever player hits a new wall
        if (wallLeft || wallRight || wallFront || wallBack)
        {
            if (NewWallHit())
            {
                ResetReadyToClimb();
                ResetWallJumpsDone();

                if (resetDoubleJumpsOnNewWall)
                    pm.ResetDoubleJumps();

                wallRunTimer = maxWallRunTime;
                climbTimer = maxClimbTime;
            }

            if (resetDoubleJumpsOnEveryWall)
                pm.ResetDoubleJumps();
        }
    }

    private void RememberLastWall()
    {
        if (wallLeft)
            lastWall = leftWallHit.transform;

        if (wallRight)
            lastWall = rightWallHit.transform;

        if (wallFront)
            lastWall = frontWallHit.transform;

        if (wallBack)
            lastWall = backWallHit.transform;
    }

    private bool NewWallHit()
    {
        if (lastWall == null)
            return true;

        if (wallLeft && leftWallHit.transform != lastWall)
            return true;

        else if (wallRight && rightWallHit.transform != lastWall)
            return true;

        else if (wallFront && frontWallHit.transform != lastWall)
            return true;

        else if (wallBack && backWallHit.transform != lastWall)
            return true;

        return false;
    }

    private bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    #endregion

    #region WallRunning

    private void StartWallRun()
    {
        pm.wallrunning = true;

        pm.maxYSpeed = maxClimbYSpeed;

        rb.useGravity = useGravity;

        wallRemembered = false;

        cam.DoFov(100f);

        if (wallRight) cam.DoTilt(5f);
        if (wallLeft) cam.DoTilt(-5f);
    }

    private void WallRunningMovement()
    {
        if (rb.useGravity) rb.useGravity = false;

        // calculate directions

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // lerp upwards velocity of rb to 0 if gravity is turned off

        float velY = rb.velocity.y;

        if (!useGravity)
        {
            if (velY > 0)
                velY -= yDrossleSpeed;

            rb.velocity = new Vector3(rb.velocity.x, velY, rb.velocity.z);
        }

        // add forces

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upward force
        //if ((leftWall && input.x < 0) || (rightWall && input.x > 0))
        //    rb.AddForce(orientation.up * climbForce, ForceMode.Force);

        /// not the best way to handle this
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, maxClimbYSpeed * 0.5f, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -maxClimbYSpeed * 0.5f, rb.velocity.z);

        if (!exitingWall)
            rb.AddForce(-wallNormal * pushToWallForce, ForceMode.Force);

        if (customGravity != 0)
            rb.AddForce(-orientation.up * customGravity, ForceMode.Force);

        // remember the last wall

        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;

        pm.wallrunning = false;

        pm.maxYSpeed = -1;

        cam.ResetFov();
        cam.ResetTilt();
    }

    #endregion

    #region Climbing

    private void StartClimbing()
    {
        pm.climbing = true;

        pm.maxYSpeed = maxClimbYSpeed;

        rb.useGravity = false;

        wallRemembered = false;

        cam.DoShake(1, 1);
    }

    private void ClimbingMovement()
    {
        if (rb.useGravity != false)
            rb.useGravity = false;

        // calculate directions

        Vector3 upwardsDirection = Vector3.up;

        Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

        // add forces

        rb.AddForce(upwardsDirection * climbForce, ForceMode.Force);

        if (!exitingWall)
            rb.AddForce(againstWallDirection * pushToWallForce, ForceMode.Force);

        // remember the last wall

        if (!wallRemembered)
        {
            RememberLastWall();
            wallRemembered = true;
        }
    }

    private void StopClimbing()
    {
        rb.useGravity = true;

        pm.climbing = false;

        // maxYSpeed is reseted when jumping as well
        pm.maxYSpeed = -1;

        readyToClimb = false;

        cam.ResetShake();

        cam.ResetFov();
        cam.ResetTilt();
    }

    private void ResetReadyToClimb()
    {
        readyToClimb = true;
        Debug.Log("ReadyToClimb resetted");
    }

    #endregion

    #region Wall- and ClimbJumping

    public void WallJump()
    {
        // idea: allow one full jump, the second one is without upward force

        bool firstJump = true;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = new Vector3();

        if (wallLeft)
        {
            forceToApply = transform.up * wallRunJumpUpForce + leftWallHit.normal * wallRunJumpSideForce;

            firstJump = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if (wallRight)
        {
            forceToApply = transform.up * wallRunJumpUpForce + rightWallHit.normal * wallRunJumpSideForce;

            firstJump = wallJumpsDone < allowedWallJumps;
            wallJumpsDone++;
        }

        else if (wallFront)
        {
            rb.useGravity = true;

            pm.maxYSpeed = -1;

            Vector3 againstWallDirection = (frontWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * climbJumpUpForce + -againstWallDirection * climbJumpBackForce;

            firstJump = climbJumpsDone < allowedClimbJumps;
            climbJumpsDone++;
        }

        else if (wallBack)
        {
            pm.maxYSpeed = -1;

            Vector3 againstWallDirection = (backWallHit.point - orientation.position).normalized;

            forceToApply = Vector3.up * backWallJumpUpForce + -againstWallDirection * backWallJumpForwardForce;

            firstJump = true;
        }

        else
        {
            Debug.LogError("WallJump was called, but there is no wall in range");
        }

        // before jumping off, make sure that the last wall is remembered
        RememberLastWall();

        // apply force
        if (firstJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }

        else
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            Vector3 noUpwardForce = new Vector3(forceToApply.x, 0f, forceToApply.z);

            rb.AddForce(noUpwardForce, ForceMode.Impulse);
        }

        Debug.Log("Walljump!");

        // stop wallRun and climbing immediately
        StopWallRun();
        StopClimbing();
    }

    private void ResetWallJumpsDone()
    {
        wallJumpsDone = 0;
        climbJumpsDone = 0;
    }

    #endregion

    #region Debugging

    private void OnDrawGizmosSelected()
    {
        float difference = doubleRayCheckDistance * 0.5f;
        Vector3 differenceV = orientation.forward * difference;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - differenceV, orientation.right * wallDistanceSide);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + differenceV, orientation.right * wallDistanceSide);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position - differenceV, -orientation.right * wallDistanceSide);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + differenceV, -orientation.right * wallDistanceSide);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, orientation.forward * wallDistanceFront);

        Gizmos.color = Color.grey;
        Gizmos.DrawRay(transform.position, -orientation.forward * wallDistanceBack);
    }

    #endregion

    #region LoadingData

    public void LoadSkillData(WallRunningData data)
    {
        wallRunForce = data.wallRunForce;
        wallRunJumpUpForce = data.wallRunJumpUpForce;
        wallRunJumpSideForce = data.wallRunJumpSideForce;
        pushToWallForce = data.pushToWallForce;
        maxWallRunTime = data.maxWallRunTime;

        useGravity = data.useGravity;
        customGravity = data.customGravity;
        yDrossleSpeed = data.yDrossleSpeed;
    }
        
#endregion
        */
}

[Serializable]
public class WallRunningData
{
    [Header("Wall Running")]
    public float wallRunForce;
    public float wallRunJumpUpForce;
    public float wallRunJumpSideForce;
    public float pushToWallForce;
    public float maxWallRunTime;

    [Header("Gravity")]
    public bool useGravity;
    public float customGravity;
    public float yDrossleSpeed;
}