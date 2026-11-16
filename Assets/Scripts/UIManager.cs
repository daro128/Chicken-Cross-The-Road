using UnityEngine;
using UnityEngine.UI;

namespace ChickenRush
{
    /// <summary>Shows the right panel for the current game state and keeps the score texts up to date.</summary>
    public class UIManager : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScoreManager scoreManager;

        [Header("Panels")]
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestText;
        [SerializeField] private Text targetText;

        [Header("End screens")]
        [SerializeField] private Text gameOverScoreText;
        [SerializeField] private Text gameOverBestText;
        [SerializeField] private Text victoryScoreText;
        [SerializeField] private Text victoryBestText;

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button gameOverRestartButton;
        [SerializeField] private Button victoryRestartButton;

        private void Awake()
        {
            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
            if (gameOverRestartButton != null) gameOverRestartButton.onClick.AddListener(OnRestartClicked);
            if (victoryRestartButton != null) victoryRestartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnEnable()
        {
            if (gameManager != null) gameManager.StateChanged += OnStateChanged;
            if (scoreManager != null) scoreManager.ScoreChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.StateChanged -= OnStateChanged;
            if (scoreManager != null) scoreManager.ScoreChanged -= OnScoreChanged;
        }

        private void OnStateChanged(GameState state)
        {
            SetActive(menuPanel, state == GameState.Menu);
            SetActive(hudPanel, state == GameState.Playing);
            SetActive(gameOverPanel, state == GameState.GameOver);
            SetActive(victoryPanel, state == GameState.Victory);
            RefreshTexts();
        }

        private void OnScoreChanged(int score)
        {
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (scoreManager == null) return;
            int score = scoreManager.Score;
            int best = Mathf.Max(scoreManager.BestScore, score);
            int target = scoreManager.TargetScore;

            SetText(scoreText, "Score: " + score);
            SetText(bestText, "Best: " + best);
            SetText(targetText, "Goal: reach row " + target);
            SetText(gameOverScoreText, "Score: " + score + " / " + target);
            SetText(gameOverBestText, "Best: " + best);
            SetText(victoryScoreText, "Score: " + score + " / " + target);
            SetText(victoryBestText, "Best: " + best);
        }

        private void OnStartClicked()
        {
            if (gameManager != null) gameManager.StartGame();
        }

        private void OnRestartClicked()
        {
            if (gameManager != null) gameManager.Restart();
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static void SetActive(GameObject go, bool active)
        {
            if (go != null) go.SetActive(active);
        }

        private static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }
    }
}
