namespace LabirintGame.View;

public partial class GameOver : Form
{
    private readonly Button[] _buttons;
    public Action OnRestartGame { get; set; }
    public Action OnBackToMenu { get; set; }

    public GameOver()
    {
        InitializeComponent();
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Resize += (s, e) => Invalidate();

        _buttons =
        [
            CreateButton(170, 625, menuButton_Click),
            CreateButton(1090, 625, restartButton_Click)
        ];


        foreach (var btn in _buttons)
            Controls.Add(btn);

        Paint += MainMenu_Paint;
        BackColor = Color.Black;
    }
    
    private Button CreateButton(int x, int y,  EventHandler onClick)
    {
        var button = new Button
        {
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Width = 660,
            Height = 170,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            TabStop = false,
        };
        
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.Transparent;
        button.FlatAppearance.MouseOverBackColor = Color.Transparent;
        button.FlatAppearance.CheckedBackColor = Color.Transparent;
    
        button.Click += onClick;

        button.Location = new Point(
            x,
            y
        );

        return button;
    }
    public void MainMenu_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var textImage = new Bitmap("Assets/GameOver.png");
        g.DrawImage(textImage, 0, 0, ClientSize.Width, ClientSize.Height);
    }
    
    private void restartButton_Click(object sender, EventArgs e)
    {
        Close();
        OnRestartGame?.Invoke();
    }

    private void menuButton_Click(object sender, EventArgs e)
    {
        var f = new MainMenu();
        f.Show();
        Close();
    }
}