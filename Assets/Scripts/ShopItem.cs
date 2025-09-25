using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string Id;
    public string Name;
    public int Cost;
    public Vector3 HomePosition;
    public string Type;
    public GameObject Prefab;  // <-- add this

    // Constructor
    public ShopItem(string id, string name, int cost, Vector3 homePosition, GameObject prefab = null)
    {
        Id = id;
        Name = name;
        Cost = cost;
        HomePosition = homePosition;
        Prefab = prefab;

        if (id.Contains("bed")) Type = "bed";
        else if (id.Contains("chair")) Type = "chair";
        else if (id.Contains("desk")) Type = "desk";
        else if (id.Contains("lamp")) Type = "lamp";
        else Type = "misc";
    }
}
