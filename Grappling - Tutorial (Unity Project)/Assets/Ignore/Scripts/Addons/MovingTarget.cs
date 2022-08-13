using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTarget : MonoBehaviour
{
    [Header("Moving")]
    public Vector3 startPos;
    public Vector3 endPos;
    public float moveSpeed;

    [Header("Fine Tuning")]
    public float distanceThreshold;

    Vector3 destination;

    bool movingToEnd;

    float distanceToStartPos;
    float distanceToEndPos;

    private void Start()
    {
        movingToEnd = true;

        destination = endPos;
    }

    private void Update()
    {
        // change target destination
        if (PositionReached())
        {
            movingToEnd = !movingToEnd;

            if (movingToEnd)
                destination = endPos;
            else
                destination = startPos;
        }

        MoveTarget();
    }
    private void MoveTarget()
    {
        Vector3 direction = destination - transform.localPosition;

        transform.Translate(direction.normalized * Time.deltaTime * moveSpeed * -1f);
    }

    private bool PositionReached()
    {
        distanceToStartPos = (startPos - transform.localPosition).magnitude;
        distanceToEndPos = (endPos - transform.localPosition).magnitude;

        if (distanceToStartPos < distanceThreshold && !movingToEnd)
            return true;

        else if (distanceToEndPos < distanceThreshold && movingToEnd)
            return true;

        else
            return false;
    }
}