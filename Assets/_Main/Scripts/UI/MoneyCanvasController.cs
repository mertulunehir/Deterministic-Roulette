using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyCanvasController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private TextMeshProUGUI winningNumberText;
    [SerializeField] private TextMeshProUGUI winningAmountText;
    
    [Header("Settings")]
    private int startingBalance = 10000;
    [SerializeField] private float winningPanelDisplayTime = 5f; 
    
    private int currentBalance;
    private int currentBetAmount;
    
    private Coroutine hideWinningPanelCoroutine;
    
    
    private SaveManager saveManager;
    
    private void Awake()
    {
        saveManager = SaveManager.Instance;
        
        if (saveManager != null)
        {
            currentBalance = saveManager.GetCurrentBalance();
        }
        else
        {
            currentBalance = startingBalance;
        }
        
        UpdateBalanceDisplay();
        
        if (winningPanel)
        {
            winningPanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Subscribe(GameEvents.OnChipPlaced, OnBetChanged);
        EventManager.Subscribe(GameEvents.OnChipRemoved, OnBetChanged);
        EventManager.Subscribe(GameEvents.OnCancelBetButtonClicked, OnBetCancelled);
        EventManager.Subscribe(GameEvents.OnBalanceUpdated, OnBalanceUpdated);
        EventManager.Subscribe(GameEvents.OnGameLoaded, OnGameLoaded);
    }
    
    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Unsubscribe(GameEvents.OnChipPlaced, OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnChipRemoved, OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnCancelBetButtonClicked, OnBetCancelled);
        EventManager.Unsubscribe(GameEvents.OnBalanceUpdated, OnBalanceUpdated);
        EventManager.Unsubscribe(GameEvents.OnGameLoaded, OnGameLoaded);
    }
    
    private void OnBetChanged(object[] obj)
    {
        BetCanvasController betCanvas = FindObjectOfType<BetCanvasController>();
        if (betCanvas != null)
        {
            currentBetAmount = betCanvas.CurrentBetAmount;
        }
    }
    
    private void OnBetCancelled(object[] obj)
    {
        currentBetAmount = 0;
    }
    
    private void OnSpinStarted(object[] obj)
    {
        BetCanvasController betCanvas = FindObjectOfType<BetCanvasController>();
        if (betCanvas != null)
        {
            currentBetAmount = betCanvas.CurrentBetAmount;
        }
        
        currentBalance -= currentBetAmount;
        UpdateBalanceDisplay();
        
        if (saveManager != null)
        {
            
        }
        
        if (winningPanel && winningPanel.activeSelf)
        {
            winningPanel.SetActive(false);
            
            if (hideWinningPanelCoroutine != null)
            {
                StopCoroutine(hideWinningPanelCoroutine);
                hideWinningPanelCoroutine = null;
            }
        }
    }
    
    private void OnWinningsCalculated(object[] obj)
    {
        if (obj.Length >= 3)
        {
            int winningNumber = (int)obj[0];
            int winningAmount = (int)obj[1];
            int newBalance = (int)obj[2];
            
            currentBalance = newBalance;
            UpdateBalanceDisplay();
            
            UpdateWinningPanel(winningNumber, winningAmount);
            ShowWinningPanel();
        }
    }
    
    private void OnBalanceUpdated(object[] obj)
    {
        if (obj.Length > 0 && obj[0] is int)
        {
            int newBalance = (int)obj[0];
            currentBalance = newBalance;
            UpdateBalanceDisplay();
        }
    }
    
    private void OnGameLoaded(object[] obj)
    {
        if (saveManager != null)
        {
            currentBalance = saveManager.GetCurrentBalance();
            UpdateBalanceDisplay();
        }
    }
    
    private void UpdateBalanceDisplay()
    {
        if (balanceText)
        {
            balanceText.text = $"${currentBalance}";
        }
    }
    
    private void UpdateWinningPanel(int winningNumber, int winningAmount)
    {
        if (winningNumberText)
        {
            winningNumberText.text = $"{winningNumber}";
        }
        
        if (winningAmountText)
        {
            if (winningAmount > 0)
            {
                winningAmountText.text = $"${winningAmount}";
                winningAmountText.color = Color.green;
            }
            else
            {
                winningAmountText.text = $"{-currentBetAmount}";
                winningAmountText.color = Color.red;
            }
        }
    }
    
    private void ShowWinningPanel()
    {
        if (winningPanel)
        {
            winningPanel.SetActive(true);
            
            if (hideWinningPanelCoroutine != null)
            {
                StopCoroutine(hideWinningPanelCoroutine);
            }
            
            hideWinningPanelCoroutine = StartCoroutine(HideWinningPanelAfterDelay(winningPanelDisplayTime));
        }
    }
    
    private IEnumerator HideWinningPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (winningPanel)
        {
            winningPanel.SetActive(false);
        }
        
        hideWinningPanelCoroutine = null;
    }
    
    
    public int GetCurrentBalance()
    {
        return currentBalance;
    }
    
    public void AddFunds(int amount)
    {
        currentBalance += amount;
        UpdateBalanceDisplay();
        
        if (saveManager != null)
        {
            EventManager.TriggerEvent(GameEvents.OnBalanceUpdated, currentBalance);
        }
    }
    
    public void SetBalance(int newBalance)
    {
        currentBalance = newBalance;
        UpdateBalanceDisplay();
    }
    
    public bool HasEnoughFunds(int amount)
    {
        return currentBalance >= amount;
    }
}