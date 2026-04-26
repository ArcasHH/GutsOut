// IGameState.cs
public interface IGameState
{
    bool IsActive { get; }
    void StartGame();
    void EndGame(int stars);
    void RestartGame();
}

