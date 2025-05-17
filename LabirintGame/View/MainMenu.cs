namespace LabirintGame.View;

public partial class MainMenu : Form
{
    private Button[] buttons;
    public MainMenu()
    {
        InitializeComponent();
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Resize += (s, e) => CenterButtons();

        buttons = new Button[]
        {
            CreateButton("Начать игру", 30, startButton_Click),
            CreateButton("Выход", 250, exitButton_Click)
        };
        
        BackgroundImage = LoadTexture("Assets/WallBg.png");
        
        foreach (var btn in buttons)
            Controls.Add(btn);
    }
    
    private Button CreateButton(string text, int top, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            ForeColor = Color.White,
            Font = new Font("impact", 42, FontStyle.Bold),
            Width = 600,
            Height = 200,
            Top = top,
            Anchor = AnchorStyles.Top | AnchorStyles.Left
        };
        button.Click += onClick;
        button.Location = new Point(
            (ClientSize.Width - button.Width) / 2,
            top
        );
        return button;
    }
    private void startButton_Click(object sender, EventArgs e)
    {
        var gameForm = new MainForm();
        gameForm.Show();
        this.Hide();
    }
    private void CenterButtons()
    {
        int spacing = 15;
        int buttonHeight = buttons[0].Height;
        int totalHeight = buttons.Length * buttonHeight + (buttons.Length - 1) * spacing;
        int startY = (ClientSize.Height - totalHeight) / 2;

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].Top = startY + i * (buttonHeight + spacing);
            buttons[i].Left = (ClientSize.Width - buttons[i].Width) / 2;
        }
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
}