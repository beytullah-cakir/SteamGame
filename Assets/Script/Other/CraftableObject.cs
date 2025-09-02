using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Craftable Object/Item")]
public class CraftableObject : ScriptableObject
{
    public List<RequiredItemForCreate> requiredItems;
    public Item resultItem;
    public int resultCount = 1;


    public string GetRequiredItems()
    {
        string result = "";
        foreach (var requiredItem in requiredItems)
        {
            result += $"{requiredItem.item.name}x{requiredItem.amount}\n";
        }
        return result;
    }
}


[System.Serializable]
public class RequiredItemForCreate
{
    public Item item;
    public int amount;    
}
