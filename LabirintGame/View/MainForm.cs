using System;
using System.Drawing;
using System.Windows.Forms;
using LabirintGame.Model;
using LabirintGame.Controller;

namespace LabirintGame.View
{
    public partial class MainForm : Form
    {
        private readonly GameController _controller;
        private const int CellSize = 45;

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
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            int dx = 0, dy = 0;
            switch (e.KeyCode)
            {
                case Keys.W: dy = -1; break;
                case Keys.S: dy = 1; break;
                case Keys.A: dx = -1; break;
                case Keys.D: dx = 1; break;
            }

            _controller.MovePlayer(dx, dy);
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            var maze = _controller.Maze;
            var player = _controller.Player;
            Graphics g = e.Graphics;
            g.Clear(Color.White);

            int mazePixelWidth = maze.Width * CellSize;
            int mazePixelHeight = maze.Height * CellSize;

            int offsetX = (ClientSize.Width - mazePixelWidth) / 2;
            int offsetY = (ClientSize.Height - mazePixelHeight) / 2;

            var Keys = maze.Keys;
            var Doors = maze.Doors;

            for (int y = 0; y < maze.Height; y++)
            {
                for (int x = 0; x < maze.Width; x++)
                {
                    Brush brush = maze.Grid[y, x] switch
                    {
                        0 => Brushes.Black,
                        1 => Brushes.White,
                        10 => Brushes.Red,
                        9 => Brushes.Green,
                        8 => Brushes.Blue,
                        20 => Brushes.DarkRed,
                        19 => Brushes.DarkGreen,
                        18 => Brushes.DarkBlue,
                        _ => Brushes.Gray
                        
                    };

                    Rectangle rect = new Rectangle(offsetX + x * CellSize, offsetY + y * CellSize, CellSize, CellSize);

                    if (maze.Grid[y, x] == 2)
                        g.FillEllipse(brush, rect);
                    else
                        g.FillRectangle(brush, rect);
                }
            }

            g.FillEllipse(Brushes.Red,
                offsetX + player.X * CellSize + CellSize / 4,
                offsetY + player.Y * CellSize + CellSize / 4,
                CellSize / 2, CellSize / 2);

            g.FillRectangle(Brushes.Green,
                offsetX + (maze.Width - 2) * CellSize,
                offsetY + (maze.Height - 2) * CellSize,
                CellSize, CellSize);
            g.FillRectangle(
                Brushes.Black,
                offsetX + (maze.Width - 1) * CellSize,
                offsetY + (maze.Height - 2) * CellSize,
                CellSize,
                CellSize
            );
            g.FillRectangle(
                Brushes.Black,
                offsetX + (maze.Width - 2) * CellSize,
                offsetY + (maze.Height - 1) * CellSize,
                CellSize,
                CellSize
            );
        }
    }
}
