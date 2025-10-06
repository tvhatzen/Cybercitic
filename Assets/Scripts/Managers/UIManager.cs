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
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject tutorialUI;

    private void Awake()
    {
        base.Awake();

        mainMenuUI.SetActive(true);
        gameplayUI.SetActive(true);
        pauseUI.SetActive(true);
        upgradeUI.SetActive(true);
        resultsUI.SetActive(true);
        winUI.SetActive(true);
        loseUI.SetActive(true);

        // Then immediately hide them all
        ShowScreen(MenuScreen.MainMenu, 0f);

        GameState.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        GameState.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState.GameStates state)
    {
        switch (state)
        {
            case GameState.GameStates.MainMenu: ShowScreen(MenuScreen.MainMenu, 0f); break;
            case GameState.GameStates.Tutorial: ShowScreen(MenuScreen.Tutorial, 0f); break;
            case GameState.GameStates.Playing: ShowScreen(MenuScreen.Gameplay, 1f); break;
            case GameState.GameStates.Paused: ShowScreen(MenuScreen.Pause, 0f); break;
            case GameState.GameStates.Upgrade: ShowScreen(MenuScreen.Upgrade, 0f); break;
            case GameState.GameStates.Results: ShowScreen(MenuScreen.Results, 0f); break;
            case GameState.GameStates.Win: ShowScreen(MenuScreen.WinGame, 0f); break;
            case GameState.GameStates.Lose: ShowScreen(MenuScreen.LoseGame, 0f); break;
            default: ShowScreen(MenuScreen.None, 1f); break;
        }
    }

    private void ShowScreen(MenuScreen screen, float timescale)
    {
        mainMenuUI.SetActive(screen == MenuScreen.MainMenu);
        tutorialUI.SetActive(screen == MenuScreen.Tutorial); // firing a null reference exception on scene change 
        gameplayUI.SetActive(screen == MenuScreen.Gameplay);
        pauseUI.SetActive(screen == MenuScreen.Pause);
        upgradeUI.SetActive(screen == MenuScreen.Upgrade);
        resultsUI.SetActive(screen == MenuScreen.Results);
        winUI.SetActive(screen == MenuScreen.WinGame);
        loseUI.SetActive(screen == MenuScreen.LoseGame);

        // only scale time if gameplay screen, else normal time
        Time.timeScale = timescale;

        Debug.Log($"Showing screen: {screen}");
    }

    #region Public Menu Buttons

    public void TogglePause()
    {
        if (GameState.Instance.CurrentState == GameState.GameStates.Playing)
            GameState.Instance.ChangeState(GameState.GameStates.Paused);
        else if (GameState.Instance.CurrentState == GameState.GameStates.Paused)
            GameState.Instance.ChangeState(GameState.GameStates.Playing);
    }

    public void StartGameFromMainMenu()
    {
        GameState.Instance.StartFromMainMenu();
    }

    public void FinishTutorialAndStartGame()
    {
        Debug.Log("Tutorial button pressed!");
        //Time.timeScale = 1f; // need?
        GameState.Instance.FinishTutorial();
    }

    public void RestartAfterDeath()
    {
        // Player clicked to retry after death
        GameState.Instance.StartGameplay(fromDeath: true);
    }


    public void GoToMenu() => GameState.Instance.ChangeState(GameState.GameStates.MainMenu);

    public void ResumeGame()
    {
        GameState.Instance.ChangeState(GameState.GameStates.Playing);
        Debug.Log("resumed game");
    } 
    public void GoToUpgrade() => GameState.Instance.ChangeState(GameState.GameStates.Upgrade);
    public void ShowResults() => GameState.Instance.ChangeState(GameState.GameStates.Results);
    public void QuitGame() => Application.Quit();

    #endregion
}
