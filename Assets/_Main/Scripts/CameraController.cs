using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Kamera GameObject'leri")]
    [SerializeField] private GameObject tableCamera;
    [SerializeField] private GameObject spinCamera;
    [SerializeField] private GameObject ballCamera;
    
    [Header("Ayarlar")]
    [SerializeField] private float ballCamDuration = 5f; // Ball kamerasının aktif kalacağı süre
    
    // Mevcut aktif kamerayı takip etmek için
    private GameObject currentActiveCamera;
    
    private void Start()
    {
        // Başlangıçta sadece table kamerası aktif olsun
        SetInitialCameraState();
    }
    
    private void OnEnable()
    {
        // Event'lere abone ol
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }
    
    private void OnDisable()
    {
        // Event aboneliklerini kaldır
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }
    
    private void SetInitialCameraState()
    {
        // Tüm kameraları kapat
        if (tableCamera) tableCamera.SetActive(false);
        if (spinCamera) spinCamera.SetActive(false);
        if (ballCamera) ballCamera.SetActive(false);
        
        // Table kamerasını aç
        if (tableCamera)
        {
            tableCamera.SetActive(true);
            currentActiveCamera = tableCamera;
        }
        else
        {
            Debug.LogError("Table kamerası atanmamış!");
        }
    }
    
    private void OnSpinStarted(object[] obj)
    {
        // Spin kamerasına geçiş yap
        SwitchToCamera(spinCamera);
    }
    
    private void OnSpinFinished(object[] obj)
    {
        // Ball kamerasına geçiş yap
        SwitchToCamera(ballCamera);
        
        // Belirli bir süre sonra table kamerasına geri dön
        StartCoroutine(ReturnToTableCameraAfterDelay(ballCamDuration));
    }
    
    private void SwitchToCamera(GameObject targetCamera)
    {
        // Eğer hedef kamera null ise işlemi iptal et
        if (targetCamera == null)
            return;
            
        // Eğer zaten hedef kameradaysak bir şey yapma
        if (currentActiveCamera == targetCamera)
            return;
        
        // Mevcut kamerayı kapat
        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);
        }
        
        // Hedef kamerayı aç
        targetCamera.SetActive(true);
        currentActiveCamera = targetCamera;
    }
    
    private IEnumerator ReturnToTableCameraAfterDelay(float delay)
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(delay);
        
        // Table kamerasına geri dön
        SwitchToCamera(tableCamera);
        
        // Bet canvas'ı tekrar aktifleştir
        EventManager.TriggerEvent(GameEvents.OnEnableBetCanvas);
        
    }
    
    // Manuel olarak table kamerasına geçiş yapmak için public metot
    public void SwitchToTableCamera()
    {
        SwitchToCamera(tableCamera);
    }
    
    // Manuel olarak spin kamerasına geçiş yapmak için public metot
    public void SwitchToSpinCamera()
    {
        SwitchToCamera(spinCamera);
    }
    
    // Manuel olarak ball kamerasına geçiş yapmak için public metot
    public void SwitchToBallCamera()
    {
        SwitchToCamera(ballCamera);
    }
}