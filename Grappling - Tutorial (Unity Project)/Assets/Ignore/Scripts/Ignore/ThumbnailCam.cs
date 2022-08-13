using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbnailCam : MonoBehaviour
{
    public float moveSpeed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.I))
            transform.Translate(transform.up * Time.deltaTime * moveSpeed);

        if (Input.GetKey(KeyCode.K))
            transform.Translate(-transform.up * Time.deltaTime * moveSpeed);

        if (Input.GetKey(KeyCode.L))
            transform.Translate(transform.right * Time.deltaTime * moveSpeed);

        if (Input.GetKey(KeyCode.J))
            transform.Translate(-transform.right * Time.deltaTime * moveSpeed);
    }
}
