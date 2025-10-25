using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

// *** eventually set up coroutine to count up the text variables.

// results screen to display run statistics
// shows enemies killed, credits collected, floors cleared

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
    
    [Header("Optional - Animated Display")]
    [SerializeField] private bool useCountUpAnimation = true;
    [SerializeField] private float countUpDuration = 2f;
    
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
                StartCoroutine(AnimateCountUp(targetFloors));
                floorsText.text = $"Floors Cleared: {targetFloors}";
            }
            if (enemiesText != null)
            {
                StartCoroutine(AnimateCountUp(targetEnemies));
                enemiesText.text = $"Enemies Killed: {targetEnemies}";
            }
            if (creditsText != null)
            {
                StartCoroutine(AnimateCountUp(targetCredits));
                creditsText.text = $"Credits Collected: {targetCredits} ȼ";
            }
            
            if (progress >= 1f)
            {
                isCountingUp = false;
            }
        }
    }

    IEnumerator AnimateCountUp(int targetCount)
    {
        float startTime = Time.time;
        int currentCount = 0;

        while(Time.time < startTime)
        {
            float elapsedPercentage = (Time.time - startTime) / countUpDuration;
            currentCount = Mathf.RoundToInt(Mathf.Lerp(0, targetCount, elapsedPercentage));
            
            yield return null;
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

        // get stats from tracker
        targetFloors = RunStatsTracker.Instance.FloorsCleared;
        targetEnemies = RunStatsTracker.Instance.EnemiesKilled;
        targetCredits = RunStatsTracker.Instance.CreditsCollected;
        bool beatBoss = RunStatsTracker.Instance.BeatBoss;
        string unlockedSkill = RunStatsTracker.Instance.UnlockedSkill;

        if(debug) Debug.Log($"[ResultsScreenUI] Got stats - Floors: {targetFloors}, Enemies: {targetEnemies}, Credits: {targetCredits}");

        if (floorsText != null) { floorsText.text = $"Floors Cleared: {targetFloors}"; }
                
        if (enemiesText != null) { enemiesText.text = $"Enemies Killed: {targetEnemies}"; }
                
        if (creditsText != null) { creditsText.text = $"Credits Collected: {targetCredits} ȼ"; }

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

    private void OnDestroy()
    {
        // clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }
}

