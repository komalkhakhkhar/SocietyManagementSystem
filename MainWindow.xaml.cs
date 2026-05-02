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
        MainFrame.Navigate(new Login());
    }

    public void Navigate(Page page)
    {
        MainFrame.Navigate(page);
    }
}
