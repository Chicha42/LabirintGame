using System.Diagnostics;

namespace LabirintGame.Model
{
    public class Player : Pos
    {
        public int Health { get; set; }
        private readonly Maze _maze;
        private readonly List<Key> CollectedKeys;
        public float DrawX { get; private set; }
        public float DrawY { get; private set; }
        private bool _isMoving;
        private float _moveProgress;
        private const float MoveDuration = 0.1f;
        private (int fromX, int fromY, int toX, int toY) _moveData;
        

        public Player(Maze maze, int startX, int startY, List<Key> collectedKeys) : base(startX, startY)
        {
            CollectedKeys = collectedKeys;
            _maze = maze;
            Health = 100;
            DrawX = startX;
            DrawY = startY;
            _isMoving = false;
        }

        public void Move(int deltaX, int deltaY)
        {
            if (_isMoving) return;
            
            int newX = X + deltaX;
            int newY = Y + deltaY;

            if (CanMoveTo(newX, newY))
            {
                _moveData = (X, Y, newX, newY);
                _isMoving = true;
                _moveProgress = 0f;
            }
            CheckForCollectables(newY, newX);
        }

        private void CheckForCollectables(int newY, int newX)
        {
            if (_maze.Grid[newY, newX] == 10 ||
                _maze.Grid[newY, newX] == 9 ||
                _maze.Grid[newY, newX] == 8)
            {
                CollectedKeys.Add(new Key(_maze.Grid[newY, newX], 
                    _maze.KeyColors[_maze.Grid[newY, newX]-8], newX, newY));
                _maze.Grid[newY, newX] = 1;
            }

            if (_maze.Grid[newY, newX] == 20 ||
                _maze.Grid[newY, newX] == 19 ||
                _maze.Grid[newY, newX] == 18)
            {
                for (var i = 0;i < CollectedKeys.Count; i++)
                    if (CollectedKeys[i].Id +10 ==  _maze.Grid[newY, newX])
                        _maze.Grid[newY, newX] = 1;
            }
        }

        private bool CanMoveTo(int x, int y)
        {
            return x >= 0 && y >= 0 &&
                x < _maze.Width && y < _maze.Height &&
                _maze.Grid[y, x] != 0 &&
                _maze.Grid[y, x] != 20 &&
                _maze.Grid[y, x] != 19 &&
                _maze.Grid[y, x] != 18;
        }

        public void Update(float deltaTime)
        {
            if (!_isMoving) return;
            
            _moveProgress += deltaTime / MoveDuration;
            
            if (_moveProgress >= 1f)
            {
                _moveProgress = 1f;
                _isMoving = false;
                X = _moveData.toX;
                Y = _moveData.toY;
            }

            DrawX = Lerp(_moveData.fromX, _moveData.toX, _moveProgress);
            DrawY = Lerp(_moveData.fromY, _moveData.toY, _moveProgress);
        }
        
        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}