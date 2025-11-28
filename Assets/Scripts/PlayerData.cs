using System;
using System.Collections.Generic;
using Firebase.Firestore;

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
    [FirestoreProperty] public Dictionary<string, Vector2Data> HomeItemPositions { get; set; } = new Dictionary<string, Vector2Data>();
    [FirestoreProperty] public Dictionary<string, int> HomeItemLayers { get; set; }
    [FirestoreProperty] public List<SleepLog> SleepLogs { get; set; } = new List<SleepLog>();
    [FirestoreProperty] public List<CustomGoal> CustomGoals { get; set; }
    [FirestoreProperty] public int SleepLogStreak { get; set; } = 0;
    [FirestoreProperty] public string LastSleepLogDate { get; set; } = "";
    [FirestoreProperty] public int Level { get; set; } = 1;
    [FirestoreProperty] public int XP { get; set; } = 0;
    [FirestoreProperty] public string LastLoginDate { get; set; } = "";
    [FirestoreProperty] public string LastGoalResetDate { get; set; } = "";
    [FirestoreProperty] public int DailyGoalXP { get; set; } = 0;
    [FirestoreProperty] public int DailyGoalMoney { get; set; } = 0;
    [FirestoreProperty] public string LastGoalRewardDate { get; set; } = "";
    [FirestoreProperty] public List<string> SeenTutorials { get; set; } = new List<string>();

}

[FirestoreData]
[Serializable]
public class SleepLog
{
    [FirestoreProperty] public string Date { get; set; }   // Format "2025-10-01"
    [FirestoreProperty] public float Hours { get; set; }  // Hours slept
}