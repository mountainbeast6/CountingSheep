using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class GoalsManager : MonoBehaviour
{
    [Header("Goals UI")]
    public Transform goalsListContainer;
    public GameObject goalPrefab;
    public Button openAddGoalButton;
    public Button closeAddGoalButton;
    public TMP_InputField goalNameInput;
    public Button createGoalButton;
    public GameObject addGoalPanel;

    [Header("Reward Toggles")]
    public Toggle reward10Toggle;
    public Toggle reward50Toggle;
    public Toggle reward100Toggle;
    public ToggleGroup rewardToggleGroup;

    [Header("Daily Goal Toggle")]
    public Toggle dailyGoalToggle;

    [Header("Audio")]
    public AudioClip goalCompleteSound;
    public AudioClip clickSound;

    private FirebaseController firebaseController;
    private List<Goal> allGoals = new List<Goal>();

    void Start()
    {
        firebaseController = FindObjectOfType<FirebaseController>();

        // Set up button listeners
        if (openAddGoalButton != null)
            openAddGoalButton.onClick.AddListener(OpenAddGoalPanel);
        
        if (closeAddGoalButton != null)
            closeAddGoalButton.onClick.AddListener(CloseAddGoalPanel);
        
        if (createGoalButton != null)
            createGoalButton.onClick.AddListener(CreateCustomGoal);

        // Set up reward toggle group
        if (rewardToggleGroup != null)
        {
            reward10Toggle.group = rewardToggleGroup;
            reward50Toggle.group = rewardToggleGroup;
            reward100Toggle.group = rewardToggleGroup;
        }

        // Set default reward selection
        reward50Toggle.isOn = true;

        // Ensure AddGoalPanel starts closed
        if (addGoalPanel != null)
            addGoalPanel.SetActive(false);

        // Check for daily reset
        CheckDailyReset();

        // Initialize predefined goals
        InitializePredefinedGoals();
    }

    void InitializePredefinedGoals()
    {
        // Create predefined goals
        allGoals.Add(new Goal 
        { 
            Id = "sleep_goal", 
            Name = "Sleep 8 hours", 
            Reward = 50, 
            IsCompleted = false,
            IsPredefined = true,
            IsDaily = true  // Predefined goals are daily by default
        });

        allGoals.Add(new Goal
        {
            Id = "workout_goal",
            Name = "Workout for 30 mins",
            Reward = 100,
            IsCompleted = false,
            IsPredefined = true,
            IsDaily = true  // Predefined goals are daily by default
        });
        allGoals.Add(new Goal 
        { 
            Id = "sleep_streak_goal", 
            Name = "Log sleep for 5 consecutive days", 
            Reward = 200, 
            IsCompleted = false,
            IsPredefined = true,
            IsDaily = false  // This is a streak goal, not daily
        });
    }

    private void CheckDailyReset()
    {
        string today = System.DateTime.Today.ToString("yyyy-MM-dd");
        string lastResetDate = PlayerPrefs.GetString("LastGoalResetDate", "");

        // If it's a new day, reset daily goals
        if (lastResetDate != today)
        {
            ResetDailyGoals();
            PlayerPrefs.SetString("LastGoalResetDate", today);
            PlayerPrefs.Save();
        }
    }

    private void ResetDailyGoals()
    {
        // Reset only daily goals
        foreach (var goal in allGoals.Where(g => g.IsDaily && !g.IsCompleted))
        {
            goal.IsCompleted = false;
        }

        // Remove completed daily goals from Firebase
        if (firebaseController?.currentPlayer != null)
        {
            // Clear completed goals that are daily
            if (firebaseController.currentPlayer.CompletedGoals != null)
            {
                // We need to track which goals are daily to know which to reset
                // For now, we'll reset all predefined goals (which are daily)
                firebaseController.currentPlayer.CompletedGoals.Clear();
            }
            
            // Reset custom daily goals completion status
            if (firebaseController.currentPlayer.CustomGoals != null)
            {
                foreach (var customGoal in firebaseController.currentPlayer.CustomGoals)
                {
                    // Find if this custom goal is marked as daily
                    var goal = allGoals.FirstOrDefault(g => g.Id == customGoal.Id);
                    if (goal != null && goal.IsDaily)
                    {
                        customGoal.IsCompleted = false;
                    }
                }
            }
            
            // Save to database if user is logged in
            if (!string.IsNullOrEmpty(firebaseController.currentUserId))
            {
                _ = firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);
            }
        }

        Debug.Log("Daily goals reset!");
        DisplayGoals(); // Refresh the display
    }

    public async void DisplayGoals()
    {
        if (goalsListContainer == null || goalPrefab == null) return;

        // Clear existing goals
        foreach (Transform child in goalsListContainer)
            Destroy(child.gameObject);

        // Ensure AddGoalPanel is closed when displaying goals
        if (addGoalPanel != null && addGoalPanel.activeSelf)
            addGoalPanel.SetActive(false);

        // Load current player data to check completion status
        if (firebaseController != null && firebaseController.currentPlayer != null)
        {
            // Update completion status from player data
            UpdateGoalsCompletionStatus();
        }

        // Display all active goals
        foreach (var goal in allGoals.Where(g => !g.IsCompleted))
        {
            CreateGoalItem(goal);
        }
    }

    private void UpdateGoalsCompletionStatus()
    {
        if (firebaseController?.currentPlayer == null) return;
        
        var player = firebaseController.currentPlayer;
        if (player.CompletedGoals == null) return;

        // Update predefined goals completion status
        foreach (var goal in allGoals.Where(g => g.IsPredefined))
        {
            goal.IsCompleted = player.CompletedGoals.Contains(goal.Id);
        }

        // Update custom goals from player data
        if (player.CustomGoals != null)
        {
            // Remove existing custom goals and add updated ones
            allGoals.RemoveAll(g => !g.IsPredefined);
            
            foreach (var customGoal in player.CustomGoals)
            {
                allGoals.Add(new Goal
                {
                    Id = customGoal.Id,
                    Name = customGoal.Name,
                    Reward = customGoal.Reward,
                    IsCompleted = customGoal.IsCompleted,
                    IsPredefined = false,
                    IsDaily = customGoal.IsDaily
                });
            }
        }
        else
        {
            // If CustomGoals is null, remove any existing custom goals from the list
            allGoals.RemoveAll(g => !g.IsPredefined);
        }
    }

    private void CreateGoalItem(Goal goal)
    {
        GameObject goalObj = Instantiate(goalPrefab, goalsListContainer);
        
        // Find the text and toggle components
        TMP_Text goalNameText = goalObj.transform.Find("GoalName")?.GetComponent<TMP_Text>();
        Toggle completeToggle = goalObj.transform.Find("CompleteGoal")?.GetComponent<Toggle>();
        TMP_Text rewardText = goalObj.transform.Find("RewardText")?.GetComponent<TMP_Text>();
        TMP_Text dailyBadge = goalObj.transform.Find("DailyBadge")?.GetComponent<TMP_Text>();
        TMP_Text progressText = goalObj.transform.Find("ProgressText")?.GetComponent<TMP_Text>(); // Add this UI element
        
        // Special handling for sleep streak goal
        if (goal.Id == "sleep_streak_goal" && firebaseController?.currentPlayer != null)
        {
            int currentStreak = firebaseController.currentPlayer.SleepLogStreak;
            int requiredStreak = 5;
            
            // Update goal name to show progress
            if (goalNameText != null)
                goalNameText.text = $"{goal.Name} ({currentStreak}/{requiredStreak})";
            
            // Show progress text if available
            if (progressText != null)
            {
                progressText.gameObject.SetActive(true);
                progressText.text = $"{currentStreak}/{requiredStreak} days";
            }
            
            // Only enable toggle if streak is complete
            if (completeToggle != null)
            {
                completeToggle.interactable = currentStreak >= requiredStreak && !goal.IsCompleted;
                completeToggle.isOn = goal.IsCompleted;
            }
        }
        else
        {
            // Normal goal handling
            if (goalNameText != null)
                goalNameText.text = goal.Name;

            if (progressText != null)
                progressText.gameObject.SetActive(false);
                
            if (completeToggle != null)
            {
                completeToggle.interactable = !goal.IsCompleted;
                completeToggle.isOn = goal.IsCompleted;
            }
        }

        // Set reward text
        if (rewardText != null)
            rewardText.text = $"+${goal.Reward}";

        // Show daily badge if it's a daily goal
        if (dailyBadge != null)
        {
            dailyBadge.gameObject.SetActive(goal.IsDaily);
            if (goal.IsDaily)
                dailyBadge.text = "DAILY";
        }

        // Set up the toggle listener
        if (completeToggle != null)
        {
            completeToggle.onValueChanged.RemoveAllListeners();
            completeToggle.onValueChanged.AddListener((isOn) => {
                if (isOn && !goal.IsCompleted)
                {
                    CompleteGoal(goal);
                }
            });
        }
    }

    public async void CompleteGoal(Goal goal)
    {
        if (firebaseController?.currentPlayer == null || string.IsNullOrEmpty(firebaseController.currentUserId)) 
            return;

        PlayGoalCompleteSound();

        // Mark goal as completed
        goal.IsCompleted = true;

        // Add money to player
        firebaseController.currentPlayer.Money += goal.Reward;

        // Update completed goals list (only for non-daily goals, or we track completion differently)
        // For daily goals, we don't add to CompletedGoals since they reset daily
        if (!goal.IsDaily)
        {
            if (firebaseController.currentPlayer.CompletedGoals == null)
                firebaseController.currentPlayer.CompletedGoals = new List<string>();

            if (!firebaseController.currentPlayer.CompletedGoals.Contains(goal.Id))
                firebaseController.currentPlayer.CompletedGoals.Add(goal.Id);
        }

        // For custom goals, also update the CustomGoals list
        if (!goal.IsPredefined && firebaseController.currentPlayer.CustomGoals != null)
        {
            var customGoal = firebaseController.currentPlayer.CustomGoals.FirstOrDefault(g => g.Id == goal.Id);
            if (customGoal != null)
            {
                customGoal.IsCompleted = true;
            }
        }

        // Save to database
        await firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);

        // Update UI
        if (firebaseController.userMoney != null)
            firebaseController.userMoney.text = firebaseController.currentPlayer.Money.ToString();

        // Refresh goals display (this will remove completed goals)
        DisplayGoals();
    }

    public void OpenAddGoalPanel()
    {
        PlayClickSound();
        if (addGoalPanel != null)
        {
            addGoalPanel.SetActive(true);
            // Reset form
            if (goalNameInput != null) goalNameInput.text = "";
            if (dailyGoalToggle != null) dailyGoalToggle.isOn = false;
            if (reward50Toggle != null) reward50Toggle.isOn = true; // Default to 50
        }
    }

    public void CloseAddGoalPanel()
    {
        PlayClickSound();
        if (addGoalPanel != null)
        {
            addGoalPanel.SetActive(false);
        }
    }

    public async void CreateCustomGoal()
    {
        if (firebaseController?.currentPlayer == null || string.IsNullOrEmpty(firebaseController.currentUserId))
        {
            ShowNotification("Error", "No user logged in.");
            return;
        }

        // Validate goal name
        if (string.IsNullOrEmpty(goalNameInput.text) || string.IsNullOrWhiteSpace(goalNameInput.text))
        {
            ShowNotification("Error", "Goal must have a name.");
            return;
        }

        // Get selected reward amount
        int reward = GetSelectedReward();
        if (reward == 0)
        {
            ShowNotification("Error", "Please select a reward amount.");
            return;
        }

        // Get daily goal setting
        bool isDaily = dailyGoalToggle != null && dailyGoalToggle.isOn;

        // Create new custom goal
        Goal newGoal = new Goal
        {
            Id = System.Guid.NewGuid().ToString(),
            Name = goalNameInput.text.Trim(),
            Reward = reward,
            IsCompleted = false,
            IsPredefined = false,
            IsDaily = isDaily
        };

        // Add to the list
        allGoals.Add(newGoal);

        // Initialize CustomGoals list if it's null
        if (firebaseController.currentPlayer.CustomGoals == null)
        {
            firebaseController.currentPlayer.CustomGoals = new List<CustomGoal>();
        }

        // Add the new custom goal
        firebaseController.currentPlayer.CustomGoals.Add(new CustomGoal
        {
            Id = newGoal.Id,
            Name = newGoal.Name,
            Reward = newGoal.Reward,
            IsCompleted = newGoal.IsCompleted,
            IsDaily = newGoal.IsDaily
        });

        // Save to database
        await firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);

        // Close panel and refresh display
        CloseAddGoalPanel();
        DisplayGoals();

        string dailyText = isDaily ? " (Daily)" : "";
        ShowNotification("Success", $"Goal '{newGoal.Name}' created!{dailyText}");
        Debug.Log($"Goal '{newGoal.Name}' created! Reward: ${reward}, Daily: {isDaily}");
    }

    private int GetSelectedReward()
    {
        if (reward10Toggle != null && reward10Toggle.isOn) return 10;
        if (reward50Toggle != null && reward50Toggle.isOn) return 50;
        if (reward100Toggle != null && reward100Toggle.isOn) return 100;
        return 0; // No reward selected
    }

    private void ShowNotification(string title, string message)
    {
        // Use FirebaseController's notification system
        if (firebaseController != null)
        {
            firebaseController.showNotificationMessage(title, message);
        }
        // Fallback to debug log
        else
        {
            Debug.Log($"{title}: {message}");
        }
    }

    private void PlayGoalCompleteSound()
    {
        if (firebaseController != null && goalCompleteSound != null)
        {
            firebaseController.sfxSource.PlayOneShot(goalCompleteSound);
        }
    }

    private void PlayClickSound()
    {
        if (firebaseController != null && clickSound != null)
        {
            firebaseController.sfxSource.PlayOneShot(clickSound);
        }
    }
}

[System.Serializable]
public class Goal
{
    public string Id;
    public string Name;
    public int Reward;
    public bool IsCompleted;
    public bool IsPredefined;
    public bool IsDaily;
}