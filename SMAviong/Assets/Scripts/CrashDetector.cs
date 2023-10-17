using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] GameObject parent;
    [SerializeField] GameObject fx;

    private void Start()
    {
        fx = GameObject.FindGameObjectWithTag("fx");
    }

    void OnTriggerEnter(Collider other)
    {
        CrashDetector otherCrashdetector = other.GetComponent<CrashDetector>();
        if (otherCrashdetector != null)
        {
            Instantiate(fx, parent.transform.position, Quaternion.identity);
            Debug.Log("AVION CRASH BOUM BOUMM");
            Destroy(parent);
        }
    }
}
