using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// results screen to display run statistics
// shows enemies killed, scrap collected, floors cleared

public class ResultsScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI floorsText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private Sprite scrapSprite;
    
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    
    [Header("Optional - Animated Display")]
    [SerializeField] private bool useCountUpAnimation = true;
    [SerializeField] private float countUpDuration = 2f;
    
    private int targetFloors;
    private int targetEnemies;
    private int targetCredits;

    public bool debug = false;

    private void Awake()
    {
        // setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void OnEnable() => DisplayResults();
    private void OnDestroy()
    {
        // clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    IEnumerator AnimateCountUp(int targetCount, TextMeshProUGUI textComponent, string prefix)
    {
        if (debug) Debug.Log($"[ResultsScreenUI] Starting animation for {prefix} to {targetCount}");
        
        float startTime = Time.time;
        int currentCount = 0;

        // handle case where targetCount is 0
        if (targetCount == 0)
        {
            if (textComponent != null)
            {
                textComponent.text = $"{prefix}: 0";
            }
            if (debug) Debug.Log($"[ResultsScreenUI] Target count is 0 for {prefix}, setting immediately");
            yield break;
        }

        while (currentCount < targetCount)
        {
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / countUpDuration);
            currentCount = Mathf.RoundToInt(Mathf.Lerp(0, targetCount, progress));
            
            if (textComponent != null)
            {
                textComponent.text = $"{prefix}: {currentCount}";
            }
            
            yield return null;
        }
        
        // ensure final value is set
        if (textComponent != null)
        {
            textComponent.text = $"{prefix}: {targetCount}";
        }
        
        if (debug) Debug.Log($"[ResultsScreenUI] Animation completed for {prefix}: {targetCount}");
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

        if(debug) Debug.Log($"[ResultsScreenUI] Got stats - Floors: {targetFloors}, Enemies: {targetEnemies}, Scrap: {targetCredits}");

        // start count up animations if enabled
        if (useCountUpAnimation)
        {
            if (debug) Debug.Log($"[ResultsScreenUI] Starting count up animations - Duration: {countUpDuration}s");
            
            if (floorsText != null) { StartCoroutine(AnimateCountUp(targetFloors, floorsText, "Floors Cleared")); }
            if (enemiesText != null) { StartCoroutine(AnimateCountUp(targetEnemies, enemiesText, "Enemies Killed")); }
        }
        else
        {
            // otherwise set text immediately without animation
            if (floorsText != null) { floorsText.text = $"Floors Cleared: {targetFloors}"; }
            if (enemiesText != null) { enemiesText.text = $"Enemies Killed: {targetEnemies}"; }
            if (creditsText != null) 
            { 
                string spriteTag = scrapSprite != null ? $"<sprite name=\"{scrapSprite.name}\">" : "";
                creditsText.text = $"{spriteTag} Scrap Collected: {targetCredits}"; 
            }
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
        {
            string spriteTag = scrapSprite != null ? $"<sprite name=\"{scrapSprite.name}\">" : "";
            creditsText.text = $"{spriteTag} Scrap Collected: 0";
        }
    }

    private void OnContinueClicked()
    {
        // continue to next floor or upgrade shop
        if (GameState.Instance != null)
            GameState.Instance.ChangeState(GameState.GameStates.Upgrade);
        else
            gameObject.SetActive(false);
    }
}

