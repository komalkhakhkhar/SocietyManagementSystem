using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;

namespace SocietyManagementSystem;

/// <summary>
/// Interaction logic for Signup.xaml
/// </summary>
public partial class Signup : Window
{
    public Signup()
    {
        InitializeComponent();
        this.WindowState = WindowState.Maximized;
    }

    private async void btnSignup_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        string societyName = txtSocietyName.Text.Trim();
        string username = txtUsername.Text.Trim();
        string email = txtEmail.Text.Trim();
        string password = txtPassword.Password;

        if (string.IsNullOrEmpty(societyName) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            lblMessage.Text = "All fields are required.";
            return;
        }

        var mongoDbService = new MongoDbService(societyName);
        var user = new User
        {
            Username = username,
            Password = password,
            Email = email
        };

        bool success = await mongoDbService.RegisterUserAsync(user);
        if (success)
        {
            MessageBox.Show("Signup Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
        else
        {
            lblMessage.Text = "Signup failed. User may already exist.";
        }
    }
}