using System.Collections.Generic;
using UnityEngine;

// Class to track a bet placed on a specific spot
[System.Serializable]
public class PlacedBet
{
    public TableNumberPlace place;
    public BetTypes betType;
    public List<int> connectedNumbers;
    public Dictionary<Chips, int> chipCounts = new Dictionary<Chips, int>();
    public int totalAmount;

    public PlacedBet(TableNumberPlace place)
    {
        this.place = place;
        this.betType = place.PlaceBetType;
        this.connectedNumbers = place.ConnectedNumbers;
        
        // Initialize chip counts dictionary
        chipCounts[Chips.Ten] = 0;
        chipCounts[Chips.Fifty] = 0;
        chipCounts[Chips.Hundered] = 0;
        chipCounts[Chips.TwoHundered] = 0;
        
        totalAmount = 0;
    }

    public void AddChip(Chips chipType)
    {
        if (chipCounts.ContainsKey(chipType))
        {
            chipCounts[chipType]++;
        }
        else
        {
            chipCounts[chipType] = 1;
        }
        
        // Update total amount
        totalAmount += GetChipValue(chipType);
    }

    public void RemoveChip(Chips chipType)
    {
        if (chipCounts.ContainsKey(chipType) && chipCounts[chipType] > 0)
        {
            chipCounts[chipType]--;
            totalAmount -= GetChipValue(chipType);
        }
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
}