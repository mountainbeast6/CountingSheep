using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase
{
    public Dictionary<string, ShopItem> Items { get; private set; }
    public Dictionary<string, Vector3> HomePositions { get; private set; }
    
    // Sprite storage
    public Dictionary<string, Sprite> ItemSprites { get; private set; }

    public ShopDatabase()
    {
        Items = new Dictionary<string, ShopItem>();
        HomePositions = new Dictionary<string, Vector3>();
        ItemSprites = new Dictionary<string, Sprite>();

        // Beds
        Items["bed1"] = new ShopItem("bed1", "Small Bed", "bed", 200);
        Items["bed2"] = new ShopItem("bed2", "Pink Bed", "bed", 300);
        Items["bed3"] = new ShopItem("bed3", "Large Bed", "bed", 250);
        Items["bed4"] = new ShopItem("bed4", "Fancy Bed", "bed", 500);

        // Chairs
        Items["chair1"] = new ShopItem("chair1", "Wooden Chair", "chair", 100);
        Items["chair2"] = new ShopItem("chair2", "Folding Chair", "chair", 150);
        Items["chair3"] = new ShopItem("chair3", "Office Chair", "chair", 200);
        Items["chair4"] = new ShopItem("chair4", "Comfy Chair", "chair", 300);

        // Desks
        Items["desk1"] = new ShopItem("desk1", "Wooden Desk", "desk", 250);
        Items["desk2"] = new ShopItem("desk2", "Simple Desk", "desk", 350);
        Items["desk3"] = new ShopItem("desk3", "Study Desk", "desk", 400);
        Items["desk4"] = new ShopItem("desk4", "Pink Desk", "desk", 500);

        // Lamps
        Items["lamp1"] = new ShopItem("lamp1", "Desk Lamp", "lamp", 80);
        Items["lamp2"] = new ShopItem("lamp2", "Floor Lamp", "lamp", 120);
        Items["lamp3"] = new ShopItem("lamp3", "Fancy Lamp", "lamp", 200);

        // Home positions
        HomePositions["bed"] = new Vector3(0, 0, 0);
        HomePositions["chair"] = new Vector3(2, 0, 0);
        HomePositions["desk"] = new Vector3(-2, 0, 0);
        HomePositions["lamp"] = new Vector3(0, 0, 2);
    }

    public ShopItem GetItem(string id)
    {
        return Items.ContainsKey(id) ? Items[id] : null;
    }

    public Vector3 GetHomePosition(string type)
    {
        return HomePositions.ContainsKey(type) ? HomePositions[type] : Vector3.zero;
    }
    
    // Get sprite for a specific item
    public Sprite GetSprite(string itemId)
    {
        return ItemSprites.ContainsKey(itemId) ? ItemSprites[itemId] : null;
    }
    
    // Set sprite for a specific item (called from FirebaseController)
    public void SetSprite(string itemId, Sprite sprite)
    {
        ItemSprites[itemId] = sprite;
    }
}