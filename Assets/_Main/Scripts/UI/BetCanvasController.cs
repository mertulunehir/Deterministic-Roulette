using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BetCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject canvasParent;
    [SerializeField] private Button tenButton;
    [SerializeField] private Button fiftyButton;
    [SerializeField] private Button hunderedButton;
    [SerializeField] private Button twoHunderedButton;
    [SerializeField] private Button cancelBetButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Color buttonOutlineOpenColor;
    [SerializeField] private Color buttonOutlineCloseColor;
    [SerializeField] private TextMeshProUGUI currentBetText; 
    [SerializeField] private MoneyCanvasController moneyController;
    
    [SerializeField] private GameObject insufficientFundsPanel;
    [SerializeField] private float insufficientFundsDisplayTime = 2f; 
    
    private int currentBetAmount = 0; 
    private Coroutine hideInsufficientFundsCoroutine;
    
    private void Awake()
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        tenButton.onClick.AddListener(OnTenButtonClicked);
        fiftyButton.onClick.AddListener(OnFiftyButtonClicked);
        hunderedButton.onClick.AddListener(OnHunderedButtonClicked);
        twoHunderedButton.onClick.AddListener(OnTwoHunderedButtonClicked);
        cancelBetButton.onClick.AddListener(OnCancelButtonClicked);
        spinButton.onClick.AddListener(OnSpinButtonClicked);
        
        EventManager.Subscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Subscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Subscribe(GameEvents.OnEnableBetCanvas, ShowBetCanvas);
        EventManager.Subscribe(GameEvents.OnInsufficientFunds, OnInsufficientFunds);
        
        UpdateBetText();
    }

    private void OnDisable()
    {
        tenButton.onClick.RemoveAllListeners();
        fiftyButton.onClick.RemoveAllListeners();
        hunderedButton.onClick.RemoveAllListeners();
        twoHunderedButton.onClick.RemoveAllListeners();
        cancelBetButton.onClick.RemoveAllListeners();
        spinButton.onClick.RemoveAllListeners();
        
        EventManager.Unsubscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Unsubscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Unsubscribe(GameEvents.OnEnableBetCanvas, ShowBetCanvas);
        EventManager.Unsubscribe(GameEvents.OnInsufficientFunds, OnInsufficientFunds);
    }
    
    private void OnInsufficientFunds(object[] obj)
    {
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(true);
            
            if (hideInsufficientFundsCoroutine != null)
            {
                StopCoroutine(hideInsufficientFundsCoroutine);
            }
            
            // Start a new coroutine to hide the panel after delay
            hideInsufficientFundsCoroutine = StartCoroutine(HideInsufficientFundsAfterDelay());
        }
    }
    
    private IEnumerator HideInsufficientFundsAfterDelay()
    {
        yield return new WaitForSeconds(insufficientFundsDisplayTime);
        
        if (insufficientFundsPanel != null)
        {
            insufficientFundsPanel.SetActive(false);
        }
        
        hideInsufficientFundsCoroutine = null;
    }

    private void OnBetChangeButtonClicked(Chips chips)
    {
        switch (chips)
        {
            case Chips.Ten:
                tenButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.Fifty:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.Hundered:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                break;
            case Chips.TwoHundered:
                tenButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                fiftyButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                hunderedButton.GetComponent<Image>().color = buttonOutlineCloseColor;
                twoHunderedButton.GetComponent<Image>().color = buttonOutlineOpenColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(chips), chips, null);
        }
    }

    private void OnTwoHunderedButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.TwoHundered);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged, Chips.TwoHundered);
    }

    private void OnHunderedButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Hundered);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged, Chips.Hundered);
    }

    private void OnFiftyButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Fifty);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged, Chips.Fifty);
    }

    private void OnTenButtonClicked()
    {
        OnBetChangeButtonClicked(Chips.Ten);
        EventManager.TriggerEvent(GameEvents.OnGameBetChanged, Chips.Ten);
    }
    
    private void OnCancelButtonClicked()
    {
        EventManager.TriggerEvent(GameEvents.OnCancelBetButtonClicked);
        
        currentBetAmount = 0;
        
        UpdateBetText();

        
        Debug.Log("Cancel button clicked - all bets will be removed");
    }
    
    private void OnSpinButtonClicked()
    {
        if (currentBetAmount <= 0)
        {
            Debug.Log("bet first!");
            return;
        }
        
        if (moneyController != null && !moneyController.HasEnoughFunds(currentBetAmount))
        {
            Debug.Log("insufficient chips!");
            EventManager.TriggerEvent(GameEvents.OnInsufficientFunds);
            return;
        }
        
        canvasParent.SetActive(false);
        
        EventManager.TriggerEvent(GameEvents.OnSpinButtonClicked);
    }

    private void OnChipPlaced(object[] obj)
    {
        if (obj.Length >= 2)
        {
            Chips chipType = (Chips)obj[1];
            int chipValue = GetChipValue(chipType);
            
            currentBetAmount += chipValue;
            
            UpdateBetText();
            
            Debug.Log($"Chip placed: {chipType}, Value: {chipValue}, Total Bet: {currentBetAmount}");
        }
        else
        {
            Debug.LogWarning("OnChipPlaced called with insufficient parameters");
        }
    }

    private void OnChipRemoved(object[] obj)
    {
        if (obj.Length >= 2)
        {
            Chips chipType = (Chips)obj[1];
            int chipValue = GetChipValue(chipType);
            
            currentBetAmount -= chipValue;
            
            if (currentBetAmount < 0)
                currentBetAmount = 0;
            
            UpdateBetText();
        }
    }

    private void UpdateBetText()
    {
        if (currentBetText != null)
        {
            CalculateTotalBets();
            currentBetText.text = $"${currentBetAmount}";
        }
    }
    
    private void CalculateTotalBets()
    {
        int totalBet = 0;
        
        // Find all TableNumberPlace components that have active bets
        TableNumberPlace[] allPlaces = FindObjectsOfType<TableNumberPlace>();
        foreach (var place in allPlaces)
        {
            if (place.HasChips)
            {
                totalBet += place.CurrentBetAmount;
            }
        }
        
        // Update our local tracking
        currentBetAmount = totalBet;
    }

    private int GetChipValue(Chips chipType)
    {
        switch (chipType)
        {
            case Chips.Ten: return 10;
            case Chips.Fifty: return 50;
            case Chips.Hundered: return 100;
            case Chips.TwoHundered: return 200;
            default: return 0;
        }
    }
    
    public int CurrentBetAmount
    {
        get { return currentBetAmount; }
    }

    // method to show the bet canvas
    public void ShowBetCanvas(object[] objects)
    {
        if (canvasParent != null)
        {
            canvasParent.SetActive(true);
            
            currentBetAmount = 0;
            UpdateBetText();
        }
    }
}