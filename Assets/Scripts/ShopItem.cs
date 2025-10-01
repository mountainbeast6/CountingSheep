using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Cost { get; set; }
    public GameObject Prefab { get; set; }
    public Vector3 HomePosition { get; set; }
    public Sprite Icon { get; set; }

    public ShopItem(string id, string name, string type, int cost)
    {
        Id = id;
        Name = name;
        Type = type;
        Cost = cost;
    }
}
