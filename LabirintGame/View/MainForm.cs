using System;
using System.Drawing;
using System.Windows.Forms;
using LabirintGame.Model;
using LabirintGame.Controller;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LabirintGame.View
{
    public sealed partial class MainForm : Form
    {
        private readonly GameController _controller;
        private const int CellSize = 200;
        private readonly Timer _timer = new Timer();
        private bool _wPressed, _aPressed, _sPressed, _dPressed, _escPressed;

        public MainForm()
        {   
            InitializeComponent();
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            Resize += (_, _) => Invalidate();

            _controller = new GameController(this, 3);

            Paint += MainForm_Paint;
            
            _timer.Interval = 16;
            _timer.Tick += (_, _) =>
            {
                int dx = 0, dy = 0;
                if (_wPressed) dy = 1;
                if (_sPressed) dy = -1;
                if (_aPressed) dx = 1;
                if (_dPressed) dx = -1;
                if (_escPressed) Application.Exit();
    
                if (dx != 0 || dy != 0)
                {
                    _controller.MovePlayer(dx, dy);
                }
    
                _controller.Update();
                Invalidate();
            };  
            _timer.Start();

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
            var maze = _controller.Maze;
            var g = e.Graphics;
            g.Clear(Color.DarkGray);
            
            var centerX = ClientSize.Width / 2;
            var centerY = ClientSize.Height / 2;
            
            var (startX, startY, endX, endY) = GetVisibleBounds(maze);
            
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    var screenX = centerX - (x - _controller.CameraX) * CellSize;
                    var screenY = centerY - (y - _controller.CameraY) * CellSize;
                    
                    Brush brush = maze.Grid[y, x] switch
                    {
                        0 => Brushes.Black,
                        1 => Brushes.White,
                        3 => Brushes.Green,
                        10 => Brushes.Red,
                        9 => Brushes.Green,
                        8 => Brushes.Blue,
                        20 => Brushes.DarkRed,
                        19 => Brushes.DarkGreen,
                        18 => Brushes.DarkBlue,
                        _ => Brushes.Gray
                    };

                    RectangleF rect = new RectangleF(
                        screenX - CellSize / 2f, 
                        screenY - CellSize / 2f, 
                        CellSize, 
                        CellSize);
                    
                    g.FillRectangle(brush, rect);
                }
            }

            g.FillEllipse(Brushes.Black,
                centerX - CellSize / 4,
                centerY - CellSize / 4,
                CellSize / 2, CellSize / 2);
            
            foreach (var en in _controller.Enemies)
            {
                float x = Width/2;
                float y = Height/2;
                float cell = CellSize; // ваша константа
                float sx = x - (en.DrawX - _controller.CameraX)*cell;
                float sy = y - (en.DrawY - _controller.CameraY)*cell;
                float size = cell * 0.5f;
                g.FillEllipse(Brushes.DarkRed,
                    sx - size/2, sy - size/2,
                    size, size);
            }
            
            g.FillRectangle(Brushes.DarkGray,
                0f,
                0f,
                CellSize*2.3f,
                (float)ClientSize.Height);
            g.FillRectangle(Brushes.DarkGray,
                ClientSize.Width-(CellSize*2.3f),
                0f,
                CellSize*2.3f,
                (float)ClientSize.Height);
        }

        private (int startX, int startY, int endX, int endY) GetVisibleBounds(Maze maze)
        {
            var visibleCellsX = (int)Math.Ceiling(ClientSize.Width*2 / (float)CellSize) + 2;
            var visibleCellsY = (int)Math.Ceiling(ClientSize.Height *2/ (float)CellSize) + 2;
            
            var startX = (int)Math.Floor(_controller.CameraX - visibleCellsX / 2f);
            var startY = (int)Math.Floor(_controller.CameraY - visibleCellsY / 2f);
            var endX = startX + visibleCellsX;
            var endY = startY + visibleCellsY;
            
            return (Math.Max(0, startX),
                Math.Max(0, startY), 
                Math.Min(maze.Width - 1, endX),
                Math.Min(maze.Height - 1, endY));
        }
    }
}