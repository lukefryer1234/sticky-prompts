using Microsoft.UI.Xaml;
using StickyPrompts.Services;
using StickyPrompts.ViewModels;

namespace StickyPrompts;

/// <summary>
/// Application entry point with service registration.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    
    // Simple service locator for this small app (can be replaced with DI container if needed)
    public static IStickyNoteStorage Storage { get; } = new StickyNoteStorage();
    public static IWindowService WindowService { get; } = new WindowService();
    public static MainViewModel MainViewModel { get; private set; } = null!;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Create the MainViewModel with dependencies
        MainViewModel = new MainViewModel(Storage, WindowService);

        // Create and activate the main window
        _window = new MainWindow();
        
        // Initialize window service with the main window
        WindowService.Initialize(_window);
        
        _window.Activate();
    }
}
