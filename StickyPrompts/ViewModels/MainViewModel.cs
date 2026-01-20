using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StickyPrompts.Models;
using StickyPrompts.Services;

namespace StickyPrompts.ViewModels;

/// <summary>
/// Main ViewModel for the application, managing the collection of prompts.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IStickyNoteStorage _storage;
    private readonly IWindowService _windowService;

    [ObservableProperty]
    private ObservableCollection<PromptEntry> _prompts = [];

    [ObservableProperty]
    private bool _isCompactMode;

    [ObservableProperty]
    private bool _isAlwaysOnTop = true;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private PromptEntry? _selectedPrompt;

    public MainViewModel(IStickyNoteStorage storage, IWindowService windowService)
    {
        _storage = storage;
        _windowService = windowService;
    }

    /// <summary>
    /// Loads prompts from storage on startup.
    /// </summary>
    [RelayCommand]
    private async Task LoadPromptsAsync()
    {
        IsLoading = true;
        try
        {
            var loadedPrompts = await _storage.LoadAsync();
            Prompts = new ObservableCollection<PromptEntry>(loadedPrompts);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves current prompts to storage.
    /// </summary>
    [RelayCommand]
    private async Task SavePromptsAsync()
    {
        await _storage.SaveAsync([.. Prompts]);
    }

    /// <summary>
    /// Adds a new prompt to the collection.
    /// </summary>
    [RelayCommand]
    private async Task AddPromptAsync(PromptEntry? newPrompt)
    {
        if (newPrompt is null) return;
        
        Prompts.Add(newPrompt);
        await SavePromptsAsync();
    }

    /// <summary>
    /// Updates an existing prompt in the collection.
    /// </summary>
    [RelayCommand]
    private async Task UpdatePromptAsync(PromptEntry? updatedPrompt)
    {
        if (updatedPrompt is null) return;

        var index = -1;
        for (int i = 0; i < Prompts.Count; i++)
        {
            if (Prompts[i].Id == updatedPrompt.Id)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            Prompts[index] = updatedPrompt;
            await SavePromptsAsync();
        }
    }

    /// <summary>
    /// Deletes a prompt from the collection.
    /// </summary>
    [RelayCommand]
    private async Task DeletePromptAsync(PromptEntry? prompt)
    {
        if (prompt is null) return;

        Prompts.Remove(prompt);
        await SavePromptsAsync();
    }

    /// <summary>
    /// Marks a prompt as recently used (updates LastUsed timestamp).
    /// </summary>
    [RelayCommand]
    private async Task MarkAsUsedAsync(PromptEntry? prompt)
    {
        if (prompt is null) return;

        var updated = prompt.WithUpdatedLastUsed();
        await UpdatePromptAsync(updated);
    }

    /// <summary>
    /// Toggles compact mode for the window.
    /// </summary>
    [RelayCommand]
    private void ToggleCompactMode()
    {
        IsCompactMode = !IsCompactMode;
        _windowService.SetCompactMode(IsCompactMode);
    }

    /// <summary>
    /// Toggles always-on-top state for the window.
    /// </summary>
    [RelayCommand]
    private void ToggleAlwaysOnTop()
    {
        IsAlwaysOnTop = !IsAlwaysOnTop;
        _windowService.SetAlwaysOnTop(IsAlwaysOnTop);
    }
}
