namespace RusRunner.Core.Contracts
{
    public interface IAbility
    {
        string Id { get; }
        bool CanActivate(GameContext context);
        void Activate(GameContext context);
        void Tick(GameContext context, float deltaTime);
        void Deactivate(GameContext context);
    }
}
