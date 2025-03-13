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
    [SerializeField] private float gameRestartDelay = 3f;
    
    private bool isGameInProgress = false;
    private SaveManager saveManager;
    
    private void Awake()
    {
        // Get SaveManager reference
        saveManager = SaveManager.Instance;
        if (saveManager == null)
        {
            Debug.LogWarning("SaveManager not found, game progress will not be saved");
        }
    }
    
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
    }
    
    private void OnWinningsCalculated(object[] obj)
    {
        
        EventManager.TriggerEvent(GameEvents.OnGameHistoryUpdated);
        
        StartCoroutine(RestartGameAfterDelay(gameRestartDelay));
    }
    
    private void OnInsufficientFunds(object[] obj)
    {
        Debug.LogWarning("Yetersiz bakiye! Ek bakiye yüklemeniz gerekiyor.");
    }
    
    private IEnumerator RestartGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (betCanvasController != null)
        {
            betCanvasController.ShowBetCanvas(null);
        }
        

        yield return new WaitForSeconds(0.2f);

        if (wheelController != null)
        {
            wheelController.ResetRoulette();
        }
        
        isGameInProgress = false;
        
        Debug.Log("Oyun yeniden başladı");
    }
}