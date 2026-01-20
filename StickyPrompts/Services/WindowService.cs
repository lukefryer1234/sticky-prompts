using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace StickyPrompts.Services;

/// <summary>
/// Interface for window management operations.
/// </summary>
public interface IWindowService
{
    void Initialize(Window window);
    void SetAlwaysOnTop(bool enabled);
    void SetCompactMode(bool enabled);
    bool IsCompactMode { get; }
    bool IsAlwaysOnTop { get; }
}

/// <summary>
/// Manages window behavior including always-on-top, compact mode, and Mica backdrop.
/// </summary>
public class WindowService : IWindowService
{
    private Window? _window;
    private AppWindow? _appWindow;
    private OverlappedPresenter? _presenter;
    
    private const int NormalWidth = 320;
    private const int NormalHeight = 600;
    private const int CompactWidth = 70;
    
    private int _savedWidth = NormalWidth;
    private int _savedHeight = NormalHeight;
    
    public bool IsCompactMode { get; private set; }
    public bool IsAlwaysOnTop { get; private set; } = true;

    /// <summary>
    /// Initializes the service with the main window and applies default settings.
    /// </summary>
    public void Initialize(Window window)
    {
        _window = window;
        
        // Get the AppWindow for advanced window management
        var hwnd = WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        
        // Get the OverlappedPresenter for always-on-top control
        _presenter = _appWindow.Presenter as OverlappedPresenter;
        
        // Apply Mica backdrop
        ApplyMicaBackdrop();
        
        // Set initial size
        _appWindow.Resize(new Windows.Graphics.SizeInt32(NormalWidth, NormalHeight));
        
        // Enable always-on-top by default
        SetAlwaysOnTop(true);
        
        // Set window title
        _appWindow.Title = "Sticky Prompts";
        
        // Remove default title bar for cleaner look (optional)
        // _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
    }

    /// <summary>
    /// Toggles the always-on-top state of the window.
    /// </summary>
    public void SetAlwaysOnTop(bool enabled)
    {
        if (_presenter is null) return;
        
        _presenter.IsAlwaysOnTop = enabled;
        IsAlwaysOnTop = enabled;
    }

    /// <summary>
    /// Toggles compact mode (narrow strip view).
    /// </summary>
    public void SetCompactMode(bool enabled)
    {
        if (_appWindow is null) return;

        if (enabled && !IsCompactMode)
        {
            // Save current dimensions before going compact
            var currentSize = _appWindow.Size;
            _savedWidth = currentSize.Width;
            _savedHeight = currentSize.Height;
            
            // Resize to compact strip
            _appWindow.Resize(new Windows.Graphics.SizeInt32(CompactWidth, _savedHeight));
        }
        else if (!enabled && IsCompactMode)
        {
            // Restore saved dimensions
            _appWindow.Resize(new Windows.Graphics.SizeInt32(_savedWidth, _savedHeight));
        }

        IsCompactMode = enabled;
    }

    private void ApplyMicaBackdrop()
    {
        if (_window is null) return;

        // Try to apply Mica backdrop (Windows 11 only)
        if (MicaController.IsSupported())
        {
            var micaBackdrop = new MicaBackdrop
            {
                Kind = MicaKind.BaseAlt
            };
            _window.SystemBackdrop = micaBackdrop;
        }
        else if (DesktopAcrylicController.IsSupported())
        {
            // Fall back to Acrylic on older Windows versions
            var acrylicBackdrop = new DesktopAcrylicBackdrop();
            _window.SystemBackdrop = acrylicBackdrop;
        }
    }
}
