using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SocietyManagementSystem;

public enum AppTheme
{
    Light,
    Dark
}

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public static void SetTheme(AppTheme theme)
    {
        if (Current is not Application app)
            return;

        var resourceUri = new Uri($"Themes/{(theme == AppTheme.Light ? "LightTheme" : "DarkTheme")}.xaml", UriKind.Relative);
        var themeDictionary = new ResourceDictionary { Source = resourceUri };

        var existing = app.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.EndsWith("LightTheme.xaml") || d.Source.OriginalString.EndsWith("DarkTheme.xaml")));

        if (existing != null)
            app.Resources.MergedDictionaries.Remove(existing);

        app.Resources.MergedDictionaries.Add(themeDictionary);
        CurrentTheme = theme;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        SetTheme(CurrentTheme);
    }
}

