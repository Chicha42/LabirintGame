using Timer = System.Windows.Forms.Timer;

namespace LabirintGame.View;

public partial class Prologue : Form
{
    private readonly Timer _timer;
    
    public Prologue()
    {
        InitializeComponent();
        DoubleBuffered = true;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Resize += (_, _) => Invalidate();
        
        BackgroundImageLayout = ImageLayout.Stretch;
        BackgroundImage = Image.FromFile("Assets/Prologue.png");
        
        _timer = new Timer { Interval = 60000 };
        _timer.Tick += (s, e) => Close();
        _timer.Start();
        
        Click += (s, e) =>
        {
            var gameForm = new Level1();
            gameForm.Show();
            Close();
        };
    }
}