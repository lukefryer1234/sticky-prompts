using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyPrompts.Models;

namespace StickyPrompts.ViewModels;

/// <summary>
/// ViewModel for the prompt editor dialog.
/// </summary>
public partial class PromptEditorViewModel : ObservableObject
{
    private readonly PromptEntry? _existingPrompt;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _selectedColor = "#4CAF50";

    [ObservableProperty]
    private bool _isEditMode;

    /// <summary>
    /// Available color options for prompts.
    /// </summary>
    public static string[] AvailableColors =>
    [
        "#4CAF50", // Green
        "#2196F3", // Blue  
        "#F44336", // Red
        "#9C27B0", // Purple
        "#FF9800", // Orange
        "#00BCD4", // Cyan
        "#E91E63", // Pink
        "#FFEB3B", // Yellow
        "#795548", // Brown
        "#607D8B"  // Blue Grey
    ];

    /// <summary>
    /// Creates a new editor for adding a prompt.
    /// </summary>
    public PromptEditorViewModel()
    {
        _existingPrompt = null;
        IsEditMode = false;
    }

    /// <summary>
    /// Creates an editor for modifying an existing prompt.
    /// </summary>
    public PromptEditorViewModel(PromptEntry existingPrompt)
    {
        _existingPrompt = existingPrompt;
        Title = existingPrompt.Title;
        Content = existingPrompt.Content;
        SelectedColor = existingPrompt.ColorHex;
        IsEditMode = true;
    }

    /// <summary>
    /// Gets whether the current input is valid.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Content);

    /// <summary>
    /// Creates a PromptEntry from the current editor state.
    /// </summary>
    public PromptEntry ToPromptEntry()
    {
        if (_existingPrompt is not null)
        {
            // Update existing prompt
            return _existingPrompt with
            {
                Title = Title.Trim(),
                Content = Content.Trim(),
                ColorHex = SelectedColor
            };
        }

        // Create new prompt
        return PromptEntry.Create(Title.Trim(), Content.Trim(), SelectedColor);
    }

    [RelayCommand]
    private void SelectColor(string color)
    {
        SelectedColor = color;
    }
}
