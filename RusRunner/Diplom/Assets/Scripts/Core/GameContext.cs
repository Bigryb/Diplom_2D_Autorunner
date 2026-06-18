namespace RusRunner.Core
{
    public sealed class GameContext
    {
        public float CurrentSpeed { get; set; }
        public float DistanceMeters { get; set; }
        public float Score { get; set; }
        public bool IsGrounded { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool IsInvulnerable { get; set; }
        public float SpeedMultiplier { get; set; } = 1f;
        public float ScoreMultiplier { get; set; } = 1f;
        public int AirJumpsLeft { get; set; }

        public void Reset()
        {
            CurrentSpeed = 0f;
            DistanceMeters = 0f;
            Score = 0f;
            IsGrounded = true;
            IsAlive = true;
            IsInvulnerable = false;
            SpeedMultiplier = 1f;
            ScoreMultiplier = 1f;
            AirJumpsLeft = 0;
        }
    }
}
