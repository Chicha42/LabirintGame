using System.Drawing.Drawing2D;
using LabirintGame.Model;
using LabirintGame.Controller;
using Timer = System.Windows.Forms.Timer;

namespace LabirintGame.View
{
    public sealed partial class Level2 : Form, IGameView
    {
        private GameController _controller;
        private const int CellSize = 200;
        private readonly Timer _timer = new();
        private bool _wPressed, _aPressed, _sPressed, _dPressed, _escPressed;
        private Bitmap _wall, _floor, _blueKey;
        private readonly Dictionary<TileType, Bitmap> _tileTextures = new();
        public Form GetForm() => this;
        private float _cameraX, _cameraY;
        private const float CameraSpeed = 0.1f;
        private Bitmap _miniMapBackgroundTexture;
        private bool[,] _visitedCells;
        
        //Анимация главного героя
        private Bitmap _playerSpriteSheet;
        private Bitmap _miniMapPlayerTexture;
        private const int SpriteSize = 32;
        private const int FramesPerDirection = 4;
        private int _animationFrame;
        private int _animationTick;
        private const int FrameChangeRate = 10;
        private bool _isPlayerMoving;
        private int _playerDirection;
        
        //Анимация врага
        private Bitmap _enemySpriteSheet;
        private Bitmap _enemyAttackSpriteSheet;
        private const int EnemySpriteSize = 64;
        private const int EnemyFramesPerDirection = 6;
        private int _enemyAnimationFrame;
        private int _enemyAnimationTick;
        private const int EnemyFrameChangeRate = 10;
        private const int EnemyAttackFrameChangeRate = 4;

        
        void IGameView.Invalidate() => Invalidate();
        void IGameView.BeginInvoke(Action action) => BeginInvoke(action);
        
        public Level2()
        {   
            InitializeComponent();
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            Resize += (_, _) => Invalidate();
            LoadTextures();
            
            InitializeGame();
            
            _visitedCells = new bool[_controller.GetMazeHeight(), _controller.GetMazeWidth()];

            var (playerX, playerY) = _controller.GetPlayerDrawPosition();
            _cameraX = playerX;
            _cameraY = playerY;
            
            Paint += MainForm_Paint;
            
            _timer.Interval = 10;
            _timer.Tick += (_, _) =>
            {
                int dx = 0, dy = 0;
                if (_wPressed)
                {
                    dy = 1;
                    _playerDirection = 1;
                }

                if (_sPressed)
                {
                    dy = -1;
                    _playerDirection = 0;
                }

                if (_aPressed)
                {
                    dx = 1;
                    _playerDirection = 3;
                }

                if (_dPressed)
                {
                    dx = -1;
                    _playerDirection = 2;
                }
                if (_escPressed) Application.Exit();

                _controller.MovePlayer(dx, dy);
                
                _isPlayerMoving = _controller.IsPlayerMoving();
                
                var (drawX, drawY) = _controller.GetPlayerDrawPosition();
                _cameraX += (drawX - _cameraX) * CameraSpeed;
                _cameraY += (drawY - _cameraY) * CameraSpeed;
                
                _controller.Update();
                Invalidate();
            };  
            _timer.Start();
        }
        
        private void InitializeGame()
        {
            _controller = new GameController(this, 1, 7,21,21,1, 10)
            {
                _onWin = () =>
                {
                    var nextForm = new Level3();
                    nextForm.Show();
                    Close();
                },

                RestartGame = () =>
                {
                    BeginInvoke((Action)(() =>
                    {
                        Controls.Clear();
                        InitializeGame();
                    }));
                }
            };
        }

