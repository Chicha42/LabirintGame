using System;
using System.Windows.Forms;
using LabirintGame;
using LabirintGame.View;
using LabirintGame.Controller;

namespace LabirintGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            Console.WriteLine("Hello, World!");
        }
    }
}