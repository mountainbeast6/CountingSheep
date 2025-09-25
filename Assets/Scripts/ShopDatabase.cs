using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase
{
    public Dictionary<string, ShopItem> Items { get; private set; }
    public Dictionary<string, Vector3> HomePositions { get; private set; }

    public ShopDatabase()
    {
        Items = new Dictionary<string, ShopItem>();
        HomePositions = new Dictionary<string, Vector3>();

        // Beds
        Items["bed1"] = new ShopItem("bed1", "Wooden Bed", "bed", 200);
        Items["bed2"] = new ShopItem("bed2", "Modern Bed", "bed", 300);
        Items["bed3"] = new ShopItem("bed3", "Classic Bed", "bed", 250);
        Items["bed4"] = new ShopItem("bed4", "Luxury Bed", "bed", 500);

        // Chairs
        Items["chair1"] = new ShopItem("chair1", "Simple Chair", "chair", 100);
        Items["chair2"] = new ShopItem("chair2", "Office Chair", "chair", 150);
        Items["chair3"] = new ShopItem("chair3", "Rocking Chair", "chair", 180);
        Items["chair4"] = new ShopItem("chair4", "Gaming Chair", "chair", 300);

        // Desks
        Items["desk1"] = new ShopItem("desk1", "Study Desk", "desk", 250);
        Items["desk2"] = new ShopItem("desk2", "Office Desk", "desk", 350);
        Items["desk3"] = new ShopItem("desk3", "Standing Desk", "desk", 400);
        Items["desk4"] = new ShopItem("desk4", "Luxury Desk", "desk", 600);

        // Lamps
        Items["lamp1"] = new ShopItem("lamp1", "Table Lamp", "lamp", 80);
        Items["lamp2"] = new ShopItem("lamp2", "Floor Lamp", "lamp", 120);
        Items["lamp3"] = new ShopItem("lamp3", "Modern Lamp", "lamp", 200);
        Items["lamp4"] = new ShopItem("lamp4", "Crystal Lamp", "lamp", 350);

        // Home positions (adjust these to fit your home layout in Unity)
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
}
