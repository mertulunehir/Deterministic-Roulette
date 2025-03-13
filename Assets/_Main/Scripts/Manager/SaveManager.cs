using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "roulette_save_data";
    private const int MAX_GAME_HISTORY = 30;
    
    private GameSaveData saveData;
    
    private Dictionary<TableNumberPlace, PlacedBet> currentGameBets = new Dictionary<TableNumberPlace, PlacedBet>();
    private int currentGameBetAmount = 0;
    private int currentWinAmount = 0;
    private int lastWinningNumber = -1;
    
    private bool isGameInProgress = false;
    
    
    private MoneyCanvasController moneyController;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // TODO: 
        moneyController = FindObjectOfType<MoneyCanvasController>();
        if (moneyController == null)
        {
            Debug.LogError("MoneyCanvasController not found in scene!");
        }
        else
        {
            // Update the money controller with our saved balance
            UpdateMoneyControllerBalance();
        }
    }
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnSpinFinished, OnSpinFinished);
        EventManager.Subscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Subscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Subscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Subscribe(GameEvents.OnCancelBetButtonClicked, OnCancelBet);
    }
    
    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnSpinFinished, OnSpinFinished);
        EventManager.Unsubscribe(GameEvents.OnWinningsCalculated, OnWinningsCalculated);
        EventManager.Unsubscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Unsubscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Unsubscribe(GameEvents.OnCancelBetButtonClicked, OnCancelBet);
    }
    
    private void OnApplicationQuit()
    {
        SaveGameData();
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameData();
        }
    }
    
    // Event handlers
    private void OnSpinStarted(object[] obj)
    {
        if (!isGameInProgress)
        {
            isGameInProgress = true;
            
            TrackCurrentGameBets();
            
            currentGameBetAmount = GetTotalBetAmount();
            
            if (moneyController != null)
            {
                saveData.currentBalance = moneyController.GetCurrentBalance();
                SaveGameData();
            }
            
            Debug.Log($"SaveManager: Spin started. Current bet amount: {currentGameBetAmount}");
        }
    }
    
    private void OnSpinFinished(object[] obj)
    {
        if (obj.Length > 0 && obj[0] is int)
        {
            lastWinningNumber = (int)obj[0];
            Debug.Log($"SaveManager: Spin finished. Winning number: {lastWinningNumber}");
        }
    }
    
    private void OnWinningsCalculated(object[] obj)
    {
        if (!isGameInProgress) return;
        
        if (obj.Length >= 3)
        {
            int winningNumber = (int)obj[0];
            int winningAmount = (int)obj[1];
            int newBalance = (int)obj[2];
            
            currentWinAmount = winningAmount;
            
            saveData.currentBalance = newBalance;
            
            RecordGameResult(winningNumber, winningAmount > 0 ? winningAmount : 0, currentGameBetAmount);
            
            SaveGameData();
            
            ResetGameState();
        }
    }
    
    private void OnChipPlaced(object[] obj)
    {
        if (obj.Length >= 2)
        {
            TableNumberPlace place = obj[0] as TableNumberPlace;
            Chips chipType = (Chips)obj[1];
            
            Debug.Log($"SaveManager: Chip placed on {place?.PlaceBetType}");
        }
    }
    
    private void OnChipRemoved(object[] obj)
    {
        if (obj.Length >= 2)
        {
            TableNumberPlace place = obj[0] as TableNumberPlace;
            Chips chipType = (Chips)obj[1];
            
            Debug.Log($"SaveManager: Chip removed from {place?.PlaceBetType}");
        }
    }
    
    private void OnCancelBet(object[] obj)
    {
        currentGameBets.Clear();
        currentGameBetAmount = 0;
        Debug.Log("SaveManager: All bets canceled");
    }
    
    // Helper methods
    private void TrackCurrentGameBets()
    {
        currentGameBets.Clear();
        
        // TODO: Find all TableNumberPlace components in the scene 
        TableNumberPlace[] allPlaces = FindObjectsOfType<TableNumberPlace>();
        
        // Check each place for active bets
        foreach (TableNumberPlace place in allPlaces)
        {
            if (place.HasChips)
            {
                // TODO : 
                RouletteBetController betController = FindObjectOfType<RouletteBetController>();
                if (betController != null)
                {
                    PlacedBet bet = new PlacedBet(place);
                    bet.AddChip(place.CurrentBetAmount);

                    currentGameBets[place] = bet;
                    
                    Debug.Log($"SaveManager: Tracking bet on {place.PlaceBetType}, amount: {place.CurrentBetAmount}");
                }
            }
        }
    }
    
    private int GetTotalBetAmount()
    {
        int total = 0;
        
        foreach (var bet in currentGameBets.Values)
        {
            total += bet.totalAmount;
        }
        
        return total;
    }
    
    private void RecordGameResult(int winningNumber, int winningAmount, int betAmount)
    {
        GameRecord record = new GameRecord
        {
            gameId = saveData.gameCounter++,
            winningNumber = winningNumber,
            isWin = winningAmount > 0,
            betAmount = betAmount,
            winAmount = winningAmount,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        
        foreach (var bet in currentGameBets.Values)
        {
            bool isBetWin = IsWinningBet(bet, winningNumber);
            
            BetTypeRecord betRecord = new BetTypeRecord
            {
                betType = bet.betType,
                amount = bet.totalAmount,
                isWin = isBetWin
            };
            
            record.bets.Add(betRecord);
            
            if (isBetWin)
            {
                saveData.winsByBetType[bet.betType]++;
            }
            else
            {
                saveData.lossesByBetType[bet.betType]++;
            }
        }
        
        if (winningAmount > 0)
        {
            saveData.totalWins++;
        }
        else
        {
            saveData.totalLosses++;
        }
        
        saveData.gameHistory.Add(record);
        if (saveData.gameHistory.Count > MAX_GAME_HISTORY)
        {
            saveData.gameHistory.RemoveAt(0);
        }
        
        Debug.Log($"SaveManager: Recorded game result. Winning number: {winningNumber}, Win amount: {winningAmount}, Bet amount: {betAmount}, IsWin: {record.isWin}");
    }
    
    private bool IsWinningBet(PlacedBet bet, int winningNumber)
    {
        switch (bet.betType)
        {
            case BetTypes.Straight:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.SlipBetTwo:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.StreetBetThree:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.CornerBetFour:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.SixLineBet:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.ColumnBetOnTwelveNumbers:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.DozenBet:
                return bet.connectedNumbers.Contains(winningNumber);
                
            case BetTypes.High:
                return winningNumber > 0 && winningNumber >= 19 && winningNumber <= 36;
                
            case BetTypes.Low:
                return winningNumber > 0 && winningNumber >= 1 && winningNumber <= 18;
                
            case BetTypes.Red:
                return IsRedNumber(winningNumber);
                
            case BetTypes.Black:
                return !IsRedNumber(winningNumber) && winningNumber > 0;
                
            case BetTypes.Odd:
                return winningNumber > 0 && winningNumber % 2 == 1;
                
            case BetTypes.Even:
                return winningNumber > 0 && winningNumber % 2 == 0;
                
            default:
                return false;
        }
    }
    
    private bool IsRedNumber(int number)
    {
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        return Array.IndexOf(redNumbers, number) != -1;
    }
    
    private void ResetGameState()
    {
        isGameInProgress = false;
        currentGameBets.Clear();
        currentGameBetAmount = 0;
        currentWinAmount = 0;
    }
    
    // Save and load methods
    public void SaveGameData()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            
            string winsByBetTypeJson = JsonHelper.ToJson(saveData.winsByBetType);
            string lossesByBetTypeJson = JsonHelper.ToJson(saveData.lossesByBetType);
            
            // Combine all JSON data
            string combinedJson = "{\"mainData\":" + json + 
                                 ",\"winsByBetType\":" + winsByBetTypeJson + 
                                 ",\"lossesByBetType\":" + lossesByBetTypeJson + "}";
            
            // Save to PlayerPrefs
            PlayerPrefs.SetString(SAVE_FILE_NAME, combinedJson);
            PlayerPrefs.Save();
            
            Debug.Log($"SaveManager: Game data saved to PlayerPrefs with key: {SAVE_FILE_NAME}");
            
            EventManager.TriggerEvent(GameEvents.OnGameSaved);
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: Error saving game data: {e.Message}");
        }
    }
    
    public void LoadGameData()
    {
        if (PlayerPrefs.HasKey(SAVE_FILE_NAME))
        {
            try
            {
                string json = PlayerPrefs.GetString(SAVE_FILE_NAME);
                
                // Parse the combined JSON
                if (json.Contains("\"mainData\":"))
                {
                    // Extract mainData
                    int startIndex = json.IndexOf("\"mainData\":") + "\"mainData\":".Length;
                    int endIndex = json.IndexOf(",\"winsByBetType\":", startIndex);
                    
                    string mainDataJson = json.Substring(startIndex, endIndex - startIndex);
                    saveData = JsonUtility.FromJson<GameSaveData>(mainDataJson);
                    
                    // Extract the dictionary data
                    if (json.Contains("\"winsByBetType\":"))
                    {
                        startIndex = json.IndexOf("\"winsByBetType\":") + "\"winsByBetType\":".Length;
                        endIndex = json.IndexOf(",\"lossesByBetType\":", startIndex);
                        if (endIndex == -1) // If lossesByBetType is not found
                            endIndex = json.IndexOf("}", startIndex);
                            
                        string winsByBetTypeJson = json.Substring(startIndex, endIndex - startIndex);
                        saveData.winsByBetType = JsonHelper.FromJson(winsByBetTypeJson);
                    }
                    
                    if (json.Contains("\"lossesByBetType\":"))
                    {
                        startIndex = json.IndexOf("\"lossesByBetType\":") + "\"lossesByBetType\":".Length;
                        endIndex = json.LastIndexOf("}");
                        
                        string lossesByBetTypeJson = json.Substring(startIndex, endIndex - startIndex);
                        saveData.lossesByBetType = JsonHelper.FromJson(lossesByBetTypeJson);
                    }
                }
                else
                {
                    // If the format is not as expected, create new save data
                    Debug.LogWarning("SaveManager: Invalid JSON format in PlayerPrefs");
                    CreateNewSaveData();
                    return;
                }
                
                // Initialize any missing parts
                if (saveData.winsByBetType == null || saveData.lossesByBetType == null)
                {
                    InitializeBetTypeStats();
                }
                
                Debug.Log($"SaveManager: Game data loaded from PlayerPrefs with key: {SAVE_FILE_NAME}");
                
                // Trigger load event
                EventManager.TriggerEvent(GameEvents.OnGameLoaded);
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: Error loading game data: {e.Message}");
                CreateNewSaveData();
            }
        }
        else
        {
            Debug.Log("SaveManager: No save data found in PlayerPrefs, creating new data");
            CreateNewSaveData();
        }
    }
    
    private void CreateNewSaveData()
    {
        saveData = new GameSaveData();
        InitializeBetTypeStats();
        SaveGameData();
    }
    
    private void InitializeBetTypeStats()
    {
        if (saveData.winsByBetType == null)
        {
            saveData.winsByBetType = new Dictionary<BetTypes, int>();
        }
        
        if (saveData.lossesByBetType == null)
        {
            saveData.lossesByBetType = new Dictionary<BetTypes, int>();
        }
        
        foreach (BetTypes betType in Enum.GetValues(typeof(BetTypes)))
        {
            if (!saveData.winsByBetType.ContainsKey(betType))
            {
                saveData.winsByBetType[betType] = 0;
            }
            
            if (!saveData.lossesByBetType.ContainsKey(betType))
            {
                saveData.lossesByBetType[betType] = 0;
            }
        }
    }
    
    // Balance management
    private void UpdateMoneyControllerBalance()
    {
        if (moneyController != null)
        {
            // Set the current balance in the MoneyCanvasController
            // This requires adding a method to MoneyCanvasController to set balance directly
            Debug.Log($"SaveManager: Setting balance to {saveData.currentBalance}");
            
            // Reset current balance then add our saved balance
            moneyController.AddFunds(saveData.currentBalance - moneyController.GetCurrentBalance());
        }
    }
    
    // Public getters
    public int GetCurrentBalance()
    {
        return saveData.currentBalance;
    }
    
    public int GetTotalWins()
    {
        return saveData.totalWins;
    }
    
    public int GetTotalLosses()
    {
        return saveData.totalLosses;
    }
    
    public List<GameRecord> GetGameHistory()
    {
        return saveData.gameHistory;
    }
    
    public Dictionary<BetTypes, int> GetWinsByBetType()
    {
        return saveData.winsByBetType;
    }
    
    public Dictionary<BetTypes, int> GetLossesByBetType()
    {
        return saveData.lossesByBetType;
    }
}