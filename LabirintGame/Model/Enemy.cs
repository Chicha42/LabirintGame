namespace LabirintGame.Model
{
    public enum EnemyState { Wandering, Chasing }
    
    public enum Direction
    {
        Up = 0,
        Down = 1,
        Right = 2,
        Left = 3
    }
    public class Enemy : Pos
    {
        private readonly Maze _maze;
        public int Health { get; private set; }
        public int Damage { get; private set; }
        
        public float DrawX { get; private set; }
        public float DrawY { get; private set; }
        
        private bool _isMoving;
        private float _moveProgress;
        private const float MoveDuration = 0.35f;
        private (int fromX, int fromY, int toX, int toY) _moveData;
        public EnemyState CurrentState { get; set; }
        public bool CanDealDamage { get; set; } = true;
        public Direction Direction { get; set; }
        
        public Enemy(Maze maze, int health, int damage, int x, int y) : base(x, y)
        {
            _maze = maze;
            Health = health;
            Damage = damage;
            DrawX = x; DrawY = y;
            _isMoving = false;
            CurrentState = EnemyState.Wandering;
        }
        
        public bool IsMoving => _isMoving;
        
        public void Move(int dx, int dy)
        {
            if (_isMoving) return;
            if (!CanMoveTo(X + dx, Y + dy)) return;

            _isMoving = true;
            _moveProgress = 0f;
            _moveData = (X, Y, X + dx, Y + dy);
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
        
        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}