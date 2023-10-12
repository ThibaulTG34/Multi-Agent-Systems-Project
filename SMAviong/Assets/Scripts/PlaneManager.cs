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
        y = Random.Range(-5 + 1.2f, 5 + 1.2f);
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

    private List<Vector3> spawnedPlanesPositions = new List<Vector3>();

    private bool IsPointSafe(Vector3 point, float safeDistance)
    {
        foreach (Vector3 spawnedPoint in spawnedPlanesPositions)
        {
            if (Vector3.Distance(point, spawnedPoint) < safeDistance)
            {
                return false;
            }
        }
        return true;
    }


    void SpawnPlane(float halfSide, bool isXAxis, bool isPositive)
    {
        float x, z;
        Vector3 spawnPoint;
        int maxAttempts = 10;  // Définir un maximum d'essais pour éviter une boucle infinie
        int currentAttempt = 0;

        do
        {
            x = isXAxis ? (isPositive ? halfSide : -halfSide) : Random.Range(-halfSide, halfSide);
            z = isXAxis ? Random.Range(-halfSide, halfSide) : (isPositive ? halfSide : -halfSide);
            spawnPoint = new Vector3(x, this.y, z);
            currentAttempt++;
        }
        while (!IsPointSafe(spawnPoint, 50f) && currentAttempt < maxAttempts);

        // Si après maxAttempts, nous ne trouvons pas de point sûr, nous annulons le spawn.
        if (currentAttempt == maxAttempts)
        {
            Debug.LogWarning("Couldn't find a safe point to spawn plane.");
            return;
        }


        GameObject plane = Instantiate(prefab, spawnPoint, Quaternion.identity);

        // Ensure the spawned plane has LineRenderer and PlaneBehaviour components.s
        LineRenderer lineRenderer = plane.GetComponentsInChildren<LineRenderer>()[0];
        PlaneBehaviour planeBehaviour = plane.GetComponentsInChildren<PlaneBehaviour>()[0];

        Vector3 finalPoint = GetOppositePoint(spawnPoint,halfSide);
        List<Vector3> trajectory = GenerateTrajectory(spawnPoint, finalPoint , nbPointsTrajectory);

        planeBehaviour.trajectory = trajectory;
        lineRenderer.positionCount = trajectory.Count;
        lineRenderer.SetPositions(trajectory.ToArray());

        spawnedPlanesPositions.Add(spawnPoint);
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
