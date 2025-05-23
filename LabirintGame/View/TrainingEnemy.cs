using System.Drawing.Drawing2D;
using System.Drawing.Text;
using LabirintGame.Model;
using LabirintGame.Controller;
using Timer = System.Windows.Forms.Timer;
namespace LabirintGame.View;

public sealed partial class TrainingEnemy : Form, IGameView
{
    private readonly GameController _controller;
    private const int CellSize = 200;
    private readonly Timer _timer = new Timer();
    private bool _wPressed, _aPressed, _sPressed, _dPressed, _escPressed;
    private Bitmap _wall, _floor, _blueKey;
    private readonly Dictionary<TileType, Bitmap> _tileTextures = new();
    public Form GetForm() => this;
        
    //Анимация главного героя
    private Bitmap _playerSpriteSheet;
    private const int SpriteSize = 32;
    private const int FramesPerDirection = 4;
    private int _animationFrame;
    private int _animationTick;
    private const int FrameChangeRate = 10;
    private bool _isPlayerMoving = false;
    private int _playerDirection;
        
    //Анимация врага
    private Bitmap _enemySpriteSheet;
    private const int EnemySpriteSize = 64;
    private const int EnemyFramesPerDirection = 6;
    private int _enemyAnimationFrame;
    private int _enemyAnimationTick;
    private const int EnemyFrameChangeRate = 10;
    
    //Для показа сообщений обучения
    private readonly Timer _tutorialTimer = new();
    private readonly int _tutorialDisplayTime = 5000;
    private bool _isMessageShown;
    private readonly Dictionary<string, Bitmap> _tutorialImages = new();
    private Bitmap _currentTutorialImage;
    
    void IGameView.Invalidate() => Invalidate();
    void IGameView.BeginInvoke(Action action) => BeginInvoke(action);
    public TrainingEnemy()
    {
        InitializeComponent();
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Resize += (_, _) => Invalidate();
        LoadTextures();
        
        _tutorialTimer.Interval = _tutorialDisplayTime;
        _tutorialTimer.Tick += (s, e) => { 
            HideTutorialMessage();
            _tutorialTimer.Stop();
        };
        
        ShowTutorialImage("enemyTutor1");
        
        var trainingGrid = new[,]
        {
            {0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,1,0,1,1,1,0,1,1,1,1,1,0},
            {0,1,0,1,0,1,0,1,0,1,0,1,0},
            {0,1,1,1,0,1,1,1,0,1,0,1,0},
            {0,1,0,0,0,1,0,0,0,0,0,1,0},
            {0,1,1,1,0,1,1,1,1,0,0,1,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0}
        };

        _controller = new GameController(this, trainingGrid, 0)
        {
            _onWin = () =>
            {
                {
                    var nextForm = new TrainingEnemy();
                    nextForm.Show();
                    Close();
                }
            }
        };
        
        Paint += Training_Paint;
        
        _timer.Interval = 10;
        _timer.Tick += (_, _) =>
        {
            if (_isMessageShown) return;
            
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

            _isPlayerMoving = _controller.Player.IsMoving;
                
            _controller.Update();
            Invalidate();
        };  
        _timer.Start();
    }

