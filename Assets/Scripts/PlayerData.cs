using Firebase.Firestore;

[FirestoreData]
public class PlayerData
{
    [FirestoreProperty]
    public string Name { get; set; }

    [FirestoreProperty]
    public string Email { get; set; }

    [FirestoreProperty]
    public int Money { get; set; }
}
