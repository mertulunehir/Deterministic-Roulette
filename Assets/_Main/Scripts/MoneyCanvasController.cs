using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoneyCanvasController : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private GameObject winningPanel;
    [SerializeField] private TextMeshProUGUI winningNumberText;
    [SerializeField] private TextMeshProUGUI winningAmountText;
    
    [Header("Ayarlar")]
    [SerializeField] private int startingBalance = 1000;
    [SerializeField] private float winningPanelDisplayTime = 5f; // Kazanç panelinin ekranda kalma süresi
    
    // Para durumu
    private int currentBalance;
    private int currentBetAmount;
    
    // Coroutine referansı
    private Coroutine hideWinningPanelCoroutine;
    
    private void Awake()
    {
        // Başlangıç bakiyesini ayarla
        currentBalance = startingBalance;
        UpdateBalanceDisplay();
        
        // Kazanç panelini başlangıçta gizle
        if (winningPanel)
        {
            winningPanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        // Event'lere abone ol
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Subscribe(GameEvents.OnChipPlaced, OnBetChanged);
        EventManager.Subscribe(GameEvents.OnChipRemoved, OnBetChanged);
        EventManager.Subscribe(GameEvents.OnCancelBetButtonClicked, OnBetCancelled);
    }
    
    private void OnDisable()
    {
        // Event aboneliklerini kaldır
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Unsubscribe(GameEvents.OnChipPlaced, OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnChipRemoved, OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnCancelBetButtonClicked, OnBetCancelled);
    }
    
    private void OnBetChanged(object[] obj)
    {
        // BetCanvasController'dan toplam bahis miktarını al
        BetCanvasController betCanvas = FindObjectOfType<BetCanvasController>();
        if (betCanvas != null)
        {
            currentBetAmount = betCanvas.CurrentBetAmount;
        }
    }
    
    private void OnBetCancelled(object[] obj)
    {
        // Bahisler iptal edildiğinde miktarı sıfırla
        currentBetAmount = 0;
    }
    
    private void OnSpinStarted(object[] obj)
    {
        BetCanvasController betCanvas = FindObjectOfType<BetCanvasController>();
        if (betCanvas != null)
        {
            currentBetAmount = betCanvas.CurrentBetAmount;
        }
        
        // Bahis tutarını bakiyeden düş
        currentBalance -= currentBetAmount;
        UpdateBalanceDisplay();
        
        // Kazanç panelini gizle (eğer gösteriliyorsa)
        if (winningPanel && winningPanel.activeSelf)
        {
            winningPanel.SetActive(false);
            
            // Eğer çalışan bir coroutine varsa durdur
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
            
            // Bakiyeyi güncelle
            currentBalance = newBalance;
            UpdateBalanceDisplay();
            
            // Kazanç panelini güncelle ve göster
            UpdateWinningPanel(winningNumber, winningAmount);
            ShowWinningPanel();
        }
    }
    
    private void UpdateBalanceDisplay()
    {
        if (balanceText)
        {
            balanceText.text = $"{currentBalance}";
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
            
            // Paneli belirli bir süre sonra gizleyecek coroutine'i başlat
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
    
    // Public metotlar
    
    public int GetCurrentBalance()
    {
        return currentBalance;
    }
    
    public void AddFunds(int amount)
    {
        currentBalance += amount;
        UpdateBalanceDisplay();
    }
    
    public bool HasEnoughFunds(int amount)
    {
        return currentBalance >= amount;
    }
}