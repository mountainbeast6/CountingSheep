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
        Items["bed1"] = new ShopItem("bed1", "Small Bed", "bed", 200, 1.0f);
        Items["bed2"] = new ShopItem("bed2", "Blue Bed", "bed", 300, 1.15f);
        Items["bed3"] = new ShopItem("bed3", "Pink Bed", "bed", 400, 1.15f);
        Items["bed4"] = new ShopItem("bed4", "Large Bed", "bed", 450, 1.15f);
        Items["bed5"] = new ShopItem("bed5", "Fancy Bed", "bed", 500, 1.15f);

        // Chairs
        Items["chair1"] = new ShopItem("chair1", "Rough Wood Chair", "chair", 100, 0.5f);
        Items["chair2"] = new ShopItem("chair2", "Folding Chair", "chair", 150, 0.5f);
        Items["chair3"] = new ShopItem("chair3", "Light Wood Chair", "chair", 200, 0.5f);
        Items["chair4"] = new ShopItem("chair4", "Dark Wood Chair", "chair", 250, 0.5f);
        Items["chair5"] = new ShopItem("chair5", "Office Chair", "chair", 300, 0.5f);
        Items["chair6"] = new ShopItem("chair6", "Comfy Chair", "chair", 400, 0.5f);
        Items["chair7"] = new ShopItem("chair7", "White Chair", "chair", 450, 0.6f);

        // Desks
        Items["desk1"] = new ShopItem("desk1", "Wooden Desk", "desk", 250, 0.5f);
        Items["desk2"] = new ShopItem("desk2", "Simple Desk", "desk", 350, 0.5f);
        Items["desk3"] = new ShopItem("desk3", "Work Desk", "desk", 400, 0.5f);
        Items["desk4"] = new ShopItem("desk4", "Office Desk", "desk", 450, 0.5f);
        Items["desk5"] = new ShopItem("desk5", "Study Desk", "desk", 500, 0.6f);
        Items["desk6"] = new ShopItem("desk6", "Pink Desk", "desk", 600, 0.5f);
        Items["desk7"] = new ShopItem("desk7", "White Desk", "desk", 700, 0.70f);

        // Lamps
        Items["lamp1"] = new ShopItem("lamp1", "Desk Lamp", "lamp", 80, 0.5f);
        Items["lamp2"] = new ShopItem("lamp2", "Floor Lamp", "lamp", 120, 0.6f);
        Items["lamp3"] = new ShopItem("lamp3", "White Lamp", "lamp", 300, 0.65f);
        Items["lamp4"] = new ShopItem("lamp4", "Blue Lamp", "lamp", 350, 0.65f);
        Items["lamp5"] = new ShopItem("lamp5", "Pink Lamp", "lamp", 400, 0.7f);
        Items["lamp6"] = new ShopItem("lamp6", "Fancy Lamp", "lamp", 500, 0.75f);

        // Bookshelves
        Items["bookshelf1"] = new ShopItem("bookshelf1", "Bookshelf", "bookshelf", 400, 1.0f);

        // Home positions
        HomePositions["bed"] = new Vector3(0, 0, 0);
        HomePositions["chair"] = new Vector3(2, 0, 0);
        HomePositions["desk"] = new Vector3(-2, 0, 0);
        HomePositions["lamp"] = new Vector3(0, 0, 2);
        HomePositions["bookshelf"] = new Vector3(0, 0, -2);
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