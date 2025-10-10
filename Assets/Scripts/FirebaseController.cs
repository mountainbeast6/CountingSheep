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
using System.Linq;

public class FirebaseController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel, signupPanel, profilePanel, resetPasswordPanel, notificationPanel, tabsPanel, goalsPanel, statsPanel, settingsPanel, homePanel, shopPanel;

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

    [Header("Inventory UI")]
    public GameObject inventoryButtonPrefab; // Prefab for each item slot
    public Transform inventoryContent;     // Parent object for inventory buttons
    public GameObject inventoryPanel;      // Inventory panel

    [Header("Stats - Sleep Log UI")]
    public TMP_InputField sleepHoursInput;
    public Button logSleepButton;
    public Transform sleepLogContent; // parent of the scrollview content
    public GameObject sleepLogRowPrefab; // prefab row with Date, Hours, Edit button
    public GameObject editSleepPanel;        // The popup panel
    public TMP_InputField editHoursInput;    // Input field for hours
    public Button editOkButton;              // OK button
    public Button editCancelButton;          // Cancel button

    private SleepLog logBeingEdited;         // Internal reference
    public TMP_InputField sleepDateInput;


    [Header("Swap Prompt UI")]
    public GameObject swapPromptPanel;       // The panel that pops up
    public TMPro.TMP_Text swapPromptText;    // Text inside the panel
    public Button swapYesButton;             // Yes button
    public Button swapNoButton;              // No button

    [Header("Shop UI")]
    public GameObject shopButtonPrefab;    // Prefab for each shop item
    public Transform shopContent;          // Parent object for shop buttons

    [Header("Home Display UI")]
    public GameObject homeItemButtonPrefab;  // Prefab for home items display
    public Transform homeContent;           // Parent object for home item buttons
    public RectTransform furnitureDisplayArea; // The area where furniture sprites will appear
    public GameObject[] furniturePrefabs;      // Array of your furniture sprite prefabs
    public GameObject bedPrefab;
    public GameObject chairPrefab;
    public GameObject deskPrefab;
    public GameObject lampPrefab;
    public Sprite[] bedSprites;
    public Sprite[] chairSprites;
    public Sprite[] deskSprites;
    public Sprite[] lampSprites;

    [Header("Audio")]
    public AudioSource musicSource;      // For background music
    public AudioSource sfxSource;        // For sound effects
    public AudioClip[] backgroundMusicPlaylist;  // Array of background music clips
    public AudioClip purchaseSound;      // Purchase sound effect
    public AudioClip goalCompleteSound;  // Goal completion sound effect
    public AudioClip pickUpItemSound;    // Pick up item sound effect
    public AudioClip placeItemSound;     // Place item sound effect
    public AudioClip clickSound;
    public AudioClip inventorySound;

    [Header("Audio Settings UI")]
    public Slider volumeSlider;
    public Toggle muteMusicToggle;
    public Toggle muteSoundToggle;
    public Button skipSongButton;
    public Button previousSongButton;
    public Button pausePlayButton;

    private int currentSongIndex = 0;


    // Dictionary to track instantiated furniture GameObjects
    private Dictionary<string, GameObject> spawnedFurniture = new Dictionary<string, GameObject>();

    private ShopDatabase shopDatabase = new ShopDatabase();

    private PlayerData currentPlayer;

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
        if (swapPromptPanel != null)
            swapPromptPanel.SetActive(false);

        if (logSleepButton != null)
            logSleepButton.onClick.AddListener(LogSleep);

        if (editSleepPanel != null)
            editSleepPanel.SetActive(false);

        InitializeFurnitureSprites();

        LoadAudioSettings();

        PlayBackgroundMusic();

        if (skipSongButton != null)
            skipSongButton.onClick.AddListener(SkipToNextSong);
        if (previousSongButton != null)
            previousSongButton.onClick.AddListener(SkipToPreviousSong);
        if (pausePlayButton != null)
        pausePlayButton.onClick.AddListener(TogglePausePlay);

        OpenLoginPanel();

        CheckRememberedLogin();
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

    void InitializeFurnitureSprites()
    {
        if (shopDatabase == null) return;

        // Map sprites to item IDs
        for (int i = 0; i < bedSprites.Length && i < 4; i++)
        {
            if (bedSprites[i] != null)
                shopDatabase.SetSprite($"bed{i + 1}", bedSprites[i]);
        }

        for (int i = 0; i < chairSprites.Length && i < 4; i++)
        {
            if (chairSprites[i] != null)
                shopDatabase.SetSprite($"chair{i + 1}", chairSprites[i]);
        }

        for (int i = 0; i < deskSprites.Length && i < 4; i++)
        {
            if (deskSprites[i] != null)
                shopDatabase.SetSprite($"desk{i + 1}", deskSprites[i]);
        }

        for (int i = 0; i < lampSprites.Length && i < 4; i++)
        {
            if (lampSprites[i] != null)
                shopDatabase.SetSprite($"lamp{i + 1}", lampSprites[i]);
        }
    }

    public void SetPlayerData(PlayerData player)
    {
        currentPlayer = player;
    }

    private void CheckRememberedLogin()
    {
        if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail", "");
            string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

            if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
            {
                loginEmail.text = savedEmail;
                loginPassword.text = savedPassword;
                rememberMe.isOn = true;

                // Auto-login
                LoginUser();
            }
        }
    }

    // -------------------- Panels --------------------
    private bool hasInitialized = false;
    private string currentPanel = "";

    public void OpenLoginPanel()
    {
        hasInitialized = false;

        currentPanel = "Login";

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
        currentPanel = "SignUp";

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

    public async void OpenHomePanel()
    {

        if (currentPanel == "Home") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Home";

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

        if (!string.IsNullOrEmpty(currentUserId))
        {
            currentPlayer = await firestoreService.LoadPlayerAsync(currentUserId);
        }
        DisplayHomeItems();

        hasInitialized = true;
    }

    public void OpenProfilePanel()
    {
        if (currentPanel == "Profile") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Profile";

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
        if (currentPanel == "Goals") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Goals";

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
        if (currentPanel == "Stats") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Stats";

        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(true);
        shopPanel.SetActive(false);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        if (currentPlayer != null)
        {
            DisplaySleepLogs();
        }
    }

    public void OpenSettingsPanel()
    {
        if (currentPanel == "Settings") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Settings";

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
        if (currentPanel == "Shop") return;
        if (hasInitialized) PlayClickSound();
        currentPanel = "Shop";

        tabsPanel.SetActive(true);
        homePanel.SetActive(false);
        goalsPanel.SetActive(false);
        statsPanel.SetActive(false);
        shopPanel.SetActive(true);
        profilePanel.SetActive(false);
        settingsPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        PopulateShop();
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
        PlayClickSound();
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
            SetPlayerData(player);
            // Update UI
            userMoney.text = player.Money.ToString();
            profileUserName_Text.text = player.Name;
            profileUserEmail_Text.text = player.Email;
            OpenHomePanel();
            DisplaySleepLogs();
            
            // Save credentials if Remember Me is checked
            if (rememberMe.isOn)
            {
                PlayerPrefs.SetInt("RememberMe", 1);
                PlayerPrefs.SetString("SavedEmail", loginEmail.text);
                PlayerPrefs.SetString("SavedPassword", loginPassword.text);
                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.SetInt("RememberMe", 0);
                PlayerPrefs.DeleteKey("SavedEmail");
                PlayerPrefs.DeleteKey("SavedPassword");
                PlayerPrefs.Save();
            }
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

        if (signupPassword.text != signupCPassword.text)
        {
            showNotificationMessage("Error", "Passwords do not match");
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
                Money = 1000,
                Inventory = new List<string>(),
                HomeItems = new Dictionary<string, string>()
            };

            await firestoreService.SavePlayerAsync(currentUserId, newPlayer);

            // Update UI
            userMoney.text = newPlayer.Money.ToString();
            profileUserName_Text.text = newPlayer.Name;
            profileUserEmail_Text.text = newPlayer.Email;

            OpenHomePanel();
        }
        catch (FirebaseException ex)
        {
            Debug.LogError($"Signup error: {ex.Message}, ErrorCode: {ex.ErrorCode}");
            
            // Check the ErrorCode property
            AuthError errorCode = (AuthError)ex.ErrorCode;
            
            switch (errorCode)
            {
                case AuthError.EmailAlreadyInUse:
                    showNotificationMessage("Error", "This email is already registered. Please use a different email or login.");
                    break;
                case AuthError.WeakPassword:
                    showNotificationMessage("Error", "Password is too weak. Please use a stronger password.");
                    break;
                case AuthError.InvalidEmail:
                    showNotificationMessage("Error", "Invalid email address format.");
                    break;
                case AuthError.MissingEmail:
                    showNotificationMessage("Error", "Please enter an email address.");
                    break;
                case AuthError.MissingPassword:
                    showNotificationMessage("Error", "Please enter a password.");
                    break;
                default:
                    showNotificationMessage("Error", "Signup failed. Please try again.");
                    break;
            }
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
        
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.DeleteKey("SavedPassword");
        PlayerPrefs.Save();
        
        OpenLoginPanel();
    }

    // -------------------- Goals / Money --------------------
    // Goal 1
    public void CompleteGoal1()
    {
        CompleteGoal("goal1", 50);
    }

    // Goal 2
    public void CompleteGoal2()
    {
        CompleteGoal("goal2", 100);
    }

    // Goal 3
    public void CompleteGoal3()
    {
        CompleteGoal("goal3", 200);
    }

    public async void CompleteGoal(string goalId, int moneyEarned)
    {
        // Hide the button immediately
        GameObject goalObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (goalObject != null)
            goalObject.SetActive(false);

        
        PlayGoalCompleteSound();

        if (!string.IsNullOrEmpty(currentUserId))
        {
            PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);

            // Add money
            player.Money += moneyEarned;

            // Mark goal as completed
            if (player.CompletedGoals == null)
                player.CompletedGoals = new List<string>();

            if (!player.CompletedGoals.Contains(goalId))
                player.CompletedGoals.Add(goalId);

            await firestoreService.SavePlayerAsync(currentUserId, player);

            // Update UI
            userMoney.text = player.Money.ToString();
        }
    }

    // -----------------SHOP-------------------------------------
    public async void BuyShopItem(string itemId)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("No user logged in.");
            return;
        }

        ShopItem item = shopDatabase.GetItem(itemId);
        if (item == null)
        {
            Debug.LogError($"Shop item not found: {itemId}");
            return;
        }

        // Spend money
        bool success = await firestoreService.SpendMoneyAsync(currentUserId, item.Cost);
        if (success)
        {
            // Play purchase sound
            PlayPurchaseSound();

            await firestoreService.AddItemToInventoryAsync(currentUserId, item.Id);

            // Refresh local player data immediately
            currentPlayer = await firestoreService.LoadPlayerAsync(currentUserId);

            // Update UI
            if (currentPlayer != null)
            {
                userMoney.text = currentPlayer.Money.ToString();
            }

            showNotificationMessage("Success", $"{item.Name} purchased!");

            // Refresh shop to remove purchased item
            if (shopPanel.activeSelf)
                PopulateShop();
        }
        else
        {
            showNotificationMessage("Error", "Not enough money!");
        }
    }

    public void PopulateShop()
    {
        if (shopDatabase == null) return;

        // Clear existing buttons
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        // Get all shop items from your database
        foreach (var kvp in shopDatabase.Items)
        {
            ShopItem item = kvp.Value;

            // Check if player already owns this item (in inventory or home)
            bool alreadyOwned = false;
            
            if (currentPlayer != null)
            {
                // Check inventory
                if (currentPlayer.Inventory != null && currentPlayer.Inventory.Contains(item.Id))
                {
                    alreadyOwned = true;
                }
                
                // Check home items
                if (currentPlayer.HomeItems != null && currentPlayer.HomeItems.ContainsValue(item.Id))
                {
                    alreadyOwned = true;
                }
            }

            // Skip this item if already owned
            if (alreadyOwned)
                continue;

            // Create shop button for items not yet owned
            GameObject buttonObj = Instantiate(shopButtonPrefab, shopContent);

            // Update the button text to show item name and cost
            TMPro.TMP_Text buttonText = buttonObj.GetComponentInChildren<TMPro.TMP_Text>();
            buttonText.text = $"{item.Name}\n${item.Cost}";

            // Set the sprite if available
            Sprite itemSprite = shopDatabase.GetSprite(item.Id);
            if (itemSprite != null)
            {
                // Find or create an Image component for the sprite
                UnityEngine.UI.Image spriteImage = null;
                
                // Check if there's a child object named "ItemSprite" or "Sprite"
                Transform spriteTransform = buttonObj.transform.Find("ItemSprite");
                if (spriteTransform == null)
                    spriteTransform = buttonObj.transform.Find("Sprite");
                
                if (spriteTransform != null)
                {
                    spriteImage = spriteTransform.GetComponent<UnityEngine.UI.Image>();
                }
                
                // If found, set the sprite
                if (spriteImage != null)
                {
                    spriteImage.sprite = itemSprite;
                    spriteImage.preserveAspect = true; // Maintain aspect ratio
                }
            }

            // Set up the buy button
            Button buyButton = buttonObj.GetComponent<Button>();
            string itemId = item.Id; // Capture for closure
            buyButton.onClick.AddListener(() => BuyShopItem(itemId));
        }
    }


    // --------------- Inventory -----------------------------------
    public void ShowInventory()
    {
        if (currentPlayer == null || currentPlayer.Inventory == null)
        {
            Debug.Log("No inventory to display.");
            inventoryPanel.SetActive(true); // show empty panel
            return;
        }
        PlayInventorySound();

        inventoryPanel.SetActive(true);

        // Clear old buttons so we don't duplicate
        foreach (Transform child in inventoryContent)
            Destroy(child.gameObject);

        // Spawn one button per item in inventory
        foreach (string itemId in currentPlayer.Inventory)
        {
            GameObject buttonObj = Instantiate(inventoryButtonPrefab, inventoryContent);

            ShopItem item = shopDatabase.GetItem(itemId);
            string displayText = item != null ? item.Name : itemId;
            
            // Set the text
            TMPro.TMP_Text buttonText = buttonObj.GetComponentInChildren<TMPro.TMP_Text>();
            buttonText.text = displayText;

            // Set the sprite if available
            Sprite itemSprite = shopDatabase.GetSprite(itemId);
            if (itemSprite != null)
            {
                // Find or create an Image component for the sprite
                UnityEngine.UI.Image spriteImage = null;
                
                // Check if there's a child object named "ItemSprite" or "Sprite"
                Transform spriteTransform = buttonObj.transform.Find("ItemSprite");
                if (spriteTransform == null)
                    spriteTransform = buttonObj.transform.Find("Sprite");
                
                if (spriteTransform != null)
                {
                    spriteImage = spriteTransform.GetComponent<UnityEngine.UI.Image>();
                }
                
                // If found, set the sprite
                if (spriteImage != null)
                {
                    spriteImage.sprite = itemSprite;
                    spriteImage.preserveAspect = true; // Maintain aspect ratio
                }
            }

            Button btn = buttonObj.GetComponent<Button>();

            // Fire-and-forget async wrapper for the button click
            btn.onClick.AddListener(() =>
            {
                _ = OnInventoryItemClickedAsync(itemId);
            });
        }
    }

    // Async wrapper for placing item in home and closing inventory
    private async Task OnInventoryItemClickedAsync(string itemId)
    {
        Debug.Log($"Clicked {itemId}");

        await PlaceItemInHome(itemId); // your existing async placement logic

        CloseInventory(); // close the inventory UI after placing
    }



    public void CloseInventory()
    {
        PlayClickSound();
        inventoryPanel.SetActive(false);

        foreach (Transform child in inventoryContent)
        {
            Destroy(child.gameObject);
        }
    }

    // ----------------- Home Items ----------------------------
    public async Task PlaceItemInHome(string itemId)
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
        if (player == null || player.Inventory == null) return;

        // Make sure HomeItems dictionary exists
        if (player.HomeItems == null)
            player.HomeItems = new Dictionary<string, string>();

        // Get item type from shop
        ShopItem item = shopDatabase.GetItem(itemId);
        if (item == null)
        {
            Debug.LogError($"Item not found in ShopDatabase: {itemId}");
            return;
        }
        string itemType = item.Type;
        Debug.Log($"Placing item {itemId} of type {itemType}");

        // Check if slot is occupied
        if (player.HomeItems.TryGetValue(itemType, out string existingItemId))
        {
            ShowSwapPrompt(itemType, existingItemId, itemId);
            return;
        }

        // Slot empty: move item directly
        await MoveItemToHome(player, itemId, itemType);
    }

    private async Task MoveItemToHome(PlayerData player, string itemId, string itemType)
    {
        // Remove from inventory
        player.Inventory.Remove(itemId);

        // Place in HomeItems dictionary
        player.HomeItems[itemType] = itemId;

        // Save player to Firestore
        await firestoreService.SavePlayerAsync(currentUserId, player);

        // Refresh currentPlayer data
        currentPlayer = player;

        // Instantiate prefab in the home
        ShopItem item = shopDatabase.GetItem(itemId);
        if (item?.Prefab != null)
            Instantiate(item.Prefab, item.HomePosition, Quaternion.identity);

        // Refresh inventory UI
        ShowInventory();
        DisplayHomeItems();
    }

    // Replace your existing ShowSwapPrompt method with this:
    private void ShowSwapPrompt(string itemType, string currentHomeItemId, string newItemId)
    {
        swapPromptPanel.SetActive(true);
        swapPromptText.text = $"You already have a {itemType} in your home. Swap it with this one?";

        swapYesButton.onClick.RemoveAllListeners();
        swapYesButton.onClick.AddListener(async () =>
        {
            swapPromptPanel.SetActive(false);

            PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
            if (player == null) return;

            // Swap: move old home item back to inventory
            if (!player.Inventory.Contains(currentHomeItemId))
                player.Inventory.Add(currentHomeItemId);

            // Move new item to home
            await MoveItemToHome(player, newItemId, itemType);

            // Close inventory after swap
            inventoryPanel.SetActive(false);
        });

        // Add the missing No button functionality
        swapNoButton.onClick.RemoveAllListeners();
        swapNoButton.onClick.AddListener(() =>
        {
            swapPromptPanel.SetActive(false);
            // Do nothing else - just close the prompt
        });
    }

    public async void DisplayHomeItems()
    {
        if (currentPlayer == null || currentPlayer.HomeItems == null)
        {
            return;
        }

        // Reload fresh data from Firebase to get latest positions
        currentPlayer = await firestoreService.LoadPlayerAsync(currentUserId);

        if (currentPlayer == null || currentPlayer.HomeItems == null)
        {
            return;
        }

        // Clear existing home item BUTTONS
        foreach (Transform child in homeContent)
            Destroy(child.gameObject);

        // Clear existing SPRITES
        foreach (var kvp in spawnedFurniture)
            if (kvp.Value != null)
                Destroy(kvp.Value);
        spawnedFurniture.Clear();

        // Create a button AND sprite for each item in home
        foreach (var kvp in currentPlayer.HomeItems)
        {
            string itemType = kvp.Key;
            string itemId = kvp.Value;

            // 1. Create the UI button
            GameObject buttonObj = Instantiate(homeItemButtonPrefab, homeContent);
            ShopItem item = shopDatabase.GetItem(itemId);
            string displayText = item != null ? item.Name : itemId;
            buttonObj.GetComponentInChildren<TMPro.TMP_Text>().text = displayText;
            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() => ReturnItemToInventory(itemId, itemType));

            // 2. Create the draggable sprite
            SpawnFurnitureSprite(itemId, itemType);
        }
    }

    public async void ReturnItemToInventory(string itemId, string itemType)
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
        if (player == null) return;

        // Remove from home
        if (player.HomeItems != null && player.HomeItems.ContainsKey(itemType))
        {
            player.HomeItems.Remove(itemType);
        }

        // Add back to inventory
        if (player.Inventory == null)
            player.Inventory = new List<string>();

        if (!player.Inventory.Contains(itemId))
            player.Inventory.Add(itemId);

        // Save to database
        await firestoreService.SavePlayerAsync(currentUserId, player);

        // Update local reference
        currentPlayer = player;

        // Refresh displays
        DisplayHomeItems();
    }

    public async void SaveFurniturePosition(string itemId, string itemType, Vector2 position)
    {
        if (string.IsNullOrEmpty(currentUserId)) return;

        PlayerData player = await firestoreService.LoadPlayerAsync(currentUserId);
        if (player == null) return;

        if (player.HomeItemPositions == null)
            player.HomeItemPositions = new Dictionary<string, Vector2Data>();

        player.HomeItemPositions[itemId] = new Vector2Data(position.x, position.y);

        // Save layer order
        if (player.HomeItemLayers == null)
            player.HomeItemLayers = new Dictionary<string, int>();

        foreach (var kvp in spawnedFurniture)
        {
            if (kvp.Value != null)
            {
                player.HomeItemLayers[kvp.Key] = kvp.Value.transform.GetSiblingIndex();
            }
        }

        await firestoreService.SavePlayerAsync(currentUserId, player);

        Debug.Log($"Saved position for {itemId}: {position}");
    }

    private void SpawnFurnitureSprite(string itemId, string itemType)
    {
        GameObject prefab = GetFurniturePrefab(itemType);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for type: {itemType}");
            return;
        }

        GameObject furnitureObj = Instantiate(prefab, furnitureDisplayArea);

        // Set the correct sprite for this specific item variant
        UnityEngine.UI.Image img = furnitureObj.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            Sprite itemSprite = shopDatabase.GetSprite(itemId);
            if (itemSprite != null)
            {
                img.sprite = itemSprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for item: {itemId}");
            }
        }

        DraggableFurniture draggable = furnitureObj.GetComponent<DraggableFurniture>();
        if (draggable == null)
            draggable = furnitureObj.AddComponent<DraggableFurniture>();

        draggable.itemId = itemId;
        draggable.itemType = itemType;
        draggable.firebaseController = this;
        draggable.draggableArea = furnitureDisplayArea;

        // Load saved position or use default
        Vector2 position = GetSavedPosition(itemId, itemType);
        draggable.SetPosition(position);

        // NEW: Restore layer order
        if (currentPlayer.HomeItemLayers != null &&
            currentPlayer.HomeItemLayers.TryGetValue(itemId, out int savedLayer))
        {
            furnitureObj.transform.SetSiblingIndex(savedLayer);
        }

        spawnedFurniture[itemId] = furnitureObj;
    }

    // Helper to get saved position
    private Vector2 GetSavedPosition(string itemId, string itemType)
    {
        if (currentPlayer.HomeItemPositions != null &&
            currentPlayer.HomeItemPositions.TryGetValue(itemId, out Vector2Data savedPos))
        {
            return savedPos.ToVector2();
        }

        // Return default position based on type
        switch (itemType.ToLower())
        {
            case "bed": return new Vector2(-150, 100);
            case "chair": return new Vector2(150, 100);
            case "desk": return new Vector2(-150, -100);
            case "lamp": return new Vector2(150, -100);
            default: return Vector2.zero;
        }
    }

    // Helper to get the right prefab
    private GameObject GetFurniturePrefab(string itemType)
    {
        switch (itemType.ToLower())
        {
            case "bed": return bedPrefab;
            case "chair": return chairPrefab;
            case "desk": return deskPrefab;
            case "lamp": return lampPrefab;
            default: return null;
        }
    }

    // ------------------------- Sounds ---------------------------------------------
    private void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusicPlaylist != null && backgroundMusicPlaylist.Length > 0)
        {
            musicSource.clip = backgroundMusicPlaylist[currentSongIndex];
            musicSource.volume = 0.3f; // Adjust volume (0.0 to 1.0)
            musicSource.Play();
            
            // Start coroutine to play next song when current one finishes
            StartCoroutine(PlayNextSongWhenFinished());
        }
    }

    private System.Collections.IEnumerator PlayNextSongWhenFinished()
    {
        // Wait until the current song finishes, but also check if it's paused
        while (musicSource.isPlaying || musicSource.time > 0)
        {
            yield return new WaitForSeconds(0.5f); // Check every half second
            
            // If the song finished playing (not paused, time is 0, and not playing)
            if (!musicSource.isPlaying && musicSource.time == 0)
                break;
        }

        // Only move to next song if music isn't paused
        if (!musicSource.isPlaying && musicSource.time == 0)
        {
            // Move to next song (loop back to start if at end)
            currentSongIndex = (currentSongIndex + 1) % backgroundMusicPlaylist.Length;

            // Play next song
            PlayBackgroundMusic();
        }
    }

    private void PlayPurchaseSound()
    {
        if (sfxSource != null && purchaseSound != null)
        {
            sfxSource.PlayOneShot(purchaseSound);
        }
    }
    
    private void PlayGoalCompleteSound()
    {
        if (sfxSource != null && goalCompleteSound != null)
        {
            sfxSource.PlayOneShot(goalCompleteSound);
        }
    }

    public void PlayPickUpItemSound()
    {
        if (sfxSource != null && pickUpItemSound != null)
        {
            sfxSource.PlayOneShot(pickUpItemSound);
        }
    }

    public void PlayPlaceItemSound()
    {
        if (sfxSource != null && placeItemSound != null)
        {
            sfxSource.PlayOneShot(placeItemSound);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void PlayInventorySound()
    {
        if (sfxSource != null && inventorySound != null)
        {
            sfxSource.PlayOneShot(inventorySound);
        }
    }

    public void OnVolumeChanged()
    {
        if (musicSource != null)
            musicSource.volume = volumeSlider.value;
        
        if (sfxSource != null)
            sfxSource.volume = volumeSlider.value;
        
        // Save setting
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        PlayerPrefs.Save();
    }

    public void OnMuteMusicChanged()
    {
        if (musicSource != null)
            musicSource.mute = muteMusicToggle.isOn;
        
        // Save setting
        PlayerPrefs.SetInt("MuteMusic", muteMusicToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMuteSoundChanged()
    {
        if (sfxSource != null)
            sfxSource.mute = muteSoundToggle.isOn;
        
        // Save setting
        PlayerPrefs.SetInt("MuteSound", muteSoundToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        // Load volume (default to 0.7 if not set)
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.7f);
        volumeSlider.value = savedVolume;

        // Load mute settings
        muteMusicToggle.isOn = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        muteSoundToggle.isOn = PlayerPrefs.GetInt("MuteSound", 0) == 1;

        // Apply loaded settings
        OnVolumeChanged();
        OnMuteMusicChanged();
        OnMuteSoundChanged();
    }

    public void SkipToNextSong()
    {
        if (musicSource != null && backgroundMusicPlaylist != null && backgroundMusicPlaylist.Length > 0)
        {
            // Stop current coroutine if it's running
            StopCoroutine(nameof(PlayNextSongWhenFinished));

            // Move to next song
            currentSongIndex = (currentSongIndex + 1) % backgroundMusicPlaylist.Length;

            // Play the next song
            musicSource.Stop();
            musicSource.clip = backgroundMusicPlaylist[currentSongIndex];
            musicSource.Play();

            // Restart the coroutine for when this song finishes
            StartCoroutine(PlayNextSongWhenFinished());

            // Optional: Play a click sound for feedback
            PlayClickSound();

            Debug.Log($"Skipped to song {currentSongIndex + 1} of {backgroundMusicPlaylist.Length}");
        }
    }

    public void SkipToPreviousSong()
    {
        if (musicSource != null && backgroundMusicPlaylist != null && backgroundMusicPlaylist.Length > 0)
        {
            // Stop current coroutine if it's running
            StopCoroutine(nameof(PlayNextSongWhenFinished));

            // Move to previous song (with wrap-around)
            currentSongIndex--;
            if (currentSongIndex < 0)
                currentSongIndex = backgroundMusicPlaylist.Length - 1;

            // Play the previous song
            musicSource.Stop();
            musicSource.clip = backgroundMusicPlaylist[currentSongIndex];
            musicSource.Play();

            // Restart the coroutine for when this song finishes
            StartCoroutine(PlayNextSongWhenFinished());

            // Optional: Play a click sound for feedback
            PlayClickSound();

            Debug.Log($"Went back to song {currentSongIndex + 1} of {backgroundMusicPlaylist.Length}");
        }
    }

    public void TogglePausePlay()
    {
        if (musicSource != null)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
                Debug.Log("Music paused");
            }
            else
            {
                // Check if there's a clip loaded
                if (musicSource.clip != null)
                {
                    musicSource.UnPause();
                    Debug.Log("Music resumed");
                }
                else
                {
                    // No clip loaded, start playing from beginning
                    PlayBackgroundMusic();
                }
            }
            
            PlayClickSound();
        }
    }

    // Display current song info
    public string GetCurrentSongName()
    {
        if (backgroundMusicPlaylist != null && currentSongIndex < backgroundMusicPlaylist.Length)
        {
            return backgroundMusicPlaylist[currentSongIndex].name;
        }
        return "No song playing";
    }


    // ---------------------- Sleep Log -----------------------------------------------
    public async void LogSleep()
    {
        if (currentPlayer == null || string.IsNullOrEmpty(currentUserId)) return;

        // Parse hours
        if (!float.TryParse(sleepHoursInput.text, out float hours))
        {
            Debug.LogWarning("Invalid sleep hours input.");
            return;
        }

        // Parse date from input, default to today if empty
        DateTime selectedDate;
        if (string.IsNullOrEmpty(sleepDateInput.text))
            selectedDate = DateTime.Today;
        else if (!DateTime.TryParse(sleepDateInput.text, out selectedDate))
        {
            Debug.LogWarning("Invalid date input.");
            return;
        }

        // Prevent future dates
        if (selectedDate > DateTime.Today)
        {
            Debug.LogWarning("Cannot log sleep for a future date.");
            return;
        }

        string dateString = selectedDate.ToString("yyyy-MM-dd");

        // Check if log exists for this date
        SleepLog existing = currentPlayer.SleepLogs.FirstOrDefault(l => l.Date == dateString);
        if (existing != null)
        {
            existing.Hours = hours; // overwrite
        }
        else
        {
            currentPlayer.SleepLogs.Add(new SleepLog { Date = dateString, Hours = hours });
        }

        await firestoreService.SavePlayerAsync(currentUserId, currentPlayer);

        sleepHoursInput.text = "";
        sleepDateInput.text = "";
        DisplaySleepLogs();
    }

    public void DisplaySleepLogs()
    {
        foreach (Transform child in sleepLogContent)
            Destroy(child.gameObject);

        if (currentPlayer?.SleepLogs == null) return;

        foreach (var log in currentPlayer.SleepLogs.OrderByDescending(l => l.Date))
        {
            GameObject rowObj = Instantiate(sleepLogRowPrefab, sleepLogContent);
            SleepLogRow row = rowObj.GetComponent<SleepLogRow>();

            row.dateText.text = log.Date;
            row.hoursText.text = $"{log.Hours} Hours";
            row.editButton.GetComponentInChildren<TMP_Text>().text = "Edit";

            row.editButton.onClick.RemoveAllListeners();
            row.editButton.onClick.AddListener(() =>
            {
                OpenEditSleepPanel(log);
            });
        }
    }


    public void OpenEditSleepPanel(SleepLog log)
    {
        logBeingEdited = log;
        editHoursInput.text = log.Hours.ToString();
        editSleepPanel.SetActive(true);

        editOkButton.onClick.RemoveAllListeners();
        editOkButton.onClick.AddListener(SaveEditedSleep);

        editCancelButton.onClick.RemoveAllListeners();
        editCancelButton.onClick.AddListener(() =>
        {
            editSleepPanel.SetActive(false);
            logBeingEdited = null;
        });
    }

    public async void SaveEditedSleep()
    {
        if (logBeingEdited != null && float.TryParse(editHoursInput.text, out float newHours))
        {
            logBeingEdited.Hours = newHours;

            await firestoreService.SavePlayerAsync(currentUserId, currentPlayer);

            editSleepPanel.SetActive(false);
            DisplaySleepLogs();
        }
    }



}

[System.Serializable]
[Firebase.Firestore.FirestoreData]
public class Vector2Data
{
    [Firebase.Firestore.FirestoreProperty]
    public float x { get; set; }
    
    [Firebase.Firestore.FirestoreProperty]
    public float y { get; set; }
    
    public Vector2Data() { }
    
    public Vector2Data(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    
    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
}