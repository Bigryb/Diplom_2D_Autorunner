namespace RusRunner.Core.Systems
{
    public sealed class ScoreSystem
    {
        private const string BestScoreKey = "rusrunner_best_score";
        private readonly float _scorePerMeter;
        private float _bestScore;

        public float BestScore => _bestScore;

        public ScoreSystem(float scorePerMeter)
        {
            _scorePerMeter = scorePerMeter;
            _bestScore = UnityEngine.PlayerPrefs.GetFloat(BestScoreKey, 0f);
        }

        public void Tick(GameContext context, float deltaTime)
        {
            var effectiveSpeed = context.CurrentSpeed * context.SpeedMultiplier;
            context.DistanceMeters += effectiveSpeed * deltaTime;
            context.Score += effectiveSpeed * _scorePerMeter * context.ScoreMultiplier * deltaTime;
        }

        public void FinalizeRun(GameContext context)
        {
            if (context.Score > _bestScore)
            {
                _bestScore = context.Score;
                UnityEngine.PlayerPrefs.SetFloat(BestScoreKey, _bestScore);
                UnityEngine.PlayerPrefs.Save();
            }
        }
    }
}
