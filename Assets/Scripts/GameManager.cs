using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChickenRush
{
    public enum GameState { Menu, Playing, GameOver, Victory }

    /// <summary>Owns the game state machine: Menu -> Playing -> GameOver / Victory -> Restart.</summary>
    public class GameManager : MonoBehaviour
    {
        // Survives the scene reload used by Restart so we can skip the menu the second time.
        private static bool skipMenuOnNextLoad;

        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private EnvironmentGenerator environment;
        [SerializeField] private TrafficSpawner traffic;

        public GameState State { get; private set; } = GameState.Menu;
        public event Action<GameState> StateChanged;

        private void Start()
        {
            Time.timeScale = 1f;

            if (environment != null && scoreManager != null)
            {
                environment.Generate(scoreManager.TargetScore);
            }
            else
            {
                Debug.LogWarning("GameManager: Environment or ScoreManager reference is missing.");
            }

            if (scoreManager != null) scoreManager.ResetScore();

            if (skipMenuOnNextLoad)
            {
                skipMenuOnNextLoad = false;
                StartGame();
            }
            else
            {
                SetState(GameState.Menu);
            }
        }

        private void Update()
        {
            if (State == GameState.Menu && InputReader.ConfirmPressed())
            {
                StartGame();
            }
            else if ((State == GameState.GameOver || State == GameState.Victory) && InputReader.RestartPressed())
            {
                Restart();
            }
        }

        public void StartGame()
        {
            if (scoreManager != null) scoreManager.ResetScore();
            SetState(GameState.Playing);
        }

        public void GameOver()
        {
            if (State != GameState.Playing) return;
            if (scoreManager != null) scoreManager.CommitBest();
            SetState(GameState.GameOver);
        }

        public void Victory()
        {
            if (State != GameState.Playing) return;
            if (scoreManager != null)
            {
                scoreManager.ReportRow(scoreManager.TargetScore);
                scoreManager.CommitBest();
            }
            SetState(GameState.Victory);
        }

        public void Restart()
        {
            skipMenuOnNextLoad = true;
            Scene active = SceneManager.GetActiveScene();
            if (active.buildIndex >= 0) SceneManager.LoadScene(active.buildIndex);
            else SceneManager.LoadScene(active.name);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            if (traffic != null)
            {
                traffic.SetMoving(newState == GameState.Menu || newState == GameState.Playing);
            }
            StateChanged?.Invoke(newState);
        }
    }
}
