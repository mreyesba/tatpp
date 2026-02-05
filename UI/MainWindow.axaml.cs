using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // High-performance P/Invoke
    [LibraryImport("analysis_engine.dll", StringMarshalling = StringMarshalling.Utf8)]
    public static partial long analyze_massive_file(string path);

    private async void OpenFile_Click(object? sender, RoutedEventArgs e)
    {
        var textFileTypes = new List<FilePickerFileType>
        {
            new FilePickerFileType("Text files")
            {
                Patterns = new[] { "*.txt", "*.log" }
            }
        };

        var files = await this.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title = "Open text file",
                AllowMultiple = false,
                FileTypeFilter = textFileTypes
            });

        if (files.Count > 0)
        {
            string localPath = files[0].Path.LocalPath;
            
            // Run in a background thread so the UI doesn't freeze!
            long result = await Task.Run(() => analyze_massive_file(localPath));
            
            System.Console.WriteLine($"Result from Rust: {result}");
        }
    }
}
