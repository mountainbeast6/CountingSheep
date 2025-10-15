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
    public TMP_InputField goalRewardInput;
    public Button createGoalButton;
    public GameObject addGoalPanel;

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
            IsPredefined = true
        });

        allGoals.Add(new Goal 
        { 
            Id = "workout_goal", 
            Name = "Workout for 30 mins", 
            Reward = 100, 
            IsCompleted = false,
            IsPredefined = true
        });
    }

    private void CheckDailyReset()
    {
        string today = System.DateTime.Today.ToString("yyyy-MM-dd");
        string lastResetDate = PlayerPrefs.GetString("LastGoalResetDate", "");

        // If it's a new day, reset completed goals
        if (lastResetDate != today)
        {
            ResetDailyGoals();
            PlayerPrefs.SetString("LastGoalResetDate", today);
            PlayerPrefs.Save();
        }
    }

    private void ResetDailyGoals()
    {
        // Reset predefined goals
        foreach (var goal in allGoals.Where(g => g.IsPredefined))
        {
            goal.IsCompleted = false;
        }

        // Remove completed goals from Firebase (optional)
        if (firebaseController?.currentPlayer != null)
        {
            firebaseController.currentPlayer.CompletedGoals?.Clear();
            
            // Also reset custom goals completion status if they exist
            if (firebaseController.currentPlayer.CustomGoals != null)
            {
                foreach (var customGoal in firebaseController.currentPlayer.CustomGoals)
                {
                    customGoal.IsCompleted = false;
                }
            }
            
            // Save to database if user is logged in
            if (!string.IsNullOrEmpty(firebaseController.currentUserId))
            {
                _ = firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);
            }
        }

        Debug.Log("Daily goals reset!");
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
                    IsPredefined = false
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
        
        // Set the goal name text
        if (goalNameText != null)
            goalNameText.text = goal.Name;

        // Set up the toggle
        if (completeToggle != null)
        {
            // Set the toggle state based on completion status
            completeToggle.isOn = goal.IsCompleted;
            
            // Set up toggle listener
            completeToggle.onValueChanged.RemoveAllListeners();
            completeToggle.onValueChanged.AddListener((isOn) => {
                if (isOn && !goal.IsCompleted)
                {
                    CompleteGoal(goal);
                }
            });
        }

        // You can also add the reward text somewhere if you want to display it
        // For example, you could add a "RewardText" component to your prefab
        TMP_Text rewardText = goalObj.transform.Find("RewardText")?.GetComponent<TMP_Text>();
        if (rewardText != null)
            rewardText.text = $"+${goal.Reward}";
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

        // Update completed goals list
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
        }
    }

    public void CloseAddGoalPanel()
    {
        PlayClickSound();
        if (addGoalPanel != null)
        {
            addGoalPanel.SetActive(false);
            // Clear inputs
            if (goalNameInput != null) goalNameInput.text = "";
            if (goalRewardInput != null) goalRewardInput.text = "";
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

        // Validate reward amount
        if (string.IsNullOrEmpty(goalRewardInput.text))
        {
            ShowNotification("Error", "Goal must have a reward value.");
            return;
        }

        if (!int.TryParse(goalRewardInput.text, out int reward) || reward <= 0)
        {
            ShowNotification("Error", "Please enter a valid reward amount (must be a positive number).");
            return;
        }

        // Create new custom goal
        Goal newGoal = new Goal
        {
            Id = System.Guid.NewGuid().ToString(),
            Name = goalNameInput.text.Trim(),
            Reward = reward,
            IsCompleted = false,
            IsPredefined = false
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
            IsCompleted = newGoal.IsCompleted
        });

        // Save to database
        await firebaseController.firestoreService.SavePlayerAsync(firebaseController.currentUserId, firebaseController.currentPlayer);

        // Close panel and refresh display
        CloseAddGoalPanel();
        DisplayGoals();

        ShowNotification("Success", $"Goal '{newGoal.Name}' created!");
        Debug.Log($"Goal '{newGoal.Name}' created!");
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
}