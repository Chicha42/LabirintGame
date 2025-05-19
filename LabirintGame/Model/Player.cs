using System.Diagnostics;

namespace LabirintGame.Model
{
    public class Player : Pos
    {
        public int Health { get; set; }
        private static Maze _maze;
        private readonly List<Key> CollectedKeys;
        public float DrawX { get; private set; }
        public float DrawY { get; private set; }
        private bool _isMoving;
        private float _moveProgress;
        private const float MoveDuration = 0.3f;
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
            
            var canMoveX = CanMoveTo(X + deltaX, Y);
            var canMoveY = CanMoveTo(X, Y + deltaY);
            
            if (deltaX != 0 && deltaY != 0)
            {
                if (!canMoveX && !canMoveY) return;
                if (!canMoveX) deltaX = 0;
                if (!canMoveY) deltaY = 0;
            }

            if (deltaX != 0 || deltaY != 0)
            {
                var newX = X + deltaX;
                var newY = Y + deltaY;

                if(!InBounds(newX,newY)) return;
                
                if (CanMoveTo(newX, newY))
                {
                    _moveData = (X, Y, newX, newY);
                    _isMoving = true;
                    _moveProgress = 0f;
                }
                CheckForCollectables(deltaY, deltaX);
            }
            
        }

        private void CheckForCollectables(int dy, int dx)
        {
            if (_maze.Grid[Y, X] == 10 ||
                _maze.Grid[Y, X] == 9 ||
                _maze.Grid[Y, X] == 8)
            {
                CollectedKeys.Add(new Key(_maze.Grid[Y, X], 
                    _maze.KeyColors[_maze.Grid[Y, X]-8], X, Y));
                _maze.Grid[Y, X] = 1;
            }

            if (_maze.Grid[Y + dy, X + dx] == 20 ||
                _maze.Grid[Y + dy, X + dx] == 19 ||
                _maze.Grid[Y + dy, X + dx] == 18)
            {
                foreach (var t in CollectedKeys.Where(t => t.Id +10 ==  _maze.Grid[Y + dy, X + dx]))
                    _maze.Grid[Y + dy, X + dx] = 1;
            }
        }

        private bool CanMoveTo(int x, int y)
        {
            return x >= 0 && y >= 0 &&
                x < _maze.Width-1 && y < _maze.Height-1 &&
                _maze.Grid[y, x] != 0 &&
                _maze.Grid[y, x] != 20 &&
                _maze.Grid[y, x] != 19 &&
                _maze.Grid[y, x] != 18;
        }
        
        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 &&
                   x < _maze.Width - 1 && y < _maze.Height - 1;
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