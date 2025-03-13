using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RouletteWheelController : MonoBehaviour
{
    public Transform ball;
    public Rigidbody ballRigidbody;
    public SphereCollider ballCollider;     
    public PhysicMaterial ballInSlotColliderMaterial;     
    public PhysicMaterial ballRotateColliderMaterial;     
    public Transform wheel;
    public Transform wheelCenter;

    [Header("Settings")]
    public float wheelSpinSpeed = 90f;
    public float maxOrbitDuration = 25f;
    public float minOrbitDuration = 15f;
    
    [Header("Ball Settings")]
    public float initialBallSpeed = 800f;
    public float tangentialForce = 200f;
    public float slowdownRate = 0.95f;
    public float minVelocityToStop = 0.5f;
    
    [Header("Gravity Settings")]
    public float downwardForce = 25f;
    
    [Header("Target Point Settings")]
    public bool isDeterministic = true;
    public int currentTargetNumber = 0;
    public float initialTargetForce = 80f;
    public float maxTargetForce = 500f;
    public AnimationCurve targetForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    public float upwardForceInterval = 0.5f;
    public float upwardForceMagnitude = 50f;
    public float targetDistanceForceMultiplier = 3f;
    public float minTargetDistance = 0.3f;
    
    private bool isSpinning = false;
    private bool isSlowingDown = false;
    private bool isTargetingEnabled = false;
    private float currentTangentialForce;
    private float targetForceTime = 0f;
    private float targetForceMaxTime = 2f;
    private float upwardForceTimer = 0f;
    
    private Transform targetPosition;
    private RouletteWheelNumberController _numberController;

    private void Awake()
    {
        _numberController = GetComponent<RouletteWheelNumberController>();
    }

    private void Start()
    {
        if (ballRigidbody != null)
        {
            ballRigidbody.useGravity = true;
            ballRigidbody.drag = 0.2f;
            ballRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            ballRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    
    private void FixedUpdate()
    {
        if (isSpinning)
        {
            ApplyConstantForces();
            ApplyPeriodicUpwardForce();
        }
        if (isTargetingEnabled)
        {
            ApplyTargetForce();
        }
    }
    
    private void ApplyPeriodicUpwardForce()
    {
        upwardForceTimer += Time.fixedDeltaTime;
        
        if (upwardForceTimer >= upwardForceInterval)
        {
            ballRigidbody.AddForce(Vector3.up * upwardForceMagnitude, ForceMode.Impulse);
            upwardForceTimer = 0f;
        }
    }
    
    [ContextMenu("Spin")]
    public void StartRoulette()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            isSlowingDown = false;
            isTargetingEnabled = false;
            targetForceTime = 0f;
            upwardForceTimer = 0f;
            currentTangentialForce = tangentialForce;
            ballRigidbody.drag = 0;
            ballRigidbody.angularDrag = 0;
    
            SetColliderMaterial(false);
            ball.parent = transform;
            //ball.position = spinStartPos.position;
            
            int chosenNumber = isDeterministic ? currentTargetNumber : UnityEngine.Random.Range(0, 37);
            currentTargetNumber = chosenNumber;
            StartCoroutine(RouletteSequence(chosenNumber));
        }
    }

    private IEnumerator RouletteSequence(int chosenNumber)
    {
        targetPosition = _numberController.GetNumberTransform(chosenNumber);
        Debug.Log($"Chosen Number: {chosenNumber}");
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        float orbitDuration = UnityEngine.Random.Range(minOrbitDuration, maxOrbitDuration);
        StartCoroutine(SpinWheel(orbitDuration));
        
        Vector3 directionFromCenter = (ball.position - wheelCenter.position).normalized;
        Vector3 tangentDirection = Vector3.Cross(Vector3.up, directionFromCenter);
        
        ballRigidbody.AddForce(tangentDirection * initialBallSpeed, ForceMode.Impulse);
        
        yield return new WaitForSeconds(orbitDuration * 0.3f);
        
        isSlowingDown = true;
        if (targetPosition != null)
        {
            isTargetingEnabled = true;
        }
        
        while (isSpinning)
        {
            yield return null;
        }
    }
    
    private void ApplyConstantForces()
    {
        Vector3 toCenter = wheelCenter.position - ball.position;
        toCenter.y = 0;
        
        Vector3 tangentDirection = Vector3.Cross(Vector3.up, toCenter.normalized);
        
        // DownForce
        ballRigidbody.AddForce(Vector3.down * downwardForce, ForceMode.Force);
        
        if (isSlowingDown)
        {
            currentTangentialForce *= slowdownRate * Time.fixedDeltaTime;
            
            if (ballRigidbody.velocity.magnitude < minVelocityToStop && !isTargetingEnabled)
            {
                isSpinning = false;
                ballRigidbody.velocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
                return;
            }
        }
        
        // SpinForce
        ballRigidbody.AddForce(tangentDirection * currentTangentialForce, ForceMode.Force);
    }
    
    private void ApplyTargetForce()
    {
        if (targetPosition == null) return;
        
        Vector3 toTarget = targetPosition.position - ball.position;
        
        toTarget.y = Mathf.Max(0, toTarget.y * 0.7f);
        
        float distanceToTarget = toTarget.magnitude;
        
        targetForceTime += Time.fixedDeltaTime;
        float normalizedTime = Mathf.Clamp01(targetForceTime / targetForceMaxTime);
        
        float forceMultiplier = targetForceCurve.Evaluate(normalizedTime);
        
        float currentTargetForce = Mathf.Lerp(initialTargetForce, maxTargetForce, forceMultiplier);
        
        // Force increase
        if (distanceToTarget < 1.0f)
        {
            float distanceFactor = Mathf.Clamp01(1.0f - (distanceToTarget - minTargetDistance) / (1.0f - minTargetDistance));
            currentTargetForce *= (1.0f + distanceFactor * targetDistanceForceMultiplier);
        }
        
        if (distanceToTarget < 0.15f)
        {
            currentTargetForce *= distanceToTarget * 6;
            
            //down force for in the the slot 
            ballRigidbody.AddForce(Vector3.down * downwardForce * 2, ForceMode.Force);
        }
        
        ballRigidbody.AddForce(toTarget.normalized * currentTargetForce, ForceMode.Force);
        
        // Slot Check
        if (distanceToTarget < 0.08f && ballRigidbody.velocity.magnitude < 0.8f)
        {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.drag = 11;
            ballRigidbody.angularDrag = 11;
            ball.parent = wheel;
            isSpinning = false;
            Invoke("DisableTargeting", 5);
            SetColliderMaterial(true);
        }
    }
    
    private void DisableTargeting()
    {
        if (!isTargetingEnabled)
            return;
        
        isTargetingEnabled = false;
        
        int winningNumber = currentTargetNumber;
        Debug.Log($"Ball landed on number: {winningNumber}");
    
        EventManager.TriggerEvent(GameEvents.OnSpinFinished, winningNumber);
    }
    
    public void ResetRoulette()
    {
        isSpinning = false;
        isSlowingDown = false;
        isTargetingEnabled = false;
        

    

    }
    
    private IEnumerator SpinWheel(float totalDuration)
    {
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            float t = Mathf.Clamp01(elapsed / totalDuration);
            float easedFactor = 1 - EaseOut(t);
            float currentSpeed = wheelSpinSpeed * easedFactor;
            wheel.Rotate(0, currentSpeed * Time.deltaTime, 0, Space.Self);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    private void SetColliderMaterial(bool isInSlot)
    {
        ballCollider.material = isInSlot ? ballInSlotColliderMaterial : ballRotateColliderMaterial;
    }

    private float EaseOut(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}