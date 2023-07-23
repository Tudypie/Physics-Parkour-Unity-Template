using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Refereces")]
    public Transform trans;
    public Transform modelHolder;
    public CharacterController charController;

    [Header("Gravity")]
    public float maxGravity = 92;
    public float timeToMaxGravity = .6f;
    public float GravityPerSecond
    {
        get
        {
            return maxGravity / timeToMaxGravity;
        }
    }

    private float yVelocity = 0;

    [Header("Movement")]
    public float movespeed = 42;
    public float timeToMaxSpeed = .3f;
    public float timeToLoseMaxSpeed = .2f;
    public float reverseMomentumMultiplier = .6f;
    public float midairMovementMultiplier = .4f;
    public float bounciness = .2f;
    private Vector3 localMovementDirection = Vector3.zero;
    private Vector3 worldVelocity = Vector3.zero;
    private bool grounded = false;

    public float VelocityGainPerSecond
    {
        get
        {
            if(grounded)
                return movespeed / timeToMaxSpeed;
            else
                return (movespeed / timeToMaxSpeed) * midairMovementMultiplier;
        }
    }

    public float VelocityLossPerSecond
    {
        get
        {
            return movespeed / timeToLoseMaxSpeed;
        }
    }

    [Header("Jumping")]
    public float jumpPower = 76;

    [Header("Wall Jumping")]
    public float wallJumpPower = 40;
    public float wallJumpAir = 56;
    public float wallDetectionRange = 2.4f;
    public float wallJumpCooldown = .3f;
    public LayerMask wallDetectionLayerMask;
    private float lastWallJumpTime;

    private bool WallJumpIsOffCooldown
    {
        get
        {
            return Time.time > lastWallJumpTime + wallJumpCooldown;
        }
    }

    private bool WallIsNearby()
    {
        return Physics.OverlapBox(
            trans.position + Vector3.up * (charController.height * .5f),
            Vector3.one * wallDetectionRange,
            modelHolder.rotation,
            wallDetectionLayerMask.value).Length > 0;
    }

    void Movement()
    {
        localMovementDirection = Vector3.zero;

        //Right and left
        if(Input.GetKey(KeyCode.D))
            localMovementDirection.x = 1;
        else if(Input.GetKey(KeyCode.A))
            localMovementDirection.x = -1;

        //Forward and back
        if(Input.GetKey(KeyCode.W))
            localMovementDirection.z = 1;
        else if(Input.GetKey(KeyCode.S))
            localMovementDirection.z = -1;

        if(localMovementDirection != Vector3.zero)
        {
            Vector3 WorldMovementDirection = modelHolder.TransformDirection(localMovementDirection.normalized);

            float multiplier = 1;
            float dot = Vector3.Dot(WorldMovementDirection.normalized, worldVelocity.normalized);

            if(dot < 0)
                multiplier += -dot * reverseMomentumMultiplier;

            Vector3 newVelocity = worldVelocity + WorldMovementDirection * 
            VelocityGainPerSecond * multiplier * Time.deltaTime;

            if(worldVelocity.magnitude > movespeed)
                worldVelocity = Vector3.ClampMagnitude(newVelocity, worldVelocity.magnitude);
            else
                worldVelocity = Vector3.ClampMagnitude(newVelocity, movespeed);
        }
    }

    void VelocityLoss()
    {
        if(grounded && (localMovementDirection == Vector3.zero || worldVelocity.magnitude > movespeed))
        {
            float velocityLoss = VelocityLossPerSecond * Time.deltaTime;

            if(velocityLoss > worldVelocity.magnitude)
                worldVelocity = Vector3.zero;
            else    
                worldVelocity -= worldVelocity.normalized * velocityLoss;
        }
    }

    void Gravity()
    {
        if(!grounded && yVelocity > -maxGravity)
            yVelocity = Mathf.Max(yVelocity - GravityPerSecond * Time.
            deltaTime, -maxGravity);
    }

    void WallJumping()
    {
        if(!grounded && WallJumpIsOffCooldown)
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                if(WallIsNearby())
                {
                    if(localMovementDirection != Vector3.zero)
                    {
                        worldVelocity = modelHolder.TransformDirection(localMovementDirection) * wallJumpPower;

                        if(yVelocity <= 0)
                            yVelocity = wallJumpAir;
                        else
                            yVelocity += wallJumpAir;

                        lastWallJumpTime = Time.time;
                    }
                }
            }
        }
    }

    void Jumping()
    {
        if(grounded && Input.GetKeyDown(KeyCode.Space))
        {
            yVelocity = jumpPower;
            grounded = false;
        }
    }

    void ApplyVelocity()
    {
        if(grounded)
            yVelocity = -1;

        Vector3 movementThisFrame = (worldVelocity + (Vector3.up * yVelocity)) * Time.deltaTime;
        Vector3 predictedPosition = trans.position + movementThisFrame;

        if(movementThisFrame.magnitude > .03f)
            charController.Move(movementThisFrame);

        //Checking ground state:
        if(!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Below))
            grounded = true;
        else if(grounded && !charController.collisionFlags.HasFlag(CollisionFlags.Below))
            grounded = false;

        //Bounce of walls:
        if(!grounded && charController.collisionFlags.HasFlag(CollisionFlags.Sides))
            worldVelocity = (trans.position - predictedPosition).normalized * (worldVelocity.magnitude *bounciness);
        
        //Lose Y velocity if player goes up and collides with something above
        if(yVelocity > 0 && charController.collisionFlags.HasFlag(CollisionFlags.Above))
            yVelocity = 0;
    }

    
    public void AddVelocity(Vector3 amount)
    {
        worldVelocity += new Vector3(amount.x, 0, amount.z);

        yVelocity += amount.y;

        if(yVelocity > 0)
            grounded = false;
    }
   
    void Update()
    {
        Movement();
        VelocityLoss();
        Gravity();
        WallJumping();
        Jumping();
        ApplyVelocity();
    }
}
