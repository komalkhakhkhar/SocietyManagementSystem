using System.Windows;
using System.Windows.Controls;
using WPF.Models;
using WPF.Services;

namespace WPF.Views
{
    public partial class SignupPage : Page
    {
        private readonly MongoDbService _mongoDbService;

        public SignupPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
        }

        private async void SignupButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            var existingUser = await _mongoDbService.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                MessageBox.Show("Username already exists.");
                return;
            }

            var newUser = new User
            {
                Username = username,
                Password = password, // In real app, hash this
                Email = email
            };

            bool success = await _mongoDbService.RegisterUserAsync(newUser);
            if (success)
            {
                MessageBox.Show("Signup successful!");
                // Navigate to login or main
            }
            else
            {
                MessageBox.Show("Signup failed.");
            }
        }

        private void GoToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
    }
}