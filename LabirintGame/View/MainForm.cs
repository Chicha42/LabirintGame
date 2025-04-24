using System;
using System.Drawing;
using System.Windows.Forms;
using LabirintGame.Model;
using LabirintGame.Controller;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LabirintGame.View
{
    public partial class MainForm : Form
    {
        private readonly GameController _controller;
        private const int CellSize = 145;
        Timer timer = new Timer();

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            KeyDown += MainForm_KeyDown;
            Resize += (_, _) => Invalidate();

            _controller = new GameController(this);

            Paint += MainForm_Paint;
            
            timer.Interval = 16; // ~60 FPS
            timer.Tick += (_, _) =>
            {
                _controller.Update();
                Invalidate();
            };
            timer.Start();

        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            int dx = 0, dy = 0;
            switch (e.KeyCode)
            {
                case Keys.W: dy = 1; break;
                case Keys.S: dy = -1; break;
                case Keys.A: dx = 1; break;
                case Keys.D: dx = -1; break;
            }

            _controller.MovePlayer(dx, dy);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            var maze = _controller.Maze;
            var player = _controller.Player;
            var g = e.Graphics;
            g.Clear(Color.DarkGray);
            
            int centerX = ClientSize.Width / 2;
            int centerY = ClientSize.Height / 2;
            
            // Получаем границы видимой области относительно камеры
            int visibleCellsX = (int)Math.Ceiling(ClientSize.Width / (float)CellSize) + 2;
            int visibleCellsY = (int)Math.Ceiling(ClientSize.Height / (float)CellSize) + 2;
            
            int startX = (int)Math.Floor(_controller.CameraX - visibleCellsX / 2f);
            int startY = (int)Math.Floor(_controller.CameraY - visibleCellsY / 2f);
            int endX = startX + visibleCellsX;
            int endY = startY + visibleCellsY;
            
            // Корректируем границы, чтобы не выходить за пределы лабиринта
            startX = Math.Max(0, startX);
            startY = Math.Max(0, startY);
            endX = Math.Min(maze.Width - 1, endX);
            endY = Math.Min(maze.Height - 1, endY);
            
            // Отрисовываем видимую часть лабиринта
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    // Позиция клетки на экране с учетом дробной части камеры
                    float screenX = centerX - (x - _controller.CameraX) * CellSize;
                    float screenY = centerY - (y - _controller.CameraY) * CellSize;
                    
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

            // Отрисовываем игрока в центре экрана
            g.FillEllipse(Brushes.Red,
                centerX - CellSize / 4,
                centerY - CellSize / 4,
                CellSize / 2, CellSize / 2);
        }
    }
}