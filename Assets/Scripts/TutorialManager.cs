using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panel")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialTitleText;
    public TMP_Text tutorialMessageText;
    public Button tutorialCloseButton;

    private FirebaseController firebaseController;
    private HashSet<string> seenTutorials = new HashSet<string>();

    private void Start()
    {
        firebaseController = FindObjectOfType<FirebaseController>();
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            
            if (tutorialCloseButton != null)
                tutorialCloseButton.onClick.AddListener(CloseTutorial);
        }
    }

    public async Task LoadSeenTutorials(string userId)
    {
        if (firebaseController == null)
        {
            Debug.LogWarning("TutorialManager: FirebaseController not found!");
            return;
        }
        
        if (firebaseController.firestoreService == null || string.IsNullOrEmpty(userId))
        {
            Debug.LogWarning("TutorialManager: FirestoreService or userId is null!");
            return;
        }

        try
        {
            PlayerData player = await firebaseController.firestoreService.LoadPlayerAsync(userId);
            
            if (player?.SeenTutorials != null)
            {
                seenTutorials = new HashSet<string>(player.SeenTutorials);
                Debug.Log($"Loaded {seenTutorials.Count} seen tutorials for user.");
            }
            else
            {
                seenTutorials.Clear();
                Debug.Log("No previous tutorials found, starting fresh.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading seen tutorials: {ex.Message}");
        }
    }

    public async void ShowTutorial(string panelName, string title, string message)
    {
        // Check if tutorialManager is null or user has seen this tutorial before
        if (this == null || seenTutorials.Contains(panelName))
            return;

        // First time seeing this tutorial
        if (tutorialTitleText != null)
            tutorialTitleText.text = title;
        if (tutorialMessageText != null)
            tutorialMessageText.text = message;
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
        
        // Mark as seen
        seenTutorials.Add(panelName);
        await SaveSeenTutorial(panelName);
    }

    private async Task SaveSeenTutorial(string panelName)
    {
        if (firebaseController?.currentPlayer == null || 
            string.IsNullOrEmpty(firebaseController.currentUserId))
            return;

        // Initialize list if null
        if (firebaseController.currentPlayer.SeenTutorials == null)
            firebaseController.currentPlayer.SeenTutorials = new List<string>();

        // Add to player data if not already there
        if (!firebaseController.currentPlayer.SeenTutorials.Contains(panelName))
        {
            firebaseController.currentPlayer.SeenTutorials.Add(panelName);
            
            // Save to Firestore
            await firebaseController.firestoreService.SavePlayerAsync(
                firebaseController.currentUserId, 
                firebaseController.currentPlayer
            );
        }
    }

    public void CloseTutorial()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }
}