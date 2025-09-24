using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class FirebaseController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel, signupPanel, profilePanel, resetPasswordPanel, notificationPanel, tabsPanel, goalsPanel, statsPanel, settingsPanel, homePanel, inventoryPanel, shopPanel;

    [Header("Inputs")]
    public TMP_InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupUserName, resetPassEmail;

    [Header("UI Texts")]
    public TMP_Text notif_Title_Text, notif_Message_Text, profileUserName_Text, profileUserEmail_Text, userMoney;

    [Header("Other UI")]
    public Toggle rememberMe;
    public Button loginButton, signupButton;

    private FirebaseAuth auth;
    private FirebaseUser user;
    public FirestoreService firestoreService;
    private bool isSignIn = false;
    public string currentUserId;
    private bool firebaseReady = false;

    private async void Start()
    {
        notificationPanel?.SetActive(false);

        firestoreService = new FirestoreService();

        loginButton.interactable = false;
        signupButton.interactable = false;

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }

        OpenLoginPanel();
    }

    void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        firebaseReady = true;
        loginButton.interactable = true;
        signupButton.interactable = true;
    }

    // -------------------- Panels --------------------
    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        resetPasswordPanel.SetActive(false);
        tabsPanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        settingsPanel.SetActive(false);
        homePanel.SetActive(false);
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
    }

    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        resetPasswordPanel.SetActive(false);
    }

    public void OpenResetPassPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        resetPasswordPanel.SetActive(true);
    }

    public void OpenHomePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        resetPasswordPanel.SetActive(false);
        tabsPanel.SetActive(true);
        homePanel.SetActive(true);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        shopPanel.SetActive(false);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    public void OpenProfilePanel()
    {
        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        shopPanel.SetActive(false);
        profilePanel.SetActive(true);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    public void OpenGoalsPanel()
    {
        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(true);
        statsPanel.SetActive(false);
        shopPanel.SetActive(false);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    public void OpenStatsPanel()
    {
        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(true);
        shopPanel.SetActive(false);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    public void OpenSettingsPanel()
    {
        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        shopPanel.SetActive(false);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(true);
        inventoryPanel.SetActive(false);
    }

    public void OpenShopPanel()
    {
        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        shopPanel.SetActive(true);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
    }

    // -------------------- Notifications --------------------
    private void showNotificationMessage(string title, string message)
    {
        notif_Title_Text.text = title;
        notif_Message_Text.text = message;
        notificationPanel.SetActive(true);
    }

    public void CloseNotif_Panel()
    {
        notif_Title_Text.text = "";
        notif_Message_Text.text = "";
        notificationPanel.SetActive(false);
    }

    // -------------------- Auth --------------------
    public async void LoginUser()
    {
        if (!firebaseReady || auth == null)
        {
            showNotificationMessage("Error", "Firebase not ready yet.");
            return;
        }

        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            showNotificationMessage("Error", "One or more Fields Empty");
            return;
        }

        try
        {
            await auth.SignInWithEmailAndPasswordAsync(loginEmail.text, loginPassword.text);

            if (auth.CurrentUser == null)
            {
                showNotificationMessage("Error", "Login failed. Try again.");
                return;
            }

            currentUserId = auth.CurrentUser.UserId;

            // Load or create player data
            PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
            if (player == null)
            {
                player = new PlayerData
                {
                    Name = auth.CurrentUser.DisplayName ?? loginEmail.text,
                    Email = auth.CurrentUser.Email ?? loginEmail.text,
                    Money = 1000
                };
                await firestoreService.SavePlayerAsync(currentUserId, player);
            }

            // Update UI
            userMoney.text = player.Money.ToString();
            profileUserName_Text.text = player.Name;
            profileUserEmail_Text.text = player.Email;

            OpenHomePanel();
        }
        catch (Exception ex)
        {
            Debug.LogError("Login error: " + ex);
            showNotificationMessage("Error", "Login failed");
        }
    }

    public async void SignUpUser()
    {
        if (!firebaseReady || auth == null)
        {
            showNotificationMessage("Error", "Firebase not ready yet.");
            return;
        }

        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text) ||
            string.IsNullOrEmpty(signupCPassword.text) || string.IsNullOrEmpty(signupUserName.text))
        {
            showNotificationMessage("Error", "One or more Fields Empty");
            return;
        }

        try
        {
            await auth.CreateUserWithEmailAndPasswordAsync(signupEmail.text, signupPassword.text);

            if (auth.CurrentUser == null)
            {
                showNotificationMessage("Error", "Signup failed. Try again.");
                return;
            }

            await UpdateUserProfileAsync(signupUserName.text);

            currentUserId = auth.CurrentUser.UserId;

            PlayerData newPlayer = new PlayerData
            {
                Name = auth.CurrentUser.DisplayName ?? signupUserName.text,
                Email = auth.CurrentUser.Email ?? signupEmail.text,
                Money = 1000
            };

            await firestoreService.SavePlayerAsync(currentUserId, newPlayer);

            // Update UI
            userMoney.text = newPlayer.Money.ToString();
            profileUserName_Text.text = newPlayer.Name;
            profileUserEmail_Text.text = newPlayer.Email;

            OpenHomePanel();
        }
        catch (Exception ex)
        {
            Debug.LogError("Signup error: " + ex);
            showNotificationMessage("Error", "Signup failed");
        }
    }

    public async void ResetPassword()
    {
        if (string.IsNullOrEmpty(resetPassEmail.text))
        {
            showNotificationMessage("Error", "Email Empty");
            return;
        }

        try
        {
            await auth.SendPasswordResetEmailAsync(resetPassEmail.text);
            showNotificationMessage("Alert", "Reset Password Email Sent");
        }
        catch (Exception ex)
        {
            Debug.LogError("Reset password error: " + ex);
            showNotificationMessage("Error", "Failed to send reset email");
        }
    }

    async Task UpdateUserProfileAsync(string username)
    {
        if (auth.CurrentUser == null) return;

        UserProfile profile = new UserProfile { DisplayName = username };
        try
        {
            await auth.CurrentUser.UpdateUserProfileAsync(profile);
        }
        catch (Exception ex)
        {
            Debug.LogError("Update profile error: " + ex);
        }
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            isSignIn = signedIn;
        }
    }

    void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= AuthStateChanged;
    }

    public void LogOut()
    {
        auth.SignOut();
        profileUserEmail_Text.text = "";
        profileUserName_Text.text = "";
        OpenLoginPanel();
    }

    // -------------------- Goals / Money --------------------
    public async void CompleteGoal(int moneyEarned)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("No user logged in.");
            return;
        }

        await firestoreService.EarnMoneyAsync(currentUserId, moneyEarned);

        PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
        if (player != null)
        {
            userMoney.text = player.Money.ToString();
        }
    }
}
