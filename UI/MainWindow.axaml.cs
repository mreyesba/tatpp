using Avalonia.Controls;
using System.IO;

namespace UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var path = "example.txt";

        if (File.Exists(path))
        {
            FileContent.Text = File.ReadAllText(path);
        }
        else
        {
            FileContent.Text = "File not found: " + path;
        }
    }
}
