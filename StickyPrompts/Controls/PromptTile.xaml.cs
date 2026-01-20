using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.DataTransfer;
using StickyPrompts.Models;

namespace StickyPrompts.Controls;

/// <summary>
/// A draggable tile control representing a single prompt.
/// </summary>
public sealed partial class PromptTile : UserControl
{
    /// <summary>
    /// The prompt data to display.
    /// </summary>
    public static readonly DependencyProperty PromptProperty =
        DependencyProperty.Register(
            nameof(Prompt),
            typeof(PromptEntry),
            typeof(PromptTile),
            new PropertyMetadata(null, OnPromptChanged));

    /// <summary>
    /// Whether to display in compact mode (icon only).
    /// </summary>
    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(
            nameof(IsCompact),
            typeof(bool),
            typeof(PromptTile),
            new PropertyMetadata(false, OnIsCompactChanged));

    public PromptEntry? Prompt
    {
        get => (PromptEntry?)GetValue(PromptProperty);
        set => SetValue(PromptProperty, value);
    }

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    /// <summary>
    /// Brush created from the prompt's ColorHex.
    /// </summary>
    public SolidColorBrush ColorBrush
    {
        get
        {
            if (Prompt?.ColorHex is string hex && !string.IsNullOrEmpty(hex))
            {
                try
                {
                    return new SolidColorBrush(ParseHexColor(hex));
                }
                catch
                {
                    // Fall back to default color
                }
            }
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 100, 100, 100));
        }
    }

    /// <summary>
    /// Fired when the user requests to edit this prompt.
    /// </summary>
    public event EventHandler<PromptEntry>? EditRequested;

    /// <summary>
    /// Fired when the user requests to delete this prompt.
    /// </summary>
    public event EventHandler<PromptEntry>? DeleteRequested;

    /// <summary>
    /// Fired when a drag operation completes successfully.
    /// </summary>
    public event EventHandler<PromptEntry>? DragCompleted;

    public PromptTile()
    {
        this.InitializeComponent();
        
        // Set up pointer events for visual feedback
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        PointerPressed += OnPointerPressed;
        PointerReleased += OnPointerReleased;
    }

    private static void OnPromptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PromptTile tile)
        {
            tile.UpdateVisuals();
        }
    }

    private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PromptTile tile)
        {
            tile.UpdateCompactMode();
        }
    }

    private void UpdateVisuals()
    {
        // Update the background brush when prompt changes
        TileGrid.Background = ColorBrush;
    }

    private void UpdateCompactMode()
    {
        NormalView.Visibility = IsCompact ? Visibility.Collapsed : Visibility.Visible;
        CompactView.Visibility = IsCompact ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Handles the drag starting event - sets up the data package with prompt content.
    /// </summary>
    private void OnDragStarting(UIElement sender, DragStartingEventArgs args)
    {
        if (Prompt is null) return;

        // Set the text content for the drag operation
        args.Data.SetText(Prompt.Content);
        args.Data.RequestedOperation = DataPackageOperation.Copy;
        
        // Set drag UI to show the prompt title
        args.DragUI.SetContentFromDataPackage();
        
        // Show drag indicator
        DragIndicator.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Called when drag operation ends (in DropCompleted we fire our event).
    /// </summary>
    protected override void OnDropCompleted(UIElement sender, DropCompletedEventArgs e)
    {
        base.OnDropCompleted(sender, e);
        
        // Hide drag indicator
        DragIndicator.Visibility = Visibility.Collapsed;
        
        // Notify that drag was completed
        if (Prompt is not null)
        {
            DragCompleted?.Invoke(this, Prompt);
        }
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (Prompt is not null)
        {
            EditRequested?.Invoke(this, Prompt);
        }
    }

    private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (Prompt is not null)
        {
            DeleteRequested?.Invoke(this, Prompt);
        }
    }

    private async void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (Prompt is null) return;

        var dataPackage = new DataPackage();
        dataPackage.SetText(Prompt.Content);
        Clipboard.SetContent(dataPackage);
        
        // Brief visual feedback
        await Task.Delay(100);
    }

    private void OnPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "PointerOver", true);
    }

    private void OnPointerExited(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Normal", true);
    }

    private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Pressed", true);
    }

    private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "PointerOver", true);
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
