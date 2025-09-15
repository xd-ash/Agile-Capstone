using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    //Singleton setup
    public static GameStateManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    // 

    public enum GameStates
    {
        Pause,
        PlayerTurn,
        EnemyTurn,
        EnvironmentTurn
    }
    private GameStates _gameState;
    public GameStates GetGameState { get { return _gameState; } }

    private void Start()
    {
        _gameState = GameStates.PlayerTurn; //testing purposes 
    }

    public void StateSwap(int state)
    {
        _gameState = (GameStates)state;
    }
}
