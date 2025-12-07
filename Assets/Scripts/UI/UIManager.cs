using Unity.VisualScripting;
using UnityEngine;

public class UIManager : SingletonBase<UIManager>
{
    public enum MenuScreen
    {
        MainMenu,
        Tutorial,
        Pause,
        Upgrade,
        Results,
        Options,
        Credits,
        WinGame,
        LoseGame,
        Gameplay,
        None
    }

    [Header("UI Objects")]
    public GameObject mainMenuUI;
    public GameObject pauseUI;
    public GameObject upgradeUI;
    public GameObject resultsUI;
    public GameObject gameplayUI;
    public GameObject optionsUI;
    public GameObject creditsUI;
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject tutorialUI;

    [Header("DEBUG")]
    public bool debug = false;

    protected override void Awake()
    {
        base.Awake();

        mainMenuUI.SetActive(true);
        gameplayUI.SetActive(true);
        pauseUI.SetActive(true);
        upgradeUI.SetActive(true);
        resultsUI.SetActive(true);
        optionsUI.SetActive(true);
        creditsUI.SetActive(true);
        winUI.SetActive(true);
        loseUI.SetActive(true);

        ShowScreen(MenuScreen.MainMenu, 0f);

        GameEvents.OnGameStateChanged += HandleGameStateChanged;
    }

    private void Update()
    {
        TogglePause();
    }
    private void OnDestroy()
    {
        GameEvents.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameManager.GameStates state)
    {
        switch (state)
        {
            case GameManager.GameStates.MainMenu: ShowScreen(MenuScreen.MainMenu, 0f); break;
            case GameManager.GameStates.Tutorial: ShowScreen(MenuScreen.Tutorial, 0f); break;
            case GameManager.GameStates.Playing: ShowScreen(MenuScreen.Gameplay, 1f); break;
            case GameManager.GameStates.Paused: ShowScreen(MenuScreen.Pause, 0f); break;
            case GameManager.GameStates.Upgrade: ShowScreen(MenuScreen.Upgrade, 0f); break;
            case GameManager.GameStates.Results: ShowScreen(MenuScreen.Results, 0f); break;
            case GameManager.GameStates.Options: ShowScreen(MenuScreen.Options, 0f); break;
            case GameManager.GameStates.Credits: ShowScreen(MenuScreen.Credits, 0f); break;
            case GameManager.GameStates.Win: ShowScreen(MenuScreen.WinGame, 0f); break;
            case GameManager.GameStates.Lose: ShowScreen(MenuScreen.LoseGame, 0f); break;
            default: ShowScreen(MenuScreen.None, 1f); break;
        }
    }

    private void ShowScreen(MenuScreen screen, float timescale)
    {
        mainMenuUI.SetActive(screen == MenuScreen.MainMenu);
        tutorialUI.SetActive(screen == MenuScreen.Tutorial);  
        gameplayUI.SetActive(screen == MenuScreen.Gameplay);
        pauseUI.SetActive(screen == MenuScreen.Pause);
        upgradeUI.SetActive(screen == MenuScreen.Upgrade);
        resultsUI.SetActive(screen == MenuScreen.Results);
        optionsUI.SetActive(screen == MenuScreen.Options);
        creditsUI.SetActive(screen == MenuScreen.Credits);
        winUI.SetActive(screen == MenuScreen.WinGame);
        loseUI.SetActive(screen == MenuScreen.LoseGame);

        // only scale time if gameplay screen, else normal time
        Time.timeScale = timescale;

        if(debug) Debug.Log($"Showing screen: {screen}");
    }

    #region Public Menu Buttons

    public void TogglePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.CurrentState == GameManager.GameStates.Playing)
            GameManager.Instance.ChangeState(GameManager.GameStates.Paused);
        else if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance.CurrentState == GameManager.GameStates.Paused)
            GameManager.Instance.ChangeState(GameManager.GameStates.Playing);
    }

    // Note: StartGameFromMainMenu, FinishTutorialAndStartGame, and RestartAfterDeath 
    // should now handle their logic in UIManager or other appropriate systems,
    // then call GameManager.Instance.ChangeState() when ready to change state
    public void StartGameFromMainMenu() 
    {
        // TODO: Handle game start logic here (reset upgrades, stats, skills, etc.)
        // Then change state when ready
        GameManager.Instance.ChangeState(GameManager.GameStates.Playing);
    }

    public void FinishTutorialAndStartGame()
    {
        if(debug) Debug.Log("Tutorial button pressed!");
        // TODO: Handle tutorial completion logic here
        // Then change state when ready
        GameManager.Instance.ChangeState(GameManager.GameStates.Playing);
    }

    public void RestartAfterDeath() 
    {
        // TODO: Handle restart logic here (reset floor, player, etc.)
        // Then change state when ready
        GameManager.Instance.ChangeState(GameManager.GameStates.Playing);
    }
    
    public void GoToMenu() => GameManager.Instance.ChangeState(GameManager.GameStates.MainMenu);
    public void ResumeGame() => GameManager.Instance.ChangeState(GameManager.GameStates.Playing);
    public void GoToUpgrade() => GameManager.Instance.ChangeState(GameManager.GameStates.Upgrade);
    public void ShowResults() => GameManager.Instance.ChangeState(GameManager.GameStates.Results);
    public void ShowOptions() => GameManager.Instance.ChangeState(GameManager.GameStates.Options);
    public void ShowCredits() => creditsUI.SetActive(true);
    public void CloseCredits() => creditsUI.SetActive(false);
    public void QuitGame() => Application.Quit();
    public void GoToMainMenuFromWin() => GameManager.Instance.ChangeState(GameManager.GameStates.MainMenu);

    #endregion
}
