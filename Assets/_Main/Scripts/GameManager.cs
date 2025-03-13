using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private RouletteWheelController wheelController;
    [SerializeField] private RouletteBetController betController;
    [SerializeField] private BetCanvasController betCanvasController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private MoneyCanvasController moneyController;
    
    [Header("Game Settings")]
    [SerializeField] private float gameRestartDelay = 3f; // Yeni turu başlatmadan önce bekleme süresi
    
    private bool isGameInProgress = false;
    
    private void OnEnable()
    {
        // Event'lere abone ol
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnSpinFinished, OnSpinFinished);
        EventManager.Subscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Subscribe(GameEvents.OnInsufficientFunds, OnInsufficientFunds);
    }
    
    private void OnDisable()
    {
        // Event aboneliklerini kaldır
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnSpinFinished, OnSpinFinished);
        EventManager.Unsubscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Unsubscribe(GameEvents.OnInsufficientFunds, OnInsufficientFunds);
    }
    
    private void OnSpinStarted(object[] obj)
    {
        if (isGameInProgress)
            return;
        
        isGameInProgress = true;
        
        
        
        // Tekeri döndürmeye başla
        if (wheelController != null)
        {
            wheelController.StartRoulette();
        }
        
        Debug.Log("Spin başladı");
    }
    
    private void OnSpinFinished(object[] obj)
    {
        if (!isGameInProgress)
            return;
        
        int winningNumber = -1;
        
        if (obj.Length > 0 && obj[0] is int)
        {
            winningNumber = (int)obj[0];
            Debug.Log($"Spin bitti. Kazanan numara: {winningNumber}");
        }
        
        // Bu noktada kamera kontrolcüsü otomatik olarak ball kamerasına geçecek
    }
    
    private void OnWinningsCalculated(object[] obj)
    {
        // Bu noktada kazançlar hesaplanmış ve UI güncellenmiş olmalı
        
        // Belirli bir süre sonra oyunu sıfırla
        StartCoroutine(RestartGameAfterDelay(gameRestartDelay));
    }
    
    private void OnInsufficientFunds(object[] obj)
    {
        // Yetersiz bakiye durumunda yapılacak işlemler
        Debug.LogWarning("Yetersiz bakiye! Ek bakiye yüklemeniz gerekiyor.");
        
        // Burada ek bakiye yükleme ekranı veya uyarı gösterebilirsiniz
        // Örnek: ShowInsufficientFundsWarning();
    }
    
    private IEnumerator RestartGameAfterDelay(float delay)
    {
        // Belirtilen süre kadar bekle
        yield return new WaitForSeconds(delay);
        
        // Tekerleği sıfırla
        if (wheelController != null)
        {
            wheelController.ResetRoulette();
        }
        
        // BetCanvas'ı tekrar aç
        if (betCanvasController != null)
        {
            betCanvasController.ShowBetCanvas(null);
        }
        
        // Table kamerasına geri dön (kamera kontrolcüsü bunu otomatik yapabilir, 
        // ancak burada da kontrol edelim)
        if (cameraController != null)
        {
            cameraController.SwitchToTableCamera();
        }
        
        isGameInProgress = false;
        
        Debug.Log("Oyun yeniden başladı");
    }
    
    // Test amaçlı para ekleme/çıkarma metotları
    
    public void AddFunds(int amount)
    {
        if (moneyController != null)
        {
            moneyController.AddFunds(amount);
            Debug.Log($"${amount} bakiyeye eklendi.");
        }
    }
    
    // Oyunu başlatan metot (gerektiğinde dışarıdan çağrılabilir)
    public void StartGame()
    {
        if (!isGameInProgress)
        {
            // BetCanvas'ı aç
            if (betCanvasController != null)
            {
                betCanvasController.ShowBetCanvas(null);
            }
            
            // Table kamerasına geç
            if (cameraController != null)
            {
                cameraController.SwitchToTableCamera();
            }
            
            Debug.Log("Oyun başladı, bahis yapabilirsiniz.");
        }
    }
}