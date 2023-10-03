using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class SpawnTrajectory : MonoBehaviour
{
    private Vector3 contactPoint;
    private List<Vector3> trajectory = new List<Vector3>();

    [SerializeField] 
    int nbPointsTrajectory;
    [SerializeField] 
    float angleMax;

    void Awake(){
        
        trajectory.Add(this.transform.position);

        Vector3 v = contactPoint - this.transform.position;

        Vector3 offset = v/nbPointsTrajectory;
        
        for(int i = 1; i < nbPointsTrajectory - 1; i++){


            Vector3 out_ = Vector3.zero;
            if(i == 1)
            {    
                out_ = MathUtilities.Random(this.transform.position, this.transform.position + offset*i);
            }
            else{
                out_ = MathUtilities.Random(trajectory[i-1], this.transform.position + offset*i);
            }

            
            trajectory.Add(out_);


        }

        trajectory.Add(contactPoint);

        for(int i = nbPointsTrajectory -2 ; i >=0  ; i--)
        {
            trajectory.Add(-trajectory[i]);
        }

        trajectory.Add(-this.transform.position);
        this.GetComponent<PlaneBehaviour>().trajectory = this.trajectory;
        this.GetComponent<LineRenderer>().positionCount = trajectory.Count;
        this.GetComponent<LineRenderer>().SetPositions(trajectory.ToArray());
    }
}
