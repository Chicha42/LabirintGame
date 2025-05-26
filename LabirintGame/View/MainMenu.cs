namespace LabirintGame.View;

public partial class MainMenu : Form
{
    private readonly Button[] _buttons;
    public MainMenu()
    {
        InitializeComponent();
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Resize += (s, e) => Invalidate();

        _buttons =
        [
            CreateButton( 384, trainingButton_click),
            CreateButton(603, startButton_Click),
            CreateButton(822, exitButton_Click)
        ];
        
        
        foreach (var btn in _buttons)
            Controls.Add(btn);

        Paint += MainMenu_Paint;
        BackgroundImage = LoadTexture("Assets/floor.png");
    }

    private void trainingButton_click(object? sender, EventArgs e)
    {
        var trainingForm = new Tutorial();
        trainingForm.Show();
        Hide();
    }

    private Button CreateButton(int top, EventHandler onClick)
    {
        var button = new Button
        {
            BackColor = Color.Transparent,
            FlatStyle = FlatStyle.Flat,
            Width = 660,
            Height = 170,
            Top = top,
            Anchor = AnchorStyles.Top | AnchorStyles.Left,
            TabStop = false,
        };
        
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.Transparent;
        button.FlatAppearance.MouseOverBackColor = Color.Transparent;
        button.FlatAppearance.CheckedBackColor = Color.Transparent;
    
        button.Click += onClick;

        button.Location = new Point(
            630,
            top
        );

        return button;
    }

    private void startButton_Click(object sender, EventArgs e)
    {
        var gameForm = new Prologue();
        gameForm.Show();
        Hide();
    }
    
    private void exitButton_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }
    public Bitmap LoadTexture(string path)
    {
        var original = new Bitmap(path);
            
        var resized = new Bitmap(128, 128);

        using var g = Graphics.FromImage(resized);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        g.DrawImage(original, 0, 0, resized.Width, resized.Height);

        return resized;
    }
    public void MainMenu_Paint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        using var textImage = new Bitmap("Assets/MainMenu.png");
        g.DrawImage(textImage, 0, 0, ClientSize.Width, ClientSize.Height);
    }

}