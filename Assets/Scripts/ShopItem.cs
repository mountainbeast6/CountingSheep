using UnityEngine;

[System.Serializable]
public class ShopItem
{
    public string Id;
    public string Name;
    public int Cost;
    public string Type;       // "bed", "chair", "lamp", "desk"
    public GameObject Prefab; // prefab for placing in home
    public Vector3 HomePosition; // fixed home position
}
