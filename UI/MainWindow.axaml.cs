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
using System.Text;

namespace UI;

public class FilterItem
{
    public string? Pattern { get; set; } 
    public bool IsCaseSensitive { get; set; } 
    public bool IsRegex { get; set; }
    public bool IsExclude { get; set; }

    public FilterItem(String Pattern, bool IsCaseSensitive, bool IsRegex, bool IsExclude)
    {
        this.Pattern = Pattern;
        this.IsCaseSensitive = IsCaseSensitive;
        this.IsRegex = IsRegex;
        this.IsExclude = IsExclude;
    }
}

public partial class MainWindow : Window
{
    public ObservableCollection<FilterItem> Filters { get; } = new();
    private IStorageFile? _targetFile;

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
            _targetFile = files[0];
            
            ProcessFile();
        }
    }

    private bool IncludeLine(string line)
    {
        if (Filters.Count == 0) return true;

        bool hasMatch = false;
        bool hasIncludeFilters = false;

        foreach (var filter in Filters)
        {
            if (string.IsNullOrEmpty(filter.Pattern)) continue;
            
            // Check if the line matches the pattern
            var comparison = filter.IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            bool isMatch = line.Contains(filter.Pattern, comparison);

            if (filter.IsExclude)
            {
                if (isMatch) return false; // Immediate exit: Exclude always wins
            }
            else
            {
                hasIncludeFilters = true;
                if (isMatch) hasMatch = true;
            }
        }

        // If there are only Exclude filters, and we didn't return false, it's a match.
        // If there are Include filters, at least one must match.
        return !hasIncludeFilters || hasMatch;
    }

    private async void ProcessFile()
    {
        if (_targetFile == null) return;

        string localPath = _targetFile.Path.LocalPath;

        // 1. Heavy lifting in background
        var (content, count) = await Task.Run(async () => 
        {
            var sb = new StringBuilder();
            int matchCount = 0;

            using var stream = await _targetFile.OpenReadAsync();
            using var reader = new StreamReader(stream);
            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (IncludeLine(line))
                {
                    sb.AppendLine(line);
                    matchCount++;
                    
                    // Safety: Don't load more than 1MB into the UI editor
                    if (sb.Length > 1_000_000) {
                        sb.AppendLine("... [Truncated: File too large for editor] ...");
                        break;
                    }
                }
            }
            return (sb.ToString(), matchCount);
        });

        // 2. Single UI Update
        Editor.Text = content;
        Editor.IsVisible = true;
        System.Console.WriteLine($"Found {count} matches.");
    }

    private async void ConfirmAddFilter_Click(object? sender, RoutedEventArgs e)
    {
        var flyout = AddFilter_Button.Flyout as Flyout;

        // 1. Extract the values using the Names from XAML
        string pattern = FilterPatternInput.Text;
        
        // CheckBoxes use 'bool?' so we use '?? false' to handle nulls
        bool isRegex = RegexCheck.IsChecked ?? false;
        bool isMatchCase = MatchCaseCheck.IsChecked ?? false;
        bool isExclude = ExcludeCheck.IsChecked ?? false;

        // 2. Validation
        if (!string.IsNullOrWhiteSpace(pattern))
        {
            // 3. Create the object and add to your ObservableCollection
            var newFilter = new FilterItem(pattern, isMatchCase, isRegex, isExclude);

            Filters.Add(newFilter);
        }

        // 4. Reset the UI for the next time it opens
        FilterPatternInput.Text = string.Empty;
        RegexCheck.IsChecked = false;
        MatchCaseCheck.IsChecked = false;
        ExcludeCheck.IsChecked = false;

        // 5. Close the flyout
        flyout?.Hide();

        ProcessFile();
    }

    private async void DeleteFilter_Click(object? sender, RoutedEventArgs e)
    {
        // 1. Cast the sender to a Button
        if (sender is Button button)
        {
            // 2. Retrieve the FilterItem instance from the CommandParameter
            if (button.CommandParameter is FilterItem itemToRemove)
            {
                // 3. Remove it from the collection
                // Because it's an ObservableCollection, the UI updates automatically
                Filters.Remove(itemToRemove);
                
                System.Console.WriteLine($"Removed filter: {itemToRemove.Pattern}");

                ProcessFile();
            }
        }
    }
}
