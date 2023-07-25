using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attach this script to any movable object
[RequireComponent(typeof(SphereCollider))]
public class PlatformDetector : MonoBehaviour
{   
    [HideInInspector] public Transform platform = null;
    private Vector3 platformPreviousPosition;
    private bool firstPositionLogged = false;
    
    void FixedUpdate()
    {
        if(platform != null)
        {
            if(firstPositionLogged && platformPreviousPosition != platform.position)
            {
                transform.position += platform.position - platformPreviousPosition;
            }

            platformPreviousPosition = platform.position;
            firstPositionLogged = true;
        }
        else
        {
            firstPositionLogged = false;
        }


    }
}
