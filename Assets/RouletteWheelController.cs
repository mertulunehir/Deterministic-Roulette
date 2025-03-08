using System.Collections;
using UnityEngine;

public class RouletteWheelController : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform ball;               // Topun Transform'u
    public Rigidbody ballRigidbody;      // Topun Rigidbody'si
    public Transform wheel;              // Rulet tekerleğinin Transform'u
    public Transform wheelCenter;        // Rulet tekerleğinin merkez noktası

    [Header("Ayarlar")]
    public Transform spinStartPos;         // Topun spin başlangıç pozisyonu
    public float wheelSpinSpeed = 90f;   // Tekerleğin başlangıç dönüş hızı (derece/saniye)
    public float orbitDuration = 4f;     // Topun dairesel hareket süresi
    public float orbitRadius = 3f;       // Topun dairesel yörüngedeki yarıçapı
    public float finalMoveDuration = 1f; // Topun final slotuna hareket süresi
    public float targetLocalAngle = 0f;  // Belirlenen slot açısı (tekerleğe göre lokal açı)
    
    // Bu metot, tüm süreci başlatır.
    [ContextMenu("Spin")]
    public void StartRoulette()
    {
        StartCoroutine(RouletteSequence());
    }

    private IEnumerator RouletteSequence()
    {
        // 1. Top, spin başlangıç pozisyonuna gider.
        ball.position = spinStartPos.position;
        yield return null; // Bir frame bekleyerek pozisyon güncellenmesini sağlıyoruz.
        wheel.eulerAngles = Vector3.zero;

        // 2. Ball Rigidbody kinematik hale getirilir (fizik etkisi alınmaz).
        ballRigidbody.isKinematic = true;

        // 3. Wheel, ease-out deceleration ile dönmeye başlar.
        float wheelTotalDuration = orbitDuration + finalMoveDuration;
        Coroutine wheelSpinRoutine = StartCoroutine(SpinWheel(wheelTotalDuration));

        // 4. Top, wheel merkezinin etrafında dairesel hareket yapar (ease-out ile).
        yield return StartCoroutine(OrbitBall(orbitDuration));

        // 5. Topun kinematic durumu kapatılır, yani fizik devreye alınır.
        ballRigidbody.isKinematic = false;

        // 6. Top, belirlenen slota doğru ease-out hareket eder.
        yield return StartCoroutine(MoveBallToSlot(finalMoveDuration));

        // 7. Wheel artık durmuş durumda (SpinWheel coroutine'i tamamlandı).
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
            wheel.Rotate(0, currentSpeed * Time.deltaTime, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Topun dairesel yörüngede ease-out ile hareket etmesini sağlayan coroutine.
    private IEnumerator OrbitBall(float duration)
    {
        float elapsed = 0f;
        // Topun, wheelCenter etrafındaki başlangıç açısını hesaplayın.
        Vector3 offset = ball.position - wheelCenter.position;
        float startAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
    
        // Topun toplam dönüş açısı (örneğin 720 derece: 2 tam tur)
        float totalAngle = 720f;
    
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOut(t);
            float angleDelta = totalAngle * easedT;
            float currentAngle = startAngle + angleDelta;
            float rad = currentAngle * Mathf.Deg2Rad;
            Vector3 newPos = wheelCenter.position + new Vector3(
                Mathf.Cos(rad) * orbitRadius,
                0.5f,
                Mathf.Sin(rad) * orbitRadius);
            ball.position = newPos;
            yield return null;
        }
    }

    // Topun final slotuna ease-out hareketle ulaşmasını sağlayan coroutine.
    private IEnumerator MoveBallToSlot(float duration)
    {
        Vector3 startPos = ball.position;

        // Wheel'ın güncel dönüş açısını alıp, targetLocalAngle ile toplayarak final açı hesaplanır.
        float wheelRotation = wheel.eulerAngles.y;
        float finalAngle = wheelRotation + targetLocalAngle;
        float rad = finalAngle * Mathf.Deg2Rad;
        Vector3 finalPos =  wheelCenter.position +  new Vector3(
            Mathf.Cos(rad) * orbitRadius,
            0.5f,
            Mathf.Sin(rad) * orbitRadius);
        
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseOut(t);
            ball.position = Vector3.Lerp(startPos, finalPos, easedT);
            yield return null;
        }
        ball.position = finalPos;
    }

    // Ease-out cubic fonksiyonu: 1 - (1-t)^3
    private float EaseOut(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }
}
