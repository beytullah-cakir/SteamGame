using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AUpgradeableObject : MonoBehaviour
{
    public List<ItemRequirement> requiredItems;
    public Button upgradeButton;
    public int upgradeLevel = 0;
    public int increasePerLevel = 2;

    public virtual void Update()
    {        
        upgradeButton.interactable = CanUpgrade();
    }

    public string GetRequiredItems()
    {
        string result = "";
        foreach (var req in requiredItems)
        {
            result += $"{req.item.name}x{req.amountRequired}\n";
        }
        return result;
    }


    bool CanUpgrade()
    {
        foreach (var req in requiredItems)
        {
            int ownedAmount = CountItemInInventory(req.item);
            if (ownedAmount < req.amountRequired)
                return false;
        }
        return true;
    }

    public void Upgrade()
    {
        if (CanUpgrade())
        {
            foreach (var req in requiredItems)
            {
                RemoveItemsFromInventory(req.item, req.amountRequired);
                req.amountRequired += increasePerLevel;
            }
        }


        upgradeLevel++;
    }

    int CountItemInInventory(Item item)
    {
        int total = 0;
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            InventoryItem invItem = slot.GetComponentInChildren<InventoryItem>();
            if (invItem != null && invItem.item == item)
            {
                total += invItem.count;
            }
        }
        return total;
    }

    void RemoveItemsFromInventory(Item item, int amount)
    {
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            if (amount <= 0) break;

            InventoryItem invItem = slot.GetComponentInChildren<InventoryItem>();
            if (invItem != null && invItem.item == item)
            {
                int toRemove = Mathf.Min(amount, invItem.count);
                invItem.count -= toRemove;
                amount -= toRemove;

                if (invItem.count <= 0)
                    Destroy(invItem.gameObject);
                else
                    invItem.RefreshCount();
            }
        }
    }

}
[System.Serializable]
public class ItemRequirement
{
    public Item item;
    public int amountRequired;

}
