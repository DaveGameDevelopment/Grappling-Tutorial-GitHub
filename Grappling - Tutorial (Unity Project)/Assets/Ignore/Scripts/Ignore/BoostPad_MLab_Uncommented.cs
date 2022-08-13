using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad_MLab_Uncommented : MonoBehaviour
{
    [Header("Boosting")]
    public bool normalBoosting = true;
    public Vector3 boostDirection;
    public float boostForce;

    public bool localBoosting = false;
    public float boostLocalForwardForce;
    public float boostLocalUpwardForce;

    public float boostDuration = 1f;

    private PlayerMovementAdvancedFinished pm = null;

    private void OnTriggerEnter(Collider other)
    {
        AddForce(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        AddForce(collision.collider);
    }

    private void AddForce(Collider other)
    {
        if (other.GetComponentInParent<PlayerMovementAdvancedFinished>() != null)
        {
            pm = other.GetComponentInParent<PlayerMovementAdvancedFinished>();

            Rigidbody rb = pm.GetComponent<Rigidbody>();

            if (normalBoosting)
                rb.AddForce(boostDirection.normalized * boostForce, ForceMode.Impulse);

            if (localBoosting)
            {
                Vector3 localBoostedDirection = pm.orientation.forward * boostLocalForwardForce + pm.orientation.up * boostLocalUpwardForce;
                rb.AddForce(localBoostedDirection, ForceMode.Impulse);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!normalBoosting) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + boostDirection);
    }
}
