using LabirintGame.Model;
using LabirintGame.View;

namespace LabirintGame.Controller
{
    public class GameController
    {
        private readonly GameModel _model;
        private readonly MainForm _view;

        public GameController(MainForm view)
        {
            _view = view;
            _model = new GameModel(21, 21, 4);
        }

        public Maze Maze => _model.Maze;
        public Player Player => _model.Player;

        public void MovePlayer(int dx, int dy)
        {
            _model.Player.Move(dx, dy);

            if (_model.Player.X == _model.Maze.Width - 2 &&
                _model.Player.Y == _model.Maze.Height - 2)
            {
                System.Windows.Forms.MessageBox.Show("Вы победили!");
                System.Windows.Forms.Application.Exit();
            }

            _view.Invalidate();
        }
    }
}