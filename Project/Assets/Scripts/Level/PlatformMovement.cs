using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMovement : MonoBehaviour
{
    public enum State
    {
        Stationary,
        MovingToTarget,
        MovingToInitial
    }

    [SerializeField] Transform trans;
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 targetPosition;
    [SerializeField] float timeToChangePosition = 3;
    [SerializeField] float stationaryTime = 1f;
    [SerializeField] bool controlledByButton;

    private float TravelSpeed
    {
        get
        {
            return Vector3.Distance(initialPosition, targetPosition) / timeToChangePosition;
        }
    }

    private Vector3 CurrentDestination
    {
        get
        {
            if(state == State.MovingToInitial)
                return initialPosition;
            else
                return targetPosition;
        }
    }

    private float DistanceToDestination
    {
        get
        {
            return Vector3.Distance(trans.position, CurrentDestination);
        }
    }

    private Vector3 initialPosition;

    [HideInInspector] public State state = State.Stationary;
    [HideInInspector] public State nextState = State.MovingToTarget;

    public void GoToNextState()
    {
        state = nextState;
    }

    void Start()
    {
        initialPosition = trans.position;

        if(!controlledByButton)
            Invoke("GoToNextState", stationaryTime);
    }

    
    void FixedUpdate()
    {
        if(state != State.Stationary)
        {
            rb.velocity = (CurrentDestination - trans.position).normalized * TravelSpeed;

            float distanceMovedThisFrame = (rb.velocity * Time.deltaTime).magnitude;

            if(distanceMovedThisFrame >= DistanceToDestination)
            {
                rb.velocity = Vector3.zero;
                trans.position = CurrentDestination;

                if(state == State.MovingToInitial)
                    nextState = State.MovingToTarget;
                else
                    nextState = State.MovingToInitial;

                state = State.Stationary;
                if(!controlledByButton)
                    Invoke("GoToNextState", stationaryTime);
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }
}
