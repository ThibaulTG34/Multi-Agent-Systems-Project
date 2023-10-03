using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlaneBehaviour : MonoBehaviour
{
    public Vector3 currentPosition
    {
        get { return this.transform.position; }
        private set { this.transform.position = value; }
    }

    public Vector3 currentTargetPosition { get; private set; }
    public Vector3 speedDirectionnalVector;
    public List<Vector3> trajectory;
    public float speed;

    private int idxTraj = 0;
    private bool contact = false;
    public List<Vector3> alternativeTrajectories;

    void Start()
    {
        alternativeTrajectories = new List<Vector3>(trajectory);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        Vector3 currentTarget;
        if (contact)
        {
            Debug.Log(idxTraj);
            currentTarget = (idxTraj < alternativeTrajectories.Count - 1) ? alternativeTrajectories[idxTraj] : alternativeTrajectories[alternativeTrajectories.Count - 1];
        }
        else
        {
            Debug.Log(idxTraj);
            currentTarget = (idxTraj < trajectory.Count - 1) ? trajectory[idxTraj] : trajectory[trajectory.Count - 1];
        }

        float distance = Vector3.Distance(this.transform.position, currentTarget);
        if (distance < 10f && idxTraj < Mathf.Min(trajectory.Count, alternativeTrajectories.Count) - 1)
        {
            idxTraj++;
        }
        this.transform.position = Vector3.MoveTowards(this.transform.position, currentTarget, step);

        UpdateDirectionalVector();
        RotatePlaneTowardsTarget(currentTarget);
    }

    private void UpdateDirectionalVector()
    {
        if (idxTraj > 0 && idxTraj < trajectory.Count)
        {
            speedDirectionnalVector = (trajectory[idxTraj] - trajectory[idxTraj - 1]).normalized;
        }
        else if (idxTraj == 0 && trajectory.Count > 1)
        {
            speedDirectionnalVector = (trajectory[1] - trajectory[0]).normalized;
        }
    }

    private void RotatePlaneTowardsTarget(Vector3 target)
    {
        Vector3 direction = Vector3.RotateTowards(this.transform.position, target, 3, 0.0f);
        if (direction != Vector3.zero)
        {
            this.transform.rotation = Quaternion.LookRotation(direction);
            this.transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlaneBehaviour otherPlane = other.GetComponent<PlaneBehaviour>();
        if (otherPlane != null)
        {
            contact = true;
            Vector3 dodgeDirection = CalculateDodgeDirection(otherPlane);
            AdjustTrajectory(dodgeDirection);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlaneBehaviour otherPlane = other.GetComponent<PlaneBehaviour>();
        if (otherPlane != null)
        {
            contact = false;
        }
    }

    private Vector3 CalculateDodgeDirection(PlaneBehaviour otherPlane)
    {
        Vector3 dodgeLeft = Quaternion.Euler(0, -90, 0) * speedDirectionnalVector;
        Vector3 dodgeRight = Quaternion.Euler(0, 90, 0) * speedDirectionnalVector;

        float criticalityLeft = CalculcateCriticalityWithOtherAgent(currentPosition, currentTargetPosition, dodgeLeft, trajectory);
        float criticalityRight = CalculcateCriticalityWithOtherAgent(currentPosition, currentTargetPosition, dodgeRight, trajectory);

        return criticalityLeft < criticalityRight ? dodgeLeft : dodgeRight;
    }

    private void AdjustTrajectory(Vector3 dodgeDirection)
    {
        alternativeTrajectories = new List<Vector3>(trajectory);
        if (idxTraj + 1 < alternativeTrajectories.Count)
        {
            alternativeTrajectories[idxTraj + 1] = currentPosition + dodgeDirection * 10f;
        }
        speedDirectionnalVector = dodgeDirection.normalized;
    }


    private float CalculateC3k(Vector3 positionNextStep)
    {
        float minDistance = float.MaxValue;

        foreach (Vector3 point in trajectory)
        {
            float distance = Vector3.Distance(positionNextStep, point);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    private float CalculateC4k(Vector3 positionNextStep, Vector3 goalPosition)
    {
        return Vector3.Distance(positionNextStep, goalPosition);
    }

    private float CalculateC5k(Vector3 speedVector)
    {
        float angleAlpha = Vector3.Angle(trajectory[trajectory.Count - 1] - trajectory[0], speedVector);
        float angleInRadians = angleAlpha * Mathf.Deg2Rad;

        if (angleInRadians < Mathf.PI / 2)
        {
            return 0;
        }
        else
        {
            return 100f / (Mathf.PI / 2) * (angleInRadians - Mathf.PI / 2);
        }
    }

    private float CalculcateCriticalityWithOtherAgent(Vector3 agentPos, Vector3 targetPos, Vector3 speedVector, List<Vector3> trajectory)
    {
        Vector3 positionNextStep = agentPos + speedVector;

        float c3k = CalculateC3k(positionNextStep);

        float c4k = CalculateC4k(positionNextStep, targetPos);

        float c5k = CalculateC5k(speedVector);

        
        return Math.Max(c3k,Math.Max(c4k,c5k));
    }
}
