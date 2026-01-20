using System.Text.Json;
using StickyPrompts.Models;

namespace StickyPrompts.Services;

/// <summary>
/// Interface for prompt persistence operations.
/// </summary>
public interface IStickyNoteStorage
{
    Task<List<PromptEntry>> LoadAsync();
    Task SaveAsync(List<PromptEntry> prompts);
}

/// <summary>
/// Handles JSON persistence for prompt entries with atomic writes to prevent data corruption.
/// </summary>
public class StickyNoteStorage : IStickyNoteStorage
{
    private readonly string _storagePath;
    private readonly string _seedDataPath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public StickyNoteStorage()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var storageDir = Path.Combine(appData, "StickyPrompts");
        _storagePath = Path.Combine(storageDir, "prompts.json");
        
        // Seed data is embedded in the app's Assets folder
        _seedDataPath = Path.Combine(AppContext.BaseDirectory, "Assets", "seed_prompts.json");
        
        // Ensure directory exists
        Directory.CreateDirectory(storageDir);
    }

    /// <summary>
    /// Loads prompts from disk, falling back to seed data if no file exists.
    /// </summary>
    public async Task<List<PromptEntry>> LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            // If user data exists, load it
            if (File.Exists(_storagePath))
            {
                var json = await File.ReadAllTextAsync(_storagePath);
                var prompts = JsonSerializer.Deserialize(json, PromptJsonContext.Default.ListPromptEntry);
                return prompts ?? await LoadSeedDataAsync();
            }

            // Otherwise, load seed data and save it as user data
            var seedData = await LoadSeedDataAsync();
            await SaveInternalAsync(seedData);
            return seedData;
        }
        catch (JsonException)
        {
            // If JSON is corrupted, fall back to seed data
            return await LoadSeedDataAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Saves prompts to disk using atomic write (temp file + move).
    /// </summary>
    public async Task SaveAsync(List<PromptEntry> prompts)
    {
        await _lock.WaitAsync();
        try
        {
            await SaveInternalAsync(prompts);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveInternalAsync(List<PromptEntry> prompts)
    {
        var tempPath = _storagePath + ".tmp";
        
        // Serialize to temp file first
        var json = JsonSerializer.Serialize(prompts, PromptJsonContext.Default.ListPromptEntry);
        await File.WriteAllTextAsync(tempPath, json);
        
        // Atomic move (overwrites if exists)
        File.Move(tempPath, _storagePath, overwrite: true);
    }

    private async Task<List<PromptEntry>> LoadSeedDataAsync()
    {
        if (!File.Exists(_seedDataPath))
        {
            // Return default prompts if no seed file exists
            return GetDefaultPrompts();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_seedDataPath);
            return JsonSerializer.Deserialize(json, PromptJsonContext.Default.ListPromptEntry) 
                   ?? GetDefaultPrompts();
        }
        catch
        {
            return GetDefaultPrompts();
        }
    }

    private static List<PromptEntry> GetDefaultPrompts() =>
    [
        PromptEntry.Create(
            "Code Review",
            "Please review this code for bugs, performance issues, and best practices. Suggest improvements and explain your reasoning.",
            "#4CAF50"),
        
        PromptEntry.Create(
            "Explain Simply",
            "Explain the following concept in simple terms that a beginner could understand. Use analogies where helpful.",
            "#2196F3"),
        
        PromptEntry.Create(
            "Debug Helper",
            "I'm encountering an error. Please help me debug this issue. Here's the error message and relevant code:",
            "#F44336"),
        
        PromptEntry.Create(
            "Refactor Request",
            "Please refactor this code to be more readable, maintainable, and following SOLID principles:",
            "#9C27B0"),
        
        PromptEntry.Create(
            "Documentation",
            "Please generate comprehensive documentation for the following code, including purpose, parameters, return values, and usage examples:",
            "#FF9800"),
        
        PromptEntry.Create(
            "Test Cases",
            "Please generate unit test cases for the following code. Cover edge cases, error conditions, and typical usage:",
            "#00BCD4")
    ];
}
