using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SocietyManagementSystem.Services;

namespace SocietyManagementSystem;

/// <summary>
/// Interaction logic for Login.xaml
/// </summary>
public partial class Login : Window
{
    private MongoDbService _mongoDbService;

    public Login()
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
    }

    private async void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        string societyName = txtSocietyName.Text.Trim();
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Password;

        if (string.IsNullOrEmpty(societyName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            lblMessage.Text = "All fields are required.";
            return;
        }

        _mongoDbService = new MongoDbService(societyName);

        bool isValid = await _mongoDbService.ValidateLoginAsync(username, password);

        if (isValid)
        {
            MessageBox.Show("Login Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            TenantWindow dashboard = new TenantWindow(societyName);
            dashboard.Show();
            this.Close();
        }
        else
        {
            lblMessage.Text = "Invalid username or password.";
        }
    }

    private void btnSignup_Click(object sender, RoutedEventArgs e)
    {
        Signup signupWindow = new Signup();
        signupWindow.Show();
        this.Close();
    }

    private void MenuTenants_Click(object sender, RoutedEventArgs e)
    {
        var tenantWindow = new TenantWindow("");
        tenantWindow.ShowDialog();
    }
}