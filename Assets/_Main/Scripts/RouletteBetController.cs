using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteBetController : MonoBehaviour
{
    [SerializeField] private RouletteWheelController wheelController;
    [SerializeField] private MoneyCanvasController moneyController;
    
    // Bet reward multipliers for different bet types
    [Header("Bet Reward Multipliers")]
    [SerializeField] private int straightBetMultiplier = 35;
    [SerializeField] private int splitBetMultiplier = 17;
    [SerializeField] private int streetBetMultiplier = 11;
    [SerializeField] private int cornerBetMultiplier = 8;
    [SerializeField] private int sixLineBetMultiplier = 5;
    [SerializeField] private int columnBetMultiplier = 2;
    [SerializeField] private int dozenBetMultiplier = 2;
    [SerializeField] private int evenOddRedBlackHighLowMultiplier = 1;
    
    private Chips _currentSelectedChip = Chips.Ten;
    private int currentBetAmount = 0;
    
    // Dictionary to track bets by place
    private Dictionary<TableNumberPlace, PlacedBet> placedBets = new Dictionary<TableNumberPlace, PlacedBet>();
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnGameBetChanged, OnBetChanged);
        EventManager.Subscribe(GameEvents.OnCancelBetButtonClicked, OnCancelBet);
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinWheel);
        EventManager.Subscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Subscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Subscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnGameBetChanged, OnBetChanged);
        EventManager.Unsubscribe(GameEvents.OnCancelBetButtonClicked, OnCancelBet);
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinWheel);
        EventManager.Unsubscribe(GameEvents.OnChipPlaced, OnChipPlaced);
        EventManager.Unsubscribe(GameEvents.OnChipRemoved, OnChipRemoved);
        EventManager.Unsubscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }
    
    private void OnCancelBet(object[] obj)
    {
        // Find all active chips and return them to the pool
        RemoveAllChipsFromTable();
        
        // Clear all bets
        ClearAllBets();
        currentBetAmount = 0;
        Debug.Log("All bets canceled");
    }
    
    private void RemoveAllChipsFromTable()
    {
        // Find all TableNumberPlace components in the scene
        TableNumberPlace[] allPlaces = FindObjectsOfType<TableNumberPlace>();
        
        // Remove all chips from each place
        foreach (TableNumberPlace place in allPlaces)
        {
            place.ReturnAllChipsToPool();
        }
    }
    
    private void OnSpinWheel(object[] obj)
    {
        if (currentBetAmount > 0)
        {
            // Para kontrolü - yeterli bakiye var mı?
            if (moneyController != null && moneyController.HasEnoughFunds(currentBetAmount))
            {
                // Start wheel rotation
                if (wheelController != null)
                {
                    wheelController.StartRoulette();
                }
                else
                {
                    Debug.LogError("Wheel Controller reference is missing!");
                    
                    // For testing, simulate a spin result
                    OnSpinFinished(new object[] { UnityEngine.Random.Range(0, 37) });
                }
            }
            else
            {
                Debug.LogError("Yeterli bakiye yok!");
                // Burada oyuncuya yetersiz bakiye uyarısı gösterebilirsiniz
            }
        }
        else
        {
            Debug.Log("No bets placed");
        }
    }
    
    private void OnBetChanged(object[] obj)
    {
        _currentSelectedChip = (Chips)obj[0];
    }
    
    private void OnChipPlaced(object[] obj)
    {
        if (obj.Length >= 2)
        {
            TableNumberPlace place = obj[0] as TableNumberPlace;
            Chips chipType = (Chips)obj[1];
            
            if (place != null)
            {
                // Add to tracked bets
                if (!placedBets.ContainsKey(place))
                {
                    placedBets[place] = new PlacedBet(place);
                }
                
                placedBets[place].AddChip(chipType);
                currentBetAmount += GetChipValue(chipType);
                
                Debug.Log($"Chip placed on {place.PlaceBetType}, total amount: {placedBets[place].totalAmount}");
            }
        }
    }
    
    private void OnChipRemoved(object[] obj)
    {
        if (obj.Length >= 2)
        {
            TableNumberPlace place = obj[0] as TableNumberPlace;
            Chips chipType = (Chips)obj[1];
            
            if (place != null && placedBets.ContainsKey(place))
            {
                placedBets[place].RemoveChip(chipType);
                currentBetAmount -= GetChipValue(chipType);
                
                // If no chips left, remove from dictionary
                if (placedBets[place].totalAmount <= 0)
                {
                    placedBets.Remove(place);
                }
                
                Debug.Log($"Chip removed from {place.PlaceBetType}, remaining amount: {(placedBets.ContainsKey(place) ? placedBets[place].totalAmount : 0)}");
            }
        }
    }
    
    private void OnSpinFinished(object[] obj)
    {
        if (obj.Length > 0 && obj[0] is int)
        {
            int winningNumber = (int)obj[0];
            
            // Calculate winnings based on the winning number
            int totalWinnings = CalculateWinnings(winningNumber);
            
            // MoneyCanvasController'ın bakiyeyi güncellemesine izin vermek için güncel bakiye değerini al
            int currentBalance = 0;
            if (moneyController != null)
            {
                currentBalance = moneyController.GetCurrentBalance();
            }
            
            // Yeni bakiye = mevcut bakiye + kazanç
            int newBalance = currentBalance + totalWinnings;
            
            // Trigger event for UI updates
            EventManager.TriggerEvent(GameEvents.OnWinningsCalculated, winningNumber, totalWinnings, newBalance);
            
            // Remove all chips from the table
            RemoveAllChipsFromTable();
            
            // Clear all bets for next round
            ClearAllBets();
            
            // Reset the game for a new round
            StartCoroutine(PrepareForNextRound());
            
            Debug.Log($"Spin finished. Winning number: {winningNumber}, Total winnings: {totalWinnings}, New balance: {newBalance}");
        }
    }
    
    private IEnumerator PrepareForNextRound()
    {
        // Wait a few seconds after showing the results
        yield return new WaitForSeconds(5f);
        
        // Reset the roulette wheel if needed
        if (wheelController != null)
        {
            wheelController.ResetRoulette();
        }
        
        
    }
    
    private int CalculateWinnings(int winningNumber)
    {
        int totalWinnings = 0;
        
        foreach (var bet in placedBets.Values)
        {
            bool isWinningBet = false;
            int multiplier = 0;
            
            switch (bet.betType)
            {
                case BetTypes.Straight:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = straightBetMultiplier;
                    break;
                
                case BetTypes.SlipBetTwo:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = splitBetMultiplier;
                    break;
                
                case BetTypes.StreetBetThree:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = streetBetMultiplier;
                    break;
                
                case BetTypes.CornerBetFour:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = cornerBetMultiplier;
                    break;
                
                case BetTypes.SixLineBet:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = sixLineBetMultiplier;
                    break;
                
                case BetTypes.ColumnBetOnTwelveNumbers:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = columnBetMultiplier;
                    break;
                
                case BetTypes.DozenBet:
                    isWinningBet = bet.connectedNumbers.Contains(winningNumber);
                    multiplier = dozenBetMultiplier;
                    break;
                
                case BetTypes.High:
                    isWinningBet = winningNumber > 0 && winningNumber >= 19 && winningNumber <= 36;
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                case BetTypes.Low:
                    isWinningBet = winningNumber > 0 && winningNumber >= 1 && winningNumber <= 18;
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                case BetTypes.Red:
                    isWinningBet = IsRedNumber(winningNumber);
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                case BetTypes.Black:
                    isWinningBet = !IsRedNumber(winningNumber) && winningNumber > 0;
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                case BetTypes.Odd:
                    isWinningBet = winningNumber > 0 && winningNumber % 2 == 1;
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                case BetTypes.Even:
                    isWinningBet = winningNumber > 0 && winningNumber % 2 == 0;
                    multiplier = evenOddRedBlackHighLowMultiplier;
                    break;
                
                default:
                    continue;
            }
            
            if (isWinningBet)
            {
                // Return original bet + winnings
                int winnings = bet.totalAmount * (1 + multiplier);
                totalWinnings += winnings;
                
                Debug.Log($"Winning bet on {bet.betType}! Amount: {bet.totalAmount}, Winnings: {winnings}");
            }
        }
        
        return totalWinnings;
    }
    
    private bool IsRedNumber(int number)
    {
        // Red numbers in roulette are: 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
        int[] redNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
        return Array.IndexOf(redNumbers, number) != -1;
    }
    
    private void ClearAllBets()
    {
        // Clear tracked bets
        placedBets.Clear();
        currentBetAmount = 0;
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
    
    // Getter for current bet amount
    public int CurrentBetAmount => currentBetAmount;
}