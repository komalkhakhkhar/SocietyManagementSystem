using System.Windows;
using System.Windows.Controls;
using SocietyManagementSystem.Services;
using SocietyManagementSystem.Models;
using System.ComponentModel.DataAnnotations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using System.Collections.Generic;
using System.Linq;

namespace SocietyManagementSystem;

/// <summary>
/// Interaction logic for Signup.xaml
/// </summary>
public partial class Signup : Page
{
    public Signup()
    {
        InitializeComponent();
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
                var dashboard = new Dashboard(societyName);
                NavigationService?.Navigate(dashboard);
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
        var loginPage = new Login();
        NavigationService?.Navigate(loginPage);
    }
}