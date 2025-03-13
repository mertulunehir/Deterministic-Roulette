using System.Collections.Generic;
using UnityEngine;

public class TableNumberPlace : MonoBehaviour
{
    [SerializeField] private BetTypes placeBetType;
    [SerializeField] private List<int> connectedNumbers;
    
    // Properties to expose bet type and connected numbers
    public BetTypes PlaceBetType => placeBetType;
    public List<int> ConnectedNumbers => connectedNumbers;
    
    private MoneyCanvasController moneyController;
    
    private Stack<Chip> chipStack = new Stack<Chip>();
    
    private int currentBetAmount = 0;
    
    // Maximum number of chips that can be stacked
    [SerializeField] private int maxStackHeight = 10;
    
    
    public bool HasChips
    {
        get { return chipStack.Count > 0; }
    }
    
    public int ChipCount
    {
        get { return chipStack.Count; }
    }
    
    public int CurrentBetAmount => currentBetAmount;


    private void Awake()
    {
        //TODO:
        moneyController = FindObjectOfType<MoneyCanvasController>();
        if (moneyController == null)
        {
            Debug.LogError("MoneyCanvasController could not be found in the scene!");
        }
    }

    // New bet placement click
    public bool PlaceBet(Chips chipType)
    {
        if (chipStack.Count >= maxStackHeight)
        {
            Debug.Log($"Maximum stack height reached on {gameObject.name}. Cannot add more chips.");
            return false;
        }
        
        int chipValue = GetChipValue(chipType);
        
        int totalBetAmount = currentBetAmount + chipValue;
        
        if (moneyController != null && !moneyController.HasEnoughFunds(totalBetAmount))
        {
            EventManager.TriggerEvent(GameEvents.OnInsufficientFunds);
            Debug.Log("Insufficient funds to place bet!");
            return false;
        }

        GameObject chipObj = ChipPool.Instance.GetChip(chipType);
        if (chipObj == null)
        {
            Debug.LogError($"Failed to get chip of type {chipType} from the pool!");
            return false;
        }
        
        Chip newChip = chipObj.GetComponent<Chip>();
        if (newChip == null)
        {
            Debug.LogError($"Chip GameObject does not have a Chip component!");
            return false;
        }
        
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        newChip.transform.position = chipPosition;
        newChip.gameObject.SetActive(true);
        
        currentBetAmount = totalBetAmount;

        chipStack.Push(newChip);
        newChip.currentPlace = this;
        
        
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chipType);
        
        Debug.Log($"New chip placed: {chipType}, Value: {chipValue}, Place total: {currentBetAmount}, Stack size: {chipStack.Count}");
        
        return true;
    }

    // Bet drag 
    public bool PlaceDraggedChip(Chip chip)
    {
        if (chipStack.Count >= maxStackHeight)
        {
            Debug.Log($"Maximum stack height reached on {gameObject.name}. Cannot add more chips.");
            return false;
        }
        
        int chipValue = GetChipValue(chip.ChipType);
        
        
        if (chip.currentPlace != this && moneyController != null && 
            !moneyController.HasEnoughFunds(currentBetAmount + chipValue))
        {
            // Show insufficient funds message
            EventManager.TriggerEvent(GameEvents.OnInsufficientFunds);
            Debug.Log("Insufficient funds to place bet!");
            return false;
        }
        
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        chip.transform.position = chipPosition;
        chip.gameObject.SetActive(true);
        chipStack.Push(chip);
        chip.currentPlace = this;
        
        currentBetAmount += chipValue;
        
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chip.ChipType);
        
        Debug.Log($"Dragged chip placed: {chip.ChipType}, Value: {chipValue}, Place total: {currentBetAmount}, Stack size: {chipStack.Count}");
        
        return true;
    }

    
    public Chip RemoveChip()
    {
        if(chipStack.Count > 0)
        {
            Chip removedChip = chipStack.Pop();
            currentBetAmount -= GetChipValue(removedChip.ChipType);
            removedChip.currentPlace = null;
            
            EventManager.TriggerEvent(GameEvents.OnChipRemoved, this, removedChip.ChipType);
            Debug.Log($"Chip removed: {removedChip.ChipType}, Remaining total: {currentBetAmount}, Stack size: {chipStack.Count}");
            
            return removedChip;
        }
        return null;
    }

    public void ReturnAllChipsToPool()
    {
        int initialCount = chipStack.Count;
        while (chipStack.Count > 0)
        {
            Chip chip = chipStack.Pop();
            if (chip != null)
            {
                ChipPool.Instance.ReturnChip(chip.gameObject);
                
                EventManager.TriggerEvent(GameEvents.OnChipRemoved, this, chip.ChipType);
            }
        }
        
        currentBetAmount = 0;
        Debug.Log($"All {initialCount} chips returned to pool. Current bet amount reset to 0.");
    }
    
    // Helper : Return chip value
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