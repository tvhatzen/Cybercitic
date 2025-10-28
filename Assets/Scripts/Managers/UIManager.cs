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

    private void HandleGameStateChanged(GameState.GameStates state)
    {
        switch (state)
        {
            case GameState.GameStates.MainMenu: 
                ShowScreen(MenuScreen.MainMenu, 0f);
                AudioManager.Instance.PlayMusicTrack("mainMenu");
                break;

            case GameState.GameStates.Tutorial: ShowScreen(MenuScreen.Tutorial, 0f); break;

            case GameState.GameStates.Playing: 
                ShowScreen(MenuScreen.Gameplay, 1f);
                //FloorManager.Instance.PlayBackgroundMusic();
                break;

            case GameState.GameStates.Paused: ShowScreen(MenuScreen.Pause, 0f); break;

            case GameState.GameStates.Upgrade: 
                ShowScreen(MenuScreen.Upgrade, 0f);
                AudioManager.Instance.PlayMusicTrack("upgradeScreen");
                break;

            case GameState.GameStates.Results: ShowScreen(MenuScreen.Results, 0f); break;
            case GameState.GameStates.Options: ShowScreen(MenuScreen.Options, 0f); break;
            case GameState.GameStates.Credits: ShowScreen(MenuScreen.Credits, 0f); break;

            case GameState.GameStates.Win: 
                ShowScreen(MenuScreen.WinGame, 0f);
                AudioManager.Instance.PlayMusicTrack("winScreen"); 
                break;

            case GameState.GameStates.Lose: ShowScreen(MenuScreen.LoseGame, 0f); break;

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
        if (Input.GetKeyDown(KeyCode.Escape) && GameState.Instance.CurrentState == GameState.GameStates.Playing)
            GameState.Instance.ChangeState(GameState.GameStates.Paused);
        else if (Input.GetKeyDown(KeyCode.Escape) && GameState.Instance.CurrentState == GameState.GameStates.Paused)
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
    }

    public void StartGameFromMainMenu() => GameState.Instance.StartFromMainMenu();

    public void FinishTutorialAndStartGame()
    {
        if(debug) Debug.Log("Tutorial button pressed!");
        GameState.Instance.FinishTutorial();
    }

    public void RestartAfterDeath() => GameState.Instance.StartGameplay(fromDeath: true);
    public void GoToMenu() => GameState.Instance.ChangeState(GameState.GameStates.MainMenu);
    public void ResumeGame() => GameState.Instance.ChangeState(GameState.GameStates.Playing);
    public void GoToUpgrade() => GameState.Instance.ChangeState(GameState.GameStates.Upgrade);
    public void ShowResults() => GameState.Instance.ChangeState(GameState.GameStates.Results);
    public void ShowOptions() => GameState.Instance.ChangeState(GameState.GameStates.Options);
    public void ShowCredits() => creditsUI.SetActive(true);
    public void CloseCredits() => creditsUI.SetActive(false);
    public void QuitGame() => Application.Quit();
    public void GoToMainMenuFromWin() =>GameState.Instance.ChangeState(GameState.GameStates.MainMenu);

    #endregion
}
