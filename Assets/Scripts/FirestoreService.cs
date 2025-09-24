using Firebase.Firestore;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreService
{
    private FirebaseFirestore db;

    public FirestoreService()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    // Save or update player data
    public async Task SavePlayerAsync(string userId, PlayerData player)
    {
        try
        {
            DocumentReference docRef = db.Collection("players").Document(userId);
            await docRef.SetAsync(player);
            Debug.Log("Player data saved successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save player data: " + e);
        }
    }

    // Load player data
    public async Task<PlayerData> LoadPlayerAsync(string userId)
    {
        try
        {
            DocumentReference docRef = db.Collection("players").Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<PlayerData>();
            }
            else
            {
                Debug.LogWarning("No player data found.");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to load player data: " + e);
            return null;
        }
    }

    // Earn money
    public async Task EarnMoneyAsync(string userId, int amount)
    {
        PlayerData player = await LoadPlayerAsync(userId);
        if (player != null)
        {
            player.Money += amount;
            await SavePlayerAsync(userId, player);
        }
    }

    // Spend money
    public async Task<bool> SpendMoneyAsync(string userId, int amount)
    {
        PlayerData player = await LoadPlayerAsync(userId);
        if (player != null && player.Money >= amount)
        {
            player.Money -= amount;
            await SavePlayerAsync(userId, player);
            return true;
        }
        return false; // not enough money
    }
}
