using System.Collections.Generic;
using UnityEngine;

public class TableNumberPlace : MonoBehaviour
{
    [SerializeField] private BetTypes placeBetType;
    [SerializeField] private List<int> connectedNumbers;
    
    // Properties to expose bet type and connected numbers
    public BetTypes PlaceBetType => placeBetType;
    public List<int> ConnectedNumbers => connectedNumbers;
    
    // LIFO mantığı için chipleri tutan yığın
    private Stack<Chip> chipStack = new Stack<Chip>();
    
    // Toplam bahis tutarını tutmak için
    private int currentBetAmount = 0;

    // Yeni bet ekler (normal tıklama sonucu)
    public void PlaceBet(Chips chipType)
    {
        // ChipPool üzerinden yeni chip alınır
        GameObject chipObj = ChipPool.Instance.GetChip(chipType);
        Chip newChip = chipObj.GetComponent<Chip>();
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        newChip.transform.position = chipPosition;
        newChip.gameObject.SetActive(true);
        
        // Bahis tutarı güncellenir
        currentBetAmount += GetChipValue(chipType);

        // Chip yığına eklenir ve bağlı alanı güncellenir
        chipStack.Push(newChip);
        newChip.currentPlace = this;
        
        // Bet event'ini tetikle
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chipType);
    }

    // Sürükleme sonucu chip bırakılır
    public void PlaceDraggedChip(Chip chip)
    {
        int chipCount = chipStack.Count;
        Vector3 chipPosition = transform.position + (Vector3.up * 0.1f * chipCount);
        chip.transform.position = chipPosition;
        chip.gameObject.SetActive(true);
        chipStack.Push(chip);
        chip.currentPlace = this;
        currentBetAmount += GetChipValue(chip.ChipType);
        
        // Bet event'ini tetikle
        EventManager.TriggerEvent(GameEvents.OnChipPlaced, this, chip.ChipType);
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
            
            return removedChip;
        }
        return null;
    }

    // Yığının boş olup olmadığını kontrol eden property
    public bool HasChips
    {
        get { return chipStack.Count > 0; }
    }
    
    // Toplam bahis tutarı
    public int CurrentBetAmount => currentBetAmount;
    
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