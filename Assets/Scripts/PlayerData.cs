using System;
using System.Collections.Generic;
using Firebase.Firestore;  // For [FirestoreData] and [FirestoreProperty]

[FirestoreData]
[Serializable]
public class PlayerData
{
    [FirestoreProperty] public string Name { get; set; }
    [FirestoreProperty] public string Email { get; set; }
    [FirestoreProperty] public int Money { get; set; }
    [FirestoreProperty] public List<string> Inventory { get; set; } = new List<string>();
    [FirestoreProperty] public Dictionary<string, string> HomeItems { get; set; } = new Dictionary<string, string>();
    [FirestoreProperty] public List<string> CompletedGoals { get; set; } = new List<string>();
}
