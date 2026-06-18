using RusRunner.Game.Runner;
using UnityEngine;
using UnityEngine.UI;

namespace RusRunner.Game.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private RunnerBootstrap runnerBootstrap;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text stateText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Button restartButton;

        private void Start()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(Restart);
            }
        }

        private void Update()
        {
            if (runnerBootstrap == null)
            {
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {runnerBootstrap.CurrentScore:0}";
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = $"Best: {runnerBootstrap.BestScore:0}";
            }

            if (distanceText != null)
            {
                distanceText.text = $"Distance: {runnerBootstrap.CurrentDistance:0.0} m";
            }

            if (speedText != null)
            {
                speedText.text = $"Speed: {runnerBootstrap.CurrentSpeed:0.0}";
            }

            if (stateText != null)
            {
                stateText.text = runnerBootstrap.IsAlive
                    ? (runnerBootstrap.IsInvulnerable ? "State: Shielded" : "State: Running")
                    : "State: Defeated";
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(!runnerBootstrap.IsAlive);
            }

            if (gameOverText != null && !runnerBootstrap.IsAlive)
            {
                gameOverText.text = $"Game Over\nScore: {runnerBootstrap.CurrentScore:0}\nBest: {runnerBootstrap.BestScore:0}\nPress R or Tap to Restart";
            }
        }

        private void Restart()
        {
            if (runnerBootstrap == null)
            {
                return;
            }

            runnerBootstrap.RestartRun();
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
            }
        }
    }
}
