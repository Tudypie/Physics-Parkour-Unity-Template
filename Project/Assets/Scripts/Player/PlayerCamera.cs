using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    public Transform playerBaseTrans;
    public Transform trans;

    public Transform cameraXTarget;
    public Transform cameraYTarget;

    public Transform modelHolder;

    [Header("Movement and Positioning")]
    public float rotationSpeed = 2.5f;
    [Range(0, .99f)]
    public float thirdPersonSmoothing = .25f;
    public float wallMargin = .5f;
    [Range(0, .99f)]
    public float firstPersonSmoothing = .8f;
    public Vector3 firstPersonLocalPosition = new Vector3(0.5f, .4f, 0);
    public Vector3 thirdPersonLocalOrbitPosition = new Vector3(0.5f, .4f, 0);

    [Header("Bounds")]
    public float minThirdPersonDistance = 5;
    public float maxThirdPersonDistance = 42;
    public float thirdPersonDistance = 28;
    public float scrollSensivity = 8;
    public int xLookingDown = 65;
    public int xLookingUp= 310;
    
    [Header("Misc")]
    public LayerMask thirdPersonRayLayermask;
    public KeyCode modeToggleHotkey = KeyCode.C;
    public KeyCode mouseCursorShowHotkey = KeyCode.V;
    public bool firstPerson = true;
    private bool showingMouseCursor = false;
    private Vector3 thirdPersonTargetPosition;
    
    private Vector3 OrbitPoint
    {
        get
        {
            return modelHolder.TransformPoint(thirdPersonLocalOrbitPosition);
        }
    }

    private Quaternion TargetRotation
    {
        get
        {
            return Quaternion.Euler(cameraXTarget.
            eulerAngles.x, cameraYTarget.eulerAngles.y, 0);
        }
    }
    
    private Vector3 TargetForwardDirection
    {
        get
        {
            return TargetRotation * Vector3.forward;
        }
    }
    

    void Start()
    {
        SetMouseShowing(false);  
    }
 
    void Update()
    {
        Hotkeys();
    }

    void LateUpdate()
    {
        if(!showingMouseCursor){
            UpdateTargetRotation();
        }

        if(firstPerson){
            FirstPerson();
        }
        else{
            ThirdPerson();
        }

    }

    void Hotkeys()
    {
        //first and third person toggle
        if(Input.GetKeyDown(modeToggleHotkey))
        {
            firstPerson = !firstPerson;
        }

        //mouse mode toggle
        if(Input.GetKeyDown(mouseCursorShowHotkey))
        {
            SetMouseShowing(true);
        }
        if(Input.GetKeyUp(mouseCursorShowHotkey))
        {
            SetMouseShowing(false);
        }

        if(!firstPerson)
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

            thirdPersonDistance = Mathf.Clamp(thirdPersonDistance - scrollDelta * scrollSensivity, minThirdPersonDistance, maxThirdPersonDistance);
        }
    }

    void SetMouseShowing(bool value)
    {
        Cursor.visible = value;
        showingMouseCursor = value;

        if(value)
            Cursor.lockState = CursorLockMode.None;
        else   
            Cursor.lockState = CursorLockMode.Locked;
    }

    void UpdateTargetRotation()
    {
        float xRotation = (Input.GetAxis("Mouse Y") * -rotationSpeed);
        float yRotation = (Input.GetAxis("Mouse X") * rotationSpeed);

        cameraXTarget.Rotate(xRotation, 0, 0);
        cameraYTarget.Rotate(0, yRotation, 0);

        if(cameraXTarget.localEulerAngles.x >= 180)
        {
            if(cameraXTarget.localEulerAngles.x < xLookingUp)
                cameraXTarget.localEulerAngles = new Vector3(xLookingUp, 0, 0);
        }
        else
        {
            if(cameraXTarget.localEulerAngles.x > xLookingDown)
                cameraXTarget.localEulerAngles = new Vector3(xLookingDown, 0, 0);
        }
    }

    void FirstPerson()
    {
        Vector3 targetWorldPosition = modelHolder.TransformPoint(firstPersonLocalPosition);

        if(trans.position != targetWorldPosition)
        {
            trans.position = Vector3.Lerp(trans.position, targetWorldPosition, .2f);
        }

        Quaternion targetRotation = Quaternion.Slerp(trans.rotation, TargetRotation, 1.0f - firstPersonSmoothing);

        trans.eulerAngles = new Vector3(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0);

        modelHolder.forward = new Vector3(trans.forward.x, 0, trans.forward.z);
    }

    void ThirdPerson()
    {
        Ray ray = new Ray(OrbitPoint, -TargetForwardDirection);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, thirdPersonDistance + wallMargin, thirdPersonRayLayermask.value))
        {
            thirdPersonTargetPosition = hit.point;
            thirdPersonTargetPosition += (TargetForwardDirection * wallMargin);
        }
        else
        {
            thirdPersonTargetPosition = OrbitPoint - (TargetForwardDirection * thirdPersonDistance);
        }

        trans.position = Vector3.Lerp(trans.position, thirdPersonTargetPosition, 1.0f - thirdPersonSmoothing);
        
        trans.forward = (OrbitPoint - trans.position).normalized;

        modelHolder.forward = new Vector3(trans.forward.x, 0, trans.forward.z);
    }
}
