using System.Collections.Generic;
using UnityEngine;

public class ShopDatabase : MonoBehaviour
{
    public static ShopDatabase Instance;

    [SerializeField] private List<ShopItem> items = new List<ShopItem>();

    private Dictionary<string, ShopItem> itemDict;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Convert list → dictionary
        itemDict = new Dictionary<string, ShopItem>();
        foreach (var item in items)
        {
            if (!itemDict.ContainsKey(item.Id))
                itemDict.Add(item.Id, item);
        }
    }

    public ShopItem GetItem(string id)
    {
        return itemDict.ContainsKey(id) ? itemDict[id] : null;
    }
}
