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
    private MongoDbService? _mongoDbService;

    public Login()
    {
        InitializeComponent();
    }

    private async void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        try
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
                Dashboard dashboard = new Dashboard(societyName);
                dashboard.Show();
                this.Close();
            }
            else
            {
                lblMessage.Text = "Invalid username or password.";
            }
        }
        catch (Exception ex)
        {
            lblMessage.Text = $"Error: {ex.Message}";
            MessageBox.Show($"Login error: {ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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