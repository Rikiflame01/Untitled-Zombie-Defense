public static class GameStateManager
{
    public enum GameState
    {
        MainMenu,
        Defending,
        Building,
        ChooseCard,
        Paused
    }

    private static bool _initialized;
    private static GameState _currentState = GameState.MainMenu;

    public static GameState CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public static bool IsMainMenu => _currentState == GameState.MainMenu;
    public static bool IsDefending => _currentState == GameState.Defending;
    public static bool IsBuilding  => _currentState == GameState.Building;
    public static bool IsChoosingCard  => _currentState == GameState.ChooseCard;
    public static bool IsPaused    => _currentState == GameState.Paused;

    public static void Initialize()
    {
        if (_initialized) return;

        ActionManager.OnPaused       += HandleGamePaused;
        ActionManager.OnDefenseStart += HandleDefenseStart;
        ActionManager.OnBuildStart   += HandleBuildingStart;
        ActionManager.OnChooseCard += HandleChooseCard;

        _initialized = true;
    }

    private static void HandleGamePaused()
    {
        SetPaused();
    }

    private static void HandleDefenseStart()
    {
        NavMeshManager.Instance.RebuildNavMesh();
        SetDefending();
    }

    private static void HandleBuildingStart()
    {
        SetBuilding();
    }

    private static void HandleChooseCard()
    {
        SetChooseCard();
    }

    public static void SetDefending() => _currentState = GameState.Defending;
    public static void SetBuilding()  => _currentState = GameState.Building;
    public static void SetPaused()    => _currentState = GameState.Paused;
    public static void SetChooseCard()    => _currentState = GameState.ChooseCard;
}
