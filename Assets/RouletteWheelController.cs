using System.Collections;
using UnityEngine;

public class RouletteWheelController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform ball;               // Topun Transform'u
    public Rigidbody ballRigidbody;      // Topun Rigidbody'si
    public SphereCollider ballCollider;     
    public PhysicMaterial ballInSlotColliderMaterial;     
    public PhysicMaterial ballRotateColliderMaterial;     
    public Transform wheel;              // Rulet tekerleğinin Transform'u
    public Transform wheelCenter;        // Rulet tekerleğinin merkez noktası

    [Header("Ayarlar")]
    public Transform spinStartPos;       // Topun spin başlangıç pozisyonu
    public float wheelSpinSpeed = 90f;   // Tekerleğin başlangıç dönüş hızı (derece/saniye)
    public float maxOrbitDuration = 25f;     // Topun dairesel hareket süresi
    public float minOrbitDuration = 15f;     // Topun dairesel hareket süresi
    
    [Header("Top Ayarları")]
    public float initialBallSpeed = 800f;   // Topun başlangıç açısal hızı
    public float tangentialForce = 200f;    // Sürekli uygulanan teğetsel kuvvet
    public float centerForce = 150f;        // Merkeze doğru uygulanan kuvvet
    public float slowdownRate = 0.95f;      // Topun zaman içinde yavaşlama oranı
    public float minVelocityToStop = 0.5f;  // Topun durması için gereken minimum hız
    public float optimalDistance = 2.0f;    // Topun merkeze ideal uzaklığı
    public float distanceForceMultiplier = 10f;  // Mesafeye göre kuvvet çarpanı
    
    [Header("Hedef Noktası Ayarları")]
    public Transform targetPosition;     // Topun yönlendirileceği hedef nokta
    public float targetForceStartTime = 0.7f;  // Hedef kuvvetin başlama zamanı (orbit süresinin yüzdesi)
    public float initialTargetForce = 50f;     // Başlangıç hedef kuvveti
    public float targetForceGrowthRate = 1.5f;  // Hedef kuvvetin büyüme oranı
    public float maxTargetForce = 500f;         // Maksimum hedef kuvveti
    public AnimationCurve targetForceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Hedef kuvvet eğrisi
    
    private bool isSpinning = false;
    private bool isSlowingDown = false;
    private bool isTargetingEnabled = false;
    private float currentTangentialForce;
    private float targetForceTime = 0f;
    private float targetForceMaxTime = 2f;  // Hedef kuvvetin maksimum uygulama süresi
    
    private void Start()
    {
        // Ball rigidbody özelliklerini ayarlayalım
        if (ballRigidbody != null)
        {
            ballRigidbody.useGravity = true;
            ballRigidbody.drag = 0.2f;  // Hava sürtünmesi
            ballRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            ballRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    
    private void FixedUpdate()
    {
        if (isSpinning)
        {
            ApplyConstantForces();
        }
        if (isTargetingEnabled)
        {
            ApplyTargetForce();
        }
    }
    
    // Sürekli kuvvet uygulama
    private void ApplyConstantForces()
    {
        // Merkeze olan vektör
        Vector3 toCenter = wheelCenter.position - ball.position;
        toCenter.y = 0; // Sadece yatay düzlemde
        float distanceToCenter = toCenter.magnitude;
        
        // Teğet yönde kuvvet hesaplama (merkez etrafında dönmesi için)
        Vector3 tangentDirection = Vector3.Cross(Vector3.up, toCenter.normalized);
        tangentDirection.y = -0.1f;
        // Merkeze olan mesafeye göre içeri/dışarı kuvvet uygulama
        float distanceDiff = distanceToCenter - optimalDistance;
        Vector3 centerwardForce = toCenter.normalized * distanceDiff * distanceForceMultiplier;
        
        // Yavaşlama modu aktifse, teğetsel kuvveti azalt
        if (isSlowingDown)
        {
            currentTangentialForce *= slowdownRate * Time.fixedDeltaTime;
            
            // Eğer yeterince yavaşladıysa, durdur
            if (ballRigidbody.velocity.magnitude < minVelocityToStop && !isTargetingEnabled)
            {
                isSpinning = false;
                ballRigidbody.velocity = Vector3.zero;
                ballRigidbody.angularVelocity = Vector3.zero;
                return;
            }
        }
        
        // Kuvvetleri uygula
        ballRigidbody.AddForce(tangentDirection * currentTangentialForce, ForceMode.Force);
        ballRigidbody.AddForce(centerwardForce, ForceMode.Force);
    }
    
    // Hedef noktasına doğru kuvvet uygulama
    private void ApplyTargetForce()
    {
        if (targetPosition == null) return;
        
        // Hedefe olan vektör
        Vector3 toTarget = targetPosition.position - ball.position;
        
        // Y eksenini biraz yukarı doğru ayarla ki top yüksekliğe de ulaşabilsin
        toTarget.y = Mathf.Max(0, toTarget.y);
        
        // Hedef ile topun arasındaki mesafe
        float distanceToTarget = toTarget.magnitude;
        
        // Hedef kuvvetin zamanla artması
        targetForceTime += Time.fixedDeltaTime;
        float normalizedTime = Mathf.Clamp01(targetForceTime / targetForceMaxTime);
        
        // Animasyon eğrisinden kuvvet çarpanı al
        float forceMultiplier = targetForceCurve.Evaluate(normalizedTime);
        
        // Zamana ve mesafeye bağlı olarak artan bir kuvvet hesapla
        float currentTargetForce = Mathf.Lerp(initialTargetForce, maxTargetForce, forceMultiplier);
        
        // Eğer hedefe çok yaklaşıldıysa, kuvveti azalt (doğal duruş için)
        if (distanceToTarget < 0.5f)
        {
            currentTargetForce *= distanceToTarget * 2;
        }
        
        // Hedef kuvveti uygula
        ballRigidbody.AddForce(toTarget.normalized * currentTargetForce, ForceMode.Force);
        
        // Eğer hedefe ulaşıldıysa ve top yeterince yavaşsa, tam hedef pozisyonuna taşı
        if (distanceToTarget < 0.1f && ballRigidbody.velocity.magnitude < 1.0f)
        {
            ball.position = targetPosition.position;
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

    // Bu metot, tüm süreci başlatır.
    [ContextMenu("Spin")]
    public void StartRoulette()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            isSlowingDown = false;
            isTargetingEnabled = false;
            targetForceTime = 0f;
            currentTangentialForce = tangentialForce;
            ballRigidbody.drag = 0;
            ballRigidbody.angularDrag = 0;
    
            SetColliderMaterial(false);
            ball.parent = transform;

            StartCoroutine(RouletteSequence());
        }
    }

    private IEnumerator RouletteSequence()
    {
        // Başlangıç pozisyonuna taşı
        ball.position = spinStartPos.position;
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        float orbitDuration = Random.Range(minOrbitDuration, maxOrbitDuration);
        // Wheel dönmeye başlasın
        StartCoroutine(SpinWheel(orbitDuration));
        
        // Topa başlangıç kuvveti uygula
        Vector3 directionFromCenter = (ball.position - wheelCenter.position).normalized;
        Vector3 tangentDirection = Vector3.Cross(Vector3.up, directionFromCenter);
        
        
        ballRigidbody.AddForce(tangentDirection * initialBallSpeed, ForceMode.Impulse);
        
        // Belli bir süre sonra yavaşlamaya başla
        yield return new WaitForSeconds(orbitDuration * 0.3f);
        
        isSlowingDown = true;
        // Hedefleme modunu aktifleştir
        if (targetPosition != null)
        {
            isTargetingEnabled = true;
        }
        
        // Top tamamen durana kadar bekle
        while (isSpinning)
        {
            yield return null;
        }
    }
    
    // Wheel'ın ease-out deceleration ile dönmesini sağlayan coroutine.
    private IEnumerator SpinWheel(float totalDuration)
    {
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            float t = Mathf.Clamp01(elapsed / totalDuration);
            // Ease-out: t=0 => faktör=1, t=1 => faktör=0
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

    // Ease-out cubic fonksiyonu: 1 - (1-t)^3
    private float EaseOut(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}