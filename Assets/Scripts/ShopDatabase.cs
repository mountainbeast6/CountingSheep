using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase
{
    private static ShopDatabase _instance;
    public static ShopDatabase Instance => _instance ??= new ShopDatabase();

    private Dictionary<string, ShopItem> items;

    private ShopDatabase()
    {
        items = new Dictionary<string, ShopItem>();

        // Create 4 beds
        for (int i = 1; i <= 4; i++)
        {
            items.Add($"bed{i}", new ShopItem
            {
                Id = $"bed{i}",
                Name = $"Bed {i}",
                Type = "bed",
                Cost = 100,
                Prefab = Resources.Load<GameObject>($"Prefabs/Bed{i}"),
                HomePosition = GetPositionForSlot("bed", i)
            });
        }

        // Create 4 chairs
        for (int i = 1; i <= 4; i++)
        {
            items.Add($"chair{i}", new ShopItem
            {
                Id = $"chair{i}",
                Name = $"Chair {i}",
                Type = "chair",
                Cost = 50,
                Prefab = Resources.Load<GameObject>($"Prefabs/Chair{i}"),
                HomePosition = GetPositionForSlot("chair", i)
            });
        }

        // Create 4 desks
        for (int i = 1; i <= 4; i++)
        {
            items.Add($"desk{i}", new ShopItem
            {
                Id = $"desk{i}",
                Name = $"Desk {i}",
                Type = "desk",
                Cost = 150,
                Prefab = Resources.Load<GameObject>($"Prefabs/Desk{i}"),
                HomePosition = GetPositionForSlot("desk", i)
            });
        }

        // Create 4 lamps
        for (int i = 1; i <= 4; i++)
        {
            items.Add($"lamp{i}", new ShopItem
            {
                Id = $"lamp{i}",
                Name = $"Lamp {i}",
                Type = "lamp",
                Cost = 30,
                Prefab = Resources.Load<GameObject>($"Prefabs/Lamp{i}"),
                HomePosition = GetPositionForSlot("lamp", i)
            });
        }
    }

    public ShopItem GetItem(string itemId)
    {
        items.TryGetValue(itemId, out ShopItem item);
        return item;
    }

    private Vector3 GetPositionForSlot(string type, int index)
    {
        // Example positions for home placement
        switch (type)
        {
            case "bed": return new Vector3(index * 2, 0, 0);
            case "chair": return new Vector3(index * 2, 0, 2);
            case "desk": return new Vector3(index * 2, 0, 4);
            case "lamp": return new Vector3(index * 2, 0, 6);
            default: return Vector3.zero;
        }
    }
}
