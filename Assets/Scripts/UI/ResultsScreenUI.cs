using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// results screen to display run statistics
/// shows enemies killed, credits collected, floors cleared
public class ResultsScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI floorsText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private GameObject skillUnlockedPanel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button retryButton;
    
    [Header("Optional - Animated Display")]
    [SerializeField] private bool useCountUpAnimation = true;
    [SerializeField] private float countUpDuration = 1f;
    
    private int targetFloors;
    private int targetEnemies;
    private int targetCredits;
    private float countUpTimer = 0f;
    private bool isCountingUp = false;

    public bool debug = false;

    private void Awake()
    {
        // setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
    }

    private void OnEnable() => DisplayResults();

    private void Update()
    {
        if (isCountingUp && useCountUpAnimation)
        {
            countUpTimer += Time.deltaTime;
            float progress = Mathf.Clamp01(countUpTimer / countUpDuration);
            
            if (floorsText != null)
            {
                int currentFloors = Mathf.RoundToInt(Mathf.Lerp(0, targetFloors, progress));
                floorsText.text = $"Floors Cleared: {currentFloors}";
            }
            if (enemiesText != null)
            {
                int currentEnemies = Mathf.RoundToInt(Mathf.Lerp(0, targetEnemies, progress));
                enemiesText.text = $"Enemies Killed: {currentEnemies}";
            }
            if (creditsText != null)
            {
                int currentCredits = Mathf.RoundToInt(Mathf.Lerp(0, targetCredits, progress));
                creditsText.text = $"Credits Collected: {currentCredits} ȼ";
            }
            
            if (progress >= 1f)
            {
                isCountingUp = false;
            }
        }
    }

    private void DisplayResults()
    {
        if (RunStatsTracker.Instance == null)
        {
            if(debug) Debug.LogWarning("[ResultsScreenUI] RunStatsTracker not found!");
            SetDefaultValues();
            return;
        }

        // Get stats from tracker
        targetFloors = RunStatsTracker.Instance.FloorsCleared;
        targetEnemies = RunStatsTracker.Instance.EnemiesKilled;
        targetCredits = RunStatsTracker.Instance.CreditsCollected;
        bool beatBoss = RunStatsTracker.Instance.BeatBoss;
        string unlockedSkill = RunStatsTracker.Instance.UnlockedSkill;

        if(debug) Debug.Log($"[ResultsScreenUI] Got stats - Floors: {targetFloors}, Enemies: {targetEnemies}, Credits: {targetCredits}");

        
        if (floorsText != null)
        {
            floorsText.text = $"Floors Cleared: {targetFloors}";
            if(debug) Debug.Log($"[ResultsScreenUI] Set floors text: {floorsText.text}");
        }
                
        if (enemiesText != null)
        {
            enemiesText.text = $"Enemies Killed: {targetEnemies}";
            if(debug) Debug.Log($"[ResultsScreenUI] Set enemies text: {enemiesText.text}");
        }
                
        if (creditsText != null)
        {
            creditsText.text = $"Credits Collected: {targetCredits} ȼ";
            if(debug) Debug.Log($"[ResultsScreenUI] Set credits text: {creditsText.text}");
        }

        // display skill unlocked panel
        if (skillUnlockedPanel != null)
        {
            if (beatBoss && !string.IsNullOrEmpty(unlockedSkill))
            {
                skillUnlockedPanel.SetActive(true);
                
                if (skillNameText != null)
                    skillNameText.text = $"New Skill Unlocked!\n<b>{unlockedSkill}</b>";
            }
            else
                skillUnlockedPanel.SetActive(false);
        }

        if(debug) Debug.Log($"[ResultsScreenUI] Displaying results:\n{RunStatsTracker.Instance.GetRunSummary()}");
    }

    private void SetDefaultValues()
    {
        if (floorsText != null)
            floorsText.text = "Floors Cleared: 0";
            
        if (enemiesText != null)
            enemiesText.text = "Enemies Killed: 0";
            
        if (creditsText != null)
            creditsText.text = "Credits Collected: 0 ȼ";
            
        if (skillUnlockedPanel != null)
            skillUnlockedPanel.SetActive(false);
    }

    private void OnContinueClicked()
    {
        // continue to next floor or upgrade shop
        if (GameState.Instance != null)
            GameState.Instance.ChangeState(GameState.GameStates.Upgrade);
        else
            gameObject.SetActive(false);
    }

    private void OnMainMenuClicked()
    {
        // reset stats for next run
        if (RunStatsTracker.Instance != null)
            RunStatsTracker.Instance.ResetStats();
        
        // load main menu
        SceneManager.LoadScene("MainMenu");
    }

    private void OnRetryClicked()
    {
        // reset run stats for retry (keep upgrades and credits)
        if (RunStatsTracker.Instance != null)
            RunStatsTracker.Instance.ResetStatsForRetry();
        
        // reset floor to 1 but keep player upgrades
        if (FloorManager.Instance != null)
            FloorManager.Instance.ResetToFloor1();
        
        // restart the game
        if (GameState.Instance != null)
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
        else
        {
            // reload current scene 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            
        if (retryButton != null)
            retryButton.onClick.RemoveListener(OnRetryClicked);
    }
}

