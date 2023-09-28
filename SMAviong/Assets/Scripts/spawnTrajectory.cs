using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class SpawnTrajectory : MonoBehaviour
{
    private Vector3 finalPoint;
    private List<Vector3> trajectory = new List<Vector3>();

    [SerializeField] 
    int nbPointsTrajectory;
    [SerializeField] 
    float angleMax;

    void Awake(){
        
        trajectory.Add(this.transform.position);

        Vector3 v = finalPoint - this.transform.position;

        Vector3 offset = v/nbPointsTrajectory;
        
        for(int i = 1; i < nbPointsTrajectory - 1; i++){
            
            // float newAngleDir = (Random.Range(-angleMax, angleMax));
            
            // Debug.Log(newAngleDir);

            Vector3 out_ = Vector3.zero;
            if(i == 1)
            {    
                out_ = MathUtilities.Random(this.transform.position, this.transform.position + offset*i);
            }
            else{
                out_ = MathUtilities.Random(trajectory[i-1], this.transform.position + offset*i);
            }

            
            trajectory.Add(out_);

            // currentDirection = currentDirection - finalPoint;

        }

        trajectory.Add(finalPoint);

        this.GetComponent<PlaneBehaviour>().trajectory = this.trajectory;
        this.GetComponent<LineRenderer>().positionCount = trajectory.Count;
        // Debug.Log("count" + trajectory.Count);
        this.GetComponent<LineRenderer>().SetPositions(trajectory.ToArray());
    }
}
