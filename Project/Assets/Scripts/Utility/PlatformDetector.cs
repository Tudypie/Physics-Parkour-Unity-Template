using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this script has to be added on every movable prop
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
