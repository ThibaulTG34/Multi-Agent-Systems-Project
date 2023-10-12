using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashDetector : MonoBehaviour
{
    [SerializeField] GameObject parent;

    void OnTriggerEnter(Collider other)
    {
        CrashDetector otherCrashdetector = other.GetComponent<CrashDetector>();
        if (otherCrashdetector != null)
        {
            Debug.Log("AVION CRASH BOUM BOUMM");
            Destroy(parent);
        }
    }
}
