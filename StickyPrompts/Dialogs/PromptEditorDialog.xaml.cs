using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using StickyPrompts.Models;
using StickyPrompts.ViewModels;

namespace StickyPrompts.Dialogs;

/// <summary>
/// Dialog for creating or editing a prompt.
/// </summary>
public sealed partial class PromptEditorDialog : ContentDialog
{
    public PromptEditorViewModel ViewModel { get; }

    /// <summary>
    /// The resulting prompt after the dialog is closed (null if cancelled).
    /// </summary>
    public PromptEntry? ResultPrompt { get; private set; }

    /// <summary>
    /// Whether the current input is valid for saving.
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(ViewModel.Title) && 
                          !string.IsNullOrWhiteSpace(ViewModel.Content);

    /// <summary>
    /// Creates a dialog for adding a new prompt.
    /// </summary>
    public PromptEditorDialog()
    {
        ViewModel = new PromptEditorViewModel();
        this.InitializeComponent();
        
        Title = "Add Prompt";
        SetupBindings();
    }

    /// <summary>
    /// Creates a dialog for editing an existing prompt.
    /// </summary>
    public PromptEditorDialog(PromptEntry existingPrompt)
    {
        ViewModel = new PromptEditorViewModel(existingPrompt);
        this.InitializeComponent();
        
        Title = "Edit Prompt";
        SetupBindings();
    }

    private void SetupBindings()
    {
        // Update preview color when selection changes
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.SelectedColor))
            {
                UpdatePreviewColor();
            }
            
            // Update primary button enabled state
            IsPrimaryButtonEnabled = IsValid;
        };

        // Set initial preview color
        UpdatePreviewColor();

        // Set initial selection in color picker
        ColorPicker.SelectedItem = ViewModel.SelectedColor;

        // Handle primary button click
        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    private void UpdatePreviewColor()
    {
        try
        {
            var color = ParseHexColor(ViewModel.SelectedColor);
            PreviewBorder.Background = new SolidColorBrush(color);
        }
        catch
        {
            PreviewBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100));
        }
    }

    private void ColorPicker_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is string color)
        {
            ViewModel.SelectColorCommand.Execute(color);
        }
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        if (IsValid)
        {
            ResultPrompt = ViewModel.ToPromptEntry();
        }
        else
        {
            args.Cancel = true;
        }
    }

    private static Windows.UI.Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');

        byte a = 255;
        byte r, g, b;

        if (hex.Length == 6)
        {
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else if (hex.Length == 8)
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else
        {
            throw new ArgumentException("Invalid hex color format");
        }

        return Windows.UI.Color.FromArgb(a, r, g, b);
    }
}
