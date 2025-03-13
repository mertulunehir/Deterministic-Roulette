using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private Button openStatsButton;
    [SerializeField] private Button closeStatsButton;
    
    [Header("Stats Text Fields")]
    [SerializeField] private TextMeshProUGUI totalWinsText;
    [SerializeField] private TextMeshProUGUI totalLossesText;
    [SerializeField] private TextMeshProUGUI winRateText;
    
    [Header("History Panel")]
    [SerializeField] private Transform historyContentParent;
    [SerializeField] private GameObject historyItemPrefab;
    [SerializeField] private int maxHistoryItems = 10; 
    
    
    private SaveManager saveManager;
    
    private void Awake()
    {
        if (statsPanel)
        {
            statsPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        
        saveManager = SaveManager.Instance;
        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
        }
        
        if (openStatsButton)
        {
            openStatsButton.onClick.AddListener(OpenStatsPanel);
        }
        
        if (closeStatsButton)
        {
            closeStatsButton.onClick.AddListener(CloseStatsPanel);
        }
    }
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnWinningsCalculated, OnGameCompleted);
        EventManager.Subscribe(GameEvents.OnGameHistoryUpdated, RefreshUI);
    }
    
    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnWinningsCalculated, OnGameCompleted);
        EventManager.Unsubscribe(GameEvents.OnGameHistoryUpdated, RefreshUI);
    }
    
    private void OnGameCompleted(object[] obj)
    {
        RefreshUI(null);
    }
    
    private void RefreshUI(object[] obj)
    {
        if (saveManager == null) return;
        
        UpdateStatsText();
        
        UpdateHistoryItems();
    }
    
    private void UpdateStatsText()
    {
        int totalWins = saveManager.GetTotalWins();
        int totalLosses = saveManager.GetTotalLosses();
        
        if (totalWinsText)
        {
            totalWinsText.text = $"{totalWins}";
        }
        
        if (totalLossesText)
        {
            totalLossesText.text = $"{totalLosses}";
        }
        
        if (winRateText)
        {
            float winRate = 0;
            if (totalWins + totalLosses > 0)
            {
                winRate = (float)totalWins / (totalWins + totalLosses) * 100f;
            }
            
            winRateText.text = $"{winRate:F1}%";
        }
    }
    
    private void UpdateHistoryItems()
    {
        if (historyContentParent)
        {
            foreach (Transform child in historyContentParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        List<GameRecord> history = saveManager.GetGameHistory();
        
        int startIndex = Mathf.Max(0, history.Count - maxHistoryItems);
        
        
        for (int i = history.Count - 1; i >= startIndex; i--)
        {
            GameRecord record = history[i];
            
            if (historyItemPrefab && historyContentParent)
            {
                GameObject newItem = Instantiate(historyItemPrefab, historyContentParent);
                HistoryItemUI itemUI = newItem.GetComponent<HistoryItemUI>();
                
                if (itemUI)
                {
                    itemUI.SetupItem(record);
                }
            }
        }
    }
    
    private void OpenStatsPanel()
    {
        if (statsPanel)
        {
            EventManager.TriggerEvent(GameEvents.OnStatisticPanelOpened);
            statsPanel.SetActive(true);
            RefreshUI(null);
        }
    }
    
    private void CloseStatsPanel()
    {
        if (statsPanel)
        {
            EventManager.TriggerEvent(GameEvents.OnStatisticPanelClosed);
            statsPanel.SetActive(false);
        }
    }
}