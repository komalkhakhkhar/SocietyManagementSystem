using System.Windows;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

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

        if (string.IsNullOrEmpty(societyName))
        {
            lblMessage.Text = "Society name is required.";
            return;
        }

        var user = new User
        {
            Username = username,
            Password = password,
            Email = email
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(user);
        if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
        {
            string errors = string.Join("\n", validationResults.Select(v => v.ErrorMessage));
            lblMessage.Text = $"Validation errors:\n{errors}";
            return;
        }

        try
        {
            var mongoDbService = new MongoDbService(societyName);
            bool success = await mongoDbService.RegisterUserAsync(user);
            if (success)
            {
                MessageBox.Show("Account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Dashboard dashboard = new Dashboard(societyName);
                dashboard.Show();
                this.Close();
            }
            else
            {
                lblMessage.Text = "Account creation failed. User may already exist.";
            }
        }
        catch (Exception ex)
        {
            lblMessage.Text = $"Error: {ex.Message}";
            MessageBox.Show($"Signup error: {ex.Message}\n\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void btnLogin_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Login loginWindow = new Login();
        loginWindow.Show();
        this.Close();
    }
}