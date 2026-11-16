using System;
using UnityEngine;

namespace ChickenRush
{
    /// <summary>Tracks score (furthest row reached), best score (saved with PlayerPrefs) and the target.</summary>
    public class ScoreManager : MonoBehaviour
    {
        private const string BestKey = "ChickenRush.BestScore";

        [SerializeField, Min(1)] private int targetScore = 30;

        public int Score { get; private set; }
        public int BestScore { get; private set; }
        public int TargetScore => targetScore;

        public event Action<int> ScoreChanged;

        private void Awake()
        {
            BestScore = PlayerPrefs.GetInt(BestKey, 0);
        }

        public void ResetScore()
        {
            Score = 0;
            ScoreChanged?.Invoke(Score);
        }

        /// <summary>Called with the row the chicken just landed on. Only forward progress counts.</summary>
        public void ReportRow(int row)
        {
            int clamped = Mathf.Min(row, targetScore);
            if (clamped <= Score) return;
            Score = clamped;
            ScoreChanged?.Invoke(Score);
        }

        public void CommitBest()
        {
            if (Score <= BestScore) return;
            BestScore = Score;
            PlayerPrefs.SetInt(BestKey, BestScore);
            PlayerPrefs.Save();
        }
    }
}
