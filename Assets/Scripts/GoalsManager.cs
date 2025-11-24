using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System;

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

    [Header("Goal Type Toggles")]
    public Toggle dailyGoalToggle;
    public Toggle weeklyGoalToggle;
    public Toggle oneTimeGoalToggle;

    [Header("Completed Goals Panel")]
    public GameObject completedGoalsPanel;
    public Button openCompletedGoalsButton;
    public Button closeCompletedGoalsButton;
    public Button showDailyCompletedButton;
    public Button showWeeklyCompletedButton;
    public GameObject dailyCompletedPanel;
    public GameObject weeklyCompletedPanel;
    public Transform dailyContent;
    public Transform weeklyContent;
    

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

        // Set default selections
        reward10Toggle.isOn = true;
        oneTimeGoalToggle.isOn = true;

        // Ensure AddGoalPanel starts closed
        if (addGoalPanel != null)
            addGoalPanel.SetActive(false);

        // Set up goal type toggles
        if (dailyGoalToggle != null && weeklyGoalToggle != null && oneTimeGoalToggle != null)
        {
            // Make them mutually exclusive
            dailyGoalToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) { weeklyGoalToggle.isOn = false; oneTimeGoalToggle.isOn = false; }
            });
            weeklyGoalToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) { dailyGoalToggle.isOn = false; oneTimeGoalToggle.isOn = false; }
            });
            oneTimeGoalToggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn) { dailyGoalToggle.isOn = false; weeklyGoalToggle.isOn = false; }
            });
        }

        // ------------------ Completed Goals ------------------------------------
        if (openCompletedGoalsButton != null)
            openCompletedGoalsButton.onClick.AddListener(OpenCompletedGoalsPanel);
        
        if (closeCompletedGoalsButton != null)
            closeCompletedGoalsButton.onClick.AddListener(CloseCompletedGoalsPanel);

        if (showDailyCompletedButton != null)
            showDailyCompletedButton.onClick.AddListener(ShowDailyCompletedGoals);

        if (showWeeklyCompletedButton != null)
            showWeeklyCompletedButton.onClick.AddListener(ShowWeeklyCompletedGoals);

        // Ensure panels start in correct state
        if (completedGoalsPanel != null)
            completedGoalsPanel.SetActive(false);
            
        if (dailyCompletedPanel != null)
            dailyCompletedPanel.SetActive(true); // Daily starts active
            
        if (weeklyCompletedPanel != null)
            weeklyCompletedPanel.SetActive(false); // Weekly starts hidden

        // Initialize predefined goals
        InitializePredefinedGoals();

        // DELAY the sound setup to avoid playing on app start
        StartCoroutine(DelayedSoundSetup());
    }

    private System.Collections.IEnumerator DelayedSoundSetup()
    {
        // Wait for one frame to ensure all toggles are set up
        yield return null;
        
        // Now safely add sound listeners
        AddToggleSoundListeners();
    }

    private void AddToggleSoundListeners()
    {
        // Add sound to REWARD toggles
        if (reward10Toggle != null)
            reward10Toggle.onValueChanged.AddListener(PlayToggleSound);
        if (reward50Toggle != null)
            reward50Toggle.onValueChanged.AddListener(PlayToggleSound);
        if (reward100Toggle != null)
            reward100Toggle.onValueChanged.AddListener(PlayToggleSound);

        // Add sound to GOAL TYPE toggles
        if (dailyGoalToggle != null)
            dailyGoalToggle.onValueChanged.AddListener(PlayToggleSound);
        if (weeklyGoalToggle != null)
            weeklyGoalToggle.onValueChanged.AddListener(PlayToggleSound);
        if (oneTimeGoalToggle != null)
            oneTimeGoalToggle.onValueChanged.AddListener(PlayToggleSound);
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

    public async void CheckAndResetDailyGoals()
    {
        if (firebaseController?.currentPlayer == null || string.IsNullOrEmpty(firebaseController.currentUserId)) 
        {
            Debug.LogWarning("Cannot check daily reset - no player data");
            return;
        }

        string today = System.DateTime.Today.ToString("yyyy-MM-dd");
        string lastResetDate = firebaseController.currentPlayer.LastGoalResetDate;

        Debug.Log($"Checking daily reset. Today: {today}, Last reset: {lastResetDate}");

        // If it's a new day, reset daily goals
        if (string.IsNullOrEmpty(lastResetDate) || lastResetDate != today)
        {
            Debug.Log("Resetting daily goals!");
            ResetDailyGoals();
            firebaseController.currentPlayer.DailyGoalXP = 0;
            firebaseController.currentPlayer.DailyGoalMoney = 0;
            firebaseController.currentPlayer.LastGoalRewardDate = today;

            
            // Check if it's a new week (Monday)
            if (System.DateTime.Today.DayOfWeek == DayOfWeek.Monday)
            {
                Debug.Log("It's Monday - resetting weekly goals too!");
                ResetWeeklyGoals();
            }
            
            // Update the reset date in Firebase
            firebaseController.currentPlayer.LastGoalResetDate = today;
            await firebaseController.firestoreService.SavePlayerAsync(
                firebaseController.currentUserId, 
                firebaseController.currentPlayer
            );

            firebaseController.UpdateDailyLimitUI();
            Debug.Log($"Saved new reset date: {today}");
        }
        else
        {
            Debug.Log("Goals already reset today - no action needed");
        }
    }

    private void ResetWeeklyGoals()
    {
        // Reset only weekly goals in allGoals list
        foreach (var goal in allGoals.Where(g => g.IsWeekly))
        {
            goal.IsCompleted = false;
        }

        // Remove completed weekly goals from Firebase
        if (firebaseController?.currentPlayer != null)
        {
            // Remove weekly goals from CompletedGoals list
            if (firebaseController.currentPlayer.CompletedGoals != null)
            {
                // Get IDs of weekly goals to remove
                var weeklyGoalIds = allGoals.Where(g => g.IsWeekly).Select(g => g.Id).ToList();
                firebaseController.currentPlayer.CompletedGoals.RemoveAll(id => weeklyGoalIds.Contains(id));
            }
            
            // Reset custom weekly goals completion status
            if (firebaseController.currentPlayer.CustomGoals != null)
            {
                foreach (var customGoal in firebaseController.currentPlayer.CustomGoals)
                {
                    if (customGoal.IsWeekly)
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

        Debug.Log("Weekly goals reset!");
    }

    private void ResetDailyGoals()
    {
        // Reset only daily goals in allGoals list
        foreach (var goal in allGoals.Where(g => g.IsDaily))
        {
            goal.IsCompleted = false;
        }

        // Remove completed daily goals from Firebase
        if (firebaseController?.currentPlayer != null)
        {
            // Remove daily goals from CompletedGoals list
            if (firebaseController.currentPlayer.CompletedGoals != null)
            {
                // Get IDs of daily goals to remove
                var dailyGoalIds = allGoals.Where(g => g.IsDaily).Select(g => g.Id).ToList();
                firebaseController.currentPlayer.CompletedGoals.RemoveAll(id => dailyGoalIds.Contains(id));
            }
            
            // Reset custom daily goals completion status
            if (firebaseController.currentPlayer.CustomGoals != null)
            {
                foreach (var customGoal in firebaseController.currentPlayer.CustomGoals)
                {
                    if (customGoal.IsDaily)
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

        // Handle sleep streak goal separately BEFORE checking completed goals
        var sleepStreakGoal = allGoals.FirstOrDefault(g => g.Id == "sleep_streak_goal");
        if (sleepStreakGoal != null && firebaseController?.currentPlayer != null)
        {
            // Check if already completed (one-time goal)
            bool alreadyCompleted = player.CompletedGoals.Contains("sleep_streak_goal");
            
            // Only mark as completable if streak is 5+ AND not already completed
            sleepStreakGoal.IsCompleted = alreadyCompleted || firebaseController.currentPlayer.SleepLogStreak >= 5;
        }

        // Update predefined goals completion status
        foreach (var goal in allGoals.Where(g => g.IsPredefined && g.Id != "sleep_streak_goal"))
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
                    IsDaily = customGoal.IsDaily,
                    IsWeekly = customGoal.IsWeekly
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
        
        TMP_Text goalNameText = goalObj.transform.Find("GoalName")?.GetComponent<TMP_Text>();
        Toggle completeToggle = goalObj.transform.Find("CompleteGoal")?.GetComponent<Toggle>();
        TMP_Text rewardText = goalObj.transform.Find("RewardText")?.GetComponent<TMP_Text>();
        TMP_Text dailyBadge = goalObj.transform.Find("DailyBadge")?.GetComponent<TMP_Text>();
        
        if (goalNameText != null)
            goalNameText.text = goal.Name;

        if (rewardText != null)
            rewardText.text = $"+${goal.Reward}";

        if (dailyBadge != null)
        {
            if (goal.IsDaily)
            {
                dailyBadge.gameObject.SetActive(true);
                dailyBadge.text = "DAILY";
                dailyBadge.color = Color.yellow;
            }
            else if (goal.IsWeekly)
            {
                dailyBadge.gameObject.SetActive(true);
                dailyBadge.text = "WEEKLY";
                dailyBadge.color = Color.blue;
            }
            else
            {
                dailyBadge.gameObject.SetActive(false);
            }
        }

        if (completeToggle != null)
        {
            completeToggle.isOn = goal.IsCompleted;
            
            // Special check for sleep streak goal
            if (goal.Id == "sleep_streak_goal")
            {
                int currentStreak = firebaseController?.currentPlayer?.SleepLogStreak ?? 0;
                completeToggle.interactable = currentStreak >= 5 && !goal.IsCompleted;
            }
            else
            {
                completeToggle.interactable = !goal.IsCompleted;
            }
            
            completeToggle.onValueChanged.RemoveAllListeners();
            completeToggle.onValueChanged.AddListener((isOn) => {
                if (isOn) PlayClickSound();
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

        if (goal.IsCompleted) return;

        PlayGoalCompleteSound();

        // Mark goal as completed
        goal.IsCompleted = true;

        // ----- Daily reward cap -----
        string today = System.DateTime.Today.ToString("yyyy-MM-dd");

        // Reset if it's a new day
        if (firebaseController.currentPlayer.LastGoalRewardDate != today)
        {
            firebaseController.currentPlayer.DailyGoalXP = 0;
            firebaseController.currentPlayer.DailyGoalMoney = 0;
            firebaseController.currentPlayer.LastGoalRewardDate = today;
        }

        // Set your caps (you can tune these numbers)
        int dailyMoneyCap = 200;
        int dailyXPCap = 150;

        // Check if reward would exceed cap
        if (firebaseController.currentPlayer.DailyGoalMoney + goal.Reward > dailyMoneyCap)
        {
            // Notify user (optional)
            firebaseController.showNotificationMessage(
                "Limit Reached",
                "You reached today's goal reward limit."
            );
            return;
        }

        // Give money only if under cap
        firebaseController.currentPlayer.Money += goal.Reward;
        firebaseController.currentPlayer.DailyGoalMoney += goal.Reward;

        // XP reward (optional)
        int xpReward = goal.Reward / 2;  // Example: reward 50 gives 25 XP

        if (firebaseController.currentPlayer.DailyGoalXP + xpReward <= dailyXPCap)
        {
            firebaseController.AddXP(xpReward);
            firebaseController.currentPlayer.DailyGoalXP += xpReward;
        }

        firebaseController.UpdateDailyLimitUI();

        // Add to CompletedGoals
        if (firebaseController.currentPlayer.CompletedGoals == null)
            firebaseController.currentPlayer.CompletedGoals = new List<string>();

        if (!firebaseController.currentPlayer.CompletedGoals.Contains(goal.Id))
            firebaseController.currentPlayer.CompletedGoals.Add(goal.Id);

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
        try
        {
            await firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);
            Debug.Log($"Goal '{goal.Name}' completed and saved to database");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save goal completion: {ex.Message}");
            // Revert changes if save failed
            goal.IsCompleted = false;
            firebaseController.currentPlayer.Money -= goal.Reward;
            return;
        }

        // Update UI
        if (firebaseController.userMoney != null)
            firebaseController.userMoney.text = firebaseController.currentPlayer.Money.ToString();

        // Refresh goals display
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
            if (oneTimeGoalToggle != null) oneTimeGoalToggle.isOn = true;
            if (reward10Toggle != null) reward10Toggle.isOn = true;
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

        // Get goal type
        bool isDaily = dailyGoalToggle != null && dailyGoalToggle.isOn;
        bool isWeekly = weeklyGoalToggle != null && weeklyGoalToggle.isOn;
        // If neither is selected, it's a one-time goal

        // Create new custom goal
        Goal newGoal = new Goal
        {
            Id = System.Guid.NewGuid().ToString(),
            Name = goalNameInput.text.Trim(),
            Reward = reward,
            IsCompleted = false,
            IsPredefined = false,
            IsDaily = isDaily,
            IsWeekly = isWeekly
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
            IsDaily = newGoal.IsDaily,
            IsWeekly = newGoal.IsWeekly
        });

        // Save to database
        await firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);

        // Close panel and refresh display
        CloseAddGoalPanel();
        DisplayGoals();

        string typeText = isDaily ? " (Daily)" : isWeekly ? " (Weekly)" : "";
        ShowNotification("Success", $"Goal '{newGoal.Name}' created!{typeText}");
        Debug.Log($"Goal '{newGoal.Name}' created! Reward: ${reward}, Daily: {isDaily}, Weekly: {isWeekly}");
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

    public void PlayToggleSound(bool isOn)
    {
        // Play sound for EVERY toggle change
        if (firebaseController != null && clickSound != null)
        {
            firebaseController.sfxSource.PlayOneShot(clickSound);
        }
    }

    // ----------------------------- Completed Goals -----------------------------------------------------
    public void OpenCompletedGoalsPanel()
    {
        PlayClickSound();
        if (completedGoalsPanel != null)
        {
            completedGoalsPanel.SetActive(true);
            ShowDailyCompletedGoals(); // Daily panel starts opened
        }
    }

    public void CloseCompletedGoalsPanel()
    {
        PlayClickSound();
        if (completedGoalsPanel != null)
        {
            completedGoalsPanel.SetActive(false);
        }
    }

    public void ShowDailyCompletedGoals()
    {
        PlayClickSound();
        if (dailyCompletedPanel != null) dailyCompletedPanel.SetActive(true);
        if (weeklyCompletedPanel != null) weeklyCompletedPanel.SetActive(false);
        
        DisplayCompletedGoals(true); // true for daily
    }

    public void ShowWeeklyCompletedGoals()
    {
        PlayClickSound();
        if (dailyCompletedPanel != null) dailyCompletedPanel.SetActive(false);
        if (weeklyCompletedPanel != null) weeklyCompletedPanel.SetActive(true);
        
        DisplayCompletedGoals(false); // false for weekly
    }

    private void DisplayCompletedGoals(bool showDaily)
    {
        Transform currentContainer = showDaily ? dailyContent : weeklyContent;

        if (currentContainer == null || goalPrefab == null) return;

        // Clear existing goals (safe now since it's a dedicated container)
        foreach (Transform child in currentContainer)
            Destroy(child.gameObject);

        // Load current completion status
        UpdateGoalsCompletionStatus();

        // Display completed goals based on type
        foreach (var goal in allGoals.Where(g => g.IsCompleted &&
                                            ((showDaily && g.IsDaily) ||
                                                (!showDaily && g.IsWeekly))))
        {
            CreateCompletedGoalItem(goal, currentContainer);
        }
    }
    
    public void CloseCompletedGoalsPanelIfOpen()
    {
        if (completedGoalsPanel != null && completedGoalsPanel.activeSelf)
        {
            completedGoalsPanel.SetActive(false);
        }
    }

    private void CreateCompletedGoalItem(Goal goal, Transform container)
    {
        GameObject goalObj = Instantiate(goalPrefab, container);
        
        TMP_Text goalNameText = goalObj.transform.Find("GoalName")?.GetComponent<TMP_Text>();
        Toggle completeToggle = goalObj.transform.Find("CompleteGoal")?.GetComponent<Toggle>();
        TMP_Text rewardText = goalObj.transform.Find("RewardText")?.GetComponent<TMP_Text>();
        TMP_Text dailyBadge = goalObj.transform.Find("DailyBadge")?.GetComponent<TMP_Text>();
        
        if (goalNameText != null)
            goalNameText.text = goal.Name;

        if (rewardText != null)
            rewardText.text = $"+${goal.Reward}";

        if (dailyBadge != null)
        {
            if (goal.IsDaily)
            {
                dailyBadge.gameObject.SetActive(true);
                dailyBadge.text = "DAILY";
                dailyBadge.color = Color.yellow;
            }
            else if (goal.IsWeekly)
            {
                dailyBadge.gameObject.SetActive(true);
                dailyBadge.text = "WEEKLY";
                dailyBadge.color = Color.blue;
            }
            else
            {
                dailyBadge.gameObject.SetActive(false);
            }
        }

        if (completeToggle != null)
        {
            completeToggle.isOn = true;
            completeToggle.interactable = false; // Can't un-complete goals
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
    public bool IsWeekly;
}