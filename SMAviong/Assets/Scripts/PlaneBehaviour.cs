using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
    public LineRenderer altTrajectoryLineRenderer;

    private int idxTraj = 0;
    private bool contact = false;
    public List<Vector3> alternativeTrajectories;
    public float TimerContact = 10f;
    public bool TimerRunnig = false;

    //public List<PlaneBehaviour> nearByPlanes;
    public PlaneBehaviour closestPlane;
    private float currentMaxCrit;


    void Start()
    {
        alternativeTrajectories = new List<Vector3>(trajectory);

        // If the LineRenderer is not assigned through the inspector, try getting it from a child component
        if (altTrajectoryLineRenderer == null)
        {
            altTrajectoryLineRenderer = GetComponentInChildren<LineRenderer>();
        }

        if (altTrajectoryLineRenderer != null)
        {
            // Optional: configure the LineRenderer here if needed (e.g., color, width, material, etc.)
            altTrajectoryLineRenderer.positionCount = 0; // Initially we do not draw any line
        }
        
        TimerContact = 0f;
    }


    void Update()
    {
        float step = speed * Time.deltaTime;

        Vector3 currentTarget = Vector3.zero;
        //Debug.Log(TimerContact);

        if (TimerRunnig)
            TimerContact -= Time.deltaTime;

        if (TimerContact <= 0)
            TimerRunnig = false;

        if (contact)
        {
            currentTarget = (idxTraj < alternativeTrajectories.Count - 1) ? alternativeTrajectories[idxTraj] : alternativeTrajectories[alternativeTrajectories.Count - 1];
        }
        else if (TimerContact > 0)
        {
            //Debug.Log("ici");
            currentTarget = (idxTraj < alternativeTrajectories.Count - 1) ? alternativeTrajectories[idxTraj] : alternativeTrajectories[alternativeTrajectories.Count - 1];
        } 
        else
        {
            currentTarget = (idxTraj < trajectory.Count - 1) ? trajectory[idxTraj] : trajectory[trajectory.Count - 1];
        }

        float distance = Vector3.Distance(this.transform.position, currentTarget);
        if (distance < 10f && idxTraj < Mathf.Min(trajectory.Count, alternativeTrajectories.Count) - 1)
        {
            idxTraj++;
        }
        this.transform.position = Vector3.MoveTowards(this.transform.position, currentTarget, step);


        if (contact)
        {
            DrawAlternativeTrajectory();
        }
        else if (altTrajectoryLineRenderer != null && altTrajectoryLineRenderer.positionCount > 0)
        {
            altTrajectoryLineRenderer.positionCount = 0;
        }

        UpdateDirectionalVector();
        RotatePlaneTowardsTarget(currentTarget);

        if(idxTraj == trajectory.Count -1)
        {
            GameObject.Destroy(this.gameObject);
        }
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

    private void DrawAlternativeTrajectory()
    {
        if (altTrajectoryLineRenderer != null && alternativeTrajectories != null && alternativeTrajectories.Count > 0)
        {
            altTrajectoryLineRenderer.positionCount = alternativeTrajectories.Count;
            altTrajectoryLineRenderer.SetPositions(alternativeTrajectories.ToArray());
        }
    }

    private void RotatePlaneTowardsTarget(Vector3 target)
    {
        Vector3 directionToTarget = target - this.transform.position;
        if (directionToTarget.sqrMagnitude > 0.01f) // prevent trying to look at self/origin which results in NaN
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 3.0f);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        PlaneBehaviour otherPlane = other.GetComponent<PlaneBehaviour>();
        if (otherPlane != null)
        {
            contact = true;
            TimerRunnig = true;
            Tuple<Vector3, float> res = CalculateDodgeDirection(otherPlane);
            if(res.Item2 > currentMaxCrit)
            {
                currentMaxCrit = res.Item2;
                AdjustTrajectory(res.Item1);
                TimerContact = 10f;
            }
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

    private Tuple<Vector3,float> CalculateDodgeDirection(PlaneBehaviour otherPlane)
    {
        Vector3 dodgeLeft = Quaternion.Euler(0, -90, 0) * speedDirectionnalVector;
        Vector3 dodgeRight = Quaternion.Euler(0, 90, 0) * speedDirectionnalVector;

        float criticalityLeft = CalculcateCriticalityWithOtherAgent(currentPosition, currentTargetPosition, dodgeLeft, trajectory);
        float criticalityRight = CalculcateCriticalityWithOtherAgent(currentPosition, currentTargetPosition, dodgeRight, trajectory);
        float criticalityCoef = CalculcateCriticalityWithOtherAgent(currentPosition, currentTargetPosition, dodgeRight, trajectory);

        return new Tuple<Vector3,float> ((criticalityLeft < criticalityRight ? dodgeLeft : dodgeRight), criticalityCoef);
    }

    private void AdjustTrajectory(Vector3 dodgeDirection)
    {
        alternativeTrajectories = new List<Vector3>(trajectory);
        if (idxTraj + 1 < alternativeTrajectories.Count)
        {
            alternativeTrajectories[idxTraj + 1] = currentPosition + dodgeDirection * 50f;
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
