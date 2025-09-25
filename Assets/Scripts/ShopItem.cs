using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string Id;
    public string Name;
    public string Type;
    public int Cost;
    public GameObject Prefab;
    public Vector3 HomePosition;

    public ShopItem(string id, string name, string type, int cost, GameObject prefab = null, Vector3 homePos = default)
    {
        Id = id;
        Name = name;
        Type = type;
        Cost = cost;
        Prefab = prefab;
        HomePosition = homePos;
    }
}
