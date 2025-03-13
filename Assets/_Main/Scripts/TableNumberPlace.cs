using System.Collections.Generic;
using UnityEngine;

public class TableNumberPlace : MonoBehaviour
{
    [SerializeField] private BetTypes placeBetType;
    [SerializeField] private List<int> connectedNumbers;
    
    // Properties to expose bet type and connected numbers
    public BetTypes PlaceBetType => placeBetType;
    public List<int> ConnectedNumbers => connectedNumbers;
    
    // Reference to MoneyCanvasController to check balance
    private MoneyCanvasController moneyController;
    
    // LIFO mantığı için chipleri tutan yığın
    private Stack<Chip> chipStack = new Stack<Chip>();
    
    // Toplam bahis tutarını tutmak için
    private int currentBetAmount = 0;
    
    // Maximum number of chips that can be stacked
    [SerializeField] private int maxStackHeight = 10;

    private void Awake()
    {
        // Find the MoneyCanvasController in the scene
        moneyController = FindObjectOfType<MoneyCanvasController>();
        if (moneyController == null)
        {
            Debug.LogError("MoneyCanvasController could not be found in the scene!");
        }
    }

    // Yeni bet ekler (normal tıklama sonucu)
    public bool PlaceBet(Chips chipType)
    {
        // Check if we've reached the maximum stack height
        if (chipStack.Count >= maxStackHeight)
        {
            Debug.Log($"Maximum stack height reached on {gameObject.name}. Cannot add more chips.");
            return false;
        }
        
        // Check if player has enough balance for this bet
        int chipValue = GetChipValue(chipType);
        
        // Bu sonraki total bahis tutarı olacak
        int totalBetAmount = currentBetAmount + chipValue;
        
        if (moneyController != null && !moneyController.HasEnoughFunds(totalBetAmount))
        {
            // Show insufficient funds message
            EventManager.TriggerEvent(GameEvents.OnInsufficientFunds);
            Debug.Log("Insufficient funds to place bet!");
            return false;
        }

        // ChipPool üzerinden yeni chip alınır
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
        
        // Position the chip on top of the stack
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        newChip.transform.position = chipPosition;
        newChip.gameObject.SetActive(true);
        
        // Bahis tutarı güncellenir
        currentBetAmount = totalBetAmount;

        // Chip yığına eklenir ve bağlı alanı güncellenir
        chipStack.Push(newChip);
        newChip.currentPlace = this;
        
        // Bet event'ini tetikle
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chipType);
        Debug.Log($"New chip placed: {chipType}, Value: {chipValue}, Place total: {currentBetAmount}, Stack size: {chipStack.Count}");
        
        return true;
    }

    // Sürükleme sonucu chip bırakılır
    public bool PlaceDraggedChip(Chip chip)
    {
        // Check if we've reached the maximum stack height
        if (chipStack.Count >= maxStackHeight)
        {
            Debug.Log($"Maximum stack height reached on {gameObject.name}. Cannot add more chips.");
            return false;
        }
        
        // Check if player has enough balance for this bet
        int chipValue = GetChipValue(chip.ChipType);
        
        // If this is the same place the chip came from, we don't need to check funds
        if (chip.currentPlace != this && moneyController != null && 
            !moneyController.HasEnoughFunds(currentBetAmount + chipValue))
        {
            // Show insufficient funds message
            EventManager.TriggerEvent(GameEvents.OnInsufficientFunds);
            Debug.Log("Insufficient funds to place bet!");
            return false;
        }

        // Position the chip on top of the stack
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        chip.transform.position = chipPosition;
        chip.gameObject.SetActive(true);
        chipStack.Push(chip);
        chip.currentPlace = this;
        
        // Bahis miktarını güncelle - bu currentBetAmount değişkenine ekleme yapar
        currentBetAmount += chipValue;
        
        // Bet event'ini tetikle - bu BetCanvasController'daki bahisi günceller
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chip.ChipType);
        Debug.Log($"Dragged chip placed: {chip.ChipType}, Value: {chipValue}, Place total: {currentBetAmount}, Stack size: {chipStack.Count}");
        
        return true;
    }

    // Yığının tepesindeki (son eklenen) chipi çıkarır
    public Chip RemoveChip()
    {
        if(chipStack.Count > 0)
        {
            Chip removedChip = chipStack.Pop();
            currentBetAmount -= GetChipValue(removedChip.ChipType);
            removedChip.currentPlace = null;
            
            // Chip removal event'ini tetikle
            EventManager.TriggerEvent(GameEvents.OnChipRemoved, this, removedChip.ChipType);
            Debug.Log($"Chip removed: {removedChip.ChipType}, Remaining total: {currentBetAmount}, Stack size: {chipStack.Count}");
            
            return removedChip;
        }
        return null;
    }

    // Yığının boş olup olmadığını kontrol eden property
    public bool HasChips
    {
        get { return chipStack.Count > 0; }
    }
    
    // Chip stack size
    public int ChipCount
    {
        get { return chipStack.Count; }
    }
    
    // Toplam bahis tutarı
    public int CurrentBetAmount => currentBetAmount;
    
    // Tüm chipleri havuza geri döndürür
    public void ReturnAllChipsToPool()
    {
        // Stakteki tüm chipleri döndür
        int initialCount = chipStack.Count;
        while (chipStack.Count > 0)
        {
            Chip chip = chipStack.Pop();
            if (chip != null)
            {
                // ChipPool'a geri döndür
                ChipPool.Instance.ReturnChip(chip.gameObject);
                
                // Trigger removal event
                EventManager.TriggerEvent(GameEvents.OnChipRemoved, this, chip.ChipType);
            }
        }
        
        // Reset bet amount
        currentBetAmount = 0;
        Debug.Log($"All {initialCount} chips returned to pool. Current bet amount reset to 0.");
    }
    
    // Yardımcı: Chip değerini döndürür
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