using System.Collections.Generic;
using UnityEngine;


public class ChipPool : MonoBehaviour
{
    // Singleton instance
    public static ChipPool Instance;

    [Header("Chip Prefabs")]
    public GameObject chipPrefabTen;
    public GameObject chipPrefabFifty;
    public GameObject chipPrefabHundred;
    public GameObject chipPrefabTwoHundred;

    [Header("Pool Settings")]
    public int initialPoolCount = 10;

    private List<GameObject> poolTen = new List<GameObject>();
    private List<GameObject> poolFifty = new List<GameObject>();
    private List<GameObject> poolHundred = new List<GameObject>();
    private List<GameObject> poolTwoHundred = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializePools();
    }

    private void InitializePools()
    {
        for (int i = 0; i < initialPoolCount; i++)
        {
            GameObject obj = Instantiate(chipPrefabTen,transform);
            obj.SetActive(false);
            poolTen.Add(obj);
        }
        for (int i = 0; i < initialPoolCount; i++)
        {
            GameObject obj = Instantiate(chipPrefabFifty,transform);
            obj.SetActive(false);
            poolFifty.Add(obj);
        }
        for (int i = 0; i < initialPoolCount; i++)
        {
            GameObject obj = Instantiate(chipPrefabHundred,transform);
            obj.SetActive(false);
            poolHundred.Add(obj);
        }
        for (int i = 0; i < initialPoolCount; i++)
        {
            GameObject obj = Instantiate(chipPrefabTwoHundred,transform);
            obj.SetActive(false);
            poolTwoHundred.Add(obj);
        }
    }

    public GameObject GetChip(Chips chipType)
    {
        List<GameObject> selectedPool = null;
        GameObject prefab = null;
        switch (chipType)
        {
            case Chips.Ten:
                selectedPool = poolTen;
                prefab = chipPrefabTen;
                break;
            case Chips.Fifty:
                selectedPool = poolFifty;
                prefab = chipPrefabFifty;
                break;
            case Chips.Hundered:
                selectedPool = poolHundred;
                prefab = chipPrefabHundred;
                break;
            case Chips.TwoHundered:
                selectedPool = poolTwoHundred;
                prefab = chipPrefabTwoHundred;
                break;
        }

        foreach (var obj in selectedPool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        
        GameObject newObj = Instantiate(prefab,transform);
        newObj.SetActive(false);
        selectedPool.Add(newObj);
        return newObj;
    }

    public void ReturnChip(GameObject chip)
    {
        if (chip != null)
        {
            chip.transform.position = transform.position;
        
            Chip chipComponent = chip.GetComponent<Chip>();
            if (chipComponent != null)
            {
                chipComponent.currentPlace = null;
            }
        
            chip.SetActive(false);
        }
    }
}