        private void LoadTextures()
        {
            _tileTextures[TileType.Wall] = LoadTexture("Assets/wall.png");
            _tileTextures[TileType.Floor] = LoadTexture("Assets/floor.png");
            _tileTextures[TileType.KeyBlue] = LoadTexture("Assets/BlueKey.png");
            _tileTextures[TileType.KeyGreen] = LoadTexture("Assets/GreenKey.png");
            _tileTextures[TileType.KeyRed] = LoadTexture("Assets/RedKey.png");
            _tileTextures[TileType.DoorGreen] = LoadTexture("Assets/GreenDoor.png");
            _tileTextures[TileType.DoorRed] = LoadTexture("Assets/RedDoor.png");
            _tileTextures[TileType.DoorBlue] = LoadTexture("Assets/BlueDoor.png");
            _tileTextures[TileType.Finish] = LoadTexture("Assets/Finish.png");
            _playerSpriteSheet = new Bitmap("Assets/PlayerAnim.png");
            _enemySpriteSheet = new Bitmap("Assets/OrcWalk.png");
            _enemyAttackSpriteSheet = new Bitmap("Assets/OrcAttack.png");
            _miniMapPlayerTexture = LoadTexture("Assets/MiniMapPlayer.png");
            _miniMapBackgroundTexture = LoadTexture("Assets/MiniMap.png");
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: _wPressed = true; break;
                case Keys.S: _sPressed = true; break;
                case Keys.A: _aPressed = true; break;
                case Keys.D: _dPressed = true; break;
                case Keys.Escape: _escPressed = true; break;
            }
            base.OnKeyDown(e);
        }
        
        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: _wPressed = false; break;
                case Keys.S: _sPressed = false; break;
                case Keys.A: _aPressed = false; break;
                case Keys.D: _dPressed = false; break;
            }
            base.OnKeyUp(e);
        }

        private void MainForm_Paint(object? sender, PaintEventArgs e)
        {
            var maze = _controller.GetMaze();
            var g = e.Graphics;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            var sideBrush = new SolidBrush(Color.FromArgb(25, 25, 25));
            g.Clear(sideBrush.Color);
            
            var centerX = ClientSize.Width / 2;
            var centerY = ClientSize.Height / 2;
            
            DrawMaze(maze, centerX, centerY, g);

            DrawPlayer(centerX, centerY, g);

            DrawEnemy(g);
            
            DrawBorders(g, sideBrush);

            DrawHealthBar(g);

            DrawCollectedKeys(g);
            
            UpdateVisitedCells();
            
            DrawMiniMap(g);
        }

        private void DrawMaze(Maze maze, int centerX, int centerY, Graphics g)
        {
            var (startX, startY, endX, endY) = GetVisibleBounds(maze);

            for (var y = startY; y <= endY; y++)
            {
                for (var x = startX; x <= endX; x++)
                {
                    var screenX = centerX - (x - _cameraX) * CellSize;
                    var screenY = centerY - (y - _cameraY) * CellSize;

                    if (x >= 0 && x < maze.Width && y >= 0 && y < maze.Height)
                    {
                        var tileCode = maze.Grid[y, x];
                        var tileType = GetTileType(tileCode);

                        if (!_tileTextures.TryGetValue(tileType, out var texture))
                            texture = _tileTextures[TileType.Floor];

                        g.DrawImage(texture,
                            screenX - CellSize / 2f,
                            screenY - CellSize / 2f,
                            CellSize, CellSize);
                    }
                }
            }
        }

        private void DrawPlayer(int centerX, int centerY, Graphics g)
        {
            if (_isPlayerMoving)
            {
                _animationTick++;
                if (_animationTick >= FrameChangeRate)
                {
                    _animationTick = 0;
                    _animationFrame = (_animationFrame + 1) % FramesPerDirection;
                }
            }
            else
            {
                _animationFrame = 0;
            }

            var srcRect = new Rectangle(
                _animationFrame * SpriteSize,
                _playerDirection * SpriteSize,
                SpriteSize, SpriteSize);

            var destRect = new RectangleF(
                centerX - CellSize / 3f,
                centerY - CellSize / 3f,
                CellSize / 1.5f,
                CellSize / 1.5f);

            g.DrawImage(_playerSpriteSheet, destRect, srcRect, GraphicsUnit.Pixel);
        }

        private void DrawBorders(Graphics g, SolidBrush sideBrush)
        {
            g.FillRectangle(sideBrush,
                0f,
                0f,
                CellSize*2.3f,
                ClientSize.Height);
            g.FillRectangle(sideBrush,
                ClientSize.Width-(CellSize*2.3f),
                0f,
                CellSize*2.3f,
                ClientSize.Height);
        }

        private void DrawEnemy(Graphics g)
        {
            _enemyAnimationTick++;
            foreach (var en in _controller.GetEnemies())
            {
                var enemyDirection = (int)en.Direction;
                float x = Width / 2;
                float y = Height / 2;
                var (cameraX, cameraY) = (_cameraX, _cameraY);
                var (playerX, playerY) = _controller.GetPlayerPosition();
                var sx = x - (en.DrawX - cameraX) * CellSize;
                var sy = y - (en.DrawY - cameraY) * CellSize;
                var isAttacking = Math.Abs(en.X - playerX) + 
                    Math.Abs(en.Y - playerY) <= 1;
                
                var spriteSheet = isAttacking ? _enemyAttackSpriteSheet : _enemySpriteSheet;
                var frameRate = isAttacking ? EnemyAttackFrameChangeRate : EnemyFrameChangeRate;
                
                if (_enemyAnimationTick >= frameRate)
                {
                    _enemyAnimationTick = 0;
                    _enemyAnimationFrame = (_enemyAnimationFrame + 1) % EnemyFramesPerDirection;
                }
                
                var srcRectE = new Rectangle(
                    _enemyAnimationFrame * EnemySpriteSize,
                    enemyDirection * EnemySpriteSize,
                    EnemySpriteSize, EnemySpriteSize);

                var destRectE = new RectangleF(
                    sx - CellSize * 0.75f,
                    sy - CellSize * 0.6f,
                    CellSize*1.5f, CellSize*1.5f);

                g.DrawImage(spriteSheet, destRectE, srcRectE, GraphicsUnit.Pixel);
            }
        }

        private void DrawHealthBar(Graphics g)
        {
            var barWidth = 400;
            var barHeight = 45;
            var margin = 30;
            
            var healthBarX = margin;
            var healthBarY = ClientSize.Height/2 - barHeight/2;

            var healthRatio = Math.Clamp(_controller.GetPlayerHealth() / 100f, 0f, 1f);

            g.FillRectangle(Brushes.Gray, healthBarX, healthBarY, barWidth, barHeight);

            using (var healthBrush = new SolidBrush(Color.FromArgb(139, 0, 0)))
            {
                g.FillRectangle(healthBrush, healthBarX, healthBarY, barWidth * healthRatio, barHeight);
            }

            g.DrawRectangle(Pens.Black, healthBarX, healthBarY, barWidth, barHeight);
        }
        
        private void DrawCollectedKeys(Graphics g)
        {
            var panelWidth = 400;
            var panelHeight = 150;
            var margin = 20;
            var xPos = ClientSize.Width - panelWidth - margin;
            var yPos = ClientSize.Height/2 - panelHeight/2;
        
            using (var panelBrush = new SolidBrush(Color.FromArgb(150, 50, 50, 50)))
            {
                g.FillRectangle(panelBrush, xPos, yPos, panelWidth, panelHeight);
                g.DrawRectangle(Pens.Gold, xPos, yPos, panelWidth, panelHeight);
            }
        
            var keySize = 80;
            var spacing = 20;
            var startX = xPos + spacing;
            var startY = yPos + (panelHeight - keySize) / 2;

            foreach (var key in _controller.GetCollectedKeys())
            {
                var keyTexture = GetKeyTexture(key.Id);
                g.DrawImage(keyTexture, startX, startY, keySize, keySize);
                startX += keySize + spacing;
            }
        }
        
        private void DrawMiniMap(Graphics g)
        {
            const int miniMapCellSize = 15;
            const int marginLeft = 80;
            const int marginTop = 80;

            var maze = _controller.GetMaze();

            var mapWidth = maze.Width * miniMapCellSize;
            var mapHeight = maze.Height * miniMapCellSize;

            var startX = marginLeft + mapWidth;
            var startY = marginTop + mapHeight;

            if (_miniMapBackgroundTexture != null)
            {
                var backgroundWidth = _miniMapBackgroundTexture.Width;
                var backgroundHeight = _miniMapBackgroundTexture.Height;

                var backgroundX = marginLeft + (mapWidth - backgroundWidth) / 2;
                var backgroundY = marginTop - backgroundHeight - 10;

                g.DrawImage(_miniMapBackgroundTexture, backgroundX, backgroundY, backgroundWidth, backgroundHeight);
            }

            for (var y = 0; y < maze.Height; y++)
            {
                for (var x = 0; x < maze.Width; x++)
                {
                    if (_tileTextures.TryGetValue(TileType.Wall, out var wallTexture))
                    {
                        g.DrawImage(wallTexture,
                            startX - (x + 1) * miniMapCellSize,
                            startY - (y + 1) * miniMapCellSize,
                            miniMapCellSize,
                            miniMapCellSize);
                    }

                    if (_visitedCells[y, x])
                    {
                        var tileCode = maze.Grid[y, x];
                        var tileType = GetTileType(tileCode);

                        if (_tileTextures.TryGetValue(tileType, out var texture))
                        {
                            g.DrawImage(texture,
                                startX - (x + 1) * miniMapCellSize,
                                startY - (y + 1) * miniMapCellSize,
                                miniMapCellSize,
                                miniMapCellSize);
                        }
                    }
                }
            }

            var (playerX, playerY) = _controller.GetPlayerPosition();
            g.DrawImage(_miniMapPlayerTexture,
                startX - (playerX + 1) * miniMapCellSize,
                startY - (playerY + 1) * miniMapCellSize,
                miniMapCellSize,
                miniMapCellSize);

            g.DrawRectangle(new Pen(Color.FromArgb(42, 44, 64), 5), marginLeft, marginTop, mapWidth, mapHeight);
        }
        
        private void UpdateVisitedCells()
        {
            var (playerX, playerY) = _controller.GetPlayerPosition();

            if (playerX >= 0 && playerX < _visitedCells.GetLength(1) &&
                playerY >= 0 && playerY < _visitedCells.GetLength(0))
            {
                _visitedCells[playerY, playerX] = true;
            }
            
            int offsetX = 0, offsetY = 0;
            switch (_playerDirection)
            {
                case 0: offsetY = -1; break;
                case 1: offsetY = 1; break;
                case 2: offsetX = -1; break;
                case 3: offsetX = 1; break;
            }

            var visibleX = playerX + offsetX;
            var visibleY = playerY + offsetY;

            if (visibleX >= 0 && visibleX < _visitedCells.GetLength(1) &&
                visibleY >= 0 && visibleY < _visitedCells.GetLength(0))
            {
                _visitedCells[visibleY, visibleX] = true;
            }
        }

        private Bitmap GetKeyTexture(int keyId)
        {
            return keyId switch
            {
                8 => LoadTexture("Assets/InvGreenKey.png"),
                9 => LoadTexture("Assets/InvRedKey.png"),
                10 => LoadTexture("Assets/InvBlueKey.png"),
                _ => _tileTextures[TileType.Floor]
            };
        }

        private (int startX, int startY, int endX, int endY) GetVisibleBounds(Maze maze)
        {
            var visibleCellsX = (int)Math.Ceiling(ClientSize.Width / (float)CellSize) + 2;
            var visibleCellsY = (int)Math.Ceiling(ClientSize.Height / (float)CellSize) + 2;

            var (cameraX, cameraY) = (_cameraX, _cameraY);
            
            var startX = (int)Math.Floor(cameraX - visibleCellsX / 2f);
            var startY = (int)Math.Floor(cameraY - visibleCellsY / 2f);
            var endX = startX + visibleCellsX;
            var endY = startY + visibleCellsY;
            
            return (Math.Max(0, startX),
                Math.Max(0, startY), 
                Math.Min(maze.Width - 1, endX),
                Math.Min(maze.Height - 1, endY));
        }
        
        private static TileType GetTileType(int code)
        {
            return code switch
            {
                0 => TileType.Wall,
                1 => TileType.Floor,
                3 => TileType.Finish,
                8 => TileType.KeyGreen,
                9 => TileType.KeyRed,
                10 => TileType.KeyBlue,
                18 => TileType.DoorGreen,
                19 => TileType.DoorRed,
                20 => TileType.DoorBlue,
                _ => TileType.Floor
            };
        }
        
        private Bitmap LoadTexture(string path)
        {
            var original = new Bitmap(path);
            
            var resized = new Bitmap(original.Width, original.Height);

            using var g = Graphics.FromImage(resized);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.DrawImage(original, 0, 0, resized.Width, resized.Height);

            return resized;
        }
    }
}