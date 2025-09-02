using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int maxStackedItems = 4;
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    int selectedSlot = -1;

    public static InventoryManager Instance;

    private void Awake()
    {
        Instance = this;
    }


    public void AddItem(Item item)
    {
        // Ayný item varsa üzerine ekle
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null &&
                itemInSlot.item == item &&
                itemInSlot.count < maxStackedItems)
            {
                itemInSlot.count++;
                itemInSlot.RefreshCount();
                return;
            }
        }

        // Boþ slot bul ve yeni item spawnla
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot == null)
            {
                SpawnNewItem(item, slot);
                return;
            }
        }
    }



    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public Item GetSelectedItem(bool use)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot == null)
        {
            Item item = itemInSlot.item;
            if (use)
            {
                itemInSlot.count--;
                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
                else
                {
                    itemInSlot.RefreshCount();
                }
            }
            return item;
        }
        return null;
    }


    public void Craft(CraftableObject craftable)
    {
        // 1. Malzemeler yeterli mi?
        foreach (var req in craftable.requiredItems)
        {
            int totalInInventory = 0;
            foreach (var slot in inventorySlots)
            {
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null && itemInSlot.item == req.item)
                {
                    totalInInventory += itemInSlot.count;
                }
            }

            if (totalInInventory < req.amount)
            {
                Debug.Log("Yetersiz malzeme: " + req.item.name);
                return;
            }
        }

        // 2. Malzemeleri eksilt
        foreach (var req in craftable.requiredItems)
        {
            int toRemove = req.amount;

            for (int i = 0; i < inventorySlots.Length; i++)
            {
                InventoryItem itemInSlot = inventorySlots[i].GetComponentInChildren<InventoryItem>();
                if (itemInSlot != null && itemInSlot.item == req.item)
                {
                    int removed = Mathf.Min(itemInSlot.count, toRemove);
                    itemInSlot.count -= removed;
                    toRemove -= removed;

                    if (itemInSlot.count <= 0)
                    {
                        Destroy(itemInSlot.gameObject);
                    }
                    else
                    {
                        itemInSlot.RefreshCount();
                    }

                    if (toRemove <= 0)
                        break;
                }
            }
        }

        
        for (int i = 0; i < craftable.resultCount; i++)
        {
            AddItem(craftable.resultItem);
        }
        Debug.Log($"Üretildi: {craftable.resultItem.name} x{craftable.resultCount}");

       
    }


}
