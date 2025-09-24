using Firebase.Firestore;
using System.Collections.Generic;

[FirestoreData]
public class PlayerData
{
    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Email { get; set; }

    [FirestoreProperty]
    public int Money { get; set; }

    [FirestoreProperty]
    public List<string> CompletedGoals { get; set; } 
    
    [FirestoreProperty]
    public List<string> Inventory { get; set; }
}
