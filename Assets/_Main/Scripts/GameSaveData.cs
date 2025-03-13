using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameRecord
{
    public int gameId;
    public int winningNumber;
    public bool isWin;
    public int betAmount;
    public int winAmount;
    public List<BetTypeRecord> bets = new List<BetTypeRecord>();
    public string timestamp;
    
    public GameRecord() { }
    
    public GameRecord(int gameId, int winningNumber, bool isWin, int betAmount, int winAmount, string timestamp)
    {
        this.gameId = gameId;
        this.winningNumber = winningNumber;
        this.isWin = isWin;
        this.betAmount = betAmount;
        this.winAmount = winAmount;
        this.timestamp = timestamp;
        this.bets = new List<BetTypeRecord>();
    }
}

[Serializable]
public class BetTypeRecord
{
    public BetTypes betType;
    public int amount;
    public bool isWin;
}

[Serializable]
public class GameSaveData
{
    public List<GameRecord> gameHistory = new List<GameRecord>();
    
    public int totalWins = 0;
    public int totalLosses = 0;
    public int currentBalance = 10000; // Default starting balance
    //MONEYSET
    public int gameCounter = 0;
    
    public Dictionary<BetTypes, int> winsByBetType = new Dictionary<BetTypes, int>();
    public Dictionary<BetTypes, int> lossesByBetType = new Dictionary<BetTypes, int>();
    
    public GameSaveData()
    {
        foreach (BetTypes betType in Enum.GetValues(typeof(BetTypes)))
        {
            winsByBetType[betType] = 0;
            lossesByBetType[betType] = 0;
        }
    }
}