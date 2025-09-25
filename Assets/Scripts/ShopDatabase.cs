using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase
{
    private static ShopDatabase _instance;
    public static ShopDatabase Instance => _instance ??= new ShopDatabase();

    public Dictionary<string, ShopItem> items;

    public ShopDatabase()
    {
        items = new Dictionary<string, ShopItem>
        {
            { "bed1", new ShopItem("bed1", "Bed 1", 100, Vector3.zero) },
            { "bed2", new ShopItem("bed2", "Bed 2", 100, Vector3.zero) },
            { "bed3", new ShopItem("bed3", "Bed 3", 100, Vector3.zero) },
            { "bed4", new ShopItem("bed4", "Bed 4", 100, Vector3.zero) },
            { "chair1", new ShopItem("chair1", "Chair 1", 50, Vector3.zero) },
            { "chair2", new ShopItem("chair2", "Chair 2", 50, Vector3.zero) },
            { "chair3", new ShopItem("chair3", "Chair 3", 50, Vector3.zero) },
            { "chair4", new ShopItem("chair4", "Chair 4", 50, Vector3.zero) },
            { "desk1", new ShopItem("desk1", "Desk 1", 150, Vector3.zero) },
            { "desk2", new ShopItem("desk2", "Desk 2", 150, Vector3.zero) },
            { "desk3", new ShopItem("desk3", "Desk 3", 150, Vector3.zero) },
            { "desk4", new ShopItem("desk4", "Desk 4", 150, Vector3.zero) },
            { "lamp1", new ShopItem("lamp1", "Lamp 1", 30, Vector3.zero) },
            { "lamp2", new ShopItem("lamp2", "Lamp 2", 30, Vector3.zero) },
            { "lamp3", new ShopItem("lamp3", "Lamp 3", 30, Vector3.zero) },
            { "lamp4", new ShopItem("lamp4", "Lamp 4", 30, Vector3.zero) },
        };
    }

    public string GetItemType(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId].Type : null;
    }

    public ShopItem GetItem(string itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : null;
    }
}
