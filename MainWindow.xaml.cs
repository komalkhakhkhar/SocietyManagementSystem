using System.Windows;
using System.Windows.Controls;

namespace SocietyManagementSystem;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateThemeButton();
        MainFrame.Navigate(new Login());
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        var nextTheme = App.CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;
        App.SetTheme(nextTheme);
        UpdateThemeButton();
    }

    private void UpdateThemeButton()
    {
        if (ThemeToggleButton == null)
            return;

        ThemeToggleButton.Content = App.CurrentTheme == AppTheme.Light ? "Dark Mode" : "Light Mode";
    }

    public void Navigate(Page page)
    {
        MainFrame.Navigate(page);
    }
}
