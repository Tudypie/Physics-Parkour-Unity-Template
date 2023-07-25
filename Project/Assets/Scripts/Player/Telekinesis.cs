using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Telekinesis : MonoBehaviour
{
    public enum State
    {
        Idle,
        Pushing,
        Pulling
    }

    public State state = State.Idle;
    
    [Header("References")]
    [SerializeField] Transform baseTrans;
    [SerializeField] Camera cam;
    [SerializeField] Image cursor;
    [SerializeField] AudioSource telekinesisAudio;

    [Header("Settings")]
    [SerializeField] float pullForce = 60;
    [SerializeField] float pushForce = 60;
    [SerializeField] float range = 70;
    [SerializeField] LayerMask detectionLayerMask;

    private Transform target;
    private Transform lastTarget;
    private Vector3 targetHitPoint;
    private Rigidbody targetRigidbody;
    private bool targetIsOutsideRange = false;

    private Color CursorColor
    {
        get
        {
            if(state == State.Idle)
                if(target == null)
                    return Color.grey;
                else if(targetIsOutsideRange)
                    return new Color(1, .6f, 0);
                else   
                    return Color.white;
            else 
                return Color.green;
        }
    }

    void TargetDetection()
    {
        var ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, detectionLayerMask.value))
        {
            if(hit.rigidbody != null && !hit.rigidbody.isKinematic)
            {
                if(lastTarget != null && lastTarget != hit.transform)
                    ClearTarget();
                    
                target = hit.transform;
                lastTarget = target;
                targetRigidbody = hit.rigidbody;
                targetHitPoint = hit.point;

                if(target.GetComponent<Outline>() == null)
                    target.gameObject.AddComponent(typeof(Outline));
                else
                    target.GetComponent<Outline>().enabled = true;

                if(Vector3.Distance(baseTrans.position, hit.point) > range)
                    targetIsOutsideRange = true;
                else   
                    targetIsOutsideRange = false;
            }
            else
            {
                ClearTarget();
            }
        }
        else
        {
            ClearTarget();
        }
    }

    void ClearTarget()
    {
        if(target != null)
        {
            if(target.GetComponent<Outline>() != null)
                target.GetComponent<Outline>().enabled = false;
        }

        target = null;
        targetRigidbody = null;
        targetIsOutsideRange = false;
    }

    void PullingAndPushing()
    {
        if(target == null || targetIsOutsideRange)
        {
            state = State.Idle;
            return;
        }

        if(!Input.anyKey)
        {
            state = State.Idle;
            return;
        }

        if(Input.GetMouseButton(0))
        {
            targetRigidbody.AddForce((baseTrans.position - targetHitPoint).normalized * pullForce, ForceMode.Acceleration);
            state = State.Pulling;
        }
        else if(Input.GetMouseButton(1))
        {
            targetRigidbody.AddForce((targetHitPoint - baseTrans.position).normalized * pushForce, ForceMode.Acceleration);
            state = State.Pushing;
        }
    }

    void Update()
    {
        cursor.color = CursorColor;
        TargetDetection();

        if(state == State.Idle)
            telekinesisAudio.mute = true;
        else
            telekinesisAudio.mute = false;
    }

    void FixedUpdate()
    {
        PullingAndPushing();
    }
}
