using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RouletteWheelController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform ball;
    public Rigidbody ballRigidbody;
    public SphereCollider ballCollider;     
    public PhysicMaterial ballInSlotColliderMaterial;     
    public PhysicMaterial ballRotateColliderMaterial;     
    public Transform wheel;
    public Transform wheelCenter;

    [Header("Ayarlar")]
    public Transform spinStartPos;
    public float wheelSpinSpeed = 90f;
    public float maxOrbitDuration = 25f;
    public float minOrbitDuration = 15f;
    
    [Header("Top Ayarları")]
    public float initialBallSpeed = 800f;
    public float tangentialForce = 200f;
    public float centerForce = 150f;
    public float slowdownRate = 0.95f;
    public float minVelocityToStop = 0.5f;
    public float optimalDistance = 2.0f;
    public float distanceForceMultiplier = 10f;
    
    [Header("Hedef Noktası Ayarları")]
    public bool isDeterministic = true;
    public int currentTargetNumber=0;
    public float targetForceStartTime = 0.7f;
    public float initialTargetForce = 50f;
    public float targetForceGrowthRate = 1.5f;
    public float maxTargetForce = 500f;
    public AnimationCurve targetForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    public float upwardForceInterval = 0.5f;      // Yukarı doğru kuvvetin uygulanma sıklığı (saniye)
    public float upwardForceMagnitude = 50f;      // Yukarı doğru kuvvetin şiddeti
    public float targetDistanceForceMultiplier = 2f;  // Hedefe yaklaştıkça kuvveti artırma çarpanı
    public float minTargetDistance = 0.3f;        // Kuvvet artışının başlayacağı minimum mesafe
    
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
            ball.gameObject.SetActive(false);
            ball.position = spinStartPos.position;
            ball.gameObject.SetActive(true);
            
            int chosenNumber = isDeterministic ? currentTargetNumber : Random.Range(0, 37);
            StartCoroutine(RouletteSequence(chosenNumber));
        }
    }

    private IEnumerator RouletteSequence(int chosenNumber)
    {
        targetPosition = _numberController.GetNumberTransform(chosenNumber);
        Debug.Log($"Chosen Number: {chosenNumber}");
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        float orbitDuration = Random.Range(minOrbitDuration, maxOrbitDuration);
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
        float distanceToCenter = toCenter.magnitude;
        
        Vector3 tangentDirection = Vector3.Cross(Vector3.up, toCenter.normalized);
        tangentDirection.y = -0.1f;
        float distanceDiff = distanceToCenter - optimalDistance;
        Vector3 centerwardForce = toCenter.normalized * distanceDiff * distanceForceMultiplier;
        
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
        
        ballRigidbody.AddForce(tangentDirection * currentTangentialForce, ForceMode.Force);
        ballRigidbody.AddForce(centerwardForce, ForceMode.Force);
    }
    
    private void ApplyTargetForce()
    {
        if (targetPosition == null) return;
        
        Vector3 toTarget = targetPosition.position - ball.position;
        
        toTarget.y = Mathf.Max(0, toTarget.y);
        
        float distanceToTarget = toTarget.magnitude;
        
        targetForceTime += Time.fixedDeltaTime;
        float normalizedTime = Mathf.Clamp01(targetForceTime / targetForceMaxTime);
        
        float forceMultiplier = targetForceCurve.Evaluate(normalizedTime);
        
        float currentTargetForce = Mathf.Lerp(initialTargetForce, maxTargetForce, forceMultiplier);
        
        // Hedefe yaklaştıkça kuvveti artır
        if (distanceToTarget < 1.0f)
        {
            // Mesafe azaldıkça kuvvet artar
            float distanceFactor = Mathf.Clamp01(1.0f - (distanceToTarget - minTargetDistance) / (1.0f - minTargetDistance));
            currentTargetForce *= (1.0f + distanceFactor * targetDistanceForceMultiplier);
        }
        
        // Eğer hedefe çok yaklaşıldıysa (0.2f'den daha yakın), kuvveti azalt
        // Bu, topun hedefe sert çarpmasını ve sekip gitmesini önler
        if (distanceToTarget < 0.2f)
        {
            currentTargetForce *= distanceToTarget * 5;
        }
        
        ballRigidbody.AddForce(toTarget.normalized * currentTargetForce, ForceMode.Force);
        
        if (distanceToTarget < 0.1f && ballRigidbody.velocity.magnitude < 1.0f)
        {
            ballRigidbody.velocity = Vector3.zero;
            ballRigidbody.angularVelocity = Vector3.zero;
            ballRigidbody.drag = 11;
            ballRigidbody.angularDrag = 11;
            ball.parent = wheel;
            isSpinning = false;
            Invoke("DisableTargeting",5);
            SetColliderMaterial(true);
        }
    }
    
    private void DisableTargeting()
    {
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
        ballCollider.material = isInSlot? ballInSlotColliderMaterial:ballRotateColliderMaterial;
    }

    private float EaseOut(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}