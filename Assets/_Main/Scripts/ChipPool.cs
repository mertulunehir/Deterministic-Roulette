using System.Collections.Generic;
using UnityEngine;


public class ChipPool : MonoBehaviour
{
    // Singleton instance
    public static ChipPool Instance;

    [Header("Chip Prefabları")]
    public GameObject chipPrefabTen;
    public GameObject chipPrefabFifty;
    public GameObject chipPrefabHundred;
    public GameObject chipPrefabTwoHundred;

    [Header("Pool Ayarları")]
    // Her chip tipi için oluşturulacak başlangıç adedi
    public int initialPoolCount = 10;

    // Her chip tipi için ayrı havuz listeleri
    private List<GameObject> poolTen = new List<GameObject>();
    private List<GameObject> poolFifty = new List<GameObject>();
    private List<GameObject> poolHundred = new List<GameObject>();
    private List<GameObject> poolTwoHundred = new List<GameObject>();

    private void Awake()
    {
        // Singleton kontrolü
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

    // Her chip tipi için havuzu başlatır
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

    // İstenilen chip tipine ait havuzdan, inactive olan objeyi döndürür.
    public GameObject GetChip(Chips chipType)
    {
        List<GameObject> selectedPool = null;
        GameObject prefab = null;
        // Hangi chip tipinin havuzundan çekileceğini switch ile belirliyoruz.
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

        // Havuzdaki inactive objeyi döndür.
        foreach (var obj in selectedPool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }
        
        // Eğer havuzda kullanılabilir obje yoksa, yeni obje oluşturup havuza ekle.
        GameObject newObj = Instantiate(prefab,transform);
        newObj.SetActive(false);
        selectedPool.Add(newObj);
        return newObj;
    }

    // Kullanım sonrası chipi havuza geri vermek için kullanılabilir.
    public void ReturnChip(GameObject chip)
    {
        chip.SetActive(false);
        // İsteğe bağlı: chip konumunu resetleyebilirsiniz.
    }
}
