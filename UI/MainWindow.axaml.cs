using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.IO;
using System.Collections.Generic;

namespace UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

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
            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
    
            Editor.Document = new AvaloniaEdit.Document.TextDocument(content);
            Editor.ShowLineNumbers = true; // Show them now
            Editor.IsVisible = true;       // Make the editor appear
        }
    }
}
