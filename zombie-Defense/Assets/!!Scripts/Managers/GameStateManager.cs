public static class GameStateManager
{
    public enum GameState
    {
        Defending,
        Building,
        Paused
    }

    private static bool _initialized;
    private static GameState _currentState = GameState.Defending;

    public static GameState CurrentState
    {
        get => _currentState;
        set => _currentState = value;
    }

    public static bool IsDefending => _currentState == GameState.Defending;
    public static bool IsBuilding  => _currentState == GameState.Building;
    public static bool IsPaused    => _currentState == GameState.Paused;

    public static void Initialize()
    {
        if (_initialized) return;

        ActionManager.OnPaused       += HandleGamePaused;
        ActionManager.OnDefenseStart += HandleDefenseStart;
        ActionManager.OnBuildStart   += HandleBuildingStart;

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

    public static void SetDefending() => _currentState = GameState.Defending;
    public static void SetBuilding()  => _currentState = GameState.Building;
    public static void SetPaused()    => _currentState = GameState.Paused;
}
