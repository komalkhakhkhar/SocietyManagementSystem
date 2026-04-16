using System.Windows;
using System.Windows.Controls;
using WPF.Services;

namespace WPF.Views
{
    public partial class LoginPage : Page
    {
        private readonly MongoDbService _mongoDbService;

        public LoginPage()
        {
            InitializeComponent();
            _mongoDbService = new MongoDbService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            bool isValid = await _mongoDbService.ValidateLoginAsync(username, password);
            if (isValid)
            {
                MessageBox.Show("Login successful!");
                // Navigate to main app or something
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void GoToSignupButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to signup page
            NavigationService.Navigate(new SignupPage());
        }
    }
}