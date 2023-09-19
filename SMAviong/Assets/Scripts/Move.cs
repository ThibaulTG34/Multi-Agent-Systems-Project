using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Transform avion;
    public Transform target;
    public float speed;

    void Start()
    {
        
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        avion.position = Vector3.MoveTowards(avion.position, target.position, step);

        if(avion.position == target.position)
        {
            Destroy(this.gameObject);
        }
    }
}
