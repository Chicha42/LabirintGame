using LabirintGame.Model;
using LabirintGame.View;

namespace LabirintGame.Controller
{
    public class GameController
    {
        private readonly GameModel _model;
        private readonly MainForm _view;
        private DateTime _lastUpdateTime;
        
        // Позиция камеры (может быть дробной)
        public float CameraX { get; private set; }
        public float CameraY { get; private set; }
        private const float CameraFollowSpeed = 5f; // Скорость следования камеры

        public GameController(MainForm view)
        {
            _view = view;
            _model = new GameModel(21, 21, 4);
            _lastUpdateTime = DateTime.Now;
            
            // Инициализируем камеру на позиции игрока
            CameraX = _model.Player.DrawX;
            CameraY = _model.Player.DrawY;
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
        
        public void Update()
        {
            var now = DateTime.Now;
            float deltaTime = (float)(now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;
            
            // Обновляем позицию игрока
            _model.Player.Update(deltaTime);
            
            // Плавно перемещаем камеру к игроку
            CameraX = Lerp(CameraX, _model.Player.DrawX, CameraFollowSpeed * deltaTime);
            CameraY = Lerp(CameraY, _model.Player.DrawY, CameraFollowSpeed * deltaTime);
            
            _view.Invalidate();
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0, 1);
        }
        
        

    }
}