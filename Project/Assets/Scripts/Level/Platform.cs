using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PlatformDetector detector = other.GetComponent<PlatformDetector>();
            
        if(detector != null)
            detector.platform = transform;
    }

    void OnTriggerExit(Collider other)
    {
        PlatformDetector detector = other.GetComponent<PlatformDetector>();

        if(detector != null)
            detector.platform = null;
    }
}
