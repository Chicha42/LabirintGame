using System.Diagnostics;

namespace LabirintGame.Model
{
    public class Player : Pos
    {
        public int Health { get; set; }
        private static Maze _maze;
        public readonly List<Key> CollectedKeys;
        public float DrawX { get; private set; }
        public float DrawY { get; private set; }
        private bool _isMoving;
        public bool IsMoving => _isMoving;
        private float _moveProgress;
        private const float MoveDuration = 0.3f;
        private (int fromX, int fromY, int toX, int toY) _moveData;
        private float TimeStationary { get; set; }
        private float RegenTimer { get; set; }
        
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
            if (_maze.Grid[Y, X] >= 7 && _maze.Grid[Y, X] <= 10)
            {
                CollectedKeys.Add(new Key(_maze.Grid[Y, X], 
                    _maze.KeyColors[_maze.Grid[Y, X]-7], X, Y));
                _maze.Grid[Y, X] = 1;
            }

            if (_maze.Grid[Y + dy, X + dx] >= 17 && _maze.Grid[Y + dy, X + dx] <= 20)
            {
                foreach (var t in CollectedKeys.Where(t => t.Id + 10 == _maze.Grid[Y + dy, X + dx]).ToList())
                {
                    CollectedKeys.Remove(t);
                    _maze.Grid[Y + dy, X + dx] = 1;
                }
                    
            }
        }

        private bool CanMoveTo(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _maze.Width || y >= _maze.Height)
                return false;
            
            var cell = _maze.Grid[y, x];
            return cell != 0 && cell != 17 && cell != 18 && cell != 19 && cell != 20; 
        }
        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < _maze.Width && y < _maze.Height;
        }

        public void Update(float deltaTime)
        {
            if (!_isMoving)
            {
                TimeStationary += deltaTime;

                if (TimeStationary > 1.5f)
                {
                    RegenTimer += deltaTime;

                    if (RegenTimer >= 0.5f)
                    {
                        if (Health < 100)
                            Health = Math.Min(100, Health + 5);
            
                        RegenTimer = 0f;
                    }
                }
                else
                {
                    RegenTimer = 0f;
                }
                return;
            }

            TimeStationary = 0f;
            RegenTimer = 0f;


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