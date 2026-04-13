using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System;
using System.Collections.ObjectModel;

namespace UI;

public class FilterItem
{
    public string? Pattern { get; set; } 
    public bool match_case { get; set; } 
    public bool regex { get; set; }
    public bool exclude { get; set; }

    public FilterItem(String Pattern, bool match_case, bool regex, bool exclude)
    {
        this.Pattern = Pattern;
        this.match_case = match_case;
        this.regex = regex;
        this.exclude = exclude;
    }
}

public partial class MainWindow : Window
{
    public ObservableCollection<FilterItem> Filters { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this; // Vital for binding to work
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

            await using var stream = await files[0].OpenReadAsync();
            using var reader = new StreamReader(stream);

            // 4. For now, we read the whole thing (Keep the file small!)
            string content = await reader.ReadToEndAsync();
            
            // 5. Update the UI
            Editor.Text = content;
            Editor.IsVisible = true;
        }
    }

    private async void ConfirmAddFilter_Click(object? sender, RoutedEventArgs e)
    {
        Filters.Add(new FilterItem("hola", false, false, false));
    }

    private async void DeleteFilter_Click(object? sender, RoutedEventArgs e)
    {
        
    }
}
