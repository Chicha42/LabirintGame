using LabirintGame.Model;
using LabirintGame.View;
using System;
using System.Security.Cryptography.Xml;
using Timer = System.Windows.Forms.Timer;

namespace LabirintGame.Controller
{
    public class GameController
    {
        private readonly GameModel _model;
        private readonly MainForm _viewMain;
        private readonly Training _viewTraining;
        private DateTime _lastUpdateTime;
        public float CameraX { get; private set; }
        public float CameraY { get; private set; }
        private const float CameraFollowSpeed = 5f;
        private bool _isGameOver;
        public Maze Maze => _model.Maze;
        public Player Player => _model.Player;
        public IReadOnlyList<Enemy> Enemies => _model.Enemies;

        public GameController(MainForm view, int enemyCount)
        {
            _viewMain = view;
            _model = new GameModel(21, 21, 4);
            _lastUpdateTime = DateTime.Now;
            
            CameraX = _model.Player.DrawX;
            CameraY = _model.Player.DrawY;

            SpawnEnemies(enemyCount);
        }

        public GameController(Training view, int enemyCount)
        {
            _viewTraining = view;
            _model = new GameModel(21, 21, 4);
            _lastUpdateTime = DateTime.Now;
            
            CameraX = _model.Player.DrawX;
            CameraY = _model.Player.DrawY;

            SpawnEnemies(enemyCount);
        }

        private void SpawnEnemies(int enemyCount)
        {
            var rnd = _model.Maze.Random;
            var empty = _model.Maze.GetEmptyCells();
            
            for (var i = 0; i < enemyCount; i++)
            {
                var (x, y) = empty[rnd.Next(empty.Count)];
                _model.Enemies.Add(new Enemy(_model.Maze, health:50, damage:5, x, y));
            }
        }

        public void MovePlayer(int dx, int dy)
        {
            if (_isGameOver) return;

            _model.Player.Move(dx, dy);

            if (!_isGameOver && 
                _model.Player.X == _model.Maze.Width - 2 &&
                _model.Player.Y == _model.Maze.Height - 2)
            {
                _isGameOver = true;
                MessageBox.Show("Вы победили!");
                _viewMain.BeginInvoke(new Action(() => Application.Exit()));
            }

            _viewMain.Invalidate();
        }

        public void Update()
        {
            if (_isGameOver) return;
            var now = DateTime.Now;
            var dt = (float)(now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;

            _model.Player.Update(dt);
            UpdateEnemies(dt);
            UpdateCamera(dt);
            
            _viewMain.Invalidate();
        }

        private void UpdateCamera(float dt)
        {
            const float speed = 5f;
            CameraX = Lerp(CameraX, Player.DrawX, speed * dt);
            CameraY = Lerp(CameraY, Player.DrawY, speed * dt);
        }

        private void UpdateEnemies(float dt)
        {
            foreach (var enemy in _model.Enemies)
            {
                enemy.Update(dt);

                if (!enemy.IsMoving)
                {
                    var canSee = CanSeePlayer(enemy);
                    enemy.CurrentState = canSee
                        ? EnemyState.Chasing
                        : (enemy.CurrentState == EnemyState.Chasing ? EnemyState.Wandering : enemy.CurrentState);

                    (int dx, int dy) move = (0, 0);
                    
                    if (enemy.CurrentState == EnemyState.Chasing)
                    {
                        var path = FindPath(enemy.X, enemy.Y, Player.X, Player.Y);

                        if (path.Count > 2)
                        {
                            move = (path[1].x - enemy.X, path[1].y - enemy.Y);
                            UpdateDirection(enemy, move.dx, move.dy);
                        }
                    }
                    else
                    {
                            var rnd = new Random();
                            var dirs = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }
                                .OrderBy(_ => rnd.Next()).ToArray();
                            foreach (var (dx0,dy0) in dirs)
                                if (_model.Maze.Grid[enemy.Y + dy0, enemy.X + dx0] > 0)
                                {
                                    UpdateDirection(enemy, dx0, dy0);
                                    move = (dx0,dy0);
                                    break;
                                }
                    }

                    if (move != (0,0)) enemy.Move(move.dx, move.dy);
                }

                var isClose = Math.Abs(enemy.X - Player.X) + Math.Abs(enemy.Y - Player.Y) <= 1;

                if (isClose && enemy.CanDealDamage)
                {
                    Player.Health -= enemy.Damage;
                    enemy.CanDealDamage = false;

                    var cooldownTimer = new Timer();
                    cooldownTimer.Interval = 500;

                    cooldownTimer.Tick += (s, args) =>
                    {
                        enemy.CanDealDamage = true;
                        cooldownTimer.Stop();
                        cooldownTimer.Dispose();
                    };
                    cooldownTimer.Start();

                    if (Player.Health <= 0)
                    {
                        _isGameOver = true;
                        MessageBox.Show("Game Over!");
                        _viewMain.BeginInvoke((Action)(() => Application.Exit()));
                    }
                }

            }
        }
        
        private bool CanSeePlayer(Enemy e)
        {
            if (e.X == Player.X)
            {
                var dir = Math.Sign(Player.Y - e.Y);
                for (var y = e.Y + dir; y != Player.Y; y += dir)
                    if (_model.Maze.Grid[y,e.X] == 0) return false;
                return true;
            }
            if (e.Y == Player.Y)
            {
                var dir = Math.Sign(Player.X - e.X);
                for (var x = e.X + dir; x != Player.X; x += dir)
                    if (_model.Maze.Grid[e.Y,x] == 0) return false;
                return true;
            }
            return false;
        }

        private List<(int x, int y)> FindPath(int sx, int sy, int tx, int ty)
        {
            var maze = _model.Maze;
            int w = maze.Width, h = maze.Height;
            var visited = new bool[h, w];
            var prev = new (int, int)[h, w];
            var q = new Queue<(int, int)>();
            q.Enqueue((sx, sy));
            visited[sy, sx] = true;

            var dirs = new (int dx, int dy)[] { (1,0), (-1,0), (0,1), (0,-1) };
            var found = false;
            while (q.Count > 0)
            {
                var (x, y) = q.Dequeue();
                if (x == tx && y == ty) { found = true; break; }
                foreach (var (dx, dy) in dirs)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h
                        && !visited[ny, nx] && maze.Grid[ny, nx] > 0)
                    {
                        visited[ny, nx] = true;
                        prev[ny, nx] = (x, y);
                        q.Enqueue((nx, ny));
                    }
                }
            }

            var path = new List<(int x, int y)>();
            if (!found) return path;
            var cur = (tx, ty);
            while (cur != (sx, sy))
            {
                path.Add(cur);
                cur = prev[cur.Item2, cur.Item1];
            }
            path.Add((sx, sy));
            path.Reverse();
            return path;
        }
        
        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Clamp(t, 0, 1);
        }
        
        private void UpdateDirection(Enemy enemy, int dx, int dy)
        {
            if (dx == 1) enemy.Direction = Direction.Right;
            else if (dx == -1) enemy.Direction = Direction.Left;
            else if (dy == 1) enemy.Direction = Direction.Down;
            else if (dy == -1) enemy.Direction = Direction.Up;
        }

    }
}