using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;
    public float multiplier;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    [Header("Fov")]
    public bool useFluentFov;
    public PlayerMovementDashing pm;
    public Rigidbody rb;
    public Camera cam;
    public float minMovementSpeed;
    public float maxMovementSpeed;
    public float minFov;
    public float maxFov;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        yRotation += mouseX * multiplier;

        xRotation -= mouseY * multiplier;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);

        if (useFluentFov) HandleFov();
    }

    private void HandleFov()
    {
        float moveSpeedDif = maxMovementSpeed - minMovementSpeed;
        float fovDif = maxFov - minFov;

        float rbFlatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        float currMoveSpeedOvershoot = rbFlatVel - minMovementSpeed;
        float currMoveSpeedProgress = currMoveSpeedOvershoot / moveSpeedDif;

        float fov = (currMoveSpeedProgress * fovDif) + minFov;

        float currFov = cam.fieldOfView;

        float lerpedFov = Mathf.Lerp(fov, currFov, Time.deltaTime * 200);

        cam.fieldOfView = lerpedFov;
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}