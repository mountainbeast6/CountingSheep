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
        Items["bed1"] = new ShopItem("bed1", "Small Bed", "bed", 200, 1.0f, 1);
        Items["bed2"] = new ShopItem("bed2", "Yellow Bed", "bed", 200, 1.0f, 2);
        Items["bed3"] = new ShopItem("bed3", "Blue Bed", "bed", 300, 1.15f, 3);
        Items["bed4"] = new ShopItem("bed4", "Green Bed", "bed", 300, 1.15f, 3);
        Items["bed5"] = new ShopItem("bed5", "Pink Bed", "bed", 400, 1.15f, 5);
        Items["bed6"] = new ShopItem("bed6", "Large Bed", "bed", 450, 1.15f, 7);
        Items["bed7"] = new ShopItem("bed7", "Red Fancy Bed", "bed", 500, 1.15f, 9);
        Items["bed8"] = new ShopItem("bed8", "Black Fancy Bed", "bed", 700, 1.15f, 10);

        // Chairs
        Items["chair1"] = new ShopItem("chair1", "Rough Wood Chair", "chair", 100, 0.5f, 1);
        Items["chair2"] = new ShopItem("chair2", "Folding Chair", "chair", 150, 0.5f, 2);
        Items["chair3"] = new ShopItem("chair3", "Light Wood Chair", "chair", 200, 0.5f, 3);
        Items["chair4"] = new ShopItem("chair4", "Dark Wood Chair", "chair", 250, 0.5f, 4);
        Items["chair5"] = new ShopItem("chair5", "Office Chair", "chair", 300, 0.5f, 5);
        Items["chair6"] = new ShopItem("chair6", "Comfy Chair", "chair", 400, 0.5f, 6);
        Items["chair7"] = new ShopItem("chair7", "White Chair", "chair", 450, 0.6f, 8);

        // Desks
        Items["desk1"] = new ShopItem("desk1", "Wooden Desk", "desk", 250, 0.5f, 1);
        Items["desk2"] = new ShopItem("desk2", "Simple Desk", "desk", 350, 0.5f, 2);
        Items["desk3"] = new ShopItem("desk3", "Work Desk", "desk", 400, 0.5f, 4);
        Items["desk4"] = new ShopItem("desk4", "Office Desk", "desk", 450, 0.5f, 6);
        Items["desk5"] = new ShopItem("desk5", "Study Desk", "desk", 500, 0.6f, 8);
        Items["desk6"] = new ShopItem("desk6", "Pink Desk", "desk", 600, 0.5f, 10);
        Items["desk7"] = new ShopItem("desk7", "White Desk", "desk", 700, 0.70f, 12);

        // Lamps
        Items["lamp1"] = new ShopItem("lamp1", "Desk Lamp", "lamp", 80, 0.5f, 1);
        Items["lamp2"] = new ShopItem("lamp2", "Floor Lamp", "lamp", 120, 0.6f, 2);
        Items["lamp3"] = new ShopItem("lamp3", "White Lamp", "lamp", 300, 0.65f, 3);
        Items["lamp4"] = new ShopItem("lamp4", "Green Lamp", "lamp", 350, 0.65f, 4);
        Items["lamp5"] = new ShopItem("lamp5", "Red Lamp", "lamp", 400, 0.65f, 4);
        Items["lamp6"] = new ShopItem("lamp6", "Blue Lamp", "lamp", 400, 0.65f, 6);
        Items["lamp7"] = new ShopItem("lamp7", "Orange Lamp", "lamp", 450, 0.65f, 6);
        Items["lamp8"] = new ShopItem("lamp8", "Pink Lamp", "lamp", 500, 0.7f, 8);
        Items["lamp9"] = new ShopItem("lamp9", "Fancy Lamp", "lamp", 600, 0.75f, 10);

        // Bookshelves
        Items["bookshelf1"] = new ShopItem("bookshelf1", "Bookshelf", "bookshelf", 400, 1.0f, 3);
        Items["bookshelf2"] = new ShopItem("bookshelf2", "Light Brown Bookshelf", "bookshelf", 500, 1.0f, 5);
        Items["bookshelf3"] = new ShopItem("bookshelf3", "White Bookshelf", "bookshelf", 700, 1.0f, 7);

        // Wall Deco
        Items["walldeco1"] = new ShopItem("walldeco1", "Corkboard", "walldeco", 100, 1.5f, 1);
        Items["walldeco2"] = new ShopItem("walldeco2", "Sunset Painting", "walldeco", 200, 2.0f, 2);
        Items["walldeco3"] = new ShopItem("walldeco3", "Sunflowers Painting", "walldeco", 250, 2.0f, 3);
        Items["walldeco4"] = new ShopItem("walldeco4", "Wave Painting", "walldeco", 300, 2.0f, 5);
        Items["walldeco5"] = new ShopItem("walldeco5", "Ghostly Scream", "walldeco", 400, 1.5f, 6);


        // Home positions
        HomePositions["bed"] = new Vector3(0, 0, 0);
        HomePositions["chair"] = new Vector3(2, 0, 0);
        HomePositions["desk"] = new Vector3(-2, 0, 0);
        HomePositions["lamp"] = new Vector3(0, 0, 2);
        HomePositions["bookshelf"] = new Vector3(0, 0, -2);
        HomePositions["walldeco"] = new Vector3(-2, 0, 2);
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