namespace LabirintGame.View;

public interface IGameView
{
    void Invalidate();
    void BeginInvoke(Action action);
}