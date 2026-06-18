namespace RusRunner.Core.Contracts
{
    public interface IPowerUp
    {
        string Id { get; }
        float DurationSeconds { get; }
        void Apply(GameContext context);
        void Expire(GameContext context);
    }
}
