using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using StickyPrompts.Controls;
using StickyPrompts.Dialogs;
using StickyPrompts.Models;
using StickyPrompts.ViewModels;

namespace StickyPrompts;

/// <summary>
/// Main application window code-behind.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel => App.MainViewModel;

    public MainWindow()
    {
        this.InitializeComponent();
        
        // Load prompts when window is created
        Activated += MainWindow_Activated;
        
        // Listen for collection changes to update UI state
        ViewModel.Prompts.CollectionChanged += Prompts_CollectionChanged;
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Only load once
        Activated -= MainWindow_Activated;
        
        await ViewModel.LoadPromptsCommand.ExecuteAsync(null);
        UpdateUIState();
    }

    private void Prompts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        var hasPrompts = ViewModel.Prompts.Count > 0;
        var isLoading = ViewModel.IsLoading;

        LoadingRing.IsActive = isLoading;
        LoadingRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        
        PromptsScrollViewer.Visibility = hasPrompts && !isLoading ? Visibility.Visible : Visibility.Collapsed;
        EmptyState.Visibility = !hasPrompts && !isLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PromptEditorDialog
        {
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary && dialog.ResultPrompt is not null)
        {
            await ViewModel.AddPromptCommand.ExecuteAsync(dialog.ResultPrompt);
        }
    }

    private void CompactModeToggle_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleCompactModeCommand.Execute(null);
        
        // Update UI for compact mode
        var isCompact = ViewModel.IsCompactMode;
        AppTitle.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
        AddButton.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
    }

    private void AlwaysOnTopToggle_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ToggleAlwaysOnTopCommand.Execute(null);
    }

    private async void PromptTile_EditRequested(object sender, PromptEntry prompt)
    {
        var dialog = new PromptEditorDialog(prompt)
        {
            XamlRoot = Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary && dialog.ResultPrompt is not null)
        {
            await ViewModel.UpdatePromptCommand.ExecuteAsync(dialog.ResultPrompt);
        }
    }

    private async void PromptTile_DeleteRequested(object sender, PromptEntry prompt)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = Content.XamlRoot,
            Title = "Delete Prompt",
            Content = $"Are you sure you want to delete \"{prompt.Title}\"?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        var result = await dialog.ShowAsync();
        
        if (result == ContentDialogResult.Primary)
        {
            await ViewModel.DeletePromptCommand.ExecuteAsync(prompt);
        }
    }

    private async void PromptTile_DragCompleted(object sender, PromptEntry prompt)
    {
        // Mark the prompt as used when dragged
        await ViewModel.MarkAsUsedCommand.ExecuteAsync(prompt);
    }
}
