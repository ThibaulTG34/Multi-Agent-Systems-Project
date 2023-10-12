using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    [Header("Spawn Configuration")]
    public GameObject prefab;
    public int nbObjects = 10;
    public float sideSize = 1000f;
    public float y = 1.2f;

    [Header("Trajectory Configuration")]
    public int nbPointsTrajectory = 2;

    void Start()
    {
        float halfSide = sideSize / 2;
        int planesPerSide = nbObjects / 4;

        for (int i = 0; i < planesPerSide; i++)
        {
            SpawnPlane(halfSide, true, true);
            SpawnPlane(halfSide, false, true);
            SpawnPlane(halfSide, true, false);
            SpawnPlane(halfSide, false, false);
        }
    }

    void SpawnPlane(float halfSide, bool isXAxis, bool isPositive)
    {
        float x = isXAxis ? (isPositive ? halfSide : -halfSide) : Random.Range(-halfSide, halfSide);
        float z = isXAxis ? Random.Range(-halfSide, halfSide) : (isPositive ? halfSide : -halfSide);

        Vector3 spawnPoint = new Vector3(x, y, z);
        GameObject plane = Instantiate(prefab, spawnPoint, Quaternion.identity);

        // Ensure the spawned plane has LineRenderer and PlaneBehaviour components.s
        LineRenderer lineRenderer = plane.GetComponentsInChildren<LineRenderer>()[0];
        PlaneBehaviour planeBehaviour = plane.GetComponentsInChildren<PlaneBehaviour>()[0];

        Vector3 finalPoint = GetOppositePoint(spawnPoint,halfSide);
        List<Vector3> trajectory = GenerateTrajectory(spawnPoint, finalPoint , nbPointsTrajectory);

        planeBehaviour.trajectory = trajectory;
        lineRenderer.positionCount = trajectory.Count;
        lineRenderer.SetPositions(trajectory.ToArray());
    }

    private List<Vector3> GenerateTrajectory(Vector3 startPoint, Vector3 endPoint, int pointsCount)
    {
        List<Vector3> trajectory = new List<Vector3>();
        trajectory.Add(startPoint);

        for (int i = 1; i < pointsCount - 1; i++)
        {
            float t = (float)i / (pointsCount - 1);
            Vector3 midPoint = Vector3.Lerp(startPoint, endPoint, t);
            trajectory.Add(midPoint);
        }

        trajectory.Add(endPoint);
        return trajectory;
    }


    private Vector3 GetOppositePoint(Vector3 point, float halfSide)
    {
        Vector3 oppositePoint = point;

        if (Mathf.Abs(point.x) == halfSide)
        {
            oppositePoint.x = -point.x;
            oppositePoint.z += Random.Range(-halfSide, halfSide) * Mathf.Tan(Mathf.Deg2Rad * Random.Range(-30, 30));
        }
        else if (Mathf.Abs(point.z) == halfSide)
        {
            oppositePoint.z = -point.z;
            oppositePoint.x += Random.Range(-halfSide, halfSide) * Mathf.Tan(Mathf.Deg2Rad * Random.Range(-30, 30));
        }
        return oppositePoint;
    }

}
