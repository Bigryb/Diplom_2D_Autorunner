namespace RusRunner.Game.Input
{
    public interface IInputSource
    {
        bool JumpPressed();
        bool SlidePressed();
        bool DashPressed();
    }
}