    private void ShowTutorialImage(string imageKey)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowTutorialImage(imageKey)));
            return;
        }

        if (_tutorialImages.TryGetValue(imageKey, out var image))
        {
            _currentTutorialImage = image;
            _isMessageShown = true;
            _tutorialTimer.Stop();
            _tutorialTimer.Start();
            Invalidate();
        }
    }

    private void HideTutorialMessage()
    {
        if (_currentTutorialImage == _tutorialImages["enemyTutor1"])
        {
            _currentTutorialImage = _tutorialImages["enemyTutor2"];
        }
        else if (_currentTutorialImage == _tutorialImages["enemyTutor2"])
        {
            _currentTutorialImage = _tutorialImages["enemyTutor3"];
        }
        else
        {
            _currentTutorialImage = null;
            _isMessageShown = false;
        }
        Invalidate();
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
        _playerSpriteSheet = new Bitmap("Assets/PlayerAnim.png");
        _enemySpriteSheet = new Bitmap("Assets/orc1_walk_full.png");
        _tutorialImages["enemyTutor1"] = LoadTexture("Assets/EnemyTutor1.png");
        _tutorialImages["enemyTutor2"] = LoadTexture("Assets/EnemyTutor2.png");
        _tutorialImages["enemyTutor3"] = LoadTexture("Assets/EnemyTutor3.png");
    }

    private void Training_Paint(object? sender, PaintEventArgs e)
    {
        var maze = _controller.Maze;
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
        
        if (_currentTutorialImage != null)
        { 
            g.DrawImage(_currentTutorialImage, 0, 0, ClientSize.Width, ClientSize.Height);
        }
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
    
    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
    
        if (_currentTutorialImage != null)
        {
            HideTutorialMessage();
            _tutorialTimer.Stop();
        }
    }
    
    private void DrawMaze(Maze maze, int centerX, int centerY, Graphics g)
    {
        for (var y = 0; y < maze.Grid.GetLength(0); y++)
        {
            for (var x = 0; x < maze.Grid.GetLength(1); x++)
            {
                var screenX = centerX - (x - _controller.CameraX) * CellSize;
                var screenY = centerY - (y - _controller.CameraY) * CellSize;

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
            (float)ClientSize.Height);
        g.FillRectangle(sideBrush,
            ClientSize.Width-(CellSize*2.3f),
            0f,
            CellSize*2.3f,
            (float)ClientSize.Height);
    }
    
    private void DrawEnemy(Graphics g)
    {
            
        _enemyAnimationTick++;
        foreach (var en in _controller.Enemies)
        {
            var enemyDirection = (int)en.Direction;
            float x = Width / 2;
            float y = Height / 2;
            var sx = x - (en.DrawX - _controller.CameraX) * CellSize;
            var sy = y - (en.DrawY - _controller.CameraY) * CellSize;
                
            if (Math.Abs(en.X - _controller.Player.X) + Math.Abs(en.Y - _controller.Player.Y) <= 1)
            {
                if (enemyDirection <= 1)
                    _enemyAnimationFrame = 1;
                else
                    _enemyAnimationFrame = 2;
            }
            else
            {
                    
                if (_enemyAnimationTick >= EnemyFrameChangeRate)
                {
                    _enemyAnimationTick = 0;
                    _enemyAnimationFrame = (_enemyAnimationFrame + 1) % EnemyFramesPerDirection;
                }
            }
            
            var srcRectE = new Rectangle(
                _enemyAnimationFrame * EnemySpriteSize,
                enemyDirection * EnemySpriteSize,
                EnemySpriteSize, EnemySpriteSize);

            var destRectE = new RectangleF(
                sx - CellSize * 0.75f,
                sy - CellSize * 0.75f,
                CellSize*1.5f, CellSize*1.5f);

            g.DrawImage(_enemySpriteSheet, destRectE, srcRectE, GraphicsUnit.Pixel);
        }
    }
    
    private void DrawHealthBar(Graphics g)
    {
        var barWidth = 400;
        var barHeight = 45;
        var margin = 30;
            
        var healthBarX = margin;
        var healthBarY = ClientSize.Height/2 - barHeight/2;

        var healthRatio = Math.Clamp(_controller.Player.Health / 100f, 0f, 1f);

        g.FillRectangle(Brushes.Gray, healthBarX, healthBarY, barWidth, barHeight);

        using (var healthBrush = new SolidBrush(Color.FromArgb(139, 0, 0))) // Dark red
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

        foreach (var key in _controller.Player.CollectedKeys)
        {
            var keyTexture = GetKeyTexture(key.Id);
            g.DrawImage(keyTexture, startX, startY, keySize, keySize);
            startX += keySize + spacing;
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
    
    private static TileType GetTileType(int code)
    {
        return code switch
        {
            0 => TileType.Wall,
            1 => TileType.Floor,
            3 => TileType.Player,
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