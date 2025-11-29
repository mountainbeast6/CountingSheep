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
    public float Scale { get; set; } = 1f;
    public int LevelRequirement { get; set; } = 1;

    public ShopItem(string id, string name, string type, int cost, float scale = 1f, int levelRequirement = 1)
    {
        Id = id;
        Name = name;
        Type = type;
        Cost = cost;
        Scale = scale;
        LevelRequirement = levelRequirement;
    }
}